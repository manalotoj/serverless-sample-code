using DataGen.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataGen.FuncApp.Actors
{
  /// <summary>
  /// need to accommodate automated scans vs hunting scans
  /// </summary>
  public class CustomerScanRequestMessage: DeviceScanRequestMessage
  {
    public CustomerScanRequestMessage()
    {
      DeviceIds = new List<string>();
    }

    public List<string> DeviceIds { get; set; }
  }
}
