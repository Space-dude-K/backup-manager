using backup_manager.Interfaces;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace backup_manager.Workers
{
    internal class ZipWorker : IZipWorker
    {
        private readonly ILogger<ZipWorker> logger;

        public ZipWorker(ILogger<ZipWorker> logger)
        {
            this.logger = logger;
        }
        public void SafelyCreateZipFromDirectory(string file, List<string> copyPaths, bool isDbFile = false)
        {
            try
            {
                string zipFilePath = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + ".zip");

                logger.LogInformation($"Creating zip file -> {zipFilePath}");

                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    var entryName = Path.GetFileName(file);
                    var entry = archive.CreateEntry(entryName, CompressionLevel.SmallestSize);
                    entry.LastWriteTime = File.GetLastWriteTime(file);
                    using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var stream = entry.Open())
                    {
                        fs.CopyTo(stream);
                    }
                }

                if (isDbFile)
                { 
                    MoveDatabaseFileAndDeleteAfterZip(file, zipFilePath, copyPaths);
                }
                else
                {
                    MoveFileAndDeleteAfterZip(file, zipFilePath);
                }
                
            }
            catch (Exception)
            {
                logger.LogError($"");
            }
        }
        private void MoveFileAndDeleteAfterZip(string baseFile, string zipFile) 
        {
            try
            {
                string fileName = Path.GetFileName(zipFile);
                string deviceNameAndSn = zipFile.Substring(0, zipFile.LastIndexOf(@"_"));
                string subFolderPath = zipFile.Substring(0, zipFile.LastIndexOf(@"\"));
                string newFolderPath = Path.Combine(subFolderPath, deviceNameAndSn);
                string newFilePath = Path.Combine(newFolderPath, fileName);

                if (!Directory.Exists(newFolderPath))
                    Directory.CreateDirectory(newFolderPath);

                logger.LogInformation($"Moving file {zipFile}");

                File.Move(zipFile, newFilePath);
                File.Delete(baseFile);
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception when moving file {ex.Message}");
            }
        }
        private string GetPathForDir(string basePath, string serverAndInstance, string backupName)
        {
            string serverAndInstanceFolder = Path.Combine(basePath, serverAndInstance);
            string backupNameFolder = Path.Combine(serverAndInstanceFolder, backupName);

            if (!Directory.Exists(backupNameFolder))
                Directory.CreateDirectory(backupNameFolder);

            return backupNameFolder;
        }
        private void MoveDatabaseFileAndDeleteAfterZip(string baseFile, string zipFile, List<string> copyPaths)
        {
            try
            {
                string fileName = Path.GetFileName(zipFile);
                string subFolderPath = zipFile.Substring(0, zipFile.LastIndexOf(@"\"));

                var splittedStr = zipFile.Split("_");

                string serverAndInstance = splittedStr[0] + "_" + splittedStr[1];
                string backupName = splittedStr[2];

                var backupNameFolder = GetPathForDir(subFolderPath, serverAndInstance, backupName);
                string newFilePath = Path.Combine(backupNameFolder, fileName);

                logger.LogInformation($"Moving file {zipFile}");

                File.Move(zipFile, newFilePath);
                File.Delete(baseFile);

                foreach (var copyPath in copyPaths)
                {
                    var copyDir = GetPathForDir(Path.Combine(copyPath, "Db"), splittedStr[1], backupName);
                    var copyFilePath = Path.Combine(copyDir, Path.GetFileName(newFilePath));

                    if (!Directory.Exists(copyDir))
                        Directory.CreateDirectory(copyDir);

                    File.Copy(newFilePath, copyFilePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception when moving file {ex.Message}");
            }
        }
    }
}