﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Utilities.PropertyChanged;
using Xunit;

namespace Unit.Tests.Utilities.PropertyChanged
{
	public class PropertyTests : INotifyPropertyChanged
	{
		[Fact]
		public void Test_PropertyChanged()
		{
			// Arrange.
			var property = new Property<int>("property", OnPropertyChanged);

			// Act/Assert.
			var args = AssertThat.RaisesWithEventArgs<PropertyChangedEventArgs>(
				this,
				"PropertyChanged",
				() => property.Value = 30);

			Assert.Equal(30, property.Value);
			Assert.Equal("property", args.PropertyName);
			Assert.Equal("property", property.Name);
		}

		[Fact]
		public void Test_PropertyNotChanged()
		{
			// Arrange.
			var property = new Property<int>("property", OnPropertyChanged)
			{
				Value = 5
			};

			// Act/Assert.
			AssertThat.DoesNotRaise(
				this,
				"PropertyChanged",
				() => property.Value = 5);

			Assert.Equal(5, property.Value);
		}

		[Fact]
		public void Test_PropertyChanged_CustomEquals()
		{
			// Arrange.
			var property = new Property<CustomEquals>("property", OnPropertyChanged)
			{
				Value = new CustomEquals(2)
			};

			// Act/Assert.
			AssertThat.Raises(
				this,
				"PropertyChanged",
				() => property.Value = new CustomEquals(-1));

			Assert.Equal(-1, property.Value.Value);
		}

		[Fact]
		public void Test_PropertyNotChanged_CustomEquals()
		{
			// Arrange.
			var property = new Property<CustomEquals>("property", OnPropertyChanged)
			{
				Value = new CustomEquals(2)
			};

			// Act/Assert.
			AssertThat.DoesNotRaise(
				this,
				"PropertyChanged",
				() => property.Value = new CustomEquals(0));

			Assert.Equal(2, property.Value.Value);
		}

		[Fact]
		public void Test_TrySetValue_PropertyChanged()
		{
			// Arrange.
			var property = new Property<int>("property", OnPropertyChanged)
			{
				Value = 1
			};

			// Act.
			bool changed = property.TrySetValue(30);

			// Assert.
			Assert.Equal(30, property.Value);
			Assert.True(changed);
		}

		[Fact]
		public void Test_TrySetValue_PropertyNotChanged()
		{
			// Arrange.
			var property = new Property<int>("property", OnPropertyChanged)
			{
				Value = 30
			};

			// Act.
			bool changed = property.TrySetValue(30);

			// Assert.
			Assert.Equal(30, property.Value);
			Assert.False(changed);
		}

		[Fact]
		public void Test_PropertyBuilder_AlsoChanges_PropertyWithSetter()
		{
			// Arrange.
			var propertyBuilder = Property.New(this, x => x.IntValue, OnPropertyChanged);

			// Act/Assert.
			Assert.Throws<ArgumentException>(
				() => propertyBuilder.AlsoChanges(x => x.ObjectValue));
		}

		[Fact]
		public void Test_PropertyBuilder_AlsoChanges()
		{
			// Arrange.
			Property<int> property = Property.New(this, x => x.IntValue, OnPropertyChanged)
											 .AlsoChanges(x => x.StringValue);

			var changedPropertyNames = new List<string>();
			PropertyChanged += (o, e) => changedPropertyNames.Add(e.PropertyName);

			// Act
			property.Value = 50;

			// Assert.
			AssertThat.SequenceEqual(new[] { "IntValue", "StringValue" }, changedPropertyNames);
			Assert.Equal("IntValue", property.Name);
		}

		#region Implementation of INotifyPropertyChanged

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		public int IntValue { get; set; }
		public string StringValue { get { return IntValue.ToString(); } }
		public object ObjectValue { get; set; }

		private class CustomEquals
		{
			public int Value { get; private set; }

			public CustomEquals(int value)
			{
				Value = value;
			}

			public override bool Equals(object obj)
			{
				var other = obj as CustomEquals;
				if (other == null)
					return false;

				return (Value - 2 <= other.Value && other.Value <= Value + 2);
			}

			public override int GetHashCode()
			{
				return Value;
			}
		}
	}
}