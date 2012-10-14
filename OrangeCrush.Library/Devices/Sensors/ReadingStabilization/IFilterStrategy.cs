namespace OrangeCrush.Library.Devices.Sensors
{
	public interface IFilterStrategy
	{
		double ApplyFilter(double newInput, double previousOutput);
	}
}