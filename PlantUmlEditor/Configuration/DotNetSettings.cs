﻿//  PlantUML Editor
//  Copyright 2012 Matthew Hamilton - matthamilton@live.com
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
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PlantUmlEditor.Properties;
using Utilities.InputOutput;
using Utilities.PropertyChanged;

namespace PlantUmlEditor.Configuration
{
	/// <summary>
	/// An adapter around the generated .NET Settings class.
	/// </summary>
	public class DotNetSettings : PropertyChangedNotifier, ISettings
	{
		internal DotNetSettings(Settings settings, DirectoryInfo defaultDiagramLocation)
			: this()
		{
			_settings = settings;

			GraphVizExecutable = new FileInfo(_settings.GraphVizLocation);
			PlantUmlJar = new FileInfo(_settings.PlantUmlLocation);

			LastDiagramLocation = String.IsNullOrEmpty(_settings.LastPath)
				? defaultDiagramLocation
				: new DirectoryInfo(_settings.LastPath);

			RememberOpenFiles = settings.RememberOpenFiles;
			OpenFiles = settings.OpenFiles == null ? 
				Enumerable.Empty<FileInfo>() :
				settings.OpenFiles.Cast<string>().Select(fileName => new FileInfo(fileName)).ToList();

			GraphVizLocalVersionPattern = new Regex(settings.GraphVizLocalVersionPattern);
			PlantUmlDownloadLocation = settings.DownloadUrl;
			PlantUmlVersionSource = settings.PlantUmlVersionSource;
			PlantUmlRemoteVersionPattern = new Regex(settings.PlantUmlRemoteVersionPattern);
			PlantUmlLocalVersionPattern = new Regex(settings.PlantUmlLocalVersionPattern);
			DiagramFileExtension = settings.PlantUmlFileExtension;
			PlantUmlHighlightingDefinition = new FileInfo(settings.PlantUmlHighlightingDefinition);
		}

		private DotNetSettings()
		{
			_lastDiagramLocation = Property.New(this, p => p.LastDiagramLocation, OnPropertyChanged)
										   .EqualWhen((oldValue, newValue) => oldValue.FullName.Equals(newValue.FullName, StringComparison.OrdinalIgnoreCase));

			_rememberOpenFiles = Property.New(this, p => p.RememberOpenFiles, OnPropertyChanged);
			_openFiles = Property.New(this, p => p.OpenFiles, OnPropertyChanged)

								 .EqualWhen((oldValue, newValue) => oldValue.SequenceEqual(newValue, FileInfoPathEqualityComparer.Instance));
			_autoSaveEnabled = Property.New(this, p => p.AutoSaveEnabled, OnPropertyChanged);
			_autoSaveInterval = Property.New(this, p => p.AutoSaveInterval, OnPropertyChanged);
		}

		/// <see cref="ISettings.GraphVizExecutable"/>
		public FileInfo GraphVizExecutable { get; set; }

		/// <see cref="ISettings.GraphVizLocalVersionPattern"/>
		public Regex GraphVizLocalVersionPattern { get; private set; }

		/// <see cref="ISettings.PlantUmlJar"/>
		public FileInfo PlantUmlJar { get; set; }

		/// <see cref="ISettings.LastDiagramLocation"/>
		public DirectoryInfo LastDiagramLocation { get; set; }

		/// <see cref="ISettings.RememberOpenFiles"/>
		public bool RememberOpenFiles { get; set; }

		/// <see cref="ISettings.OpenFiles"/>
		public IEnumerable<FileInfo> OpenFiles { get; set; }

		/// <see cref="ISettings.AutoSaveEnabled"/>
		public bool AutoSaveEnabled { get; set; }

		/// <see cref="ISettings.AutoSaveInterval"/>
		public TimeSpan AutoSaveInterval { get; set; }

		/// <see cref="ISettings.PlantUmlDownloadLocation"/>
		public Uri PlantUmlDownloadLocation { get; private set; }

		/// <see cref="ISettings.PlantUmlVersionSource"/>
		public Uri PlantUmlVersionSource { get; private set; }

		/// <see cref="ISettings.PlantUmlRemoteVersionPattern"/>
		public Regex PlantUmlRemoteVersionPattern { get; private set; }

		/// <see cref="ISettings.PlantUmlLocalVersionPattern"/>
		public Regex PlantUmlLocalVersionPattern { get; private set; }

		/// <see cref="ISettings.DiagramFileExtension"/>
		public string DiagramFileExtension { get; private set; }

		/// <see cref="ISettings.PlantUmlHighlightingDefinition"/>
		public FileInfo PlantUmlHighlightingDefinition { get; private set; }

		/// <see cref="ISettings.Save"/>
		public void Save()
		{
			_settings.LastPath = LastDiagramLocation.FullName;
			_settings.GraphVizLocation = GraphVizExecutable.FullName;
			_settings.PlantUmlLocation = PlantUmlJar.FullName;
			_settings.RememberOpenFiles = RememberOpenFiles;

			var openFiles = new StringCollection();
			openFiles.AddRange(OpenFiles.Select(file => file.FullName).ToArray());
			_settings.OpenFiles = openFiles;

			_settings.Save();
		}

		private readonly Property<DirectoryInfo> _lastDiagramLocation;
		private readonly Property<bool> _rememberOpenFiles;
		private readonly Property<IEnumerable<FileInfo>> _openFiles;
		private readonly Property<bool> _autoSaveEnabled;
		private readonly Property<TimeSpan> _autoSaveInterval;

		private readonly Settings _settings;
	}
}