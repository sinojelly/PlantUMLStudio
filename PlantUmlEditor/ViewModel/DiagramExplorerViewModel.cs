using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PlantUmlEditor.Model;
using PlantUmlEditor.Properties;
using Utilities.Concurrency;
using Utilities.Mvvm;
using Utilities.Mvvm.Commands;
using Utilities.PropertyChanged;

namespace PlantUmlEditor.ViewModel
{
	/// <summary>
	/// Manages diagram previews.
	/// </summary>
	public class DiagramExplorerViewModel : ViewModelBase, IDiagramExplorer
	{
		public DiagramExplorerViewModel(IProgressViewModel progressViewModel, IDiagramIOService diagramIO, 
			Func<Diagram, PreviewDiagramViewModel> previewDiagramFactory)
		{
			_progress = progressViewModel;
			_diagramIO = diagramIO;
			_previewDiagramFactory = previewDiagramFactory;

			_previewDiagrams = Property.New(this, p => PreviewDiagrams, OnPropertyChanged);
			_previewDiagrams.Value = new ObservableCollection<PreviewDiagramViewModel>();

			_currentPreviewDiagram = Property.New(this, p => p.CurrentPreviewDiagram, OnPropertyChanged);

			_diagramLocation = Property.New(this, p => p.DiagramLocation, OnPropertyChanged)
				.AlsoChanges(p => p.IsDiagramLocationValid);

			_newDiagramUri = Property.New(this, p => p.NewDiagramUri, OnPropertyChanged);

			_loadDiagramsCommand = new BoundRelayCommand<DiagramExplorerViewModel>(_ => LoadDiagrams(), p => p.IsDiagramLocationValid, this);
			_addNewDiagramCommand = new RelayCommand<Uri>(AddNewDiagram);
		}

		/// <summary>
		/// Event raised when a new diagram is created.
		/// </summary>
		public event EventHandler<NewDiagramCreatedEventArgs> NewDiagramCreated;

		private void OnNewDiagramCreated(PreviewDiagramViewModel newPreview)
		{
			var localEvent = NewDiagramCreated;
			if (localEvent != null)
				localEvent(this, new NewDiagramCreatedEventArgs(newPreview));
		}

		/// <summary>
		/// The code used for new diagrams.
		/// </summary>
		public string NewDiagramTemplate { get; set; }

		/// <summary>
		/// The location to load diagrams from.
		/// </summary>
		public DirectoryInfo DiagramLocation
		{
			get { return _diagramLocation.Value; }
			set
			{
				if (_diagramLocation.TrySetValue(value))
					LoadDiagrams();
			}
		}

		/// <summary>
		/// Whether the current diagram location is valid.
		/// </summary>
		public bool IsDiagramLocationValid
		{
			get { return DiagramLocation != null && DiagramLocation.Exists; }
		}

		/// <summary>
		/// A new diagram's selected URI.
		/// </summary>
		public Uri NewDiagramUri
		{
			get { return _newDiagramUri.Value; }
			set { _newDiagramUri.Value = value; }
		}

		/// <summary>
		/// The currently selected preview diagram.
		/// </summary>
		public PreviewDiagramViewModel CurrentPreviewDiagram
		{
			get { return _currentPreviewDiagram.Value; }
			set { _currentPreviewDiagram.Value = value; }
		}

		/// <summary>
		/// The currently available diagrams.
		/// </summary>
		public ICollection<PreviewDiagramViewModel> PreviewDiagrams
		{
			get { return _previewDiagrams.Value; }
		}

		/// <summary>
		/// Adds a new diagram with a given URI.
		/// </summary>
		public ICommand AddNewDiagramCommand
		{
			get { return _addNewDiagramCommand; }
		}

