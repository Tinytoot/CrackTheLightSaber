using Microsoft.Xna.Framework;

namespace OrangeCrush.Library.Devices.Sensors
{
	public static class Vector3Extensions
	{
		public static ThreeDimensionalVector ToThreeDimensionalVector(this Vector3 vector3)
		{
			return new ThreeDimensionalVector(vector3.X, vector3.Y, vector3.Z);
		}
	}
}