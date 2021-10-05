using System;
using System.IO;
using System.Windows.Forms;
using GrpcFileClient.Resolvers;
using GrpcFileClient.Services;
using GrpcFileClient.Types;
using Infra.FileAccess.Grpc;
using Infra.FileAccess.Physical;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using NLog.Extensions.Logging;

namespace GrpcFileClient
{
    internal static class Program
    {
        [STAThread]
        internal static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();

            ConfigureServices(services);

            using var sp = services.BuildServiceProvider();

            var grpcFileClientForm = sp.GetRequiredService<GrpcFileClientForm>();

            Application.Run(grpcFileClientForm);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddLogging(builder => builder.AddNLog("Nlog.config"));

            #region Configuration

            var releaseJsonSource = new JsonConfigurationSource()
            {
                FileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                Path = "appsettings.json",
                Optional = false,
                ReloadOnChange = true
            };

            var config = new ConfigurationBuilder()
                .Add(releaseJsonSource)
                .Build();

            services.AddSingleton<IConfiguration>(config);

            #endregion

            services.AddSingleton<PhysicalFileAccess>();
            services.AddSingleton<GrpcFileAccess>();
            services.AddSingleton<FileAccessResolver>(
                sp => fileAccessType => fileAccessType switch
                {
                    FileAccessType.Physical => sp.GetRequiredService<PhysicalFileAccess>(),
                    FileAccessType.Grpc => sp.GetRequiredService<GrpcFileAccess>(),
                    _ => null,
                });

            services.AddScoped<FileService>();

            services.AddScoped<GrpcFileClientForm>();
        }
    }
}
