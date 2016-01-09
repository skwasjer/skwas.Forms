using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;

namespace skwas.Forms
{
	/// <summary>
	/// Command line arguments helper.
	/// </summary>
	public static class CommandLine
	{
		/// <summary>
		/// Returns the command line arguments in an array.
		/// </summary>
		/// <param name="commandLine">The raw command line.</param>
		public static string[] Parse(string commandLine)
		{
			// CommandLineToArgvW expects executable name + arguments, so add foo.exe
			int argc;
			var argv = SafeNativeMethods.CommandLineToArgvW("foo.exe " + commandLine, out argc);
			if (argv == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			try
			{
				var args = new string[argc];
				for (var i = 0; i < args.Length; i++)
				{
					var p = Marshal.ReadIntPtr(argv, i * IntPtr.Size);
					args[i] = Marshal.PtrToStringUni(p);
				}

				// Ignore foo.exe. If only 1 arg return null.
				return args.Length <= 1 ? null : args.Skip(1).ToArray();
			}
			finally
			{
				Marshal.FreeHGlobal(argv);
			}
		}

		/// <summary>
		/// Packs an array of command line arguments into a single line. If an argument contains a space, the argument is enclosed in double quotes (").
		/// </summary>
		/// <param name="args">The command line arguments to pack.</param>
		public static string Pack(params string[] args)
		{
			// In case of whitespace, add quotes.
			return string.Join(" ",
				args
					.Where(a => !string.IsNullOrEmpty(a))
					.Select(a => a.IndexOf(' ') >= 0 ? "\"" + a + "\"" : a)					
			);
		}
	}
}
