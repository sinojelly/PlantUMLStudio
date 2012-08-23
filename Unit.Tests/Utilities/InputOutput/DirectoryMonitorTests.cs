﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Utilities.Chronology;
using Utilities.Concurrency;
using Utilities.InputOutput;
using Xunit;

namespace Unit.Tests.Utilities.InputOutput
{
	public class DirectoryMonitorTests
	{
		public DirectoryMonitorTests()
		{
			monitor = new DirectoryMonitor(watcher.Object, clock.Object, scheduler)
				{
					FileCreationWaitTimeout = TimeSpan.FromSeconds(2)
				};
		}

		[Fact]
		public void Test_Filter()
		{
			// Arrange.
			watcher.SetupProperty(w => w.Filter);

			// Act.
			monitor.Filter = "*.exe";

			// Assert.
			watcher.VerifySet(w => w.Filter = "*.exe");
			Assert.Equal("*.exe", monitor.Filter);
		}

		[Fact]
		public void Test_StartMonitoring()
		{
			// Arrange.
			var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

			// Act.
			monitor.StartMonitoring(directory);

			// Assert.
			watcher.VerifySet(w => w.Path = directory.FullName);
			watcher.VerifySet(w => w.EnableRaisingEvents = true);
			Assert.Equal(directory, monitor.MonitoredDirectory);
		}

		[Fact]
		public void Test_RestartMonitoring()
		{
			// Arrange.
			var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			monitor.StartMonitoring(directory);
			monitor.StopMonitoring();

			// Act.
			monitor.RestartMonitoring();

			// Assert.
			watcher.VerifySet(w => w.Path = directory.FullName);
			watcher.VerifySet(w => w.EnableRaisingEvents = true);
		}

		[Fact]
		public void Test_RestartMonitoring_NeverStarted()
		{
			// Act/Assert.
			Assert.Throws<InvalidOperationException>(() => monitor.RestartMonitoring());
		}

		[Fact]
		public void Test_StopMonitoring()
		{
			// Act.
			monitor.StopMonitoring();

			// Assert.
			watcher.VerifySet(w => w.EnableRaisingEvents = false);
		}

		[Fact]
		public void Test_Watcher_Created()
		{
			// Arrange.
			var testFile = new FileInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"TestDiagrams\class.puml"));

			var createdArgs = new List<FileSystemEventArgs>();
			EventHandler<FileSystemEventArgs> createdHandler = (o, e) => createdArgs.Add(e);
			monitor.Created += createdHandler;

			// Act: the create event is raised twice, processing occurs on second event.
			watcher.Raise(w => w.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, testFile.Directory.FullName, testFile.Name));
			watcher.Raise(w => w.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, testFile.Directory.FullName, testFile.Name));

			// Assert.
			Assert.Single(createdArgs);
			Assert.Equal(testFile.FullName, createdArgs.Single().FullPath);
		}

		[Fact]
		public void Test_Watcher_Deleted()
		{
			// Arrange.
			FileSystemEventArgs deleteArgs = null;
			EventHandler<FileSystemEventArgs> deleteHandler = (o, e) => deleteArgs = e;
			monitor.Deleted += deleteHandler;

			// Act.
			watcher.Raise(w => w.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, "Dir", "File"));

			// Assert.
			Assert.NotNull(deleteArgs);
			Assert.Equal(@"Dir\File", deleteArgs.FullPath);
		}

		[Fact]
		public void Test_Watcher_Changed()
		{
			// Arrange.
			FileSystemEventArgs changedArgs = null;
			EventHandler<FileSystemEventArgs> changedHandler = (o, e) => changedArgs = e;
			monitor.Changed += changedHandler;

			// Act.
			watcher.Raise(w => w.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, "Dir", "File"));

			// Assert.
			Assert.NotNull(changedArgs);
			Assert.Equal(@"Dir\File", changedArgs.FullPath);
		}

		[Fact]
		public void Test_Watcher_Renamed()
		{
			// Arrange.
			RenamedEventArgs renameArgs = null;
			EventHandler<RenamedEventArgs> renameHandler = (o, e) => renameArgs = e;
			monitor.Renamed += renameHandler;

			// Act.
			watcher.Raise(w => w.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, "Dir", "New", "Old"));

			// Assert.
			Assert.NotNull(renameArgs);
			Assert.Equal(@"Dir\New", renameArgs.FullPath);
			Assert.Equal(@"Dir\Old", renameArgs.OldFullPath);
		}

		[Fact]
		public void Test_Dispose()
		{
			// Arrange.
			FileSystemEventArgs createArgs = null;
			EventHandler<FileSystemEventArgs> createHandler = (o, e) => createArgs = e;
			monitor.Created += createHandler;

			FileSystemEventArgs deleteArgs = null;
			EventHandler<FileSystemEventArgs> deleteHandler = (o, e) => deleteArgs = e;
			monitor.Deleted += deleteHandler;

			FileSystemEventArgs changedArgs = null;
			EventHandler<FileSystemEventArgs> changedHandler = (o, e) => changedArgs = e;
			monitor.Changed += changedHandler;

			RenamedEventArgs renameArgs = null;
			EventHandler<RenamedEventArgs> renameHandler = (o, e) => renameArgs = e;
			monitor.Renamed += renameHandler;

			// Act.
			monitor.Dispose();

			watcher.Raise(w => w.Created += null, new FileSystemEventArgs(WatcherChangeTypes.Created, "Dir", "File"));
			watcher.Raise(w => w.Deleted += null, new FileSystemEventArgs(WatcherChangeTypes.Deleted, "Dir", "File"));
			watcher.Raise(w => w.Changed += null, new FileSystemEventArgs(WatcherChangeTypes.Changed, "Dir", "File"));
			watcher.Raise(w => w.Renamed += null, new RenamedEventArgs(WatcherChangeTypes.Renamed, "Dir", "New", "Old"));

			// Assert.
			Assert.Null(createArgs);
			Assert.Null(deleteArgs);
			Assert.Null(changedArgs);
			Assert.Null(renameArgs);

			watcher.Verify(w => w.Dispose());
		}

		private readonly DirectoryMonitor monitor;

		private readonly Mock<IFileSystemWatcher> watcher = new Mock<IFileSystemWatcher>();
		private readonly Mock<IClock> clock = new Mock<IClock>(); 
		private readonly TaskScheduler scheduler = new SynchronousTaskScheduler();
	}
}