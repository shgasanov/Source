﻿using NAudio.Dsp;
using System;
using System.Diagnostics;

namespace DiscoLights.Audio
{
	#region SampleAggregator Class

	internal class SampleAggregator
	{
		// volume
		public event EventHandler<MaxSampleEventArgs> MaximumCalculated;
		private float maxValue;
		private float minValue;
		public int NotificationCount { get; set; }
		int count;

		// FFT
		public event EventHandler<FftEventArgs> FftCalculated;
		public bool PerformFFT { get; set; }
		private Complex[] fftBuffer;
		private FftEventArgs fftArgs;
		private int fftPos;
		private int fftLength;
		private int m;

		public SampleAggregator()
		{
			if (!IsPowerOfTwo(fftLength))
			{
				throw new ArgumentException("FFT Length must be a power of two.");
			}
			this.fftLength = 1024;
			this.m = (int)Math.Log(fftLength, 2.0);
			this.fftBuffer = new Complex[fftLength];
			this.fftArgs = new FftEventArgs(fftBuffer);
		}

		public void Reset()
		{
			count = 0;
			maxValue = minValue = 0;
		}

		public void Add(float value)
		{
			if (PerformFFT && FftCalculated != null)
			{
				fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftBuffer.Length));
				fftBuffer[fftPos].Y = 0;
				fftPos++;
				if (fftPos >= fftBuffer.Length)
				{
					fftPos = 0;
					// 1024 = 2^10
					FastFourierTransform.FFT(true, m, fftBuffer);
					FftCalculated(this, fftArgs);
				}
			}

			maxValue = Math.Max(maxValue, value);
			minValue = Math.Min(minValue, value);
			count++;
			if (count >= NotificationCount && NotificationCount > 0)
			{
				if (MaximumCalculated != null)
				{
					MaximumCalculated(this, new MaxSampleEventArgs(minValue, maxValue));
				}
				Reset();
			}
		}

		private bool IsPowerOfTwo(int x)
		{
			return (x & (x - 1)) == 0;
		}
	}

	#endregion

	#region MaxSampleEventArgs Class

	internal class MaxSampleEventArgs : EventArgs
	{
		[DebuggerStepThrough]
		public MaxSampleEventArgs(float minValue, float maxValue)
		{
			this.MaxSample = maxValue;
			this.MinSample = minValue;
		}

		public float MaxSample { get; private set; }
		public float MinSample { get; private set; }
	}

	#endregion

	#region FftEventArgs Class

	internal class FftEventArgs : EventArgs
	{
		[DebuggerStepThrough]
		public FftEventArgs(Complex[] result)
		{
			this.Result = result;
		}

		public Complex[] Result { get; private set; }
	}

	#endregion
}
