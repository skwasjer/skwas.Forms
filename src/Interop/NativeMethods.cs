using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace skwas.Forms
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	internal static class NativeMethods
	{
		public delegate bool EnumThreadWindowsCallback(IntPtr hwnd, int lParam);

		public const int GWL_STYLE = (-16);
		public const int GWL_EXSTYLE = (-20);

		public const int WM_GETICON = 0x007F;
		public const int WM_COPYDATA = 0x004a;
		public const int WM_KEYDOWN = 0x100;

		//			const int ICON_SMALL = 0;
		public const int ICON_BIG = 1;
		// #if(_WIN32_WINNT >= 0x0501)
		//			const int ICON_SMALL2 = 2;
		// #endif /* _WIN32_WINNT >= 0x0501 */


		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public static RECT Empty;			

			public int Width => Right - Left;

			public int Height => Bottom - Top;

			public Size Size => new Size(Width, Height);

			public Point Location => new Point(Left, Top);

			public bool IsEmpty => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;

			public static implicit operator Rectangle(RECT rc)
			{
				return new Rectangle(rc.Left, rc.Top, rc.Width, rc.Height);
			}

			public static implicit operator RECT (Rectangle rc)
			{
				return new RECT
				{
					Left = rc.Left,
					Top = rc.Top,
					Right = rc.Right,
					Bottom = rc.Bottom
				};
			}
		}

		[Flags]
		public enum ProcessAccess
		{
			Terminate = 0x0001,
			CreateThread = 0x0002,
			SetSessionId = 0x0004,
			VmOperation = 0x0008,
			VmRead = 0x0010,
			VmWrite = 0x0020,
			DupHandle = 0x0040,
			CreateProcess = 0x0080,
			SetQuota = 0x0100,
			SetInformation = 0x0200,
			QueryInformation = 0x0400,
			SuspendResume = 0x0800,
			StandardRightsRequired = 0x000F0000,
			Synchronize = 0x00100000,
			AllAccess = StandardRightsRequired | Synchronize | 0xFFF
		}


		[Flags]
		public enum Toolhelp32Flags
		{
			SnapHeapList = 0x00000001,
			SnapProcess = 0x00000002,
			SnapThread = 0x00000004,
			SnapModule = 0x00000008,
			SnapModule32 = 0x00000010,
			SnapAll = SnapHeapList | SnapProcess | SnapThread | SnapModule,
			Inherit = unchecked((int)0x80000000)
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		public struct PROCESSENTRY32
		{
			public int dwSize;
			public int cntUsage;
			public int th32ProcessID;
			public IntPtr th32DefaultHeapID;
			public int th32ModuleID;
			public int cntThreads;
			public int th32ParentProcessID;
			public int pcPriClassBase;
			public int dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szExeFile;
		}

		[Flags]
		public enum SHGFI
		{
			SHGFI_ATTRIBUTES = 0x800,
			SHGFI_DISPLAYNAME = 0x200,
			SHGFI_EXETYPE = 0x2000,
			SHGFI_ICON = 0x100,
			SHGFI_ICONLOCATION = 0x1000,
			SHGFI_LARGEICON = 0x0,
			SHGFI_LINKOVERLAY = 0x8000,
			SHGFI_OPENICON = 0x2,
			SHGFI_PIDL = 0x8,
			SHGFI_SELECTED = 0x10000,
			SHGFI_SHELLICONSIZE = 0x4,
			SHGFI_SMALLICON = 0x1,
			SHGFI_SYSICONINDEX = 0x4000,
			SHGFI_TYPENAME = 0x400,
			SHGFI_USEFILEATTRIBUTES = 0x10
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SHFILEINFO
		{
			public IntPtr hIcon;
			public int iIcon;
			public int dwAttributes;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string szDisplayName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
			public string szTypeName;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT
			: IDisposable
		{
			public int dwData;
			public int cbData;
			public IntPtr lpData;

			#region Implementation of IDisposable

			/// <summary>
			/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
			/// </summary>
			public void Dispose()
			{
				Marshal.FreeHGlobal(lpData);
			}

			#endregion
		}


		/// <summary>
		/// Normal button.
		/// </summary>
		public const int BCM_FIRST = 0x1600;

		/// <summary>
		/// Elevated button.
		/// </summary>
		public const int BCM_SETSHIELD = (BCM_FIRST + 0x000C);


		public enum BSM
		{
			// Broadcast Special Message Recipient list
			ALLCOMPONENTS = 0x00000000,
			BSM_VXDS = 0x00000001,
			BSM_NETDRIVER = 0x00000002,
			BSM_INSTALLABLEDRIVERS = 0x00000004,
			BSM_APPLICATIONS = 0x00000008,
			BSM_ALLDESKTOPS = 0x00000010
		}

		public enum BSF
		{
			//Broadcast Special Message Flags
			BSF_QUERY = 0x00000001,
			BSF_IGNORECURRENTTASK = 0x00000002,
			BSF_FLUSHDISK = 0x00000004,
			BSF_NOHANG = 0x00000008,
			BSF_POSTMESSAGE = 0x00000010,
			BSF_FORCEIFHUNG = 0x00000020,
			BSF_NOTIMEOUTIFNOTHUNG = 0x00000040,
			// #if(_WIN32_WINNT >= 0x0500)
			BSF_ALLOWSFW = 0x00000080,
			BSF_SENDNOTIFYMESSAGE = 0x00000100,
			// #endif /* _WIN32_WINNT >= 0x0500 */
			// #if(_WIN32_WINNT >= 0x0501)
			BSF_RETURNHDESK = 0x00000200,
			BSF_LUID = 0x00000400,
			// #endif /* _WIN32_WINNT >= 0x0501 */
		}

		[DllImport("user32")]
		public static extern int BroadcastSystemMessage(
			BSF dwFlags,
			ref BSM lpdwRecipients,
			int uiMessage,
			IntPtr wParam,
			IntPtr lParam
			);

		[DllImport("user32")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32")]
		public static extern IntPtr GetParent(HandleRef hWnd);

		[DllImport("user32")]
		public static extern IntPtr GetFocus();

		[DllImport("user32")]
		public static extern bool IsWindow(HandleRef hWnd);
	}
}