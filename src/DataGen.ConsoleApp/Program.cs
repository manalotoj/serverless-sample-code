using DataGen.Common.Models;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace DataGen.ConsoleApp
{
  /// <summary>
  /// Console app to generate bogus data
  /// </summary>
  /// <remarks>
  /// From command line, execute by:
  /// > DataGen.ConsoleApp.exe -d [number-of-devices] -c [number-of-customers] -i [number-of-iterations] -s [seconds-between-iterations]
  /// All parameters default to 1 except for -s which defaults to 30 seconds
  /// </remarks>
  class Program
  {
    static void Main(string[] args)
    {
      var provider = ConfigureServices();

      var app = new CommandLineApplication<Application>();
      app.Conventions
          .UseDefaultConventions()
          .UseConstructorInjection(provider);

      app.Execute(args);
    }

    public static ServiceProvider ConfigureServices()
    {
      var services = new ServiceCollection();

      IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
      var configuration = builder.Build();

      Log.Logger = new LoggerConfiguration()
        .WriteTo.Console()
        .WriteTo.Debug()
        .WriteTo.File($"{configuration["Logging:FilePath"]}\\log.txt")
        .CreateLogger();

      services.AddSingleton(configuration);
      services.AddLogging(c => c.AddSerilog(Log.Logger));
      return services.BuildServiceProvider();
    }
  }

  public class Application
  {
    private readonly IConfigurationRoot config;
    private readonly ILogger<Application> log;

    public Application(IConfigurationRoot config, ILogger<Application> log)
    {
      this.config = config;
      this.log = log;
    }

    [Option(Description = "Number of devices to generate messages from", ShortName = "d")]
    public string NumberOfDevices { get; }

    [Option(Description = "Number of customers to generate messages for", ShortName = "c")]
    public string NumberOfCustomers { get; }

    [Option(Description = "Number of iterations", ShortName = "i")]
    public string NumberOfIterations { get; }

    [Option(Description = "Seconds between iterations", ShortName = "s")]
    public string SecondsBetweenIterations { get; }

    private void OnExecute()
    {
      int deviceCount = int.Parse(NumberOfDevices ?? "1");
      int customerCount = int.Parse(NumberOfCustomers ?? "1");
      int iterations = int.Parse(NumberOfIterations ?? "1");
      int delayInSeconds = int.Parse(SecondsBetweenIterations ?? "30");
      log.LogInformation($"Device count: {deviceCount}");
      log.LogInformation($"Number of iterations: {iterations}");
      log.LogInformation($"Delay in seconds between iterations: {delayInSeconds}");

      SendMessages(deviceCount, customerCount, iterations, delayInSeconds).GetAwaiter().GetResult();

      log.LogInformation("All done.");
    }

    private async Task SendMessages(int deviceCount, int customerCount, int iterations, int secondsBetweenIterations)
    {
      string ServiceBusConnectionString = config["eventHubConnString"];
      const string QueueName = "devicescans";

      var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

      try
      {
        //
        // todo: use multiple threads 
        //
        for (var i = 0; i < iterations; i++)
        {
          // for each customer (just 1 for now)
          for (var c = 0; c < customerCount; c++)
          {
            var now = DateTime.UtcNow;
            var deviceScanRequest = new DeviceScanRequestMessage
            {
              StartTime = now.AddMinutes(-15),
              EndTime = now,
              Query = "some sample query"
            };

            var messages = new List<Message>();
            // for each customer device
            for (var j = 0; j < deviceCount; j++)
            {
              deviceScanRequest.DeviceId = $"device-{j}";
              deviceScanRequest.CustomerId = $"customer-{c}";

              // Create a new message to send to the queue.
              byte[] messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(deviceScanRequest));
              var message = new Message { Body = messageBody, ContentType = "text/plain" };
              messages.Add(message);
              // Write the body of the message to the console.
              log.LogInformation($"added message to batch: {JsonConvert.SerializeObject(deviceScanRequest)}");
            }

            // Send the message batch to the queue.
            log.LogInformation("Sending batch to queue");
            await queueClient.SendAsync(messages);
          }
          if (i + 1 < iterations)
          {
            Thread.Sleep(secondsBetweenIterations * 1000);
          }
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
      }
    }
  }
}
