using System;

namespace skwas.Forms
{
	/// <summary>
	/// Represents the event arguments for application start up.
	/// </summary>
	public class ApplicationStartedEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="ApplicationStartedEventArgs"/>.
		/// </summary>
		/// <param name="cmdLine">The command line string.</param>
		/// <param name="args">The parsed command line arguments.</param>
		public ApplicationStartedEventArgs(string cmdLine, string[] args)
		{
			CmdLine = cmdLine;
			Arguments = args;
		}

		/// <summary>
		/// The raw command line.
		/// </summary>
		public string CmdLine { get; }

		/// <summary>
		/// The command line arguments.
		/// </summary>
		public string[] Arguments { get; }
	}
}