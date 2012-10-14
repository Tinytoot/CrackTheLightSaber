using Microsoft.Devices.Radio;

namespace OrangeCrush.Library.Devices
{
	public interface IFMRadio
	{
		RadioRegion CurrentRegion { get; set; }
		double Frequency { get; set; }
		RadioPowerMode PowerMode { get; set; }
		double SignalStrength { get; }
	}
}