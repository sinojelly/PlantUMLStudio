//  PlantUML Studio
//  Copyright 2016 Matthew Hamilton - matthamilton@live.com
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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SharpEssentials;
using SharpEssentials.Chronology;
using SharpEssentials.InputOutput;

namespace PlantUmlStudio.Core.Dependencies.Update
{
	/// <summary>
	/// Checks for and downloads updates of third party components.
	/// </summary>
    public abstract class ComponentUpdateChecker : IComponentUpdateChecker
	{
		/// <summary>
		/// Initializes a new <see cref="ComponentUpdateChecker"/>.
		/// </summary>
		/// <param name="clock">The system clock.</param>
        /// <param name="httpClient">Used for web requests.</param>
        /// <param name="versionComparer">Used to determine the equality of a component's versions.</param>
		protected ComponentUpdateChecker(IClock clock, HttpClient httpClient, IEqualityComparer<string> versionComparer)
		{
		    _clock = clock;
		    _versionComparer = versionComparer;
		    HttpClient = httpClient;
		}

	    /// <summary>
		/// The location to download updates from.
		/// </summary>
		public Uri DownloadLocation { get; set; }

		/// <summary>
		/// The local location of the file to update.
		/// </summary>
		public FileInfo LocalLocation { get; set; }

        /// <summary>
        /// The location of the latest component version number.
        /// </summary>
        public Uri VersionSource { get; set; }

        /// <summary>
        /// Pattern used to find the latest version.
        /// </summary>
        public Regex RemoteVersionPattern { get; set; }

        /// <summary>
        /// Used for web requests.
        /// </summary>
	    protected HttpClient HttpClient { get; }

	    #region IComponentUpdateChecker Members

	    /// <see cref="IComponentUpdateChecker.GetCurrentVersionAsync"/>
	    public abstract Task<string> GetCurrentVersionAsync(CancellationToken cancellationToken);

	    /// <see cref="IComponentUpdateChecker.HasUpdateAsync"/>
        public virtual async Task<Option<string>> HasUpdateAsync(CancellationToken cancellationToken)
        {
            // Download the location of the latest release version.
            var response = await HttpClient.GetAsync(VersionSource, cancellationToken).ConfigureAwait(false);
            var updateSource = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var match = RemoteVersionPattern.Match(updateSource);
            if (match.Success)
            {
                string remoteVersion = match.Groups["version"].Value;
                string currentVersion = await GetCurrentVersionAsync(cancellationToken).ConfigureAwait(false);

                bool versionsNotEqual = !_versionComparer.Equals(remoteVersion, currentVersion);
                if (versionsNotEqual)
                    return remoteVersion;
            }

            return Option.None<string>();
        }

        /// <see cref="IComponentUpdateChecker.DownloadLatestAsync"/>
        public virtual async Task DownloadLatestAsync(IProgress<DownloadProgressChangedEventArgs> progress, CancellationToken cancellationToken)
        {
			if (LocalLocation.Exists)
			{
				// Make a backup in case the new version has issues.
				var backupFile = new FileInfo($"{LocalLocation.FullName}_{_clock.Now:yyyyMMdd_HHmmss}.bak");
				await LocalLocation.CopyToAsync(backupFile, true, cancellationToken).ConfigureAwait(false);
                LocalLocation.Delete();
			}

            var response = await HttpClient.GetAsync(DownloadLocation, cancellationToken).ConfigureAwait(false);
            using (var source = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var destination = LocalLocation.Open(FileMode.OpenOrCreate))
			{
			    await source.CopyToAsync(destination).ConfigureAwait(false);
			}
        }

		#endregion IComponentUpdateChecker Members

		private readonly IClock _clock;
	    private readonly IEqualityComparer<string> _versionComparer;
	}
}
