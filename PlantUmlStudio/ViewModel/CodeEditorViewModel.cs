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

using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using PlantUmlStudio.Controls.Behaviors.AvalonEdit.Folding;
using SharpEssentials.Controls.Clipboard;
using SharpEssentials.Controls.Mvvm;
using SharpEssentials.Controls.Mvvm.Commands;
using SharpEssentials.Observable;

namespace PlantUmlStudio.ViewModel
{
    /// <summary>
    /// Represents a diagram code editor based on an AvalonEdit control.
    /// </summary>
    public class CodeEditorViewModel : ViewModelBase, ICodeEditor
	{
		/// <summary>
		/// Initializes a new code editor.
		/// </summary>
		public CodeEditorViewModel(IFoldingStrategy foldingStrategy, 
                                   IHighlightingDefinition highlightingDefinition, 
                                   IEnumerable<MenuViewModel> snippets,
			                       IClipboard clipboard) : this()
		{
			_clipboard = clipboard;
			FoldingStrategy = foldingStrategy;
			HighlightingDefinition = highlightingDefinition;
			Snippets = snippets;
        }

	    private CodeEditorViewModel()
	    {
            _selectionStart = Property.New(this, p => p.SelectionStart);
            _selectionLength = Property.New(this, p => p.SelectionLength);

	        _currentFoldings = Property.New(this, p => p.CurrentFoldings);

            _document = Property.New(this, p => p.Document);

            _isModified = Property.New(this, p => IsModified);

            UndoCommand = new RelayCommand(() => Document.UndoStack.Undo(), () => Document.UndoStack.CanUndo);
            RedoCommand = new RelayCommand(() => Document.UndoStack.Redo(), () => Document.UndoStack.CanRedo);

            CopyCommand = new RelayCommand(Copy);
            CutCommand = new RelayCommand(Cut);
            PasteCommand = new RelayCommand(Paste, () => _clipboard.ContainsText);
        }

		/// <summary>
		/// The editor folding strategy.
		/// </summary>
		public IFoldingStrategy FoldingStrategy { get; }

        public IEnumerable<NewFolding> CurrentFoldings
        {
            get { return _currentFoldings.Value; }
            set { _currentFoldings.Value = value; }
        } 

		/// <summary>
		/// The code highlighting definition.
		/// </summary>
		public IHighlightingDefinition HighlightingDefinition { get; }

		/// <summary>
		/// The available code snippets.
		/// </summary>
		public IEnumerable<MenuViewModel> Snippets { get; }

		/// <summary>
		/// The text document representing diagram code.
		/// </summary>
		public TextDocument Document
		{
			get { return _document.Value; }
			set { _document.Value = value; }
		}

		/// <summary>
		/// The content being edited.
		/// </summary>
		public string Content
		{
			get { return Document.Text; }
			set 
			{
				if (Document == null)
				{
					Document = new TextDocument(value);
					Document.UndoStack.PropertyChanged += UndoStack_PropertyChanged;
					Document.Changed += Document_Changed;
				}
				else
				{
					Document.Text = value;
				}
			}
		}

		void UndoStack_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(UndoStack.IsOriginalFile))
				_isModified.Value = !Document.UndoStack.IsOriginalFile;
		}

		void Document_Changed(object sender, DocumentChangeEventArgs e)
		{
			OnPropertyChanged(nameof(Content));
		}

		/// <summary>
		/// The current selection start position.
		/// </summary>
		public int SelectionStart
		{
			get { return _selectionStart.Value; }
			set { _selectionStart.Value = value; }
		}

		/// <summary>
		/// The current selection length.
		/// </summary>
		public int SelectionLength
		{
			get { return _selectionLength.Value; }
			set { _selectionLength.Value = value; }
		}

		/// <see cref="ICodeEditor.Options"/>
		public EditorOptions Options { get; } = new EditorOptions();

		/// <summary>
		/// Whether content has been modified since the last save.
		/// </summary>
		public bool IsModified
		{
			get { return _isModified.Value; }
			set 
			{
				if (_isModified.TrySetValue(value))
				{
					if (!value)
						Document.UndoStack.MarkAsOriginalFile();
				}
			}
		}

		/// <summary>
		/// Copies selected text.
		/// </summary>
		public ICommand CopyCommand { get; }

		private void Copy()
		{
			if (SelectionLength != 0)
			{
				var selectedText = Document.GetText(SelectionStart, SelectionLength);
				_clipboard.SetText(selectedText);
			}
		}

		/// <summary>
		/// Cuts selected text.
		/// </summary>
		public ICommand CutCommand { get; }

		private void Cut()
		{
			if (SelectionLength != 0)
			{
				var selectedText = Document.GetText(SelectionStart, SelectionLength);
				_clipboard.SetText(selectedText);
				Document.Remove(SelectionStart, SelectionLength);
			}
		}

		/// <summary>
		/// Pastes text.
		/// </summary>
		public ICommand PasteCommand { get; }

		private void Paste()
		{
			var clipboardText = _clipboard.GetText();
			if (SelectionLength != 0)
			{
				Document.Replace(SelectionStart, SelectionLength, clipboardText);
				SelectionLength = 0;
                SelectionStart = SelectionStart + clipboardText.Length;
			}
			else
			{
				Document.Insert(SelectionStart, clipboardText);
			}
		}

		/// <summary>
		/// Undoes the last operation.
		/// </summary>
		public ICommand UndoCommand { get; }

		/// <summary>
		/// Redoes the last operation.
		/// </summary>
		public ICommand RedoCommand { get; }

		/// <see cref="SharpEssentials.DisposableBase.OnDisposing"/>
		protected override void OnDisposing()
		{
			if (Document != null)
			{
				Document.UndoStack.PropertyChanged -= UndoStack_PropertyChanged;
				Document.Changed -= Document_Changed;
			}
		}

		private readonly Property<int> _selectionStart;
		private readonly Property<int> _selectionLength;
        private readonly Property<IEnumerable<NewFolding>> _currentFoldings;
		private readonly Property<TextDocument> _document;
		private readonly Property<bool> _isModified;

		private readonly IClipboard _clipboard;
	}
}