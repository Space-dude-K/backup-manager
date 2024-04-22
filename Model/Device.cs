﻿using static backup_manager.Model.Enums;

namespace backup_manager.Model
{
    internal class Device
    {
        public string Ip { get; set; }
        public string Name { get; set; }
        public BackupCmdTypes BackupCmdType { get; set; }
        public Login Login {  get; set; }
    }
}