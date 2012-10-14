#region File and License Information
/*
<File>
	<Copyright>Copyright © 2010, Daniel Vaughan. All rights reserved.</Copyright>
	<License>
		Redistribution and use in source and binary forms, with or without
		modification, are permitted provided that the following conditions are met:
			* Redistributions of source code must retain the above copyright
			  notice, this list of conditions and the following disclaimer.
			* Redistributions in binary form must reproduce the above copyright
			  notice, this list of conditions and the following disclaimer in the
			  documentation and/or other materials provided with the distribution.
			* Neither the name of the <organization> nor the
			  names of its contributors may be used to endorse or promote products
			  derived from this software without specific prior written permission.

		THIS SOFTWARE IS PROVIDED BY Daniel Vaughan ''AS IS'' AND ANY
		EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
		WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
		DISCLAIMED. IN NO EVENT SHALL Daniel Vaughan BE LIABLE FOR ANY
		DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
		(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
		LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
		ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
		(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
		SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
	</License>
	<Owner Name="Daniel Vaughan" Email="dbvaughan@gmail.com"/>
	<CreationDate>2011-08-14 13:26:52Z</CreationDate>
</File>
*/
#endregion

using System;

namespace OrangeCrush.Library.Devices.Sensors
{
	public interface IAccelerometer
	{
		/// <summary>
		/// Starts monitoring for changes.
		/// </summary>
		void Start();

		/// <summary>
		/// Stops monitoring for changes.
		/// </summary>
		void Stop();

		/// <summary>
		/// Gets a value indicating whether device supports the accelerometer sensor.
		/// It is not a requirement that all do.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if the device supports accelerometer; 
		/// otherwise, <c>false</c>.
		/// </value>
		bool DeviceSupportsAccelerometer { get; }

		/// <summary>
		/// Gets the last reading.
		/// </summary>
		/// <value>The last reading.</value>
		EnhancedAccelerometerReading Reading { get; }

		/// <summary>
		/// Occurs when a new reading is received.
		/// </summary>
		event EventHandler<EventArgs> ReadingChanged;

		/// <summary>
		/// Indicate that the calibration of the sensor 
		/// would succeed along the X and Y axis
		/// because the device is stable enough, 
		/// or not inclined beyond a reasonable amount.
		/// </summary>
		/// <returns><c>true</c> if all of the X and Y axis 
		/// were stable enough and were not too inclined.</returns>
		bool CanCalibrate();

		/// <summary>
		/// Calibrates the accelerometer on X and Y axis 
		/// and saves the value to persistent storage.
		/// </summary>
		/// <returns><c>true</c> if succeeds.</returns>
		bool Calibrate();

		/// <summary>
		/// Occurs when the device is shaken.
		/// </summary>
		event EventHandler Shake;
	}
}
