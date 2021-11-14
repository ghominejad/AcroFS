using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acrobit.AcroFS
{
    internal static class ConfigFactory
    {
        internal static IConfiguration _config = null;
        internal static IConfiguration GetConfig()
        {
            if (_config == null)
            {
                _config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", false, true)
                    .Build();

            }

            return _config;

        }
    }
}
