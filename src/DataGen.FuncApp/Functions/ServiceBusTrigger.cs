using System;
using System.Threading.Tasks;
using DataGen.Common.Models;
using DataGen.FuncApp.Actors;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DataGen.FuncApp.Functions
{
  public class ServiceBusTrigger
  {
    [FunctionName("ServiceBusTrigger")]
    public async Task Run(
      [ServiceBusTrigger("%devicescanqueue%", Connection = "devicescansqueueconnstring")] string myQueueItem,
      [DurableClient] IDurableClient client,
      ILogger log)
    {
      log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
      DeviceScanRequestMessage message = JsonConvert.DeserializeObject<DeviceScanRequestMessage>(myQueueItem);
      
      var entityId = new EntityId(nameof(ScanDeviceDispatcher), $"{message.CustomerId}~{message.DeviceId}");
      
      await client.SignalEntityAsync<IScanDevice>(entityId, proxy =>
        proxy.Scan(message));
    }
  }
}
