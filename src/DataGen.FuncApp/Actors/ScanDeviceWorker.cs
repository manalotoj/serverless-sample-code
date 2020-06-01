using DataGen.Common.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;

namespace DataGen.FuncApp.Actors
{
  public interface IScanDevice
  {
    Task Scan(DeviceScanRequestMessage request);
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class ScanDeviceWorker: IScanDevice
  {
    private readonly ILogger<ScanDeviceWorker> log;
    private readonly EventHubClient hubClient;

    public ScanDeviceWorker(EventHubClient hubClient, ILogger<ScanDeviceWorker> log)
    {
      this.hubClient = hubClient;
      this.log = log;
    }

    [JsonProperty("customerId")]
    string CustomerId { get; set; }

    [JsonProperty("deviceId")]
    string DeviceId { get; set; }

    [JsonProperty("workerId")]
    string WorkerId { get; set; }

    [JsonProperty("initialized")]
    bool Initialized { get; set; }

    public async Task Scan(DeviceScanRequestMessage request)
    {
      if (!Initialized)
      {
        var entityKey = Entity.Current.EntityKey.Split("~");
        CustomerId = entityKey[0];
        DeviceId = entityKey[1];
        WorkerId = entityKey[2];
      }

      await RequestScan(request);
    }

    private async Task RequestScan(DeviceScanRequestMessage request)
    {
      // a scan can take up to 10 seconds
      Thread.Sleep(5);
      await SendToEventHub(request);
      log.LogInformation($"########## Scan completed by {Entity.Current.EntityKey}");
    }

    private async Task SendToEventHub(object scan)
    {
      var scanBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(scan));
      EventData data = new EventData(scanBytes);
      await hubClient.SendAsync(data);
      log.LogInformation($"########## scan sent to event hub by {Entity.Current.EntityKey}");
    }

    [FunctionName(nameof(ScanDeviceWorker))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
    {
      return ctx.DispatchAsync<ScanDeviceWorker>();
    }
  }
}
