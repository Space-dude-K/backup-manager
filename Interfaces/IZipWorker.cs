namespace backup_manager.Interfaces
{
    internal interface IZipWorker
    {
        void SafelyCreateZipFromDirectory(string file);
    }
}