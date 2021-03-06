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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Autofac;
using Autofac.Core;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using PlantUmlStudio.Configuration;
using PlantUmlStudio.Container.Support;
using PlantUmlStudio.Controls.Behaviors.AvalonEdit.Folding;
using PlantUmlStudio.Core;
using PlantUmlStudio.Core.Imaging;
using PlantUmlStudio.Model.Snippets;
using PlantUmlStudio.ViewModel;
using PlantUmlStudio.ViewModel.Notifications;
using SharpEssentials.Chronology;
using SharpEssentials.Controls.Clipboard;

namespace PlantUmlStudio.Container
{
	/// <summary>
	/// Configures presentation related objects.
	/// </summary>
	public class PresentationModule : Module
	{
		/// <see cref="Module.Load"/>
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<DispatcherTimerAdapter>()
			       .OnActivating(c => c.Instance.Interval = c.Parameters.Any() ? c.Parameters.TypedAs<TimeSpan>() : TimeSpan.Zero);

			builder.RegisterType<NotificationsHub>().As<NotificationsHub, INotifications>()
			       .SingleInstance();

			builder.RegisterType<ClipboardWrapper>().As<IClipboard>()
			       .SingleInstance();

			builder.RegisterType<SettingsPropagator>()
			       .AutoActivate()
			       .SingleInstance();

			builder.RegisterType<SnippetParser>().As<ISnippetParser>();

			builder.RegisterType<SnippetProvider>()
				   .WithProperty(p => p.SnippetLocation, new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"snippets\")))
				   .OnActivating(c => c.Instance.Load())
				   .SingleInstance();

			builder.RegisterType<PreviewDiagramViewModel>()
			       .OnActivating(c =>
			       {
					   // Perform an initial render of the diagram.
				       var diagram = c.Parameters.TypedAs<Diagram>();
				       c.Instance.ImagePreview =
						   c.Context.ResolveKeyed<IDiagramRenderer>(diagram.ImageFormat).Render(diagram);
			       });	

			builder.Register(c => new SnippetsMenu(c.Resolve<SnippetProvider>().Snippets))
			       .SingleInstance();

			builder.RegisterType<PlantUmlFoldRegions>();

			builder.Register(c => new PatternBasedFoldingStrategy(c.Resolve<PlantUmlFoldRegions>()))
			       .Named<IFoldingStrategy>("PlantUmlFoldingStrategy")
			       .SingleInstance();

			builder.Register(c => HighlightingManager.Instance).As<IHighlightingDefinitionReferenceResolver>();

			builder.Register(c =>
			{
				using (var reader = XmlReader.Create(c.Resolve<ISettings>().PlantUmlHighlightingDefinition.OpenRead()))
					return HighlightingLoader.Load(reader, c.Resolve<IHighlightingDefinitionReferenceResolver>());
			}).SingleInstance();

		    builder.RegisterType<CodeEditorViewModel>()
		           .WithParameter(ResolvedParameter.ForNamed<IFoldingStrategy>("PlantUmlFoldingStrategy"))
		           .As<ICodeEditor>()
		           .ApplySettings((settings, instance) =>
		           {
		               instance.Options.HighlightCurrentLine = settings.HighlightCurrentLine;
		               instance.Options.ShowLineNumbers = settings.ShowLineNumbers;
		               instance.Options.EnableVirtualSpace = settings.EnableVirtualSpace;
		               instance.Options.EnableWordWrap = settings.EnableWordWrap;
		               instance.Options.EmptySelectionCopiesEntireLine = settings.EmptySelectionCopiesEntireLine;
		           });

			builder.RegisterType<DiagramEditorViewModel>().As<IDiagramEditor>()
			       .WithParameter((p, c) => p.Name == "autoSaveTimer", (p, c) => c.Resolve<DispatcherTimerAdapter>())
			       .WithParameter((p, c) => p.Name == "refreshTimer", (p, c) => c.Resolve<DispatcherTimerAdapter>(TypedParameter.From(TimeSpan.FromSeconds(2))))
                   .WithParameter(TaskScheduler.FromCurrentSynchronizationContext)
                   .ApplySettings((settings, instance) =>
                   {
                       instance.AutoSave = settings.AutoSaveEnabled;
                       instance.AutoSaveInterval = settings.AutoSaveInterval;
                   })
                   .ExternallyOwned();

			builder.RegisterType<DiagramExplorerViewModel>().As<IDiagramExplorer>()
                   .WithParameter(TaskScheduler.FromCurrentSynchronizationContext)
			       .WithProperty(d => d.NewDiagramTemplate, "@startuml \"{0}\"\n\n\n@enduml")
                   .ApplySettings((settings, instance) =>
                   {
                       instance.DiagramLocation = settings.LastDiagramLocation;
                       instance.FileExtension = settings.DiagramFileExtension;
                   })
			       .OnActivated(c => c.Instance.OpenDiagramFilesAsync(c.Context.Resolve<ISettings>().RememberOpenFiles
				                                                          ? c.Context.Resolve<ISettings>().OpenFiles
				                                                          : Enumerable.Empty<FileInfo>()));

			builder.RegisterType<DiagramManagerViewModel>().As<DiagramManagerViewModel, IDiagramManager>()
			       .SingleInstance();

			builder.RegisterType<ComponentViewModel>();

			builder.RegisterType<AboutViewModel>()
			       .OnActivating(c => c.Instance.LoadComponents());

			builder.RegisterType<SettingsViewModel>();

			builder.RegisterType<RecentFilesMenuViewModel>()
			       .WithParameter(c => c.Resolve<ISettings>().RecentFiles);
		}
	}
}