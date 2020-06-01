using DataGen.Common.Models;
using DataGen.FuncApp.Actors.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataGen.FuncApp.Actors
{
  public interface ICustomerScan
  {
    // update settings (query, interval, dispatcher concurrency)
    void Update(UpdateScanManagerSettingsRequestMessage request);

    void Start(CustomerScanRequestMessage request);

    void ScanDevices();

    void Cancel();
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class CustomerScanActor : ICustomerScan
  {
    private readonly ILogger<CustomerScanActor> log;

    public CustomerScanActor(ILogger<CustomerScanActor> log)
    {
      this.log = log;
    }
    #region Properties
    [JsonProperty("initialized")]
    bool Initialized { get; set; }

    [JsonProperty("customerId")]
    string CustomerId { get; set; }

    [JsonProperty("scanDefinitionId")]
    string ScanDefinitionId { get; set; }

    [JsonProperty("queery")]
    string Query { get; set; }

    /// <summary>
    /// Interval in minutes
    /// </summary>
    [JsonProperty("interval")]
    int IntervalInMinutes { get; set; }

    [JsonProperty("devices")]
    List<string> DeviceIds { get; set; }

    [JsonProperty("cancelled")]
    Boolean Cancelled { get; set; }
    #endregion

    /// <summary>
    /// Performs intial scan and schedules next scan
    /// </summary>
    /// <param name="request"></param>
    /// <remarks>
    /// Modifications required:
    /// - make scan parameters instance properties
    /// - create Initialize() method
    ///   - set properties including devices
    /// - and support to update definition
    /// </remarks>
    public void ScanDevices()
    {
      if (Cancelled) return;

      var now = DateTime.UtcNow;
      var request = 
        new DeviceScanRequestMessage 
        { 
          Query = Query, 
          StartTime = now.AddMinutes(-IntervalInMinutes),
          EndTime = now
        };

      foreach (var device in DeviceIds)
      {
        log.LogInformation($"########## Signaling device '{device}'");
        // invoke ScanDevice
        Entity.Current.SignalEntity<IScanDeviceDispatcher>(
          new EntityId(nameof(ScanDeviceDispatcher), $"{CustomerId}~{device}"),
          proxy => proxy.Scan(request));
      }

      //invoke again in [Interval] minutes
      Entity.Current.SignalEntity<ICustomerScan>(
        Entity.Current.EntityKey,
        DateTime.UtcNow.AddSeconds(IntervalInMinutes),
        proxy => proxy.ScanDevices());
    }

    public void Cancel()
    {
      Cancelled = true;
    }

    public void Update(UpdateScanManagerSettingsRequestMessage request)
    {
      if (!string.IsNullOrEmpty(request.Query)) Query = request.Query;
      if (request.IntervalInMinutes.HasValue) IntervalInMinutes = request.IntervalInMinutes.Value;
    }

    public void Start(CustomerScanRequestMessage request)
    {
      if (Initialized) return;

      var entityId = Entity.Current.EntityKey.Split("~");
      ScanDefinitionId = entityId[0];
      CustomerId = entityId[1];

      IntervalInMinutes = request.IntervalInMinutes;
      Query = request.Query;
      DeviceIds = request.DeviceIds;

      Initialized = true;

      ScanDevices();
    }

    #region Durable Entity entry point
    [FunctionName(nameof(CustomerScanActor))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
    {
      if (!ctx.HasState)
      {

      }

      return ctx.DispatchAsync<CustomerScanActor>();
    }
    #endregion
  }
}
