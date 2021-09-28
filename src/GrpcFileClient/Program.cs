using System;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddLogging();

            services.AddScoped<FileTransfer>();

            services.AddScoped<GrpcFileClientForm>();
        }
    }
}
