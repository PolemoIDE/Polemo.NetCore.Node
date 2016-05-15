using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Polemo.NetCore.Node.Models
{
    public class Config
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public Config(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public string RootPath
        {
            get
            {
                var path = _configuration["RootPath"];
                if (string.IsNullOrEmpty(path))
                {
                    var appEnv = _serviceProvider.GetRequiredService<ApplicationEnvironment>();
                    return appEnv.ApplicationBasePath;
                }
                return path;
            }
        }

        public string OmniSharpPath
        {
            get
            {
                var path = _configuration["OmniSharpPath"];
                if (string.IsNullOrEmpty(path))
                {
                    var appEnv = _serviceProvider.GetRequiredService<ApplicationEnvironment>();
                    return appEnv.ApplicationBasePath;
                }
                return path;
            }
        }

        public string OmniSharpExe
        {
            get
            {
                var path = _configuration["OmniSharpExe"];
                return string.IsNullOrEmpty(path) ? "OmniSharp.exe" : path;
            }
        }
    }
}
