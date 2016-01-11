using System;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Forms;

namespace skwas.Forms
{
	/// <summary>
	/// Provides a wrap around windows not belonging to the current process. Exposes several basic properties/methods to get information about the window in question.
	/// </summary>
	[SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
	public class Window : IWin32Window, ICloneable
	{
		private static readonly object Ref = new object();

		private int _processId;
		private int _threadId;
		private bool? _isDesktop;
		private string _moduleName;

		/// <summary>
		/// Creates a new instance of Window for specified window handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		public Window(IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				throw new WindowException("The window handle can not be null/zero.");
			Handle = handle;
		}

		#region Properties

		/// <summary>
		/// Gets the window handle.
		/// </summary>
		public IntPtr Handle { get; }

		/// <summary>
		/// Gets the process id the window belongs to.
		/// </summary>
		public int ProcessId
		{
			get
			{
				if (_processId == 0) GetWindowThreadProcessId();
				return _processId;
			}
		}

		/// <summary>
		/// Gets the thread id the window belongs to.
		/// </summary>
		public int ThreadId
		{
			get
			{
				if (_threadId == 0) GetWindowThreadProcessId();
				return _threadId;
			}
		}

		/// <summary>
		/// Gets if the window belongs to the local process.
		/// </summary>
		public bool IsCurrentProcess
		{
			get
			{
				using (var process = Process.GetCurrentProcess())
					return ProcessId == process.Id;
			}
		}

		/// <summary>
		/// Gets the window caption.
		/// </summary>
		public virtual string Text
		{
			get
			{
				if (!IsDesktop) return GetWindowText(Handle);

				IntPtr pidl;
				if (SafeNativeMethods.SHGetFolderLocation(new HandleRef(this, Handle), 0, IntPtr.Zero, 0, out pidl) == 0)
				{
					var sfi = new NativeMethods.SHFILEINFO();
					var flags = NativeMethods.SHGFI.SHGFI_DISPLAYNAME | NativeMethods.SHGFI.SHGFI_PIDL;

					string retVal;
					if (SafeNativeMethods.SHGetFileInfo(new HandleRef(this, pidl), 0, ref sfi, Marshal.SizeOf(sfi), flags) != IntPtr.Zero)
						retVal = "<" + sfi.szDisplayName + ">";
					else
						retVal = "<Desktop>";

					Marshal.FreeCoTaskMem(pidl);
					return retVal;
				}
				return null;
			}
		}

		/// <summary>
		/// Gets the classname of the window.
		/// </summary>
		public virtual string ClassName => GetClassName(Handle);

		/// <summary>
		/// Gets whether the current window is the desktop window.
		/// </summary>
		public bool IsDesktop
		{
			get
			{
				if (!_isDesktop.HasValue)
					_isDesktop = Handle == NativeMethods.GetDesktopWindow();
				return _isDesktop.Value;
			}
		}

		/// <summary>
		/// Gets the module name of the window.
		/// </summary>
		/// <remarks>This property accesses the process through the WinAPI, which is faster, compared to Process.MainModule.FileName. This is old code from .NET 2 era so not sure if still valid.</remarks>
		public virtual string ModuleName
		{
			get
			{				
				if (_moduleName != null) return _moduleName;
				_moduleName = IsDesktop 
					? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe")
					: GetModuleName(ProcessId);
				return _moduleName;
			}
		}

		/// <summary>
		/// Gets the window style attributes.
		/// </summary>
		public virtual WindowStyles WindowStyles
		{
			get { return GetWindowStyles(Handle); }
			set { SetWindowStyles(Handle, value); }
		}

		/// <summary>
		/// Gets the window style attributes.
		/// </summary>
		public ExtendedWindowStyles ExtendedWindowStyles
		{
			get { return GetExtendedWindowStyles(Handle); }
			set { SetExtendedWindowStyles(Handle, value); }
		}

		/// <summary>
		/// Gets the control style attributes.
		/// </summary>
		public int ControlStyles
		{
			get { return GetControlStyles(Handle); }
			set { SetControlStyles(Handle, value); }
		}

		/// <summary>
		/// Gets the coordinates of the upper-left corner of the window relative to the main desktop.
		/// </summary>
		public Point Location
		{
			get
			{
				return GetLocation(Handle);
			}
			set
			{
				SetLocation(Handle, value);
				//var sz = Size;
				//UnsafeNativeMethods.MoveWindow(new HandleRef(this, Handle), value.X, value.Y, sz.Width, sz.Height, false);
			}
		}

		/// <summary>
		/// Gets the height and width of the window.
		/// </summary>
		public Size Size
		{
			get
			{
				return GetSize(Handle);
			}
			set
			{
				SetSize(Handle, value);
				//var pt = Location;
				//UnsafeNativeMethods.MoveWindow(new HandleRef(this, Handle), pt.X, pt.Y, value.Width, value.Height, false);
			}
		}

		/// <summary>
		/// Gets the bounds of the window.
		/// </summary>
		public Rectangle Bounds
		{
			get { return GetBounds(Handle); }
			set { SetBounds(Handle, value); }
		}

		/// <summary>
		/// Gets the height and width of the window.
		/// </summary>
		public Size ClientSize => GetClientSize(Handle);

		/// <summary>
		/// Gets wether the window is still valid (iow. still exists on the local computer)
		/// </summary>
		public bool Exists => NativeMethods.IsWindow(new HandleRef(this, Handle));

		/// <summary>
		/// Gets the process the window belongs to.
		/// </summary>
		public Process Process
		{
			get
			{
				if (!Exists) return null;

				try
				{
					return Process.GetProcessById(ProcessId);
				}
				catch (ArgumentException)
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Gets wether the window has a maximize button.
		/// </summary>
		public bool MaximizeBox => WindowStyles.HasFlag(WindowStyles.MaximizeBox);

		/// <summary>
		/// Gets wether the window has a maximize button.
		/// </summary>
		public bool MinimizeBox => WindowStyles.HasFlag(WindowStyles.MinimizeBox);

		/// <summary>
		/// Gets wether the window has a controlbox.
		/// </summary>
		public bool ControlBox => WindowStyles.HasFlag(WindowStyles.SysMenu);

		/// <summary>
		/// Gets wether the window has a help button.
		/// </summary>
		public bool HelpButton => ExtendedWindowStyles.HasFlag(ExtendedWindowStyles.ContextHelp);

		/// <summary>
		/// Gets wether the window is a toolwindow.
		/// </summary>
		public bool ToolWindow => ExtendedWindowStyles.HasFlag(ExtendedWindowStyles.ToolWindow);

		/// <summary>
		/// Gets the border style for the window.
		/// </summary>
		public BorderStyle BorderStyle
		{
			get
			{
				var style = WindowStyles;
				if (style.HasFlag(WindowStyles.ThickFrame))
					return BorderStyle.Fixed3D;
				return style.HasFlag(WindowStyles.Border)
					? BorderStyle.FixedSingle
					: BorderStyle.None;
			}
		}

		/// <summary>
		/// Gets a <see cref="FileVersionInfo"/> representing the version information associated with the window's main module.
		/// </summary>
		public virtual FileVersionInfo FileVersionInfo
		{
			get
			{
				using (var proc = Process)
				{
					return proc == null ? null : FileVersionInfo.GetVersionInfo(ModuleName);
				}
			}
		}

		/// <summary>
		/// Gets the parent window.
		/// </summary>
		public Window Parent => GetParent(new Window(Handle));

		/// <summary>
		/// Gets whether the current window is minimized.
		/// </summary>
		public bool IsMinimized => SafeNativeMethods.IsIconic(new HandleRef(this, Handle));

		/// <summary>
		/// Gets whether the current window is maximized.
		/// </summary>
		public bool IsMaximized => SafeNativeMethods.IsZoomed(new HandleRef(this, Handle));

		#endregion




		/// <summary>
		/// Returns the window caption text for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns>The window caption text for specified handle.</returns>
		public static string GetWindowText(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return null;

			var length = SafeNativeMethods.GetWindowTextLength(handle) + 1;

			var sb = new StringBuilder(length, length);
			UnsafeNativeMethods.GetWindowText(handle, sb, sb.MaxCapacity);
			return sb.ToString();
		}

		/// <summary>
		/// Returns the window class name for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns>The window class name for specified handle.</returns>
		public static string GetClassName(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return null;
			var sb = new StringBuilder(260, 260);
			UnsafeNativeMethods.GetClassName(handle, sb, sb.MaxCapacity);
			return sb.ToString();
		}

		/// <summary>
		/// Returns the name of the main module of specified process id.
		/// </summary>
		/// <param name="processId">The process id to get the module name for.</param>
		/// <returns>The name of the main module of specified process id.</returns>
		public static string GetModuleName(int processId)
		{
			if (processId == 0) return null;

			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.Win32NT:
					return UnsafeNativeMethods.GetModuleNameWinNT(processId);
				case PlatformID.Win32Windows:
					return UnsafeNativeMethods.GetModuleNameWin32(processId);
			}

			throw new NotSupportedException();
		}

		/// <summary>
		/// Returns the window style attributes for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		public static WindowStyles GetWindowStyles(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return 0;

			return (WindowStyles)(UnsafeNativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE).ToInt64() & 0xFFFF0000);
		}

