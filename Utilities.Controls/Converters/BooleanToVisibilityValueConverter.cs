﻿//  PlantUML Studio
//  Copyright 2013 Matthew Hamilton - matthamilton@live.com
//  Copyright 2010 Omar Al Zabir - http://omaralzabir.com/ (original author)
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//      http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Utilities.Controls.Converters
{
	///<see cref="IValueConverter"/>
	/// <remarks>
	/// Converts between booleans and Visibility enum
	/// </remarks>
	[ValueConversion(typeof(bool), typeof(Visibility))]
	public class BooleanToVisibilityValueConverter : IValueConverter
	{
		///<see cref="IValueConverter.Convert"/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				if (((bool)value))
					return Visibility.Visible;
				return Visibility.Collapsed;
			}

			return Visibility.Collapsed;
		}

		///<see cref="IValueConverter.ConvertBack"/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value != null && ((Visibility)value) == Visibility.Visible;
		}
	}
}
