using System.ComponentModel.DataAnnotations;

namespace backup_manager.Model
{
    internal static class Enums
    {
        public enum BackupCmdTypes
        {
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            Default,
            [Display(Name = "copy config %configName% tftp %addr% %file%")]
            J9298A,
            [Display(Name = "copy config %configName% tftp %addr% %file%")]
            J9146A, 
            [Display(Name = "copy config %configName% tftp %addr% %file%")]
            J9145A,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            J9774A,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            J9779A,
            [Display(Name = "copy config %configName% tftp %addr% %file%")]
            J9148A,
            [Display(Name = "copy config %configName% tftp %addr% %file%")]
            J9147A,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            HP,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            J9773A,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            HP_shell,
            Cisco
        }
    }
}