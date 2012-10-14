/* Based on code by Dave Edson http://windowsteamblog.com/windows_phone/b/wpdev/archive/2010/09/08/using-the-accelerometer-on-windows-phone-7.aspx */
using System;

using OrangeCrush.Library.ComponentModel;
using OrangeCrush.Library.Data;

using Microsoft.Devices.Sensors;
using Microsoft.Xna.Framework;

namespace OrangeCrush.Library.Sensors
{
	[Flags]
	public enum CalibrationAxes
	{
		XAxis,
		YAxis
	}

	public class EnhancedAccelerometer 
		: NotifyPropertyChangeBase, IAccelerometer, IDisposable
	{
		readonly ISettingsService settingsService;
		volatile Accelerometer accelerometer;
		readonly object accelerometerLock = new object();

		public bool DeviceSupportsAccelerometer { get; private set; }

		/// <summary>
		/// Number of prior samples to keep for averaging.       
		/// The higher this number, the larger the latency will be: 
		/// At 50Hz sampling rate: Latency = 20ms * SamplesCount
		/// </summary>
		const int SamplesCount = 25; // averaging and checking stability on 500ms
			
		/// <summary>
		/// This is used for filter past data initialization
		/// </summary>
		bool initialized;

		/// <summary>
		/// Circular buffer of filtered samples
		/// </summary>
		readonly ThreeDimensionalVector[] sampleBuffer 
						= new ThreeDimensionalVector[SamplesCount];

		/// <summary>
		/// n-1 of low pass filter output
		/// </summary>
		ThreeDimensionalVector previousLowPassOutput;

		/// <summary>
		/// n-1 of optimal filter output
		/// </summary>
		ThreeDimensionalVector previousOptimalFilterOutput;

		/// <summary>
		/// Sum of all the filtered samples in the circular buffer.
		/// assume start flat: -1g in z axis.
		/// </summary>
		ThreeDimensionalVector sampleSumVector = new ThreeDimensionalVector(
														0.0 * SamplesCount,
														0.0 * SamplesCount,
														-1.0 * SamplesCount);
		/// <summary>
		/// Index in circular buffer of samples
		/// </summary>
		int sampleIndex;

		const string CalibrationSettingKey = "AccelerometerCalibration";

		public EnhancedAccelerometer(ISettingsService settingsService)
		{
			this.settingsService = ArgumentValidator.AssertNotNull(
											settingsService, "settingsService");
			SetMaximumCalibrationOffset();
			SetMaximumStabilityDeltaOffset();
			DeviceSupportsAccelerometer = Accelerometer.IsSupported;
			CalibrationOffset = GetCalibrationSetting();
		}

		public void Start()
		{
			if (accelerometer == null)
			{
				lock (accelerometerLock)
				{
					if (accelerometer == null)
					{
						accelerometer = new Accelerometer();
						accelerometer.CurrentValueChanged += HandleSensorValueChanged;
						accelerometer.Start();
					}
				}
			}
		}

		public void Stop()
		{
			if (accelerometer != null)
			{
				lock (accelerometerLock)
				{
					if (accelerometer != null)
					{
						accelerometer.CurrentValueChanged -= HandleSensorValueChanged;
						accelerometer.Stop();
						accelerometer.Dispose();
						accelerometer = null;
					}
				}
			}
		}

		#region IDisposable Implementation

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		bool disposed;

		void Dispose(bool disposing)
		{
			/* Check to see if Dispose has already been called. */
			if (disposed)
			{
				return;
			}

			/* If disposing equals true, dispose all managed 
			 * and unmanaged resources. */
			if (disposing)
			{
				Stop();
			}

			/* Disposing has been done. */
			disposed = true;
		}

		/// <summary>
		/// Releases unmanaged resources and performs other cleanup operations 
		/// before the <see cref="EnhancedAccelerometer"/> 
		/// is reclaimed by garbage collection.
		/// Use C# destructor syntax for finalization code. 
		/// This destructor will run only if the Dispose method does not get called.
		/// It gives your base class the opportunity to finalize. 
		/// Do not provide destructors in types derived from this class.
		/// </summary>
		~EnhancedAccelerometer()
		{
			/* Do not re-create Dispose clean-up code here. 
			 * Calling Dispose(false) is optimal 
			 * in terms of readability and maintainability.*/
			Dispose(false);
		}

		#endregion

		void HandleSensorValueChanged(object sender, SensorReadingEventArgs<AccelerometerReading> e)
		{
			ProcessReading(e.SensorReading);
		}

		void ProcessReading(AccelerometerReading accelerometerReading)
		{
			ThreeDimensionalVector lowPassFilteredAcceleration;
			ThreeDimensionalVector optimalFilteredAcceleration;
			ThreeDimensionalVector averagedAcceleration;

			Vector3 acceleration = accelerometerReading.Acceleration;
			ThreeDimensionalVector rawAcceleration = new ThreeDimensionalVector(
										acceleration.X, acceleration.Y, acceleration.Z);

			lock (sampleBuffer)
			{
				if (!initialized)
				{
					/* Initialize buffer with first value. */
					sampleSumVector = rawAcceleration * SamplesCount;
					averageAcceleration = rawAcceleration;

					for (int i = 0; i < SamplesCount; i++)
					{
						sampleBuffer[i] = averageAcceleration;
					}

					previousLowPassOutput = averageAcceleration;
					previousOptimalFilterOutput = averageAcceleration;

					initialized = true;
				}

				lowPassFilteredAcceleration = new ThreeDimensionalVector(
						ApplyLowPassFilter(rawAcceleration.X, previousLowPassOutput.X),
						ApplyLowPassFilter(rawAcceleration.Y, previousLowPassOutput.Y),
						ApplyLowPassFilter(rawAcceleration.Z, previousLowPassOutput.Z));

				previousLowPassOutput = lowPassFilteredAcceleration;

				optimalFilteredAcceleration = new ThreeDimensionalVector(
						ApplyLowPassFilterWithNoiseThreshold(rawAcceleration.X, previousOptimalFilterOutput.X),
						ApplyLowPassFilterWithNoiseThreshold(rawAcceleration.Y, previousOptimalFilterOutput.Y),
						ApplyLowPassFilterWithNoiseThreshold(rawAcceleration.Z, previousOptimalFilterOutput.Z));

				previousOptimalFilterOutput = optimalFilteredAcceleration;

				/* Increment circular buffer insertion index. */
				if (++sampleIndex >= SamplesCount)
				{
					/* If at max length then wrap samples back 
					 * to the beginning position in the list. */
					sampleIndex = 0;
				}

				/* Add new and remove old at sampleIndex. */
				ThreeDimensionalVector vector = optimalFilteredAcceleration;
				sampleSumVector += vector;
				sampleSumVector -= sampleBuffer[sampleIndex];
				sampleBuffer[sampleIndex] = vector;

				averagedAcceleration = sampleSumVector / SamplesCount;
				averageAcceleration = averagedAcceleration;

				/* Stablity check 
				 * If current low-pass filtered sample is deviating 
				 * for more than 1/100 g from average 
				 * (max of 0.5 deg inclination noise if device steady), 
				 * then reset the stability counter. 
				 * The calibration will be prevented until the counter is reaching 
				 * the sample count size. 
				 * Calibration enabled only if entire sampling buffer is "stable" */
				ThreeDimensionalVector deltaAcceleration = averagedAcceleration - optimalFilteredAcceleration;

				if ((Math.Abs(deltaAcceleration.X) > maximumStabilityDeltaOffset)
					|| (Math.Abs(deltaAcceleration.Y) > maximumStabilityDeltaOffset)
					|| (Math.Abs(deltaAcceleration.Z) > maximumStabilityDeltaOffset))
				{
					/* Unstable */
					deviceStableCount = 0;
				}
				else
				{
					if (deviceStableCount < SamplesCount)
					{
						++deviceStableCount;
					}
				}

				/* Adjust with calibration value. */
				rawAcceleration += CalibrationOffset;
				lowPassFilteredAcceleration += CalibrationOffset;
				optimalFilteredAcceleration += CalibrationOffset;
				averagedAcceleration += CalibrationOffset;
			}

			Reading = new EnhancedAccelerometerReading(accelerometerReading.Timestamp, 
														rawAcceleration, 
														optimalFilteredAcceleration, 
														lowPassFilteredAcceleration, 
														averagedAcceleration);

			DetectShake(acceleration);

			previousAcceleration = acceleration;
		}

		EnhancedAccelerometerReading reading;

		public EnhancedAccelerometerReading Reading
		{
			get
			{
				return reading;
			}
			private set
			{
				Assign("Reading", ref reading, value);
				ReadingChanged.Raise(this, EventArgs.Empty);
			}
		}

		public event EventHandler<EventArgs> ReadingChanged;

		/// <summary>
		/// Average acceleration
		/// This is a simple arithmetic average over the entire sampleBuffer 
		/// (SamplesCount elements) which contains filtered readings
		/// This is used for the calibration, to get a more steady reading of the acceleration
		/// </summary>
		ThreeDimensionalVector averageAcceleration;

		#region Filters

		double lowPassFilterCoefficient = 0.1;

		/// <summary>
		///  This is the smoothing factor used for the first order discrete Low-Pass filter
		///  The cut-off frequency fc = fs * K/(2*PI*(1-K))
		/// Default is 0.1, and with a 50Hz sampling rate, this is gives a 1Hz cut-off.
		/// </summary>
		public double LowPassFilterCoefficient
		{
			get
			{
				return lowPassFilterCoefficient;
			}
			set
			{
				lowPassFilterCoefficient = value;
			}
		}

		/// <summary>
		/// First order discrete low-pass filter used to remove noise from raw accelerometer.
		/// </summary>
		/// <param name="newInputValue">New input value (latest sample)</param>
		/// <param name="priorOutputValue">The previous output value (filtered, one sampling period ago)</param>
		/// <returns>The new output value</returns>
		double ApplyLowPassFilter(double newInputValue, double priorOutputValue)
		{
			double newOutputValue = priorOutputValue + LowPassFilterCoefficient * (newInputValue - priorOutputValue);
			return newOutputValue;
		}

		double noiseThreshold = 0.05;

		/// <summary>
		/// Maximum amplitude of noise from sample to sample. 
		/// This is used to remove the noise selectively 
		/// while allowing fast trending for larger amplitudes.
		/// Default is 0.05, which means up to 0.05g deviation from 
		/// filtered value is considered noise.
		/// </summary>
		public double NoiseThreshold
		{
			get
			{
				return noiseThreshold;
			}
			set
			{
				noiseThreshold = value;
			}
		}

		/// <summary>
		/// A discrete low-magnitude fast low-pass filter used to remove noise 
		/// from raw accelerometer while allowing fast trending on high amplitude changes
		/// </summary>
		/// <param name="newInput">New input value (latest sample)</param>
		/// <param name="previousOutput">The previous (n-1) output value 
		/// (filtered, one sampling period ago)</param>
		/// <returns>The new output value</returns>
		double ApplyLowPassFilterWithNoiseThreshold(double newInput, double previousOutput)
		{
			double newOutputValue = newInput;
			if (Math.Abs(newInput - previousOutput) <= NoiseThreshold)
			{
				/* A simple low-pass filter. */
				newOutputValue = previousOutput + LowPassFilterCoefficient * (newInput - previousOutput);
			}
			return newOutputValue;
		}

		#endregion

		#region Stability and Calibration
		/// <summary>
		/// 20 degree inclination from non-calibrated axis max.
		/// </summary>
		double maximumCalibrationTiltAngle = 20.0 * Math.PI / 180.0;

		/// <summary>
		/// This is the inclination angle on any axis beyond 
		/// which the device cannot be calibrated on that particular axis.
		/// Default is 20 degrees (20.0 * Math.PI / 180.0)
		/// </summary>
		public double MaximumCalibrationTiltAngle
		{
			get
			{
				return maximumCalibrationTiltAngle;
			}
			set
			{
				maximumCalibrationTiltAngle = value;
				SetMaximumCalibrationOffset();
			}
		}

		double maximumCalibrationOffset;

		void SetMaximumCalibrationOffset()
		{
			maximumCalibrationOffset = Math.Sin(MaximumCalibrationTiltAngle);
		}

		double maximumStabilityTiltDeltaAngle = 0.5 * Math.PI / 180.0;

		/// <summary>
		/// The maximum inclination angle variation on any axis 
		/// between the average acceleration and the filtered 
		/// acceleration beyond which the device cannot be calibrated 
		/// on that particular axis.
		/// The calibration cannot be done until this condition is met 
		/// on the last contiguous samples from the accelerometer.
		/// Default is 0.5 deg inclination delta at max (0.5 * Math.PI / 180.0)
		/// </summary>
		public double MaximumStabilityTiltDeltaAngle
		{
			get
			{
				return maximumStabilityTiltDeltaAngle;
			}
			set
			{
				maximumStabilityTiltDeltaAngle = value;
				SetMaximumStabilityDeltaOffset();
			}
		}

		/// <summary>
		/// Corresponding lateral acceleration offset at 1g 
		/// of Maximum Stability Tilt Delta Angle
		/// </summary>
		double maximumStabilityDeltaOffset;

		void SetMaximumStabilityDeltaOffset()
		{
			maximumStabilityDeltaOffset = Math.Sin(MaximumStabilityTiltDeltaAngle);
		}

		/// <summary>
		/// Number of samples for which the accelemeter 
		/// is "stable" (filtered acceleration is within Maximum Stability Tilt 
		/// Delta Angle of average acceleration)
		/// </summary>
		int deviceStableCount;

		/// <summary>
		/// True when the device is "stable" (no movement for about 0.5 sec)
		/// </summary>
		public bool DeviceStable
		{
			get
			{
				return deviceStableCount >= SamplesCount;
			}
		}

		public bool CanCalibrate()
		{
			return CanCalibrate(CalibrationAxes.XAxis | CalibrationAxes.YAxis);
		}

		bool CanCalibrate(CalibrationAxes calibrationAxes)
		{
			bool result = false;

			lock (sampleBuffer)
			{
				if (DeviceStable)
				{
					double accelerationMagnitude = 0;
					if (calibrationAxes == CalibrationAxes.XAxis)
					{
						accelerationMagnitude += averageAcceleration.X * averageAcceleration.X;
					}

					if (calibrationAxes == CalibrationAxes.YAxis)
					{
						accelerationMagnitude += averageAcceleration.Y * averageAcceleration.Y;
					}
					accelerationMagnitude = Math.Sqrt(accelerationMagnitude);

					if (accelerationMagnitude <= maximumCalibrationOffset)
					{
						/* Inclination is not out of bounds 
						 * to consider it a calibration offset. */
						result = true;
					}
				}
			}
			return result;
		}

		public bool Calibrate()
		{
			return Calibrate(CalibrationAxes.XAxis | CalibrationAxes.YAxis);
		}
		
		bool Calibrate(CalibrationAxes calibrationAxes)
		{
			bool result = false;

			lock (sampleBuffer)
			{
				if (CanCalibrate(calibrationAxes))
				{
					CalibrationOffset = new ThreeDimensionalVector(
							calibrationAxes == CalibrationAxes.XAxis ? -averageAcceleration.X : CalibrationOffset.X,
							calibrationAxes == CalibrationAxes.YAxis ? -averageAcceleration.Y : CalibrationOffset.Y,
							0);
					/* Persist data. */
					SetCalibrationSetting(CalibrationOffset);
					result = true;
				}
			}
			return result;
		}

		/// <summary>
		/// Persistant data (calibration of accelerometer)
		/// </summary>
		public ThreeDimensionalVector CalibrationOffset { get; private set; }

		void SetCalibrationSetting(ThreeDimensionalVector vector)
		{
			settingsService.SetSetting(CalibrationSettingKey + "X", vector.X);
			settingsService.SetSetting(CalibrationSettingKey + "Y", vector.Y);
		}

		ThreeDimensionalVector GetCalibrationSetting()
		{
			double xAxisCoordinate = settingsService.GetSetting<double>(
														CalibrationSettingKey + "X", 0);
			double yAxisCoordinate = settingsService.GetSetting<double>(
														CalibrationSettingKey + "Y", 0);

			return new ThreeDimensionalVector(xAxisCoordinate, yAxisCoordinate, 0);
		}

		#endregion

		#region Shake Detection

		Vector3? previousAcceleration;
		int shakeCount;
		bool shaking;

		double shakeThreshold = 0.7;

		public double ShakeThreshold
		{
			get
			{
				return shakeThreshold;
			}
			set
			{
				ArgumentValidator.AssertGreaterThan(0, value, "value");
				ArgumentValidator.AssertLessThan(1, value, "value");
				shakeThreshold = value;
			}
		}

		double shakeEndThreshold = 0.2;

		public double ShakeEndThreshold
		{
			get
			{
				return shakeEndThreshold;
			}
			set
			{
				ArgumentValidator.AssertGreaterThan(0, value, "value");
				ArgumentValidator.AssertLessThan(1, value, "value");
				shakeEndThreshold = value;
			}
		}
		
		public event EventHandler Shake;

		protected virtual void OnShake(EventArgs e)
		{
			EventHandler tempEvent = Shake;
			if (tempEvent != null)
			{
				tempEvent(this, e);
			}
		}

		void DetectShake(Vector3 acceleration)
		{
			EventHandler tempEvent = Shake;

			if (tempEvent == null || !previousAcceleration.HasValue)
			{
				return;
			}

			Vector3 previousValue = previousAcceleration.Value;
			bool shakeDetected = IsShake(acceleration, previousValue, shakeThreshold);

			if (shakeDetected && !shaking && shakeCount > 0)
			{
				shaking = true;
				shakeCount = 0;
				OnShake(EventArgs.Empty);
			}
			else if (shakeDetected)
			{
				shakeCount++;
			}
			else if (!IsShake(acceleration, previousValue, shakeEndThreshold))
			{
				shakeCount = 0;
				shaking = false;
			}
		}

		static bool IsShake(
			Vector3 currentAcceleration, Vector3 previousAcceleration, double threshold)
		{
			double deltaX = Math.Abs(previousAcceleration.X - currentAcceleration.X);
			double deltaY = Math.Abs(previousAcceleration.Y - currentAcceleration.Y);
			double deltaZ = Math.Abs(previousAcceleration.Z - currentAcceleration.Z);

			return deltaX > threshold && deltaY > threshold
				   || deltaX > threshold && deltaZ > threshold
				   || deltaY > threshold && deltaZ > threshold;
		}

		#endregion	
	}
}
