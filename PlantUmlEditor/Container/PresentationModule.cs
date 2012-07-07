using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using PlantUmlEditor.Model;
using PlantUmlEditor.Properties;
using PlantUmlEditor.ViewModel;
using Utilities.Chronology;
using Utilities.Mvvm;

namespace PlantUmlEditor.Container
{
	/// <summary>
	/// Configures presentation related objects.
	/// </summary>
	public class PresentationModule : Module
	{
		/// <see cref="Module.Load"/>
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ProgressViewModel>().As<IProgressViewModel>()
				.SingleInstance();

			// Diagram view-model factory.
			builder.Register<Func<Diagram, DiagramViewModel>>(c =>
			{
				var diagramRenderer = c.Resolve<IDiagramRenderer>();
				return diagram => new DiagramViewModel(diagram)
				{
					DiagramImage = diagramRenderer.Render(diagram)	// Perform an initial render of the diagram.
				};
			});

			builder.Register(c => new DefaultSnippets().SnippetCategories)
				.Named<IEnumerable<SnippetCategoryViewModel>>("DefaultSnippets")
				.SingleInstance();

			builder.Register<IEnumerable<ViewModelBase>>(c =>
			{
				var snippets = c.ResolveNamed<IEnumerable<SnippetCategoryViewModel>>("DefaultSnippets");
				var snippetRoot = new SnippetCategoryViewModel("Snippets");
				foreach (var snippet in snippets)
					snippetRoot.Snippets.Add(snippet);
				return new List<ViewModelBase> { snippetRoot };
			})
			.Named<IEnumerable<ViewModelBase>>("EditorContextMenu");

			builder.Register(c => new CodeEditorViewModel(c.ResolveNamed<IEnumerable<ViewModelBase>>("EditorContextMenu")));

			builder.RegisterType<DiagramEditorViewModel>().As<IDiagramEditor>()
				.WithParameter((p, c) => p.Name == "refreshTimer", (p, c) => new SystemTimersTimer { Interval = TimeSpan.FromSeconds(2) })
				.WithProperty(p => p.AutoSaveInterval, TimeSpan.FromSeconds(30))
				.WithProperty(p => p.AutoSave, true);

			builder.RegisterType<DiagramsViewModel>()
				.WithProperty(d => d.DiagramLocation,	// Initialize the diagram location.
					new DirectoryInfo(string.IsNullOrEmpty(Settings.Default.LastPath)
									? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"PlantUmlEditor\samples\")
									: Settings.Default.LastPath))
				.WithProperty(d => d.NewDiagramTemplate, String.Format(
					@"@startuml ""{{0}}""{0}{1}{2}@enduml", Environment.NewLine, Environment.NewLine, Environment.NewLine))
				.SingleInstance();
		}
	}
}