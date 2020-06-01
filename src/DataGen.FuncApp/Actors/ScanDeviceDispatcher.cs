using DataGen.Common.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DataGen.FuncApp.Actors
{

  /// <summary>
  /// Expected <see cref="EntityId"/>
  /// </summary>
  [JsonObject(MemberSerialization.OptIn)]
  public class ScanDeviceDispatcher : IScanDeviceDispatcher
  {
    private readonly ILogger<ScanDeviceDispatcher> log;

    public ScanDeviceDispatcher(ILogger<ScanDeviceDispatcher> log)
    {
      this.log = log;
      var entity = Entity.Current;
      log.LogInformation($"EntityKey: {entity.EntityKey}");
    }

    [JsonProperty("customerId")]
    string CustomerId { get; set; }

    [JsonProperty("deviceId")]
    string DeviceId { get; set; }

    [JsonProperty("currentId")]
    int LastWorkerIndex { get; set; }

    [JsonProperty("initialized")]
    bool Initialized { get; set; }

    public async Task Scan(DeviceScanRequestMessage request)
    {
      log.LogInformation("########## Begin scan request");

      if (!Initialized)
      {
        var entityKey = Entity.Current.EntityKey.Split("~");
        CustomerId = entityKey[0];
        DeviceId = entityKey[1];
        LastWorkerIndex = 0;
        Initialized = true;
      }
      // spawn N number of child actors (workers)
      // apply round robin to distribute load
      //  keep last worker used in its state
      LastWorkerIndex = (LastWorkerIndex + 1) % 10;
      var entityId = new EntityId(nameof(ScanDeviceWorker), Entity.Current.EntityKey + "~" + LastWorkerIndex);
      Entity.Current.SignalEntity<IScanDevice>(entityId, proxy => proxy.Scan(request));
    }

    private void Initialize()
    {
      // query device-specific settings if necessary
    }

    [FunctionName(nameof(ScanDeviceDispatcher))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
    {
      if (!ctx.HasState)
      {

      }

      return ctx.DispatchAsync<ScanDeviceDispatcher>();
    }
  }
}



