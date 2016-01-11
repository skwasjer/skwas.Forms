using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace skwas.Forms
{
	/// <summary>
	/// Ensure only a single instance of the application can run at any time for the interactive user.
	/// </summary>
	/// <remarks>
	/// When callback notifications from new instances are needed, at least one form must be registered via RegisterForm.
	/// </remarks>
	public sealed class SingleApplicationInstance 
		: IDisposable
	{
		#region Interop / message filter.				

		/// <summary>
		/// An identifier for our custom data format.
		/// </summary>
		private const int CopyDataIdentifier = 0x514CC451;
		private const string Vshost = ".vshost";

		private class CopyDataMessageFilter
			: NativeWindow
		{
			private readonly SingleApplicationInstance _owner;

			public CopyDataMessageFilter(SingleApplicationInstance owner)
			{
				_owner = owner;
			}

			//[DebuggerNonUserCode]
			protected override void WndProc(ref Message m)
			{
				if (m.Msg == NativeMethods.WM_COPYDATA)
				{
					var cds = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
					if (cds.dwData == CopyDataIdentifier)
					{
						var receivedData = Marshal.PtrToStringUni(cds.lpData);
						_owner.OnApplicationStarted(
							new ApplicationStartedEventArgs(
								receivedData, 
								CommandLine.Parse(receivedData)
							)
						);

						// Notify success.
						m.Result = new IntPtr(1);
						// Eat message.
						return;	
					}

				}
				base.WndProc(ref m);
			}
		}

		#endregion

		private CopyDataMessageFilter _messageFilter;

		private bool _disposed;
		private readonly bool _mutexOwned;
		private readonly Mutex _mutex;
		private readonly List<Form> _forms;

		#region .ctor/cleanup

		/// <summary>
		/// Initializes a new instance of <see cref="SingleApplicationInstance"/> using specified mutex name and listener form. 
		/// </summary>
		/// <param name="mutexName">The mutex name. Ensure the name is unique for your application.</param>
		/// <exception cref="ArgumentNullException">Thrown when the mutex name is null.</exception>
		/// <exception cref="ArgumentException">Thrown when the mutex name is too long or invalid.</exception>
		public SingleApplicationInstance(string mutexName)
		{
			if (mutexName == null) throw new ArgumentNullException(nameof(mutexName));
			_mutex = new Mutex(true, mutexName, out _mutexOwned);
			if (_mutexOwned)
			{
				// We are the first instance. The application must register each form it want's to be notified on (usually just the main form). We will then attach a message handler, which listens for WM_COPYDATA, sent by newly started instances... This handler is used to intercept arguments passed to those other instances.
				_forms = new List<Form>();
			}
		}

		/// <summary>
		/// Allows an object to try to free resources and perform other cleanup operations before it is reclaimed by garbage collection.
		/// </summary>
		~SingleApplicationInstance()
		{
			Dispose(false);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing">True if disposing.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed) return;

			if (disposing)
			{
				_messageFilter?.ReleaseHandle();
				_messageFilter = null;
			}

			if (_mutexOwned) _mutex.ReleaseMutex();
			_mutex.Dispose();

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		/// <summary>
		/// Gets whether another instance is already running.
		/// </summary>
		public bool IsAnotherInstanceRunning
		{
			get
			{
				if (_disposed)
					throw new ObjectDisposedException(GetType().Name);
				return !_mutexOwned;
			}
		}

		/// <summary>
		/// Gets the other running instance, or null if this is the first instance.
		/// </summary>
		/// <returns></returns>
		public Process GetOtherInstance()
		{
			if (!IsAnotherInstanceRunning) return null;

			int currentProcessId;
			string assemblyName;
			using (var currentProcess = Process.GetCurrentProcess())
			{
				currentProcessId = currentProcess.Id;
				assemblyName = currentProcess.ProcessName;
			}

			// If the receiving instance (the first instance) runs in the IDE, externally launched instances won't find it, if the Visual Studio host process is enabled. So we will also try to find this process...
			if (assemblyName.EndsWith(Vshost))
				assemblyName = assemblyName.Remove(assemblyName.Length - Vshost.Length);

			return Process
				.GetProcessesByName(assemblyName)
				.Union(Process.GetProcessesByName(assemblyName + Vshost))
				.FirstOrDefault(p => p.Id != currentProcessId);
		}

		/// <summary>
		/// Switch to the main running instance and optionally send data to the main instance.
		/// </summary>
		/// <param name="args">The command line arguments to pass.</param>
		public void SwitchTo(params string[] args)
		{
			var process = GetOtherInstance();
			if (process == null)
				throw new InvalidOperationException("Can't switch to other instance. The current instance is the first instance.");

			// Get the main window and ensure it's running in foreground.
			var mainWindow = new Window(process.MainWindowHandle);
			if (mainWindow.IsMinimized)
				mainWindow.Show(ShowWindow.Restore);
			mainWindow.SetToForeground();

			SendData(args, mainWindow.Handle);
		}

		/// <summary>
		/// Sends a string to a window handle via WM_COPYDATA.
		/// </summary>
		/// <param name="args">The command line arguments to send.</param>
		/// <param name="hwnd">The window handle to send to.</param>
		private void SendData(string[] args, IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero) return;

			var data = args != null && args.Length > 0 ? CommandLine.Pack(args) : string.Empty;

			using (var cds = new NativeMethods.COPYDATASTRUCT
			{
				dwData = CopyDataIdentifier,
				cbData = (data.Length + 1)*2, // Unicode string length + terminating zero.
				lpData = Marshal.StringToHGlobalUni(data)
			})
			{
				var hcds = Marshal.AllocHGlobal(Marshal.SizeOf(cds));
				try
				{
					Marshal.StructureToPtr(cds, hcds, true);
					UnsafeNativeMethods.SendMessage(hwnd, NativeMethods.WM_COPYDATA, IntPtr.Zero, hcds);
				}
				finally
				{
					Marshal.FreeHGlobal(hcds);
				}
			}
		}

		/// <summary>
		/// Registers a form for receiving a callback notification when another instance was started. This typically should be the main form.
		/// </summary>
		/// <param name="form">The form to register.</param>
		public void RegisterForm(Form form)
		{
			if (IsAnotherInstanceRunning)
				throw new InvalidOperationException("The application can't listen for notifications, as it is not the first instance.");

			if (form == null)
				throw new ArgumentNullException(nameof(form));
			if (_forms.Contains(form))
				throw new ArgumentException("The specified form is already registered.", nameof(form));

			// Create filter if needed.
			if (_messageFilter == null)
				_messageFilter = new CopyDataMessageFilter(this);

			// If we are not currently listening, assign this form.
			if (_messageFilter.Handle == IntPtr.Zero)
				_messageFilter.AssignHandle(form.Handle);

			// Keep a list of all registered forms.
			form.Disposed += Form_Disposed;
			_forms.Add(form);
		}

		/// <summary>
		/// Unregisters a form for receiving a callback notification when another instance was started.
		/// </summary>
		/// <param name="form">The form to register.</param>
		/// <exception cref="ArgumentNullException">Thrown when the specified form is null.</exception>
		/// <exception cref="ArgumentException">Thrown when the form is not registered.</exception>
		public void UnregisterForm(Form form)
		{
			if (form == null)
				throw new ArgumentNullException(nameof(form));
			if (!_forms.Contains(form)) return;

			// If the form being unregistered is our listener form, release the filter.
			if (_messageFilter.Handle == form.Handle)
				_messageFilter.ReleaseHandle();

			form.Disposed -= Form_Disposed;
			_forms.Remove(form);

			// If the filter was just released, reassign it to another form.
			if (_messageFilter.Handle != IntPtr.Zero || _forms.Count <= 0) return;
			foreach (var other in _forms)
			{
				Win32Exception lastEx;
				try
				{
					_messageFilter.AssignHandle(other.Handle);
					break;
				}
				catch (Win32Exception ex) { lastEx = ex; }

				if (_messageFilter.Handle == IntPtr.Zero)
					throw new InvalidOperationException("An unexpected error occurred while attempting assign message filter.", lastEx);

			}
		}

		/// <summary>
		/// Called when one of the registered forms is disposed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The event args.</param>
		private void Form_Disposed(object sender, EventArgs e)
		{
			UnregisterForm((Form) sender);
		}

		#region ApplicationStarted event

		/// <summary>
		/// Raised when another application instance has started.
		/// </summary>
		public event EventHandler<ApplicationStartedEventArgs> ApplicationStarted;

		/// <summary>
		/// Raises the <see cref="ApplicationStarted"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		private void OnApplicationStarted(ApplicationStartedEventArgs e)
		{
			ApplicationStarted?.Invoke(this, e);
		}

		#endregion
	}
}
