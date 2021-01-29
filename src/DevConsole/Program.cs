using System;
using System.IO;
using DevConsole.DatabaseTesting.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Encryption;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Metrics;
using Rn.NetCore.Common.Metrics.Interfaces;
using Rn.NetCore.DbCommon;
using Rn.NetCore.DbCommon.Helpers;
using Rn.NetCore.Metrics.Rabbit;

namespace DevConsole
{
  class Program
  {
    private static IServiceProvider _serviceProvider;
    private static ILoggerAdapter<Program> _logger;

    static void Main(string[] args)
    {
      ConfigureDI();

      var userRepo = _serviceProvider.GetRequiredService<IUserRepo>();
      var userEntity = userRepo.GetUser()
        .ConfigureAwait(false)
        .GetAwaiter()
        .GetResult();

      Console.WriteLine("Hello World!");
    }


    // Internal methods
    private static void ConfigureDI()
    {
      var services = new ServiceCollection();

      var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

      ConfigureDI_Core(services, config);
      ConfigureDI_DBComponents(services);
      ConfigureDI_Metrics(services);

      _serviceProvider = services.BuildServiceProvider();
      _logger = _serviceProvider.GetService<ILoggerAdapter<Program>>();
    }

    private static void ConfigureDI_Core(IServiceCollection services, IConfiguration config)
    {
      services
        .AddSingleton(config)
        .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
        .AddSingleton<IEncryptionService, EncryptionService>()
        .AddSingleton<IEncryptionUtils, EncryptionUtils>()
        .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
        .AddSingleton<IJsonHelper, JsonHelper>()
        .AddLogging(loggingBuilder =>
        {
          // configure Logging with NLog
          loggingBuilder.ClearProviders();
          loggingBuilder.SetMinimumLevel(LogLevel.Trace);
          loggingBuilder.AddNLog(config);
        });
    }

    private static void ConfigureDI_DBComponents(IServiceCollection services)
    {
      services
        .AddSingleton<IDbHelper, MySqlHelper>()
        .AddSingleton<IUserRepo, UserRepo>();
    }

    private static void ConfigureDI_Metrics(IServiceCollection services)
    {
      services
        .AddSingleton<IMetricService, MetricService>()
        .AddSingleton<IMetricOutput, RabbitMetricOutput>()
        .AddSingleton<IRabbitFactory, RabbitFactory>()
        .AddSingleton<IRabbitConnection, RabbitConnection>();
    }
  }
}
