using System;

namespace OrangeCrush.Library.Devices.Sensors
{
	public class ReadingSmoother
	{
		/// <summary>
		/// Number of prior samples to keep for averaging.       
		/// The higher this number, the larger the latency will be: 
		/// At 50Hz sampling rate: Latency = 20ms * SamplesCount
		/// </summary>
		readonly int samplesCount = 25; // averaging and checking stability on 500ms
			
		bool initialized;
		readonly object initilizedLock = new object();

		/// <summary>
		/// Circular buffer of filtered samples
		/// </summary>
		readonly double[] sampleBuffer;

		/// <summary>
		/// Number of samples for which the accelemeter 
		/// is "stable" (filtered acceleration is within Maximum Stability Tilt 
		/// Delta Angle of average acceleration)
		/// </summary>
		int deviceStableCount;

		/// <summary>
		/// Index in circular buffer of samples
		/// </summary>
		int sampleIndex;

		double sampleSum;

		/// <summary>
		/// Indicates how much maximum variation between two reading 
		/// that causes the reading to be deemed unstable.
		/// </summary>
		public double StabilityDelta { get; set; }

		/// <summary>
		/// Average value. This is a simple arithmetic average over the entire sampleBuffer 
		/// (SamplesCount elements) which contains filtered readings
		/// This is used for the calibration, to get a more steady reading of the value.
		/// </summary>
		double averageValue;

		readonly IFilterStrategy filterStrategy;

		public ReadingSmoother(IFilterStrategy filterStrategy = null, int samplesCount = 25)
		{
			this.samplesCount = samplesCount;
			sampleBuffer = new double[samplesCount];
			this.filterStrategy = filterStrategy;
		}

		public double ProcessReading(double rawValue)
		{
			double result = rawValue;

			if (!initialized)
			{
				lock (initilizedLock)
				{
					if (!initialized)
					{
						/* Initialize buffer with first value. */
						sampleSum = rawValue * samplesCount;
						averageValue = rawValue;

						for (int i = 0; i < samplesCount; i++)
						{
							sampleBuffer[i] = averageValue;
						}

						initialized = true;
					}
				}
			}

			double latestValue;
			if (filterStrategy != null)
			{
				latestValue = result = filterStrategy.ApplyFilter(rawValue, result);
			}
			else
			{
				latestValue = rawValue;
			}

			/* Increment circular buffer insertion index. */
			if (++sampleIndex >= samplesCount)
			{
				/* If at max length then wrap samples back 
				 * to the beginning position in the list. */
				sampleIndex = 0;
			}

			/* Add new and remove old at sampleIndex. */
			sampleSum += latestValue;
			sampleSum -= sampleBuffer[sampleIndex];
			sampleBuffer[sampleIndex] = latestValue;

			averageValue = sampleSum / samplesCount;

			/* Stability check */
			double deltaAcceleration = averageValue - latestValue;

			if (Math.Abs(deltaAcceleration) > StabilityDelta)
			{
				/* Unstable */
				deviceStableCount = 0;
			}
			else
			{
				if (deviceStableCount < samplesCount)
				{
					++deviceStableCount;
				}
			}

			if (filterStrategy == null)
			{
				result = averageValue;
			}

			return result;
		}

	}
}
