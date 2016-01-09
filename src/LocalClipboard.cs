using System;
using System.IO;
using System.Windows.Forms;
using skwas.IO;

namespace skwas.Forms
{
	/// <summary>
	/// Represents a clipboard that can be used to store objects that can not be easily serialized to the native clipboard. This clipboard implementation can not be used to store actual data on the clipboard for use in other applications. It does however utilize the native clipboard for managing clipboard state.
	/// </summary>
	public static class LocalClipboard
	{
		[Serializable]
		public class Dummy
		{
			private long _x;

			public long X
			{
				get { return _x; }
				set { _x = value; }
			}
		}

		private static readonly object LockObject = new object();
		private static readonly Dummy _dummy = new Dummy();
		private static Timer _timer;
		private static object _data;
		private static object _lastCaller;

		private static string _format;
		private static ClipboardAction _action = ClipboardAction.Clear;

		public static event ClipboardEventHandler DataChanged;

		private static void Initialize()
		{
			lock (LockObject)
			{
				if (_timer != null) return;

				_timer = new Timer {Interval = 100};
				_timer.Tick += _timer_Tick;
				_timer.Start();
			}
		}

		/// <summary>
		/// Gets the last caller, usually a form or control. This can be used to track the owner of a particular operation.
		/// </summary>
		public static object LastCaller
		{
			get { return _lastCaller; }
		}

		private static void _timer_Tick(object sender, EventArgs e)
		{
			lock (LockObject)
			{
				if (!string.IsNullOrEmpty(_format))
				{
					// Don't call local methods directly. This results in a deadlock due to thread locking.

					// Check the native clipboard.
					var data = Clipboard.GetDataObject();
					if (data != null && !data.GetDataPresent(_format))
					{
						// Clipboard was reset by other means...
						Clear(false);
					}
				}
			}
		}

		public static bool ContainsData(string format)
		{
			Initialize();

			lock (LockObject)
			{
				if (string.IsNullOrEmpty(format))
					throw new ArgumentNullException(nameof(format));

				// Check the native clipboard.
				var data = Clipboard.GetDataObject();
				return data != null && data.GetDataPresent(format);
			}
		}

		public static void Clear()
		{
			Clear(true);
		}

		private static void Clear(bool clearSystemClipboard)
		{
			Initialize();

			lock (LockObject)
			{
				_data = null;
				_format = null;
				_lastCaller = null;
				_action = ClipboardAction.Clear;

				if (clearSystemClipboard)
					// Clear native clipboard.
					Clipboard.Clear();
			}
			OnDataChanged(new ClipboardEventArgs(_action));
		}

		public static void Cut(object caller, string format, object data)
		{
			InternalCopy(caller, format, data, ClipboardAction.Cut);
		}

		public static void Copy(object caller, string format, object data)
		{
			InternalCopy(caller, format, data, ClipboardAction.Copy);
		}

		private static void InternalCopy(object caller, string format, object data, ClipboardAction action)
		{
			Initialize();

			lock (LockObject)
			{
				if (string.IsNullOrEmpty(format))
					throw new ArgumentNullException(nameof(format));

				// Store data locally.
				_data = data;
				_format = format;
				_action = action;
				_lastCaller = caller;

				var serializable = data as IRawSerializable;
				if (serializable != null)
				{
					using (var ms = new MemoryStream())
						Clipboard.SetData(format, ms.ToArray());
				}

				// Store a dummy in native clipboard.
				Clipboard.SetData(format, _dummy);
			}

			OnDataChanged(new ClipboardEventArgs(action));
		}

		private static void OnDataChanged(ClipboardEventArgs e)
		{
			if (DataChanged != null)
				DataChanged(null, e);
		}

		public static object Peek(string format)
		{
			Initialize();

			lock (LockObject)
			{
				if (string.IsNullOrEmpty(format))
					throw new ArgumentNullException(nameof(format));

				if (ContainsData(format))
				{
					return _data;
				}
			}
			return null;
		}

		public static bool IsDataCut
		{
			get { return _action == ClipboardAction.Cut; }
		}

		public static object Paste(object caller, string format)
		{
			Initialize();

			object retVal = null;
			lock (LockObject)
			{
				retVal = Peek(format);

				if (ContainsData(format))
				{
					if (_action == ClipboardAction.Cut)
					{
						OnDataChanged(new ClipboardEventArgs(ClipboardAction.Paste));
						Clear();
					}
					_lastCaller = caller;
				}
			}
			return retVal;
		}
	}

	public enum ClipboardAction
	{
		Clear,
		Cut,
		Copy,
		Paste
	}

	public delegate void ClipboardEventHandler(object sender, ClipboardEventArgs e);
	public class ClipboardEventArgs
		: EventArgs
	{
		public ClipboardEventArgs(ClipboardAction action)
		{
			Action = action;
		}

		public ClipboardAction Action { get; }
	}
}
