﻿/* Based on code by Dave Edson http://windowsteamblog.com/windows_phone/b/wpdev/archive/2010/09/08/using-the-accelerometer-on-windows-phone-7.aspx */
using System;

namespace OrangeCrush.Library.Devices.Sensors
{
	public class EnhancedAccelerometerReading
	{
		public DateTimeOffset Timestamp { get; set; }

		/// <summary>
		/// Raw, unfiltered accelerometer data (acceleration vector in all 3 dimensions) coming directly from sensor.
		/// This is required for updating rapidly reacting UI.
		/// </summary>
		public ThreeDimensionalVector RawAcceleration { get; private set; }

		/// <summary>
		/// Filtered accelerometer data using a combination of a low-pass and threshold triggered high-pass on each axis to 
		/// elimate the majority of the sensor low amplitude noise while trending very quickly to large offsets (not perfectly
		/// smooth signal in that case), providing a very low latency. This is ideal for quickly reacting UI updates.
		/// </summary>
		public ThreeDimensionalVector OptimallyFilteredAcceleration { get; private set; }

		/// <summary>
		/// Filtered accelerometer data using a 1 Hz first-order low-pass on each axis to elimate the main sensor noise
		/// while providing a medium latency. This can be used for moderately reacting UI updates requiring a very smooth signal.
		/// </summary>
		public ThreeDimensionalVector LowPassFilteredAcceleration { get; private set; }

		/// <summary>
		/// Filtered and temporally averaged accelerometer data using an arithmetic mean of the last 25 "optimally filtered" 
		/// samples (see above), so over 500ms at 50Hz on each axis, to virtually eliminate most sensor noise. 
		/// This provides a very stable reading but it has also a very high latency and cannot be used for rapidly reacting UI.
		/// </summary>
		public ThreeDimensionalVector AverageAcceleration { get; private set; }

		public EnhancedAccelerometerReading(
			DateTimeOffset timestamp, 
			ThreeDimensionalVector rawAcceleration, 
			ThreeDimensionalVector optimallyFilteredAcceleration, 
			ThreeDimensionalVector lowPassFilteredAcceleration, 
			ThreeDimensionalVector averageAcceleration)
		{
			Timestamp = timestamp;
			RawAcceleration = ArgumentValidator.AssertNotNull(rawAcceleration, "rawAcceleration");
			OptimallyFilteredAcceleration = ArgumentValidator.AssertNotNull(optimallyFilteredAcceleration, "optimallyFilteredAcceleration");
			LowPassFilteredAcceleration = ArgumentValidator.AssertNotNull(lowPassFilteredAcceleration, "lowPassFilteredAcceleration");
			AverageAcceleration = ArgumentValidator.AssertNotNull(averageAcceleration, "averageAcceleration");
		}
	}
}