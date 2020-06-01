using DataGen.FuncApp.Actors.Messages;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataGen.FuncApp.Actors
{
  public interface IScanManager
  {
    void Initialize(AutomatedScanRequestMessage request);
    void Cancel();
    void UpdateSettings(UpdateScanManagerSettingsRequestMessage request);
    void AddRemoveCustomerDevices(AddRemoveCustomerDevicesRequestMessage request);
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class ScanManagerActor: IScanManager
  {
    [JsonProperty("scanId")]
    string ScanId { get; set; }

    [JsonProperty("customerIds")]
    List<Customer> Customers { get; set; }

    [JsonProperty("interval")]
    int IntervalInMinutes { get; set; }

    [JsonProperty("initialized")]
    bool Initialized { get; set; }

    [FunctionName(nameof(ScanManagerActor))]
    public static Task Run([EntityTrigger] IDurableEntityContext ctx)
    {
      if (!ctx.HasState)
      {

      }

      return ctx.DispatchAsync<ScanManagerActor>();
    }

    #region IScanManager
    public void AddRemoveCustomerDevices(AddRemoveCustomerDevicesRequestMessage request)
    {
      var customer = request.Customer;
      var entityId = new EntityId(nameof(CustomerScanActor), $"{ScanId}~{customer.CustomerId}");
      switch (request.Action)
      { 
        case CustomerDeviceActions.Add:
          Entity.Current.SignalEntity<ICustomerScan>(entityId, 
            proxy => proxy.Start(new CustomerScanRequestMessage { DeviceIds = customer.DeviceIds, IntervalInMinutes = IntervalInMinutes }));
          break;

        case CustomerDeviceActions.Remove:
          Entity.Current.SignalEntity<ICustomerScan>(entityId,
            proxy => proxy.Cancel());
          break;

        default:
          // unknown action detected
          break;
      }
    }

    public void Cancel()
    {
      foreach (var customer in Customers)
      {
        var entityId = new EntityId(nameof(CustomerScanActor), $"{ScanId}~{customer.CustomerId}");
        Entity.Current.SignalEntity<ICustomerScan>(entityId,
            proxy => proxy.Cancel());
      }
    }

    public void Initialize(AutomatedScanRequestMessage request)
    {
      if (!Initialized)
      {
        var entityKey = Entity.Current.EntityKey.Split("~");
        ScanId = entityKey[0];
        Customers = request.Customers;
        IntervalInMinutes = request.IntervalInMinutes;
        Initialized = true;
      }

      foreach (var customer in request.Customers)
      {
        var entityId = new EntityId(nameof(CustomerScanActor), $"{ScanId}~{customer.CustomerId}");
        var customerScanRequest = new CustomerScanRequestMessage 
        {
          DeviceIds = customer.DeviceIds,
          Query = request.Query,
          IntervalInMinutes = request.IntervalInMinutes
        };
        Entity.Current.SignalEntity<ICustomerScan>(entityId,
          proxy =>
            proxy.Start(customerScanRequest)
        );
      }
    }

    public void UpdateSettings(UpdateScanManagerSettingsRequestMessage request)
    {
      foreach (var customer in Customers)
      {
        throw new NotImplementedException();   
      }
    }
    #endregion
  }
}

