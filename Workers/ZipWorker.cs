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
        public void SafelyCreateZipFromDirectory(string file)
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

                MoveFileAndDeleteAfterZip(file, zipFilePath);
            }
            catch (Exception)
            {
                logger.LogError($"");
            }
        }
        public void MoveFileAndDeleteAfterZip(string baseFile, string zipFile) 
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
    }
}