		/// <summary>
		/// Sets the window style attributes for specified window.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="styles">The new window styles.</param>
		/// <returns>true if successful, false otherwise.</returns>
		public static void SetWindowStyles(IntPtr handle, WindowStyles styles)
		{
			if (handle == IntPtr.Zero) return;

			UnsafeNativeMethods.SetWindowLong(handle, NativeMethods.GWL_STYLE, (int)styles);
		}

		/// <summary>
		/// Returns the window style attributes for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		public static ExtendedWindowStyles GetExtendedWindowStyles(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return 0;

			return (ExtendedWindowStyles)UnsafeNativeMethods.GetWindowLong(handle, NativeMethods.GWL_EXSTYLE).ToInt64();
		}

		/// <summary>
		/// Sets the extended window style attributes for specified window.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="styles">The new extended window styles.</param>
		/// <returns>true if successful, false otherwise.</returns>
		public static void SetExtendedWindowStyles(IntPtr handle, ExtendedWindowStyles styles)
		{
			if (handle == IntPtr.Zero) return;

			UnsafeNativeMethods.SetWindowLong(handle, NativeMethods.GWL_EXSTYLE, (int)styles);
		}

		/// <summary>
		/// Returns the control style attributes for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns></returns>
		public static int GetControlStyles(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return 0;

			return unchecked((int)(UnsafeNativeMethods.GetWindowLong(handle, NativeMethods.GWL_STYLE).ToInt64() & 0xFFFF));
		}

		/// <summary>
		/// Sets the control style attributes for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="styles">The new control styles.</param>
		/// <returns></returns>
		public static void SetControlStyles(IntPtr handle, int styles)
		{
			if (handle == IntPtr.Zero) return;

			styles &= 0xFFFF;

			var oldStyles = ((uint)GetWindowStyles(handle) & 0xFFFF0000);

			UnsafeNativeMethods.SetWindowLong(handle, NativeMethods.GWL_STYLE, (int)(oldStyles | (ushort)styles));
		}

		/// <summary>
		/// Sets the window position, z-order.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="insertAfter">Z-Order flags</param>
		/// <param name="bounds">The new position and size.</param>
		/// <param name="flags">Options for showing the window.</param>
		/// <returns></returns>
		public static bool SetWindowPos(IntPtr handle, WindowOrder insertAfter, Rectangle bounds, WindowPosition flags)
		{
			return SetWindowPos(handle, new IntPtr((int)insertAfter), bounds, flags);
		}

		/// <summary>
		/// Sets the window position, z-order.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="insertAfter">Z-Order flags</param>
		/// <param name="bounds">The new position and size.</param>
		/// <param name="flags">Options for showing the window.</param>
		/// <returns></returns>
		public static bool SetWindowPos(IntPtr handle, IntPtr insertAfter, Rectangle bounds, WindowPosition flags)
		{
			return UnsafeNativeMethods.SetWindowPos(handle, insertAfter, bounds.X, bounds.Y, bounds.Width, bounds.Height, (int) flags);
		}

		/// <summary>
		/// Returns the coordinates of the upper-left corner of the window relative to the main desktop for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns></returns>
		public static Point GetLocation(IntPtr handle)
		{
			return GetBounds(handle).Location;
		}

		/// <summary>
		/// Sets the coordinates of the upper-left corner of the window relative to the main desktop for specified handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="location">The new location.</param>
		/// <returns></returns>
		public static bool SetLocation(IntPtr handle, Point location)
		{
			return SetWindowPos(handle, IntPtr.Zero, new Rectangle(location, Size.Empty), WindowPosition.NoActivate | WindowPosition.NoZOrder | WindowPosition.NoSize);
		}

		/// <summary>
		/// Returns the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns></returns>
		public static Size GetSize(IntPtr handle)
		{
			return GetBounds(handle).Size;
		}

		/// <summary>
		/// Sets the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="size">The new size.</param>
		/// <returns></returns>
		public static bool SetSize(IntPtr handle, Size size)
		{
			return SetWindowPos(handle, IntPtr.Zero, new Rectangle(Point.Empty, size), WindowPosition.NoActivate | WindowPosition.NoZOrder | WindowPosition.NoMove);
		}

		/// <summary>
		/// Returns the bounds of the window for specified window handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns></returns>
		public static Rectangle GetBounds(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return Rectangle.Empty;
			NativeMethods.RECT rect;
			return UnsafeNativeMethods.GetWindowRect(handle, out rect)
				? (Rectangle)rect
				: Rectangle.Empty;
		}

		/// <summary>
		/// Sets the bounds of the window for specified window handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="bounds">The new bounds.</param>
		public static bool SetBounds(IntPtr handle, Rectangle bounds)
		{
			return SetWindowPos(handle, IntPtr.Zero, bounds, WindowPosition.NoActivate | WindowPosition.NoZOrder);
		}

		/// <summary>
		/// Returns the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <returns></returns>
		public static Size GetClientSize(IntPtr handle)
		{
			if (handle == IntPtr.Zero) return Size.Empty;
			NativeMethods.RECT rect;
			return UnsafeNativeMethods.GetClientRect(handle, out rect) 
				? rect.Size 
				: Size.Empty;
		}

		/// <summary>
		/// Computes the location of the specified screen point into client coordinates.
		/// </summary>
		/// <param name="p">The screen coordinate Point to convert.</param>
		/// <returns>A Point that represents the converted Point, p, in client coordinates.</returns>
		public Point PointToClient(Point p)
		{
			return PointToClient(Handle, p);
		}

		/// <summary>
		/// Computes the location of the specified screen point into client coordinates.
		/// </summary>
		/// <param name="handle">The window handle.</param>
		/// <param name="p">The screen coordinate Point to convert.</param>
		/// <returns>A Point that represents the converted Point, p, in client coordinates.</returns>
		public static Point PointToClient(IntPtr handle, Point p)
		{
			if (handle == IntPtr.Zero) return p;

			var loc = GetLocation(handle);
			p.X -= loc.X;
			p.Y -= loc.Y;
			return p;
		}

		/// <summary>
		/// Sends a window message to the current window with parameters.
		/// </summary>
		public IntPtr SendMessage(int msg, IntPtr wParam, IntPtr lParam)
		{
			return UnsafeNativeMethods.SendMessage(Handle, msg, wParam, lParam);
		}

		/// <summary>
		/// Sends a window message to the current window with parameters.
		/// </summary>
		public IntPtr SendMessage(int msg, int wParam, int lParam)
		{
			return SendMessage(msg, new IntPtr(wParam), new IntPtr(lParam));
		}

		/// <summary>
		/// Posts a window message to the current window with parameters.
		/// </summary>
		public bool PostMessage(int msg, IntPtr wParam, IntPtr lParam)
		{
			return UnsafeNativeMethods.PostMessage(Handle, msg, wParam, lParam);
		}

		/// <summary>
		/// Posts a window message to the current window with parameters.
		/// </summary>
		public bool PostMessage(int msg, int wParam, int lParam)
		{
			return PostMessage(msg, new IntPtr(wParam), new IntPtr(lParam));
		}


		/// <summary>
		/// Captures the mouse to the window.
		/// </summary>
		/// <returns>Returns a handle from the previous window that had mouse capture.</returns>
		public IntPtr SetCapture()
		{
			return SafeNativeMethods.SetCapture(new HandleRef(this, Handle));
		}

		/// <summary>
		/// Returns an enumerable of windows for specified process id.
		/// </summary>
		/// <param name="processId">The process (id) to get windows for.</param>
		public static IEnumerable<Window> GetWindows(int processId)
		{
			return GetWindows(w => processId == 0 || w.ProcessId == processId);
		}

		/// <summary>
		/// Returns an enumerable of windows, filtered by specified predicate match.
		/// </summary>
		/// <param name="match">The predicate to filter by.</param>
		public static IEnumerable<Window> GetWindows(Func<Window, bool> match = null)
		{
			var windows = new List<Window>();
			NativeMethods.EnumThreadWindowsCallback enumWindows = (hwnd, lParam) =>
			{
				var window = new Window(hwnd);
				if (match == null || match(window)) windows.Add(window);
				return true;
			};

			UnsafeNativeMethods.EnumWindows(enumWindows, IntPtr.Zero);
			return windows;
		}

		/// <summary>
		/// Returns an enumerable of child windows, filtered by specified predicate match.
		/// </summary>
		/// <param name="parentWindow">The parent window.</param>
		/// <param name="match">The predicate to filter by.</param>
		public static IEnumerable<Window> GetChildWindows(Window parentWindow, Func<Window, bool> match = null)
		{
			if (!parentWindow.Exists) return new Window[0];

			var windows = new List<Window>();
			NativeMethods.EnumThreadWindowsCallback enumWindows = (hwnd, lParam) =>
			{
				var window = new Window(hwnd);
				if (match == null || match(window)) windows.Add(window);
				return true;
			};

			UnsafeNativeMethods.EnumChildWindows(parentWindow.Handle, enumWindows, IntPtr.Zero);
			return windows;
		}

		/// <summary>
		/// Gets an array of type Window that represents all the child windows for this window.
		/// </summary>
		public IEnumerable<Window> Childs => GetChildWindows(this);

		/// <summary>
		/// Shows the window using specified parameters.
		/// </summary>
		/// <param name="nCmdShow"></param>
		public void Show(ShowWindow nCmdShow)
		{
			SafeNativeMethods.ShowWindow(new HandleRef(this, Handle), nCmdShow);
		}




		


		/// <summary>
		/// Activates the window. The window must be attached to the calling thread's message queue. 
		/// </summary>
		public void Activate()
		{
			SafeNativeMethods.SetActiveWindow(new HandleRef(this, Handle));
		}

		/// <summary>
		/// Brings the window to the top of the Z order. If the window is a top-level window, it is activated. If the window is a child window, the top-level parent window associated with the child window is activated. 
		/// </summary>
		public void BringToTop()
		{
			SafeNativeMethods.BringWindowToTop(new HandleRef(this, Handle));
		}

		/// <summary>
		/// Puts the thread that created the specified window into the foreground and activates the window. Keyboard input is directed to the window, and various visual cues are changed for the user. The system assigns a slightly higher priority to the thread that created the foreground window than it does to other threads.
		/// </summary>
		public void SetToForeground()
		{
			SafeNativeMethods.SetForegroundWindow(new HandleRef(this, Handle));
		}

		/// <summary>
		/// Gets the foreground window.
		/// </summary>
		/// <returns>The foreground window.</returns>
		public static Window GetForeground()
		{
			var hwnd = NativeMethods.GetForegroundWindow();
			return hwnd != IntPtr.Zero ? new Window(hwnd) : null;
		}

		/// <summary>
		/// Gets the desktop window.
		/// </summary>
		/// <returns>The desktop window.</returns>
		public static Window GetDesktopWindow()
		{
			return new Window(NativeMethods.GetDesktopWindow());
		}

		/// <summary>
		/// Assigns a new parent to the current window.
		/// </summary>
		/// <param name="newParent">The new parent.</param>
		public void SetParent(IWin32Window newParent)
		{
			if (newParent == null || newParent.Handle == IntPtr.Zero) return;
			UnsafeNativeMethods.SetParent(new HandleRef(this, Handle), new HandleRef(newParent, newParent.Handle));
		}


		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return "\"" + Text + "\" - " + ClassName;
		}
	
		/// <summary>
		/// Returns the icon associated with the application executable.
		/// </summary>
		/// <returns></returns>
		public Icon GetAssemblyIcon()
		{
			var sfi = new NativeMethods.SHFILEINFO();
			var flags = NativeMethods.SHGFI.SHGFI_LARGEICON | NativeMethods.SHGFI.SHGFI_ICON;

			Icon ret = null;
			if (SafeNativeMethods.SHGetFileInfo(ModuleName, 0, ref sfi, Marshal.SizeOf(sfi), flags) != IntPtr.Zero)
				ret = Icon.FromHandle(sfi.hIcon);
			return ret;
		}

		/// <summary>
		/// Returns the icon associated with the window.
		/// </summary>
		/// <returns></returns>
		public virtual Icon GetWindowIcon()
		{
			if (IsDesktop)
			{
				IntPtr pidl;
				if (SafeNativeMethods.SHGetFolderLocation(new HandleRef(this, Handle), 0, IntPtr.Zero, 0, out pidl) == 0)
				{
					var sfi = new NativeMethods.SHFILEINFO();
					var flags = NativeMethods.SHGFI.SHGFI_LARGEICON | NativeMethods.SHGFI.SHGFI_ICON | NativeMethods.SHGFI.SHGFI_PIDL;

					Icon retIcon = null;
					if (SafeNativeMethods.SHGetFileInfo(new HandleRef(this, pidl), 0, ref sfi, Marshal.SizeOf(sfi), flags) != IntPtr.Zero)
					{
						var ico = Icon.FromHandle(sfi.hIcon);
						retIcon = (Icon)ico.Clone();
						ico.Dispose();
						SafeNativeMethods.DestroyIcon(new HandleRef(sfi, sfi.hIcon));
					}

					Marshal.FreeCoTaskMem(pidl);
					return retIcon;
				}
				return null;
			}
			else
			{
				var hIcon = SendMessage(NativeMethods.WM_GETICON, NativeMethods.ICON_BIG, 0);
				if (hIcon != IntPtr.Zero)
				{
					// Return a cloned icon (the original icon can be destroyed at any time, and is unsafe to use.)
					// BUG: Cloning blows memory?
					var appIcon = Icon.FromHandle(hIcon);
					var retIcon = (Icon)appIcon.Clone();
					appIcon.Dispose();
					return retIcon;
				}
				else
					return null;
			}
		}

		/// <summary>
		/// Gets the current focused control.
		/// </summary>
		/// <returns>The focused control or null.</returns>
		public static IWin32Window GetFocusedControl()
		{
			IWin32Window focusedControl = null;
			var focusedHandle = NativeMethods.GetFocus();
			if (focusedHandle != IntPtr.Zero)
				focusedControl = Control.FromHandle(focusedHandle);
			return focusedControl ?? (focusedHandle != IntPtr.Zero ? new Window(focusedHandle) : null);
		} 
 
		/// <summary>
		/// Gets the window parent.
		/// </summary>
		/// <param name="window">The window to get the parent of.</param>
		/// <returns>The parent window or null if the window is a top level window.</returns>
		public static Window GetParent(IWin32Window window)
		{
			if (window == null || window.Handle == IntPtr.Zero) return null;
			var hwnd = NativeMethods.GetParent(new HandleRef(window, window.Handle));
			return hwnd != IntPtr.Zero ? new Window(hwnd) : null;
		}

		/// <summary>
		/// Gets all classes of child windows down to the parent root window in path form separated by '\'.
		/// </summary>
		/// <returns></returns>
		public string GetClassPath()
		{
			var classPath = ClassName;
			var parent = Parent;
			while (parent != null)
			{
				classPath = parent.ClassName + "\\" + classPath;
				parent = parent.Parent;
			}
			return classPath;
		}

		/// <summary>
		/// Returns the first window with specified text and class name.
		/// </summary>
		/// <param name="text">The window text, or null to search only by class name.</param>
		/// <param name="className">The window class name, or null to search only by window text.</param>
		/// <returns></returns>
		public static Window Find(string text, string className)
		{
			var hwnd = UnsafeNativeMethods.FindWindow(className, text);
			return hwnd != IntPtr.Zero ? new Window(hwnd) : null;
		}

		/*		[DllImport("user32.dll", CharSet=CharSet.Auto, EntryPoint="GetActiveWindow")]
				private static extern IntPtr GetActiveWindowInternal();
				public static Window GetActiveWindow() 
				{
					return new Window(GetActiveWindowInternal());
				}

				[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
				public static extern IntPtr GetAncestor(HandleRef hWnd, int flags);


				[DllImport("user32.dll", CharSet=CharSet.Auto)]
				private static extern IntPtr ChildWindowFromPointEx(HandleRef hwndParent, POINT pt, int uFlags);

				public static IntPtr ChildWindowFromPointEx(HandleRef hwndParent, Point pt, int uFlags)
				{
					return ChildWindowFromPointEx(hwndParent, new POINT(pt.X, pt.Y), uFlags);
				}

		*/

		private void GetWindowThreadProcessId()
		{
			_threadId = SafeNativeMethods.GetWindowThreadProcessId(new HandleRef(this, Handle), out _processId);
		}

		#region Equality members

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object. </param>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		protected bool Equals(Window other)
		{
			return Handle.Equals(other.Handle);
		}

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The object to compare with the current object. </param>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Window)obj);
		}

		/// <summary>
		/// Serves as the default hash function. 
		/// </summary>
		/// <returns>
		/// A hash code for the current object.
		/// </returns>
		public override int GetHashCode()
		{
			return Handle.GetHashCode();
		}

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="left">The first object.</param>
		/// <param name="right">The second object.</param>
		/// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
		public static bool operator ==(Window left, Window right)
		{
			return Equals(left, right);
		}

		/// <summary>
		/// Determines whether the specified objects are not equal.
		/// </summary>
		/// <param name="left">The first object.</param>
		/// <param name="right">The second object.</param>
		/// <returns>true if the specified object is not equal to the current object; otherwise, false.</returns>
		public static bool operator !=(Window left, Window right)
		{
			return !Equals(left, right);
		}

		/// <summary>
		/// Explicit cast from .NET control.
		/// </summary>
		/// <param name="window"></param>
		public static explicit operator Window(Control window)
		{			
			return new Window(window.Handle);
		}

		/// <summary>
		/// Explicit cast from NativeWindow.
		/// </summary>
		/// <param name="window"></param>
		public static explicit operator Window(NativeWindow window)
		{
			return new Window(window.Handle);
		}

		#endregion

		#region ICloneable members

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>
		/// A new object that is a copy of this instance.
		/// </returns>
		public object Clone()
		{
			return new Window(Handle);
		}

		#endregion
	}
}
