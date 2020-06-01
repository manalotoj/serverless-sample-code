using DataGen.Common.Models;
using System.Threading.Tasks;

namespace DataGen.FuncApp.Actors
{
  public interface IScanDeviceDispatcher
  {
    Task Scan(DeviceScanRequestMessage request);
  }
}
