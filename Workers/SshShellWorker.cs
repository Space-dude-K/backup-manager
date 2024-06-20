using Renci.SshNet.Common;
using Renci.SshNet;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using backup_manager.Model;
using backup_manager.Interfaces;
using System.Text;

namespace backup_manager.Workers
{
    internal class SshShellWorker : ISshShellWorker
    {
        SshClient sshClient;
        ShellStream shell;
        string pwd = "";
        string lastCommand = "";

        static Regex prompt = new Regex("[a-zA-Z0-9_.-]*\\@[a-zA-Z0-9_.-]*\\:\\~[#$] ", RegexOptions.Compiled);
        static Regex pwdPrompt = new Regex("password for .*\\:", RegexOptions.Compiled);
        static Regex promptOrPwd = new Regex(prompt + "|" + pwdPrompt);

        private readonly ILogger<SshShellWorker> logger;

        public SshShellWorker(ILogger<SshShellWorker> logger)
        {
            this.logger = logger;
        }
        // TODO. Async ssh shell calls with completion result.
        // TODO. Async ssh shell calls with completion result.
        public async Task ConnectAndExecuteAsync(Device device, string cmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));

            this.pwd = device.Login.AdminPass;

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 4096, terminalMode);

                try
                {
                    AsyncCallback onWorkDone = (ar) =>
                    {
                        /*
                        AsyncCallback onInternalWorkDone = (ar) =>
                        {
                            logger.LogInformation("Internal work  done!");
                        };
                        var asyncInternalResult = shell.BeginExpect(onInternalWorkDone, new ExpectAction("#", (_) =>
                        {
                            shell.WriteLine(cmd);
                            //await shell.WriteAsync(Encoding.ASCII.GetBytes(cmd).AsMemory(0, cmd.Length));
                        }));
                        asyncInternalResult.AsyncWaitHandle.WaitOne();
                        var internalResult = shell.EndExpect(asyncInternalResult);
                        */
                        logger.LogInformation("External work done!");
                    };

                    shell.WriteLine("\n");
                    //await shell.WriteAsync(Encoding.ASCII.GetBytes("\n").AsMemory(0, "\n".Length));

                    var asyncExternalResult = shell.BeginExpect(onWorkDone, new ExpectAction("#", (_) =>
                    {
                        shell.WriteLine(cmd);
                        //await shell.WriteAsync(Encoding.ASCII.GetBytes(cmd).AsMemory(0, cmd.Length));
                    }));

                    asyncExternalResult.AsyncWaitHandle.WaitOne();
                    var result = shell.EndExpect(asyncExternalResult);

                    //logger.LogInformation($"External work result: {result}");
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                    throw;
                }

                client.Disconnect();
            }
        }
        public void ConnectAndExecute(Device device, string cmd)
        {
            var connectionInfo =
                new ConnectionInfo(
                    device.Ip,
                    22,
                    device.Login.AdmLogin,
                    new PasswordAuthenticationMethod(device.Login.AdmLogin, device.Login.AdminPass));

            this.pwd = device.Login.AdminPass;

            using (var client = new SshClient(device.Ip, device.Login.AdmLogin, device.Login.AdminPass))
            {
                client.Connect();

                logger.LogInformation($"Conn info: {client.ConnectionInfo.Host + " "
                    + client.ConnectionInfo.ServerVersion}, isConnected -> {client.IsConnected}");

                var terminalMode = new Dictionary<TerminalModes, uint>();
                terminalMode.Add(TerminalModes.ECHO, 53);

                shell = client.CreateShellStream("", 0, 0, 0, 0, 4096, terminalMode);

                try
                {
                    shell.Expect(new ExpectAction("Press any key to continue", (_) => 
                    {
                        shell.WriteLine("\n");

                        shell.Expect(new ExpectAction("#", (_) =>
                        {
                            shell.WriteLine(cmd);
                        }));

                    }));

                    logger.LogInformation($"Read: {shell.Read()}");
                }
                catch (Exception ex)
                {
                    logger.LogError("Exception - " + ex.Message);
                    throw;
                }

                logger.LogInformation($"Run cmd -> {cmd}");

                client.Disconnect();
            }
        }
    }
}