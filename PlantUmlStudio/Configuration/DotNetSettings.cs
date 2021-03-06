﻿//  PlantUML Studio
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SharpEssentials.Collections;
using SharpEssentials.InputOutput;
using SharpEssentials.Observable;

namespace PlantUmlStudio.Configuration
{
	/// <summary>
	/// An adapter around the generated .NET Settings class.
	/// </summary>
	public class DotNetSettings : ObservableObject, ISettings
	{
		internal DotNetSettings(Settings settings)
			: this()
		{
			_settings = settings;

			LastDiagramLocation = new DirectoryInfo(_tokenReplacer.Parse(_settings.LastPath));

			RememberOpenFiles = settings.RememberOpenFiles;
			OpenFiles = new List<FileInfo>(
				settings.OpenFiles?
                        .Cast<string>()
                        .Select(fileName => new FileInfo(fileName)) ?? Enumerable.Empty<FileInfo>());

            _recentFiles.MaximumCount = settings.MaximumRecentFiles;

			settings.RecentFiles?
			        .Cast<string>()
			        .Reverse()
			        .Select(fileName => new FileInfo(fileName))
			        .ToSink(_recentFiles);

			AutoSaveEnabled = settings.AutoSaveEnabled;
			AutoSaveInterval = settings.AutoSaveInterval;

			HighlightCurrentLine = settings.HighlightCurrentLine;
			ShowLineNumbers = settings.ShowLineNumbers;
			EnableVirtualSpace = settings.EnableVirtualSpace;
			EnableWordWrap = settings.EnableWordWrap;
			EmptySelectionCopiesEntireLine = settings.EmptySelectionCopiesEntireLine;
			AllowScrollingBelowContent = settings.AllowScrollingBelowContent;

			GraphVizExecutable = new FileInfo(_settings.GraphVizLocation);
			PlantUmlJar = new FileInfo(_settings.PlantUmlLocation);

            GraphVizDownloadLocation = settings.GraphVizDownloadLocation;
            GraphVizVersionSource = settings.GraphVizVersionSource;
            GraphVizRemoteVersionPattern = new Regex(settings.GraphVizRemoteVersionPattern);
			GraphVizLocalVersionPattern = new Regex(settings.GraphVizLocalVersionPattern);

			PlantUmlDownloadLocation = settings.PlantUmlDownloadLocation;
			PlantUmlVersionSource = settings.PlantUmlVersionSource;
			PlantUmlRemoteVersionPattern = new Regex(settings.PlantUmlRemoteVersionPattern);
			PlantUmlLocalVersionPattern = new Regex(settings.PlantUmlLocalVersionPattern);

			DiagramFileExtension = settings.PlantUmlFileExtension;
			PlantUmlHighlightingDefinition = new FileInfo(settings.PlantUmlHighlightingDefinition);
		}

		private DotNetSettings()
		{
		    _lastDiagramLocation = Property.New(this, p => p.LastDiagramLocation)
		                                   .UsingPathEquality();

			_rememberOpenFiles = Property.New(this, p => p.RememberOpenFiles);
			_openFiles = Property.New(this, p => p.OpenFiles)
                                 .UsingSequenceEquality(FileSystemInfoPathEqualityComparer.Instance);

			_recentFiles = new RecentFilesCollection();
			_recentFiles.PropertyChanged += recentFiles_PropertyChanged;

			_autoSaveEnabled = Property.New(this, p => p.AutoSaveEnabled);
			_autoSaveInterval = Property.New(this, p => p.AutoSaveInterval);

			_highlightCurrentLine = Property.New(this, p => p.HighlightCurrentLine);
			_showLineNumbers = Property.New(this, p => p.ShowLineNumbers);
			_enableVirtualSpace = Property.New(this, p => p.EnableVirtualSpace);
			_enableWordWrap = Property.New(this, p => p.EnableWordWrap);
			_emptySelectionCopiesEntireLine = Property.New(this, p => p.EmptySelectionCopiesEntireLine);
			_allowScrollingBelowContent = Property.New(this, p => p.AllowScrollingBelowContent);
		}

		/// <see cref="ISettings.LastDiagramLocation"/>
		public DirectoryInfo LastDiagramLocation
		{
			get { return _lastDiagramLocation.Value; }
			set { _lastDiagramLocation.Value = value; }
		}

		/// <see cref="ISettings.RememberOpenFiles"/>
		public bool RememberOpenFiles
		{
			get { return _rememberOpenFiles.Value; }
			set { _rememberOpenFiles.Value = value; }
		}

		/// <see cref="ISettings.OpenFiles"/>
		public ICollection<FileInfo> OpenFiles
		{ 
			get { return _openFiles.Value; }
			set { _openFiles.Value = value; }
		}

		/// <see cref="ISettings.RecentFiles"/>
		public ICollection<FileInfo> RecentFiles => _recentFiles;

	    /// <see cref="ISettings.MaximumRecentFiles"/>
		public int MaximumRecentFiles
		{
			get { return _recentFiles.MaximumCount; }
			set { _recentFiles.MaximumCount = value; }
		}

		/// <see cref="ISettings.AutoSaveEnabled"/>
		public bool AutoSaveEnabled
		{ 
			get { return _autoSaveEnabled.Value; }
			set { _autoSaveEnabled.Value = value; }
		}

