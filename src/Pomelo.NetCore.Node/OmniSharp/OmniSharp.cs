using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pomelo.NetCore.Node.Common;
using Pomelo.NetCore.Node.Models;

namespace Pomelo.NetCore.Node
{
    class WorkerThread
    {
        private string Url { get; set; }
        private string Data { get; set; }
        private int Timeout { get; set; }

        public WorkerThread(string url, string data, int timeout)
        {
            Url = url;
            Data = data;
            Timeout = timeout;
        }

        public async Task<string> Run()
        {
//            using (HttpClient client = new HttpClient())
//            {
//                if (Timeout > 0)
//                    client.Timeout = new TimeSpan(Timeout);
//                var response = await client.PostAsync(Url, new StringContent(Data, Encoding.UTF8, "application/json"));
//                var result = await response.Content.ReadAsStringAsync();
//                return result;
//            }

            var httpReq = (HttpWebRequest) WebRequest.Create(Url);
            httpReq.Method = "POST";
            httpReq.ContentType = httpReq.Accept = "application/json";

            using (var stream = await httpReq.GetRequestStreamAsync())
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(Data);
            }

            using (var response = await httpReq.GetResponseAsync())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }

    public class OmniSharp
    {
        private ILogger Logger { get; set; }
        public static Config Config;

        private static readonly ConcurrentDictionary<string, int> ServerPorts;
        private static readonly ConcurrentDictionary<string, bool> LauncherProcs;

        static OmniSharp()
        {
            ServerPorts = new ConcurrentDictionary<string, int>();
            LauncherProcs = new ConcurrentDictionary<string, bool>();
        }

        public OmniSharp(IServiceProvider serviceProvider, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Config = new Config(serviceProvider, configuration);
            Logger = loggerFactory.CreateLogger<OmniSharp>();
        }

        public async Task<string> GetResponse(string endpoint, string solutionPath, Dictionary<string, string> param,
            int timeout = 0)
        {
            if (string.IsNullOrWhiteSpace(solutionPath) || !ServerPorts.ContainsKey(solutionPath))
                await CreateOmnisharpServerSubprocess(solutionPath);

            var para = new Dictionary<string, string>
            {
                {"line", "0"},
                {"column", "0"},
                {"buffer", ""},
                {"filename", ""}
            };
            para = para.UpdateBy(param);
            var host = "localhost";
            var port = ServerPorts[solutionPath];
            var url = $"http://{host}:{port}{endpoint}";
            var data = JsonConvert.SerializeObject(para);
            Logger.LogInformation($"data: {data}");
            var thread = new WorkerThread(url, data, timeout);
            var result = await thread.Run();
            return result;
        }

        public async Task<string> CreateOmnisharpServerSubprocess(string solutionPath)
        {
            if (LauncherProcs.ContainsKey(solutionPath))
            {
                Logger.LogInformation($"already_bound_solution: {solutionPath}");
                return null;
            }

            Logger.LogInformation($"solution_path: {solutionPath}");
            var omniPort = FreePort.FindNextAvailableTCPPort(2000);
            Logger.LogInformation($"omni_port: {omniPort}");
            var args = $" -s \"{solutionPath}\" -p {omniPort} --hostPID {Environment.CurrentManagedThreadId}";
            var fullFileName = Path.Combine(Config.OmniSharpPath, Config.OmniSharpExe);
            Logger.LogInformation($"OmniSharpExePath: {fullFileName}");
            Logger.LogInformation($"args: {args}");
            ProcessHelper.Run(Config.OmniSharpPath, fullFileName, args);
            LauncherProcs[solutionPath] = true;
            ServerPorts[solutionPath] = omniPort;
            return "成功";
        }

        public async Task CloseOmnisharpServerSubprocess(string solutionPath)
        {
            await GetResponse(solutionPath, "/stopserver", null);
            bool result1;
            LauncherProcs.TryRemove(solutionPath, out result1);
            int result2;
            ServerPorts.TryRemove(solutionPath, out result2);
        }

        public async Task<string> restart_omnisharp_server_subprocess(string solutionPath)
        {
            await CloseOmnisharpServerSubprocess(solutionPath);
            return await CreateOmnisharpServerSubprocess(solutionPath);
        }
    }
}