using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGen.FuncApp.Actors.Messages
{
  [JsonObject(MemberSerialization.OptIn)]
  public class AutomatedScanRequestMessage
  {
    [JsonProperty("scanId")]
    public string ScanId { get; set; }
    [JsonProperty("Query")]
    public string Query { get; set; }
    [JsonProperty("intervalInMinutes")]
    public int IntervalInMinutes { get; set; }
    [JsonProperty("customers")]
    public List<Customer> Customers { get; set; }
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class Customer
  {
    [JsonProperty("customerId")]
    public string CustomerId { get; set; }
    [JsonProperty("deviceIds")]
    public List<string> DeviceIds { get; set; }
    [JsonProperty("query")]
    public string Query { get; set; }
    [JsonProperty("intervalInMinutes")]
    public int IntervalInMinutes { get; set; }
  }

  public class HuntingScanRequest
  {

  }
}