		/// <see cref="ISettings.AutoSaveInterval"/>
		public TimeSpan AutoSaveInterval
		{ 
			get { return _autoSaveInterval.Value; }
			set { _autoSaveInterval.Value = value; }
		}

		/// <see cref="ISettings.HighlightCurrentLine"/>
		public bool HighlightCurrentLine
		{
			get { return _highlightCurrentLine.Value; }
			set { _highlightCurrentLine.Value = value; }
		}

		/// <see cref="ISettings.ShowLineNumbers"/>
		public bool ShowLineNumbers
		{
			get { return _showLineNumbers.Value; }
			set { _showLineNumbers.Value = value; }
		}

		/// <see cref="ISettings.EnableVirtualSpace"/>
		public bool EnableVirtualSpace
		{
			get { return _enableVirtualSpace.Value; }
			set { _enableVirtualSpace.Value = value; }
		}

		/// <see cref="ISettings.EnableWordWrap"/>
		public bool EnableWordWrap
		{
			get { return _enableWordWrap.Value; }
			set { _enableWordWrap.Value = value; }
		}

		/// <see cref="ISettings.EmptySelectionCopiesEntireLine"/>
		public bool EmptySelectionCopiesEntireLine
		{
			get { return _emptySelectionCopiesEntireLine.Value; }
			set { _emptySelectionCopiesEntireLine.Value = value; }
		}

		/// <see cref="ISettings.AllowScrollingBelowContent"/>
		public bool AllowScrollingBelowContent
		{
			get { return _allowScrollingBelowContent.Value; }
			set { _allowScrollingBelowContent.Value = value; }
		}

		/// <see cref="ISettings.GraphVizExecutable"/>
		public FileInfo GraphVizExecutable { get; set; }

		/// <see cref="ISettings.GraphVizLocalVersionPattern"/>
		public Regex GraphVizLocalVersionPattern { get; }

        /// <see cref="ISettings.GraphVizDownloadLocation"/>
	    public Uri GraphVizDownloadLocation { get; }

        /// <see cref="ISettings.GraphVizVersionSource"/>
	    public Uri GraphVizVersionSource { get; }

        /// <see cref="ISettings.GraphVizRemoteVersionPattern"/>
	    public Regex GraphVizRemoteVersionPattern { get; }

	    /// <see cref="ISettings.PlantUmlJar"/>
		public FileInfo PlantUmlJar { get; set; }

		/// <see cref="ISettings.PlantUmlDownloadLocation"/>
		public Uri PlantUmlDownloadLocation { get; }

		/// <see cref="ISettings.PlantUmlVersionSource"/>
		public Uri PlantUmlVersionSource { get; }

		/// <see cref="ISettings.PlantUmlRemoteVersionPattern"/>
		public Regex PlantUmlRemoteVersionPattern { get; }

		/// <see cref="ISettings.PlantUmlLocalVersionPattern"/>
		public Regex PlantUmlLocalVersionPattern { get; }

		/// <see cref="ISettings.DiagramFileExtension"/>
		public string DiagramFileExtension { get; }

		/// <see cref="ISettings.PlantUmlHighlightingDefinition"/>
		public FileInfo PlantUmlHighlightingDefinition { get; }

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

			_settings.MaximumRecentFiles = MaximumRecentFiles;
			var recentFiles = new StringCollection();
			recentFiles.AddRange(RecentFiles.Select(file => file.FullName).ToArray());
			_settings.RecentFiles = recentFiles;

			_settings.AutoSaveEnabled = AutoSaveEnabled;
			_settings.AutoSaveInterval = AutoSaveInterval;

			_settings.HighlightCurrentLine = HighlightCurrentLine;
			_settings.ShowLineNumbers = ShowLineNumbers;
			_settings.EnableVirtualSpace = EnableVirtualSpace;
			_settings.EnableWordWrap = EnableWordWrap;
			_settings.EmptySelectionCopiesEntireLine = EmptySelectionCopiesEntireLine;
			_settings.AllowScrollingBelowContent = AllowScrollingBelowContent;

			_settings.Save();
		}

		void recentFiles_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Relay the property change.
			if (e.PropertyName == nameof(RecentFilesCollection.MaximumCount))
				OnPropertyChanged(nameof(MaximumRecentFiles));
		}

		private readonly Property<DirectoryInfo> _lastDiagramLocation;
		private readonly Property<bool> _rememberOpenFiles;
		private readonly Property<ICollection<FileInfo>> _openFiles;
		private readonly RecentFilesCollection _recentFiles;

		private readonly Property<bool> _autoSaveEnabled;
		private readonly Property<TimeSpan> _autoSaveInterval;

		private readonly Property<bool> _highlightCurrentLine;
		private readonly Property<bool> _showLineNumbers;
		private readonly Property<bool> _emptySelectionCopiesEntireLine;
		private readonly Property<bool> _enableWordWrap;
		private readonly Property<bool> _enableVirtualSpace;
		private readonly Property<bool> _allowScrollingBelowContent;

		private readonly Settings _settings;
        private readonly SpecialFolderTokenReplacer _tokenReplacer = new SpecialFolderTokenReplacer();
	}
}