		private void AddNewDiagram(Uri newDiagramUri)
		{
			string newFilePath = newDiagramUri.LocalPath;

			if (String.IsNullOrEmpty(Path.GetExtension(newFilePath)))
				newFilePath += ".puml";

			var newDiagram = new Diagram
			{
				File = new FileInfo(newFilePath),
				Content = String.Format(NewDiagramTemplate, Path.GetFileNameWithoutExtension(newFilePath) + ".png")
			};

			_diagramLocation.Value = new DirectoryInfo(Path.GetDirectoryName(newFilePath));

			var saveNewTask = _diagramIO.SaveAsync(newDiagram, false)
				.Then(() => Task.Factory.StartNew(() =>
					LoadDiagrams(), CancellationToken.None, TaskCreationOptions.None, _uiScheduler).Unwrap());

			saveNewTask.ContinueWith(t =>
			{
				CurrentPreviewDiagram = t.Result.SingleOrDefault(d => d.Diagram.File.FullName == newFilePath);
				OnNewDiagramCreated(CurrentPreviewDiagram);
			}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, _uiScheduler);

			saveNewTask.ContinueWith(t =>
			{
				if (t.IsFaulted && t.Exception != null)
					_progress.Message = t.Exception.InnerException.Message;
			}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, _uiScheduler);
		}

		/// <summary>
		/// Command that loads diagrams from the current diagram location.
		/// </summary>
		public ICommand LoadDiagramsCommand
		{
			get { return _loadDiagramsCommand; }
		}

		private Task<ICollection<PreviewDiagramViewModel>> LoadDiagrams()
		{
			_previewDiagrams.Value.Clear();

			if (!IsDiagramLocationValid)
				return Tasks.FromResult(_previewDiagrams.Value);

			_progress.HasDiscreteProgress = true;
			IProgress<Tuple<int?, string>> progress = new Progress<Tuple<int?, string>>(p =>
			{
				_progress.PercentComplete = p.Item1;
				_progress.Message = p.Item2;
			});

			progress.Report(Tuple.Create((int?)0, Resources.Progress_LoadingDiagrams));
			var loadTask = _diagramIO.ReadDiagramsAsync(DiagramLocation, progress);

			loadTask.ContinueWith(t =>
			{
				if (t.Exception != null)
					progress.Report(Tuple.Create((int?)null, t.Exception.InnerException.Message));

			}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, _uiScheduler);

			return loadTask.ContinueWith(t =>
			{
				foreach (var diagramFile in t.Result)
					_previewDiagrams.Value.Add(_previewDiagramFactory(diagramFile));

				progress.Report(Tuple.Create((int?)null, Resources.Progress_DiagramsLoaded));

				return _previewDiagrams.Value;
			}, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, _uiScheduler);
		}

		/// <summary>
		/// Contains current task progress information.
		/// </summary>
		private readonly IProgressViewModel _progress;

		private readonly Property<PreviewDiagramViewModel> _currentPreviewDiagram;
		private readonly Property<ICollection<PreviewDiagramViewModel>> _previewDiagrams;

		private readonly Property<DirectoryInfo> _diagramLocation;
		private readonly Property<Uri> _newDiagramUri;

		private readonly ICommand _loadDiagramsCommand;
		private readonly ICommand _addNewDiagramCommand;

		private readonly IDiagramIOService _diagramIO;

		private readonly Func<Diagram, PreviewDiagramViewModel> _previewDiagramFactory;
		private readonly TaskScheduler _uiScheduler = TaskScheduler.FromCurrentSynchronizationContext();
	}

	/// <summary>
	/// Event args containing information about new diagrams.
	/// </summary>
	public class NewDiagramCreatedEventArgs : EventArgs
	{
		/// <summary>
		/// Creates new event args.
		/// </summary>
		/// <param name="newDiagramPreview">The preview of the newly created diagram</param>
		public NewDiagramCreatedEventArgs(PreviewDiagramViewModel newDiagramPreview)
		{
			NewDiagramPreview = newDiagramPreview;
		}

		/// <summary>
		/// The preview of the newly created diagram.
		/// </summary>
		public PreviewDiagramViewModel NewDiagramPreview { get; private set; }
	}
}