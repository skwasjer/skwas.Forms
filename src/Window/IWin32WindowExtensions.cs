using System;
using System.Drawing;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace skwas.Forms
{
	/// <summary>
	/// Extensions for I
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static class IWin32WindowExtensions
	{
		/// <summary>
		/// Returns the window style attributes for specified window.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static WindowStyles GetWindowStyles(this IWin32Window window)
		{
			return Window.GetWindowStyles(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the window style attributes for specified window.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="styles">The new window styles.</param>
		/// <returns>true if successful, false otherwise.</returns>
		public static void SetWindowStyles(this IWin32Window window, WindowStyles styles)
		{
			Window.SetWindowStyles(window?.Handle ?? IntPtr.Zero, styles);
		}

		/// <summary>
		/// Returns the extended window style attributes for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static ExtendedWindowStyles GetExtendedWindowStyles(this IWin32Window window)
		{
			return Window.GetExtendedWindowStyles(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the extended window style attributes for specified window.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="styles">The new extended window styles.</param>
		/// <returns>true if successful, false otherwise.</returns>
		public static void SetExtendedWindowStyles(this IWin32Window window, ExtendedWindowStyles styles)
		{
			Window.SetExtendedWindowStyles(window?.Handle ?? IntPtr.Zero, styles);
		}

		/// <summary>
		/// Returns the control style attributes for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <returns></returns>
		public static int GetControlStyles(this IWin32Window window)
		{
			return Window.GetControlStyles(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the control style attributes for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="styles">The new control styles.</param>
		/// <returns></returns>
		public static void SetControlStyles(this IWin32Window window, int styles)
		{
			Window.SetControlStyles(window?.Handle ?? IntPtr.Zero, styles);
		}

		/// <summary>
		/// Returns the coordinates of the upper-left corner of the window relative to the main desktop for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Point GetLocation(this IWin32Window window)
		{
			return Window.GetLocation(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the coordinates of the upper-left corner of the window relative to the main desktop for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="location">The new location.</param>
		public static bool SetLocation(this IWin32Window window, Point location)
		{
			return Window.SetLocation(window?.Handle ?? IntPtr.Zero, location);
		}

		/// <summary>
		/// Returns the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Size GetSize(this IWin32Window window)
		{
			return Window.GetSize(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="size">The new size.</param>
		public static bool SetSize(this IWin32Window window, Size size)
		{
			return Window.SetSize(window?.Handle ?? IntPtr.Zero, size);
		}

		/// <summary>
		/// Returns the bounds of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Rectangle GetBounds(this IWin32Window window)
		{
			return Window.GetBounds(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the bounds of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="bounds">The new bounds.</param>
		public static bool SetBounds(this IWin32Window window, Rectangle bounds)
		{
			return Window.SetBounds(window?.Handle ?? IntPtr.Zero, bounds);
		}

		/// <summary>
		/// Returns the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Size GetClientSize(this IWin32Window window)
		{
			return Window.GetClientSize(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Sets the window position, z-order.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="insertAfter">Z-Order flags</param>
		/// <param name="bounds">The new position and size.</param>
		/// <param name="flags">Options for showing the window.</param>
		/// <returns></returns>
		public static bool SetWindowPos(this IWin32Window window, WindowOrder insertAfter, Rectangle bounds, WindowPosition flags)
		{
			return Window.SetWindowPos(window?.Handle ?? IntPtr.Zero, insertAfter, bounds, flags);
		}

		/// <summary>
		/// Sets the window position, z-order.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="insertAfter">Z-Order flags</param>
		/// <param name="bounds">The new position and size.</param>
		/// <param name="flags">Options for showing the window.</param>
		/// <returns></returns>
		public static bool SetWindowPos(this IWin32Window window, IntPtr insertAfter, Rectangle bounds, WindowPosition flags)
		{
			return Window.SetWindowPos(window?.Handle ?? IntPtr.Zero, insertAfter, bounds, flags);
		}


		/// <summary>
		/// Returns the window caption text for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <returns>The window caption text for specified handle.</returns>
		public static string GetWindowText(this IWin32Window window)
		{
			return Window.GetWindowText(window?.Handle ?? IntPtr.Zero);
		}

		/// <summary>
		/// Returns the window class name for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <returns>The window class name for specified handle.</returns>
		public static string GetClassName(this IWin32Window window)
		{
			return Window.GetClassName(window?.Handle ?? IntPtr.Zero);
		}
	}
}