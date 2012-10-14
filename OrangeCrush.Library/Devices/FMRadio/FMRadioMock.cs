using Microsoft.Devices.Radio;

namespace OrangeCrush.Library.Devices
{
	public class FMRadioMock : IFMRadio
	{
		public RadioRegion CurrentRegion { get; set; }
		public double Frequency { get; set; }
		public RadioPowerMode PowerMode { get; set; }
		public double SignalStrength { get; set; }
	}
}
