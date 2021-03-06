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
using System.ComponentModel;
using System.IO;
using System.Linq;
using PlantUmlStudio.ViewModel;
using SharpEssentials.Collections;
using SharpEssentials.InputOutput;

namespace PlantUmlStudio.Configuration
{
	/// <summary>
	/// Responsible for updating objects in response to settings changes or
	/// capturing events that should change settings.
	/// </summary>
	public class SettingsPropagator
	{
		public SettingsPropagator(ISettings settings, IDiagramManager diagramManager)
		{
			_settings = settings;
			_diagramManager = diagramManager;

			_settings.PropertyChanged += settings_PropertyChanged;
			_diagramManager.DiagramClosed += diagramManager_DiagramClosed;
			_diagramManager.DiagramOpened += diagramManager_DiagramOpened;
			_diagramManager.Closing += diagramManager_Closing;
		}

		private void diagramManager_Closing(object sender, EventArgs e)
		{
			_settings.Save();	// Save before the app shuts down.
		}

		private void settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			EditorUpdates.TryGetValue(e.PropertyName).Apply(update =>
			{
				foreach (var editor in _diagramManager.OpenDiagrams)
					update(editor, _settings);
			});
		}

		private void diagramManager_DiagramClosed(object sender, DiagramClosedEventArgs e)
		{
			_settings.RecentFiles.Add(e.Diagram.File);

		    _settings.OpenFiles
		             .FirstOrNone(file => FileComparer.Equals(file, e.Diagram.File))
		             .Apply(file => _settings.OpenFiles.Remove(file));
		}

		private void diagramManager_DiagramOpened(object sender, DiagramOpenedEventArgs e)
		{
			if (!_settings.OpenFiles.Contains(e.Diagram.File, FileComparer))
				_settings.OpenFiles.Add(e.Diagram.File);
		}

		private readonly ISettings _settings;
		private readonly IDiagramManager _diagramManager;

		/// <summary>
		/// A mapping of settings property names to the changes that should be applied to diagram editors if such a setting changes.
		/// </summary>
		private static readonly IDictionary<string, Action<IDiagramEditor, ISettings>> EditorUpdates = new Dictionary<string, Action<IDiagramEditor, ISettings>>
		{
			{ nameof(ISettings.AutoSaveEnabled),                (ed, s) => ed.AutoSave = s.AutoSaveEnabled },
			{ nameof(ISettings.AutoSaveInterval),               (ed, s) => ed.AutoSaveInterval = s.AutoSaveInterval },
			{ nameof(ISettings.HighlightCurrentLine),           (ed, s) => ed.CodeEditor.Options.HighlightCurrentLine = s.HighlightCurrentLine },
			{ nameof(ISettings.ShowLineNumbers),                (ed, s) => ed.CodeEditor.Options.ShowLineNumbers = s.ShowLineNumbers },
			{ nameof(ISettings.EnableVirtualSpace),             (ed, s) => ed.CodeEditor.Options.EnableVirtualSpace = s.EnableVirtualSpace },
			{ nameof(ISettings.EnableWordWrap),                 (ed, s) => ed.CodeEditor.Options.EnableWordWrap = s.EnableWordWrap },
			{ nameof(ISettings.EmptySelectionCopiesEntireLine), (ed, s) => ed.CodeEditor.Options.EmptySelectionCopiesEntireLine = s.EmptySelectionCopiesEntireLine },
			{ nameof(ISettings.AllowScrollingBelowContent),     (ed, s) => ed.CodeEditor.Options.AllowScrollingBelowContent = s.AllowScrollingBelowContent }
		};

		private static readonly IEqualityComparer<FileInfo> FileComparer = FileSystemInfoPathEqualityComparer.Instance;
	}
}