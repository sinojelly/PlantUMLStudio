﻿using System;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using PlantUmlEditor.Configuration;
using PlantUmlEditor.Model;
using PlantUmlEditor.Properties;
using Utilities.Chronology;
using Utilities.Concurrency.Processes;

namespace PlantUmlEditor.Container
{
	/// <summary>
	/// Configures the application's core objects.
	/// </summary>
	public class CoreModule : Module
	{
		/// <see cref="Module.Load"/>
		protected override void Load(ContainerBuilder builder)
		{
			builder.Register(c => TaskScheduler.Default);

			builder.Register(c => new DotNetSettings(Settings.Default)).As<ISettings>()
				.SingleInstance();

			builder.RegisterType<SystemTimersTimer>().As<ITimer>();

			builder.RegisterType<DiagramBitmapRenderer>().As<IDiagramRenderer>();

			builder.RegisterType<PlantUml>().As<IDiagramCompiler>()
				.WithProperty(c => c.PlantUmlJar, new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Thirdparty\plantuml.jar")))
				.OnActivating(c => c.Instance.GraphVizExecutable = c.Context.Resolve<ISettings>().GraphVizExecutable);

			builder.RegisterType<DiagramIOService>().As<IDiagramIOService>();
		}
	}
}