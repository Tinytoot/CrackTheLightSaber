using System;
using System.Collections.Generic;
using System.Windows;

using Microsoft.Devices;

namespace OrangeCrush.Library.Devices
{
	public interface IPhotoCamera : IDisposable
	{
		void CancelFocus();
		Size Resolution { get; set; }
		FlashMode FlashMode { get; set; }
		IEnumerable<Size> AvailableResolutions { get; }
		Size PreviewResolution { get; }
		void Focus();
		void CaptureImage();
		bool IsFlashModeSupported(FlashMode flashMode);
		event EventHandler<CameraOperationCompletedEventArgs> AutoFocusCompleted;
		event EventHandler<CameraOperationCompletedEventArgs> CaptureCompleted;
		event EventHandler<ContentReadyEventArgs> CaptureImageAvailable;
		event EventHandler<CameraOperationCompletedEventArgs> Initialized;
		void GetPreviewBufferArgb32(int[] pixelData);
	}
}
