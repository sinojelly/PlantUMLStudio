using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Moq;
using PlantUmlEditor.Configuration;
using PlantUmlEditor.Core;
using PlantUmlEditor.ViewModel;
using Utilities.Concurrency;
using Xunit;
using Xunit.Extensions;

namespace Tests.Unit.PlantUmlEditor.ViewModel
{
	public class DiagramManagerViewModelTests
	{
		public DiagramManagerViewModelTests()
		{
			var previews = new List<PreviewDiagramViewModel>();
			explorer.SetupGet(p => p.PreviewDiagrams).Returns(previews);
		}

		[Fact]
		public void Test_OpenDiagramCommand()
		{
			// Arrange.
			var diagramPreview = new PreviewDiagramViewModel(new Diagram { File = testDiagramFile });
			var editor = new Mock<IDiagramEditor>();

			var diagramManager = CreateManager(d => editor.Object);

			// Act.
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Assert.
			Assert.Single(diagramManager.OpenDiagrams);
			Assert.Equal(editor.Object, diagramManager.OpenDiagram);
		}

		[Fact]
		public void Test_OpenDiagramCommand_DiagramAlreadyOpenedOnce()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };
			var diagramPreview = new PreviewDiagramViewModel(diagram);
			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);

			var diagramManager = CreateManager(null);
			diagramManager.OpenDiagrams.Add(editor.Object);

			// Act.
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Assert.
			Assert.Single(diagramManager.OpenDiagrams);
			Assert.Equal(editor.Object, diagramManager.OpenDiagram);
		}

		[Fact]
		[Synchronous]
		public void Test_OpenDiagramEditor_Closed()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };
			var diagramPreview = new PreviewDiagramViewModel(diagram);

			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);

			var diagramManager = CreateManager(d => editor.Object);
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Act.
			editor.Raise(e => e.Closed += null, EventArgs.Empty);

			// Assert.
			Assert.Empty(diagramManager.OpenDiagrams);
			editor.Verify(e => e.SaveAsync(), Times.Never());
			editor.Verify(e => e.Dispose());
		}

		[Fact]
		[Synchronous]
		public void Test_OpenDiagramEditor_Closing_Saved()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };
			var diagramPreview = new PreviewDiagramViewModel(diagram);

			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);
			editor.Setup(e => e.SaveAsync()).Returns(Tasks.FromSuccess());

			var diagramManager = CreateManager(d => editor.Object);
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Act.
			editor.Raise(e => e.Closing += null, new CancelEventArgs());
			diagramManager.SaveClosingDiagramCommand.Execute(null);

			// Assert.
			Assert.Equal(editor.Object, diagramManager.ClosingDiagram);

			// Act.
			editor.Raise(e => e.Closed += null, EventArgs.Empty);

			// Assert.
			Assert.Empty(diagramManager.OpenDiagrams);
			editor.Verify(e => e.SaveAsync(), Times.Exactly(1));
			editor.Verify(e => e.Dispose());
		}

		[Fact]
		[Synchronous]
		public void Test_OpenDiagramEditor_Closing_NotSaved()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };
			var diagramPreview = new PreviewDiagramViewModel(diagram);

			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);

			var diagramManager = CreateManager(d => editor.Object);
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Act.
			editor.Raise(e => e.Closing += null, new CancelEventArgs());

			// Assert.
			Assert.Equal(editor.Object, diagramManager.ClosingDiagram);

			// Act.
			editor.Raise(e => e.Closed += null, EventArgs.Empty);

			// Assert.
			Assert.Empty(diagramManager.OpenDiagrams);
			editor.Verify(e => e.SaveAsync(), Times.Never());
			editor.Verify(e => e.Dispose());
		}

		[Fact]
		[Synchronous]
		public void Test_OpenDiagramEditor_DifferentEditorClosing()
		{
			// Arrange.
			var diagramMap = new Dictionary<Diagram, IDiagramEditor>();

			var diagramManager = CreateManager(d => diagramMap[d]);

			var files = new[] { testDiagramFile, new FileInfo(testDiagramFile.FullName + "2") };
			var editors = new List<Mock<IDiagramEditor>>(files.Length);
			foreach (var file in files)
			{
				var diagram = new Diagram { File = file };
				var diagramPreview = new PreviewDiagramViewModel(diagram);

				var editor = new Mock<IDiagramEditor>();
				editor.SetupGet(e => e.Diagram).Returns(diagram);
				editors.Add(editor);
				diagramMap[diagram] = editor.Object;

				diagramManager.OpenDiagramCommand.Execute(diagramPreview);
			}

			// Act.
			editors.Last().Raise(e => e.Closing += null, new CancelEventArgs());
			diagramManager.SaveClosingDiagramCommand.Execute(null);

			// Assert.
			Assert.Equal(editors.Last().Object, diagramManager.ClosingDiagram);

			// Act.
			editors.First().Raise(e => e.Closed += null, EventArgs.Empty);	// Raise the closed event for a different editor.

			// Assert.
			Assert.Single(diagramManager.OpenDiagrams);
			Assert.Equal(editors.Last().Object, diagramManager.OpenDiagrams.Single());
			foreach (var editor in editors)
				editor.Verify(e => e.SaveAsync(), Times.Never());
		}

		[Fact]
		[Synchronous]
		public void Test_OpenDiagramEditor_Saved()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };
			var image = new BitmapImage();

			var diagramPreview = new PreviewDiagramViewModel(diagram);

			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);
			editor.SetupGet(e => e.DiagramImage).Returns(image);

			var diagramManager = CreateManager(d => editor.Object);
			diagramManager.Explorer.PreviewDiagrams.Add(diagramPreview);
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Act.
			editor.Raise(e => e.Saved += null, EventArgs.Empty);

			// Assert.
			Assert.Equal(image, diagramPreview.ImagePreview);
		}

		[Fact]
		[Synchronous]
		public void Test_OpenDiagramEditor_Saved_NoMatchingPreview()
		{
			// Arrange.
			var image = new BitmapImage();

			var diagramPreview = new PreviewDiagramViewModel(new Diagram { File = new FileInfo(testDiagramFile.FullName + "2") });

			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(new Diagram { File = testDiagramFile });
			editor.SetupGet(e => e.DiagramImage).Returns(image);

			var diagramManager = CreateManager(d => editor.Object);
			diagramManager.Explorer.PreviewDiagrams.Add(diagramPreview);
			diagramManager.OpenDiagramCommand.Execute(diagramPreview);

			// Act.
			editor.Raise(e => e.Saved += null, EventArgs.Empty);

			// Assert.
			Assert.Null(diagramPreview.ImagePreview);
		}

		[Fact]
		[Synchronous]
		public void Test_OpenPreviewRequested()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };
			var diagramPreview = new PreviewDiagramViewModel(diagram);

			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);

			var diagramManager = CreateManager(d => editor.Object);

			// Act.
			explorer.Raise(p => p.OpenPreviewRequested += null, new OpenPreviewRequestedEventArgs(diagramPreview));

			// Assert.
			Assert.Single(diagramManager.OpenDiagrams);
			Assert.Equal(editor.Object, diagramManager.OpenDiagrams.Single());
			Assert.Equal(editor.Object, diagramManager.OpenDiagram);
		}

		[Fact]
		[Synchronous]
		public void Test_CloseCommand_UnsavedDiagram()
		{
			// Arrange.
			var diagram = new Diagram { File = testDiagramFile };

			var unsavedCodeEditor = new Mock<ICodeEditor>();
			unsavedCodeEditor.SetupGet(ce => ce.IsModified).Returns(true);
			var unsavedEditor = new Mock<IDiagramEditor>();
			unsavedEditor.SetupGet(e => e.Diagram).Returns(diagram);
			unsavedEditor.SetupGet(e => e.CodeEditor).Returns(unsavedCodeEditor.Object);

			var codeEditor = new Mock<ICodeEditor>();
			codeEditor.SetupGet(ce => ce.IsModified).Returns(false);
			var editor = new Mock<IDiagramEditor>();
			editor.SetupGet(e => e.Diagram).Returns(diagram);
			editor.SetupGet(e => e.CodeEditor).Returns(codeEditor.Object);

			var diagramManager = CreateManager(d => unsavedEditor.Object);
			diagramManager.OpenDiagrams.Add(unsavedEditor.Object);
			diagramManager.OpenDiagrams.Add(editor.Object);

			// Act.
			diagramManager.CloseCommand.Execute(null);

			// Assert.
			unsavedEditor.Verify(c => c.Close());
			editor.Verify(c => c.Close(), Times.Never());
		}

		[Fact]
		public void Test_SaveAllCommand()
		{
			// Arrange.
			var diagramManager = CreateManager(d => null);

			var modifiedEditors = new List<Mock<IDiagramEditor>>();
			for (int i = 0; i < 2; i++)
			{
				var modifiedEditor = new Mock<IDiagramEditor>();
				modifiedEditor.SetupGet(ce => ce.CanSave).Returns(true);
				modifiedEditor.Setup(e => e.SaveAsync()).Returns(Tasks.FromSuccess());
				modifiedEditors.Add(modifiedEditor);
				diagramManager.OpenDiagrams.Add(modifiedEditor.Object);
			}

			var unmodifiedEditor = new Mock<IDiagramEditor>();
			unmodifiedEditor.SetupGet(ce => ce.CanSave).Returns(false);
			unmodifiedEditor.Setup(e => e.SaveAsync()).Returns(Tasks.FromSuccess());
			diagramManager.OpenDiagrams.Add(unmodifiedEditor.Object);
			
			// Act/Assert.
			diagramManager.SaveAllCommand.Execute(null);

			// Assert.
			modifiedEditors.ForEach(e => e.Verify(ed => ed.SaveAsync()));
			unmodifiedEditor.Verify(e => e.SaveAsync(), Times.Never());
		}

		[Theory]
		[InlineData(true, new []{ true, true, true })]
		[InlineData(true, new[] { true, true, false })]
		[InlineData(true, new[] { true, false, false })]
		[InlineData(false, new[] { false, false, false })]
		public void Test_CanSaveAll(bool expected, bool[] modified)
		{
			// Arrange.
			var diagramManager = CreateManager(d => null);

			foreach (bool value in modified)
			{
				var editor = new Mock<IDiagramEditor>();
				editor.SetupGet(e => e.CanSave).Returns(value);
				diagramManager.OpenDiagrams.Add(editor.Object);
			}

			// Act.
			bool actual = diagramManager.SaveAllCommand.CanExecute(null);

			// Assert.
			Assert.Equal(expected, actual);
		}

		[Fact]
		[Synchronous]
		public void Test_Close_RemembersOpenFiles()
		{
			// Arrange.
			var diagramManager = CreateManager(d => null);

			var diagrams = new [] { "test1.puml", "test2.puml" };
			foreach (var diagramName in diagrams)
			{
				var diagram = new Diagram { File = new FileInfo(diagramName) };
				var editor = new Mock<IDiagramEditor> { DefaultValue = DefaultValue.Mock };
				editor.SetupGet(e => e.Diagram).Returns(diagram);

				diagramManager.OpenDiagrams.Add(editor.Object);
			}

			settings.SetupProperty(s => s.OpenFiles);
			settings.SetupProperty(s => s.RememberOpenFiles, true);

			// Act.
			diagramManager.CloseCommand.Execute(null);

			// Assert.
			Assert.NotNull(settings.Object.OpenFiles);
			Assert.Equal(diagrams.Length, settings.Object.OpenFiles.Count());
			foreach (var diagramName in diagrams)
				Assert.Contains(diagramName, settings.Object.OpenFiles.Select(f => f.Name));
		}

		[Fact]
		[Synchronous]
		public async Task Test_RememberedFiles_Reopened()
		{
			// Arrange.
			var diagrams = new[] { "test1.puml", "test2.puml" };

			settings.SetupProperty(s => s.RememberOpenFiles, true);
			settings.SetupProperty(s => s.OpenFiles, diagrams.Select(dn => new FileInfo(dn)).ToList());

			explorer.Setup(e => e.OpenDiagramAsync(It.IsAny<Uri>()))
				.Returns((Uri uri) => Task.FromResult(new Diagram { File = new FileInfo(uri.AbsolutePath) }));

			var diagramManager = CreateManager(d => null);

			// Act.
			await diagramManager.InitializeAsync();

			// Assert.
			explorer.Verify(e => e.OpenDiagramAsync(It.IsAny<Uri>()), Times.Exactly(2));
			foreach (var diagram in diagrams)
				explorer.Verify(e => e.OpenDiagramAsync(It.Is<Uri>(uri => Path.GetFileName(uri.AbsolutePath) == diagram)));
		}

		private DiagramManagerViewModel CreateManager(Func<Diagram, IDiagramEditor> editorFactory)
		{
			return new DiagramManagerViewModel(explorer.Object, editorFactory, settings.Object);
		}

		private readonly Mock<IDiagramExplorer> explorer = new Mock<IDiagramExplorer>();
		private readonly Mock<ISettings> settings = new Mock<ISettings>();

		private static readonly FileInfo testDiagramFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestDiagrams\class.puml"));
	}
}