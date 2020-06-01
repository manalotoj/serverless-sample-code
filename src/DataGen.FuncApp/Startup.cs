using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DataGen.FuncApp.Actors;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(DataGen.FuncApp.Startup))]
namespace DataGen.FuncApp
{
  public class Startup : FunctionsStartup
  {
    public IServiceCollection Services => throw new NotImplementedException();

    public override void Configure(IFunctionsHostBuilder builder)
    {
      var config = new ConfigurationBuilder()
          .SetBasePath(Directory.GetCurrentDirectory())
          .AddEnvironmentVariables()
          .Build();

      var connectionStringBuilder = new EventHubsConnectionStringBuilder(config["eventHubConnString"])
      {
        EntityPath = config["eventHubName"]
      };
      EventHubClient eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());

      builder.Services.AddSingleton(eventHubClient);
      builder.Services.AddScoped(typeof(ScanDeviceWorker));
      builder.Services.AddScoped(typeof(ScanDeviceDispatcher));
      builder.Services.AddScoped(typeof(CustomerScanActor));
      
      builder.Services.AddSingleton<IConfiguration>(config);
      builder.Services.AddLogging();
    }

    public bool IsDevelopmentEnvironment()
    {
      return "Development".Equals(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT"), StringComparison.OrdinalIgnoreCase);
    }
  }
}
