/* Based on code by Dave Edson http://windowsteamblog.com/windows_phone/b/wpdev/archive/2010/09/08/using-the-accelerometer-on-windows-phone-7.aspx */
using System;

namespace OrangeCrush.Library.Devices.Sensors
{
	public sealed class ThreeDimensionalVector
	{
		/// <summary>
		/// X-axis coordinate.
		/// </summary>
		public double X { get; private set; }

		/// <summary>
		/// Y-axis coordinate.
		/// </summary>
		public double Y { get; private set; }

		/// <summary>
		/// Z-axis coordinate.
		/// </summary>
		public double Z { get; private set; }

		/// <summary>
		/// Default constructor.
		/// </summary>
		public ThreeDimensionalVector()
		{
			/* Intentionally left blank. */
		}

		/// <summary>
		/// Vector constructor from three double values
		/// </summary>
		/// <param name="xAxisCoordinate">X-axis coordinate.</param>
		/// <param name="yAxisCoordinate">Y-axis coordinate.</param>
		/// <param name="zAxisCoordinate">Z-axis coordinate.</param>
		public ThreeDimensionalVector(
			double xAxisCoordinate, double yAxisCoordinate, double zAxisCoordinate)
		{
			X = xAxisCoordinate;
			Y = yAxisCoordinate;
			Z = zAxisCoordinate;
		}

		/// <summary>
		/// Clones the specified vector.
		/// </summary>
		/// <param name="threeDimensionalVector">The vector to clone.</param>
		public ThreeDimensionalVector(ThreeDimensionalVector threeDimensionalVector)
		{
			ArgumentValidator.AssertNotNull(threeDimensionalVector, "simple3DVector");

			if (threeDimensionalVector != null)
			{
				X = threeDimensionalVector.X;
				Y = threeDimensionalVector.Y;
				Z = threeDimensionalVector.Z;
			}
		}

		public override string ToString()
		{
			return String.Format("({0:+00.000;-00.000;+00.000},{1:+00.000;-00.000;+00.000},{2:+00.000;-00.000;+00.000})", X, Y, Z);
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> 
		/// is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> 
		/// to compare with this instance.</param>
		/// <returns>
		/// 	<c>true</c> if the specified <see cref="System.Object"/> is equal 
		/// to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is ThreeDimensionalVector)
			{
				return (bool)(this == (ThreeDimensionalVector)obj);
			}
			return false;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms 
		/// and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		}

		/// <summary>
		/// Operator overload for the == operator and two vectors
		/// </summary>
		public static bool operator ==(
			ThreeDimensionalVector v1, ThreeDimensionalVector v2)
		{
			if (Object.ReferenceEquals(v1, v2))
			{
				return true;
			}

			if ((object)v1 == null || (object)v2 == null)
			{
				return false;
			}

			return v1.X == v2.X && v1.Y == v2.Y && v1.Z == v2.Z;
		}

		/// <summary>
		/// Operator overload for the != operator and two vectors
		/// </summary>
		public static bool operator !=(
			ThreeDimensionalVector v1, ThreeDimensionalVector v2)
		{
			return !(v1 == v2);
		}

		/// <summary>
		/// Operator overload for the + operator and two vectors
		/// </summary>
		public static ThreeDimensionalVector operator +(
			ThreeDimensionalVector v1, ThreeDimensionalVector v2)
		{
			return new ThreeDimensionalVector(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}

		/// <summary>
		/// Operator overload for the - operator and for two vectors
		/// </summary>
		public static ThreeDimensionalVector operator -(
			ThreeDimensionalVector v1, ThreeDimensionalVector v2)
		{
			return new ThreeDimensionalVector(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		}

		/// <summary>
		/// Operator overload for the * operator and two vectors.
		/// </summary>
		public static ThreeDimensionalVector operator *(
			ThreeDimensionalVector vector1, ThreeDimensionalVector vector2)
		{
			return new ThreeDimensionalVector(
				vector1.X * vector2.X, vector1.Y * vector2.Y, vector1.Z * vector2.Z);
		}

		/// <summary>
		/// Operator overload for the * operator and a vector 
		/// and a double to perform scaling.
		/// </summary>
		public static ThreeDimensionalVector operator *(
			ThreeDimensionalVector vector, double d)
		{
			return new ThreeDimensionalVector(d * vector.X, d * vector.Y, d * vector.Z);
		}

		/// <summary>
		/// Operator overload for the / operator and a vector 
		/// and a double to perform scaling.
		/// </summary>
		public static ThreeDimensionalVector operator /(
			ThreeDimensionalVector vector, double d)
		{
			return new ThreeDimensionalVector(
				vector.X / d, vector.Y / d, vector.Z / d);
		}
		
		/// <summary>
		/// Gets the magnitude of the vector. X^2 + Y^2 + Z^2
		/// </summary>
		/// <value>The magnitude is X^2 + Y^2 + Z^2.</value>
		public double Magnitude
		{
			get
			{
				return Math.Sqrt(X * X + Y * Y + Z * Z);
			}
		}
	}
}
