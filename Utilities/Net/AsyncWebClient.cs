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
using System.ComponentModel;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Utilities.Concurrency;

namespace Utilities.Net
{
	/// <summary>
	/// Provides asynchronous WebClient operations.
	/// </summary>
	internal class AsyncWebClient : IAsyncWebClient
	{
		/// <summary>
		/// Initializes a new AsyncWebClient.
		/// </summary>
		/// <param name="webClient">The web client to use</param>
		public AsyncWebClient(WebClient webClient)
		{
			_webClient = webClient;
		}

		#region Implementation of IAsyncWebClient

		/// <see cref="IAsyncWebClient.DownloadFileAsync"/>
		public Task DownloadFileAsync(Uri address, string fileName, IProgress<DownloadProgressChangedEventArgs> progress, CancellationToken cancellationToken)
		{
			cancellationToken.Register(() => _webClient.CancelAsync());

			var cookie = Guid.NewGuid();
			var tcs = new TaskCompletionSource<object>();

			DownloadProgressChangedEventHandler progressHandler = CreateProgressHandler(cookie, progress);
			if (progress != null)
				_webClient.DownloadProgressChanged += progressHandler;

			AsyncCompletedEventHandler completedHandler = null;
			completedHandler = (o, e) =>
			{
				if (!Equals(e.UserState, cookie))
					return;

				_webClient.DownloadProgressChanged -= progressHandler;
				_webClient.DownloadFileCompleted -= completedHandler;

				tcs.TrySetFromEventArgs(e);
			};
			_webClient.DownloadFileCompleted += completedHandler;

			_webClient.DownloadFileAsync(address, fileName, cookie);
			return tcs.Task;
		}

		/// <see cref="IAsyncWebClient.DownloadDataAsync"/>
		public Task<byte[]> DownloadDataAsync(Uri address, IProgress<DownloadProgressChangedEventArgs> progress, CancellationToken cancellationToken)
		{
			cancellationToken.Register(() => _webClient.CancelAsync());

			var cookie = Guid.NewGuid();
			var tcs = new TaskCompletionSource<byte[]>();

			DownloadProgressChangedEventHandler progressHandler = CreateProgressHandler(cookie, progress);
			if (progress != null)
				_webClient.DownloadProgressChanged += progressHandler;

			DownloadDataCompletedEventHandler completedHandler = null;
			completedHandler = (o, e) =>
			{
				if (!Equals(e.UserState, cookie))
					return;

				_webClient.DownloadProgressChanged -= progressHandler;
				_webClient.DownloadDataCompleted -= completedHandler;

				tcs.TrySetFromEventArgs(e, args => args.Result);
			};
			_webClient.DownloadDataCompleted += completedHandler;

			_webClient.DownloadDataAsync(address, cookie);
			return tcs.Task;
		}

		/// <see cref="IAsyncWebClient.DownloadStringAsync"/>
		public Task<string> DownloadStringAsync(Uri address, IProgress<DownloadProgressChangedEventArgs> progress, CancellationToken cancellationToken)
		{
			cancellationToken.Register(() => _webClient.CancelAsync());

			var cookie = Guid.NewGuid();
			var tcs = new TaskCompletionSource<string>();

			DownloadProgressChangedEventHandler progressHandler = CreateProgressHandler(cookie, progress);
			if (progress != null)
				_webClient.DownloadProgressChanged += progressHandler;

			DownloadStringCompletedEventHandler completedHandler = null;
			completedHandler = (o, e) =>
			{
				if (!Equals(e.UserState, cookie))
					return;

				_webClient.DownloadProgressChanged -= progressHandler;
				_webClient.DownloadStringCompleted -= completedHandler;

				tcs.TrySetFromEventArgs(e, args => args.Result);
			};
			_webClient.DownloadStringCompleted += completedHandler;

			_webClient.DownloadStringAsync(address, cookie);
			return tcs.Task;
		}

		#endregion

		private static DownloadProgressChangedEventHandler CreateProgressHandler(object cookie, IProgress<DownloadProgressChangedEventArgs> progress)
		{
			return (o, e) =>
			{
				if (!Equals(e.UserState, cookie))
					return;

				progress.Report(e);
			};
		}

		private readonly WebClient _webClient;
	}
}