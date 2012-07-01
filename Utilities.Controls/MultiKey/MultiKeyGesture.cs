using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace Utilities.Controls.MultiKey
{
	/// <summary>
	/// A multi-key input gesture.
	/// </summary>
	[TypeConverter(typeof(MultiKeyGestureConverter))]
	public class MultiKeyGesture : KeyGesture
	{
		public MultiKeyGesture(IEnumerable<KeyInput> keys)
			: this(keys, string.Empty) { }

		public MultiKeyGesture(IEnumerable<KeyInput> keys, string displayString)
			: base(Key.None, ModifierKeys.None, displayString)
		{
			if (!keys.Any())
				throw new ArgumentException(@"At least one key must be specified.", "keys");

			_keys = new List<KeyInput>(keys);

			Keys = new ReadOnlyCollection<Key>(_keys.Select(k => k.Key).ToList());
			Modifiers = new ReadOnlyCollection<ModifierKeys>(_keys.Select(k => k.Modifier).ToList());
		}

		/// <summary>
		/// The keys used for a gesture.
		/// </summary>
		public ICollection<Key> Keys
		{
			get;
			private set;
		}

		/// <summary>
		/// The modifiers used for a gesture.
		/// </summary>
		public new ICollection<ModifierKeys> Modifiers
		{
			get;
			private set;
		}

		/// <see cref="KeyGesture.Matches"/>
		public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
		{
			var args = inputEventArgs as KeyEventArgs;

			if ((args == null) || (Key.None <= args.Key && args.Key <= Key.OemClear))
				return false;

			if (_currentKeyIndex != 0 && ((DateTime.Now - _lastKeyPressTime) > maximumDelayBetweenKeyPresses))
			{
				// The key gesture timed-out.
				_currentKeyIndex = 0;
				return false;
			}

			// Check for the correct modifier key.
			if (_keys[_currentKeyIndex].Modifier != Keyboard.Modifiers)
			{
				// The wrong modifier was used.
				_currentKeyIndex = 0;
				return false;
			}

			if (_keys[_currentKeyIndex].Key != args.Key)
			{
				// wrong key
				_currentKeyIndex = 0;
				return false;
			}

			++_currentKeyIndex;

			if (_currentKeyIndex != _keys.Count)
			{
				// Still looking for a key sequence match.
				_lastKeyPressTime = DateTime.Now;
				inputEventArgs.Handled = true;
				return false;
			}

			// The key gesture matched.
			_currentKeyIndex = 0;
			return true;
		}

		private readonly IList<KeyInput> _keys;
		private int _currentKeyIndex;
		private DateTime _lastKeyPressTime;

		private static readonly TimeSpan maximumDelayBetweenKeyPresses = TimeSpan.FromSeconds(1);
	}

	/// <summary>
	/// Represents a key and an optional modifier.
	/// </summary>
	public class KeyInput
	{
		/// <summary>
		/// Initializes a new key input pair.
		/// </summary>
		public KeyInput()
		{
			Key = Key.None;
			Modifier = ModifierKeys.None;
		}

		/// <summary>
		/// The key to use.  This is required.
		/// </summary>
		public Key Key { get; set; }

		/// <summary>
		/// The key modifier.  This is optional.
		/// </summary>
		public ModifierKeys Modifier { get; set; }
	}
}