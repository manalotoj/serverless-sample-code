using System;
using System.Collections.Generic;
using System.Text;

namespace DataGen.FuncApp.Actors.Messages
{

  public class AddRemoveCustomerDevicesRequestMessage
  {
    public CustomerDeviceActions Action { get; set; }
    public Customer Customer { get; set; }
  }
}
