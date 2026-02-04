using GrpcService.Services;
using Microsoft.Extensions.Hosting.WindowsServices;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

namespace GrpcService
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var bootstrapLogger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
            bootstrapLogger.Debug("GrpcSample Starting...");

            try
            {
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    Args = args,
                    // when running as a windows service we have to set the root path at startup
                    // otherwise we get errors when we start to run the app.
                    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
                });
                IConfigurationSection nlogSection = builder.Configuration.GetSection("NLog");
                LogManager.Configuration = new NLogLoggingConfiguration(nlogSection);
                builder.Host.UseNLog();


                // Add services to the container.
                builder.Services.AddGrpc();
                builder.Host.UseWindowsService();

                var app = builder.Build();

                // Configure the HTTP request pipeline.
                app.MapGrpcService<GreeterService>();
                app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

                bootstrapLogger.Info("Running the application...");
                Task runTask = app.RunAsync();
                await runTask;

                bootstrapLogger.Info("Application run completed.");
            }
            catch (Exception ex)
            {
                bootstrapLogger.Error(ex, "An unexpected error occured in the grpc sample");
            }
            LogManager.Flush();
            LogManager.Shutdown();
        }
    }
}