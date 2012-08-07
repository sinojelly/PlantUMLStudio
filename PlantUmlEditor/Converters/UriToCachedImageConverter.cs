﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PlantUmlEditor.Converters
{
	public class UriToCachedImageConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (value == null)
				return null;

			string imagePath = value.ToString();

			if (!String.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
			{
				BitmapImage bi = new BitmapImage();
				bi.BeginInit();
				bi.UriSource = new Uri(value.ToString());
				// OMAR: Trick #6
				// Unless we use this option, the image file is locked and cannot be modified.
				// Looks like WPF holds read lock on the images. Very bad.
				bi.CacheOption = BitmapCacheOption.OnLoad;
				// Unless we use this option, an image cannot be refrehsed. It loads from 
				// cache. Looks like WPF caches every image it loads in memory. Very bad.
				bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
				try
				{
					bi.EndInit();
					return bi;
				}
				catch
				{
					return default(BitmapImage);
				}
			}

			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException("Two way conversion is not supported.");
		}
	}
}