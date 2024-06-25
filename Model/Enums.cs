using System.ComponentModel.DataAnnotations;

namespace backup_manager.Model
{
    internal static class Enums
    {
        public enum BackupCmdTypes
        {
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            Default,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            HP,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            HP_shell,
            Cisco
        }
    }
}