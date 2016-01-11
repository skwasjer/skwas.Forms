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
			return window == null
				? 0
				: Window.GetWindowStyles(window.Handle);
		}

		/// <summary>
		/// Sets the window style attributes for specified window.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="styles">The new window styles.</param>
		/// <returns>true if successful, false otherwise.</returns>
		public static void SetWindowStyles(this IWin32Window window, WindowStyles styles)
		{
			if (window == null) return;

			Window.SetWindowStyles(window.Handle, styles);
		}

		/// <summary>
		/// Returns the extended window style attributes for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static ExtendedWindowStyles GetExtendedWindowStyles(this IWin32Window window)
		{
			return window == null
				? 0
				: Window.GetExtendedWindowStyles(window.Handle);
		}

		/// <summary>
		/// Sets the extended window style attributes for specified window.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="styles">The new extended window styles.</param>
		/// <returns>true if successful, false otherwise.</returns>
		public static void SetExtendedWindowStyles(this IWin32Window window, ExtendedWindowStyles styles)
		{
			if (window == null) return;

			Window.SetExtendedWindowStyles(window.Handle, styles);
		}

		/// <summary>
		/// Returns the control style attributes for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <returns></returns>
		public static int GetControlStyles(this IWin32Window window)
		{
			return window == null
				? 0
				: Window.GetControlStyles(window.Handle);
		}

		/// <summary>
		/// Sets the control style attributes for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="styles">The new control styles.</param>
		/// <returns></returns>
		public static void SetControlStyles(this IWin32Window window, int styles)
		{
			if (window == null) return;

			Window.SetControlStyles(window.Handle, styles);
		}

		/// <summary>
		/// Returns the coordinates of the upper-left corner of the window relative to the main desktop for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Point GetLocation(this IWin32Window window)
		{
			return window == null
				? Point.Empty
				: Window.GetLocation(window.Handle);
		}

		/// <summary>
		/// Sets the coordinates of the upper-left corner of the window relative to the main desktop for specified handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="location">The new location.</param>
		public static bool SetLocation(this IWin32Window window, Point location)
		{
			return window != null && Window.SetLocation(window.Handle, location);
		}

		/// <summary>
		/// Returns the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Size GetSize(this IWin32Window window)
		{
			return window == null
				? Size.Empty
				: Window.GetSize(window.Handle);
		}

		/// <summary>
		/// Sets the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="size">The new size.</param>
		public static bool SetSize(this IWin32Window window, Size size)
		{
			return window != null && Window.SetSize(window.Handle, size);
		}

		/// <summary>
		/// Returns the bounds of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Rectangle GetBounds(this IWin32Window window)
		{
			return window == null
				? Rectangle.Empty
				: Window.GetBounds(window.Handle);
		}

		/// <summary>
		/// Sets the bounds of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		/// <param name="bounds">The new bounds.</param>
		public static bool SetBounds(this IWin32Window window, Rectangle bounds)
		{
			return window != null && Window.SetBounds(window.Handle, bounds);
		}

		/// <summary>
		/// Returns the height and width of the window for specified window handle.
		/// </summary>
		/// <param name="window">The window handle.</param>
		public static Size GetClientSize(this IWin32Window window)
		{
			return window == null
				? Size.Empty
				: Window.GetClientSize(window.Handle);
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
			return window != null && Window.SetWindowPos(window.Handle, insertAfter, bounds, flags);
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
			return window != null && Window.SetWindowPos(window.Handle, insertAfter, bounds, flags);
		}
	}
}