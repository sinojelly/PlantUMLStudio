﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace Utilities.Controls.Converters
{
	/// <summary>
	/// Executes another converter in reverse.
	/// </summary>
	public class ReverseConverter : IValueConverter
	{
		#region Implementation of IValueConverter

		/// <see cref="IValueConverter.Convert"/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return InnerConverter.ConvertBack(value, targetType, parameter, culture);
		}

		/// <see cref="IValueConverter.ConvertBack"/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return InnerConverter.Convert(value, targetType, parameter, culture);
		}

		#endregion

		/// <summary>
		/// The converter to reverse.
		/// </summary>
		public IValueConverter InnerConverter { get; set; }
	}
}