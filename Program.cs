using backup_manager.BackupWorkers;
using backup_manager.Cypher;
using backup_manager.Interfaces;
using backup_manager.Model;
using backup_manager.Servers;
using backup_manager.Settings;
using backup_manager.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using static backup_manager.Model.Enums;

namespace backup_manager
{
    internal class Program
    {
        static string sshClientAddress = "10.10.102.1";
        static string backupCmd = "backup startup-configuration to 10.10.200.37 test3.cfg";
        static string backupCmd1 = "copy running-configuration 10.10.200.37 run.cfg";
        static string ServerDirectory = Environment.CurrentDirectory;
        static async Task Main(string[] args)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                var config = new ConfigurationBuilder()
                   //.SetBasePath(System.IO.Directory.GetCurrentDirectory())
                   //.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                   .Build();

                var servicesProvider = BuildDi(config);

                using (servicesProvider as IDisposable)
                {
                    var conf = servicesProvider.GetRequiredService<IConfigurator>();

                    if(args.Length != 0)
                    {
                        foreach (var parameter in args)
                        {
                            switch (parameter.ToLower())
                            {
                                case "-setreq":

                                    Console.WriteLine($"Set req init.");
                                    Console.WriteLine("Введите id, логин, пароль через пробел:");

                                    while (true)
                                    {
                                        var loginAdnPass = (Console.ReadLine().Split());

                                        if (loginAdnPass.Length != 3 || string.IsNullOrEmpty(loginAdnPass[0]) 
                                            || string.IsNullOrEmpty(loginAdnPass[1]) || string.IsNullOrEmpty(loginAdnPass[2]))
                                        {
                                            Console.WriteLine("Ошибка ввода. Введите id, логин, пароль через пробел: ");
                                            continue;
                                        }
                                        else
                                        {
                                            int loginId = 0;
                                            int.TryParse(loginAdnPass[0], out  loginId);

                                            conf.SaveLoginSettings(new Login(loginId, loginAdnPass[1], loginAdnPass[2]));

                                            var addedReq = conf.LoadLoginSettings();

                                            Console.WriteLine($"Added reqs: {addedReq[addedReq.Count - 1].AdmLogin} " +
                                                $"{addedReq[addedReq.Count - 1].AdminPass}");
                                        }

                                        Console.WriteLine("Press Enter to exit.");

                                        if (Console.ReadKey().Key == ConsoleKey.Enter)
                                            break;
                                    }
                                    break;
                            }
                        }  
                    }

                    var backupManager = servicesProvider.GetRequiredService<IBackupManager>();
                    var deviceConfigs = conf.LoadDeviceSettings(conf.LoadLoginSettings());
                    var backupPaths = conf.LoadPathSettings();
                    var sftpTempPath = conf.LoadSftpTempFolderPath();

                    /*if(deviceConfigs.Any(d => d.BackupCmdType == BackupCmdTypes.HP))
                    {
                        var sftpServer = servicesProvider.GetRequiredService<ISftpServer>();
                        Task.Run(() => sftpServer.RunSftpServer(sftpTempPath));
                    }*/

                    await backupManager.Init(deviceConfigs, backupPaths, sftpTempPath);
                }
            }
            catch (Exception ex)
            {
                // NLog: catch any exception and log it.
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
        }
        private static IServiceProvider BuildDi(IConfiguration config)
        {
            return new ServiceCollection()
                  // Add DI Classes here
                  .AddSingleton<ICypher, Encryptor>()
                  .AddSingleton<IConfigurator, Configurator>()
                  .AddTransient<ITftpServer, TftpServer>()
                  .AddTransient<ISftpServer, SftpServer>()
                  .AddTransient<IBackupManager, BackupManager>()
                  .AddTransient<ISshWorker, SshWorker>()
                  .AddTransient<ISshShellWorker, SshShellWorker>()
                  .AddLogging(loggingBuilder =>
                  {
                      // configure Logging with NLog
                      loggingBuilder.ClearProviders();
                      loggingBuilder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                      loggingBuilder.AddNLog(config);
                  })
                  .BuildServiceProvider();
        }
    }
}
