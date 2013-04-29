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
using System.Collections.Generic;

namespace Utilities.Diagnostics
{
	/// <summary>
	/// Provides Exception extension methods.
	/// </summary>
	public static class ExceptionExtensions
	{
		/// <summary>
		/// Starting with the given exception as root, provides an enumerable containing all nested 
		/// inner exceptions.
		/// </summary>
		/// <param name="exception">The root exception</param>
		/// <returns>An enumerable containing all exceptions</returns>
		public static IEnumerable<Exception> GetExceptionChain(this Exception exception)
		{
			yield return exception;

			var nextException = exception.InnerException;
			while (nextException != null)
			{
				yield return nextException;
				nextException = nextException.InnerException;
			}
		}
	}
}