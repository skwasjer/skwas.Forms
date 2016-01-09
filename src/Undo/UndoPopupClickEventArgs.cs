using System;

namespace skwas.Forms
{
	/// <summary>
	/// Represents the event arguments when clicked on an undo popup.
	/// </summary>
	public class UndoPopupClickEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UndoPopupClickEventArgs"/>.
		/// </summary>
		/// <param name="count">The number of undo/redo actions selected.</param>
		/// <param name="mode">The mode (direction).</param>
		public UndoPopupClickEventArgs(int count, UndoPopupMode mode)
		{
			Count = count;
			Mode = mode;
		}

		/// <summary>
		/// Gets the number of undo/redo actions selected.
		/// </summary>
		public int Count { get; }

		/// <summary>
		/// Gets the mode (direction).
		/// </summary>
		public UndoPopupMode Mode { get; }
	}
}