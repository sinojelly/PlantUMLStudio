﻿using Utilities.Mvvm;
using Utilities.Mvvm.Commands;
using Utilities.PropertyChanged;
using Xunit;

namespace Unit.Tests.Utilities.Mvvm.Commands
{
	public class BoundRelayCommandTests
	{
		[Fact]
		public void Test_Execute()
		{
			// Arrange.
			var propertyOwner = new TestClass();

			bool executed = false;
			var command = new BoundRelayCommand<TestClass>(_ => executed = true, p => p.BoolValue, propertyOwner);

			// Act.
			command.Execute(null);

			// Assert.
			Assert.True(executed);
		}

		[Fact]
		public void Test_CanExecute()
		{
			foreach (var value in new [] { true, false })
			{
				// Arrange.
				var propertyOwner = new TestClass();
				var command = new BoundRelayCommand<TestClass>(_ => { }, p => p.BoolValue, propertyOwner);
				propertyOwner.BoolValue = value;

				// Act.
				bool canExecute = command.CanExecute(null);

				// Assert.
				Assert.Equal(value, canExecute);
			}
		}

		[Fact]
		public void Test_CanExecuteChanged()
		{
			// Arrange.
			var propertyOwner = new TestClass();
			var command = new BoundRelayCommand<TestClass>(_ => { }, p => p.BoolValue, propertyOwner);

			// Act/Assert.
			AssertThat.Raises(command, "CanExecuteChanged", () => propertyOwner.BoolValue = true);
		}

		[Fact]
		public void Test_CanExecuteChanged_DifferentProperty()
		{
			// Arrange.
			var propertyOwner = new TestClass();
			var command = new BoundRelayCommand<TestClass>(_ => { }, p => p.BoolValue, propertyOwner);

			// Act/Assert.
			AssertThat.DoesNotRaise(command, "CanExecuteChanged", () => propertyOwner.BoolValue2 = true);
		}

		private class TestClass : ViewModelBase
		{
			public TestClass()
			{
				_boolValue = Property.New(this, p => BoolValue, OnPropertyChanged);
				_boolValue2 = Property.New(this, p => BoolValue2, OnPropertyChanged);
			}

			public bool BoolValue
			{
				get { return _boolValue.Value; }
				set { _boolValue.Value = value; }
			}

			public bool BoolValue2
			{
				get { return _boolValue2.Value; }
				set { _boolValue2.Value = value; }
			}

			private readonly Property<bool> _boolValue;
			private readonly Property<bool> _boolValue2;
		}
	}
}