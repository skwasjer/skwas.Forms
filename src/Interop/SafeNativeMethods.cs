using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace skwas.Forms
{
	internal static class SafeNativeMethods
	{
		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool ShowWindow(HandleRef hWnd, ShowWindow nCmdShow);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern int GetWindowThreadProcessId(HandleRef hWnd, out int lpdwProcessId);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool CloseHandle(HandleRef hObject);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32")]
		public static extern IntPtr SetActiveWindow(HandleRef hWnd);

		[DllImport("shell32", CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern IntPtr SHGetFileInfo(
			string pszPath,
			int dwFileAttributes,
			ref NativeMethods.SHFILEINFO psfi,
			int cbFileInfo,
			NativeMethods.SHGFI uFlags);

		[DllImport("shell32")]
		public static extern IntPtr SHGetFileInfo(
			HandleRef pidl,
			int dwFileAttributes,
			ref NativeMethods.SHFILEINFO psfi,
			int cbFileInfo,
			NativeMethods.SHGFI uFlags);

		[DllImport("shell32")]
		public static extern int SHGetFolderLocation(
			HandleRef hwndOwner,
			int nFolder,
			IntPtr hToken,
			int dwReserved,
			out IntPtr ppidl
			);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool DestroyIcon(HandleRef hIcon);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool IsIconic(HandleRef hWnd);

		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool IsZoomed(HandleRef hWnd);

		[DllImport("shell32", SetLastError = true)]
		public static extern IntPtr CommandLineToArgvW(
			[MarshalAs(UnmanagedType.LPWStr)]
			string lpCmdLine,
			out int pNumArgs);

		[DllImport("user32")]
		public static extern int BringWindowToTop(HandleRef hWnd);

		[DllImport("user32")]
		public static extern bool SetForegroundWindow(HandleRef hWnd);

		/// <summary>
		/// Sets the mouse capture to the specified window belonging to the current thread.
		/// </summary>
		/// <param name="hWnd">Handle to the window in the current thread that is to capture the mouse.</param>
		/// <returns>Handle to previous window</returns>
		[DllImport("user32")]
		public static extern IntPtr SetCapture(HandleRef hWnd);

		[DllImport("uxtheme", CharSet = CharSet.Unicode)]
		public static extern int SetWindowTheme(IntPtr hWnd, string textSubAppName, string textSubIdList);

		[DllImport("user32", SetLastError = true)]
		public static extern bool SetMenuItemBitmaps(IntPtr hMenu, int uPosition, uint uFlags, IntPtr hBitmapUnchecked, IntPtr hBitmapChecked);
	}
}