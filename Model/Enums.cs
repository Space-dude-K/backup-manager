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
            QSFP28,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            JL256A,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            JL072A,
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
            J9584A,
            [Display(Name = "/system backup save name=%file%")]
            Mikrotik,
            [Display(Name = "execute backup config tftp %file% %addr%")]
            Fortigate,
            [Display(Name = "copy config tftp %addr% %file%")]
            AP_HP,
            [Display(Name = "tftp -p -l /tmp/system.cfg -r %file% %addr%")]
            NanoStation,
            [Display(Name = "transfer upload mode tftp, transfer upload datatype config, transfer upload filename %file%, transfer upload path ., transfer upload serverip %addr%, transfer upload start")]
            Cisco_vWLC,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            HP_shell,
            [Display(Name = "backup startup-configuration to %addr% %file%")]
            Sql
        }
        public enum BackupDbTypes
        {
            Full,
            Diff,
            Tail,
            TRN
        }
    }
}