using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace skwas.Forms
{
	internal static class UnsafeNativeMethods
	{
		private static readonly object Ref = new object();

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr OpenProcess(NativeMethods.ProcessAccess access, bool inherit, int processId);

		[DllImport("psapi", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumProcessModules(HandleRef hProcess, IntPtr[] hModules, int size, out int needed);

		[DllImport("psapi", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern int GetModuleFileNameEx(HandleRef processHandle, HandleRef moduleHandle, StringBuilder filename, int size);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern IntPtr FindWindow([MarshalAs(UnmanagedType.LPTStr)] string className, [MarshalAs(UnmanagedType.LPTStr)] string text);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

		public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
		{
			return IntPtr.Size == 4 
				? (IntPtr)GetWindowLong32(hWnd, nIndex) 
				: GetWindowLong64(hWnd, nIndex);
		}

		[DllImport("user32", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

		[SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist")]
		[DllImport("user32", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetWindowLong64(IntPtr hWnd, int nIndex);

		public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong)
		{
			return IntPtr.Size == 4
				? SetWindowLong32(hWnd, nIndex, dwNewLong)
				: SetWindowLong64(hWnd, nIndex, dwNewLong);
		}

		[DllImport("user32", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowLong64(IntPtr hWnd, int nIndex, int dwNewLong);


		[DllImport("user32", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool GetWindowRect(IntPtr hWnd, out NativeMethods.RECT lpRect);

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		public static extern bool GetClientRect(IntPtr hWnd, out NativeMethods.RECT lpRect);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr CreateToolhelp32Snapshot(NativeMethods.Toolhelp32Flags flags, int processId);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumWindows(
			NativeMethods.EnumThreadWindowsCallback callback,
			IntPtr extraData
			);

		[DllImport("user32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumChildWindows(
			IntPtr hWndParent,
			NativeMethods.EnumThreadWindowsCallback lpEnumFunc,
			IntPtr extraData
			);


		[DllImport("user32", CharSet = CharSet.Auto)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

		[DllImport("user32", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);

		[DllImport("user32", SetLastError = true)]
		public static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		public static string GetModuleNameWinNT(int processId)
		{
			var hProcess = OpenProcess(NativeMethods.ProcessAccess.QueryInformation | NativeMethods.ProcessAccess.VmRead, false, processId);

			if (hProcess == IntPtr.Zero) return null;

			var hMods = new IntPtr[1]; // We only want the main module.
			var sb = new StringBuilder(260, 260);
			try
			{
				int cbNeeded;
				if (!EnumProcessModules(new HandleRef(Ref, hProcess), hMods, hMods.Length * IntPtr.Size, out cbNeeded)) return null;

				var hr = GetModuleFileNameEx(new HandleRef(Ref, hProcess), new HandleRef(hMods, hMods[0]), sb, sb.MaxCapacity);

				return hr != 0 ? sb.ToString() : null;
			}
			finally
			{
				SafeNativeMethods.CloseHandle(new HandleRef(Ref, hProcess));
			}
		}

		public static string GetModuleNameWin32(int processId)
		{
			var hSnapShot = new HandleRef(Ref, CreateToolhelp32Snapshot(NativeMethods.Toolhelp32Flags.SnapProcess, 0));
			if (hSnapShot.Handle == IntPtr.Zero) return null;

			string sModule = null;
			var uProcess = new NativeMethods.PROCESSENTRY32();

			// Create a snapshot from all running processes.
			uProcess.dwSize = Marshal.SizeOf(uProcess);

			// Loop all processes in the snapshot and find the process id.
			var bProcessFound = Process32First(hSnapShot, ref uProcess);
			while (bProcessFound)
			{
				if (uProcess.th32ProcessID == processId)
				{
					// Save the module name and exit loop.
					sModule = uProcess.szExeFile.Trim('\0');
					break;
				}
				// Continue enumeration of processes?
				bProcessFound = Process32Next(hSnapShot, ref uProcess);
			}
			SafeNativeMethods.CloseHandle(hSnapShot);

			return sModule;
		}

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool Process32First(
			HandleRef hSnapshot,
			ref NativeMethods.PROCESSENTRY32 lppe
			);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool Process32Next(
			HandleRef hSnapshot,
			ref NativeMethods.PROCESSENTRY32 lppe
			);

		[DllImport("user32")]
		public static extern bool MoveWindow(HandleRef hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32")]
		public static extern IntPtr SetParent(HandleRef hWndChild, HandleRef hWndNewParent);

		[DllImport("gdi32", SetLastError = true)]
		public static extern bool DeleteObject(IntPtr hObject);
	}
}