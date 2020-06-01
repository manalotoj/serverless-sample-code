using System;

namespace DataGen.Common.Models
{
  public class DeviceScanRequestMessage
  {
    public string CustomerId { get; set; }
    public string DeviceId { get; set; }
    public string Query { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int IntervalInMinutes { get; set; }
  }
}
