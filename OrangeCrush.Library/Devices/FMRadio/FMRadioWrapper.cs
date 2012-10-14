using Microsoft.Devices.Radio;

namespace OrangeCrush.Library.Devices
{
	public class FMRadioWrapper : IFMRadio
	{
		public RadioRegion CurrentRegion
		{
			get
			{
				return FMRadio.Instance.CurrentRegion;
			}
			set
			{
				FMRadio.Instance.CurrentRegion = value;
			}
		}

		public double Frequency
		{
			get
			{
				return FMRadio.Instance.Frequency;
			}
			set
			{
				FMRadio.Instance.Frequency = value;
			}
		}

		public RadioPowerMode PowerMode
		{
			get
			{
				return FMRadio.Instance.PowerMode;
			}
			set
			{
				FMRadio.Instance.PowerMode = value;
			}
		}

		public double SignalStrength
		{
			get
			{
				return FMRadio.Instance.SignalStrength;
			}
		}
	}
}
