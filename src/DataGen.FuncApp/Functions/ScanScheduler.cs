using System.Net.Http;
using System.Threading.Tasks;
using DataGen.Common.Models;
using DataGen.FuncApp.Actors;
using DataGen.FuncApp.Actors.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace DataGen.FuncApp.Functions
{
  public class ScanScheduler
  {
    private readonly ILogger<ScanScheduler> log;

    public ScanScheduler(ILogger<ScanScheduler> log)
    {
      this.log = log;
    }

    #region External Triggers

    #region Service Bus Triggers



    #endregion Service Bus Triggers

    #region HTTP Triggers
    /// <summary>
    /// HttpTrigger for testing; triggers an orchestration
    /// </summary>
    /// <param name="req"></param>
    /// <param name="starter"></param>
    /// <param name="log"></param>
    /// <returns></returns>
    [FunctionName(nameof(HttpStart))]
    public async Task<IActionResult> HttpStart(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
      [DurableClient] IDurableOrchestrationClient starter,
      ILogger log)
    {
      var input = await req.Content.ReadAsAsync<AutomatedScanRequestMessage>();

      log.LogInformation($"input query: '{input.Query}'");

      // Function input comes from the request content.
      string instanceId = await starter.StartNewAsync(nameof(InitializeScanDefinitions), input);

      log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

      //return starter.CreateCheckStatusResponse(req, instanceId);
      return new OkObjectResult("got it");
    }

    /// <summary>
    /// Signal an entity directly from a function
    /// </summary>
    /// <param name="req"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    [FunctionName(nameof(ScanDevice))]
    public async Task<IActionResult> ScanDevice(
      [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
      [DurableClient]IDurableClient client,
      ILogger log)
    {
      var input = await req.Content.ReadAsAsync<DeviceScanRequestMessage>();

      var entityId = new EntityId(nameof(ScanDeviceDispatcher), $"{input.CustomerId}~{input.DeviceId}");
      await client.SignalEntityAsync<IScanDevice>(entityId, proxy =>
        proxy.Scan(input));
      return new OkObjectResult("got it");
    }
    #endregion HTTP Triggers

    #endregion External Triggers

    [FunctionName(nameof(InitializeScanDefinitions))]
    public async Task<string> InitializeScanDefinitions([OrchestrationTrigger] IDurableOrchestrationContext context, [DurableClient]IDurableClient client)
    {
      var automatedScanRequest = context.GetInput<AutomatedScanRequestMessage>();

      var entityId = new EntityId(nameof(ScanManagerActor), automatedScanRequest.ScanId);
      await client.SignalEntityAsync<IScanManager>(entityId, proxy =>
        proxy.Initialize(automatedScanRequest));
      return "done";
    }
  }
}