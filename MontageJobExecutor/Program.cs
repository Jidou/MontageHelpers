﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MontageJobExecutor {
    public class Program {

        public const string BasePath = "Files";

        public static void Main(string[] args) {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
#if !DEBUG
                .UseKestrel()
                .ConfigureKestrel((context, options) => {
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(180);
                    options.Limits.MaxRequestBodySize = null;
                })
#else
                .UseIIS()
#endif
                .ConfigureLogging((hostingContext, logging) => {
                    // Requires `using Microsoft.Extensions.Logging;`
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                })
                .UseStartup<Startup>();
    }
}
