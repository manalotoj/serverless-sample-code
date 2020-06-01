namespace DataGen.FuncApp.Actors.Messages
{
  public class UpdateScanManagerSettingsRequestMessage
  {
    public int? IntervalInMinutes { get; set; }
    public string Query { get; set; }
  }
}
