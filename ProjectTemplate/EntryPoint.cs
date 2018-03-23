using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Vostok.Hosting;
using Vostok.Logging;
using Vostok.Logging.Serilog;

namespace ProjectTemplate
{
    public static class EntryPoint
    {
        public static void Main(string[] args)
        {
            BuildVostokHost(args).Run();
        }

        private static IVostokHost BuildVostokHost(params string[] args)
        {
            return new VostokHostBuilder<ProjectTemplateApplication>()
                .SetServiceInfo("%project%", "%service%")
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                    configurationBuilder.AddEnvironmentVariables();
                    configurationBuilder.AddJsonFile("hostsettings.json");
                })
                .ConfigureHost((context, hostConfigurator) =>
                {
                    hostConfigurator.SetHostLog(CreateHostLog(context));
                })
                .Build();
        }

        private static ILog CreateHostLog(VostokHostBuilderContext context)
        {
            var configuration = context.Configuration.GetSection("hostLog");

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .ConfigureConsoleLogging(configuration)
                .ConfigureFileLogging(configuration)
                .CreateLogger();

            return new SerilogLog(logger);
        }

        private static LoggerConfiguration ConfigureFileLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            var pathFormat = configuration["pathFormat"];
            return string.IsNullOrEmpty(pathFormat)
                ? loggerConfiguration
                : loggerConfiguration
                    .WriteTo.RollingFile(
                        pathFormat,
                        outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u3} [{Thread}] {Message:l}{NewLine}{Exception}");
        }

        private static LoggerConfiguration ConfigureConsoleLogging(this LoggerConfiguration loggerConfiguration, IConfiguration configuration)
        {
            return !configuration.GetValue<bool>("console")
                ? loggerConfiguration
                : loggerConfiguration
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:HH:mm:ss.fff} {Level:u3} [{Thread}] {Message:l}{NewLine}{Exception}",
                        restrictedToMinimumLevel: LogEventLevel.Information);
        }
    }
}