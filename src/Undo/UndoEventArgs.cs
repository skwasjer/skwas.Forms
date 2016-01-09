using System;

namespace skwas.Forms
{
	/// <summary>
	/// Represents the event arguments for an undo/redo action.
	/// </summary>
	public class UndoEventArgs
		: EventArgs
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UndoEventArgs"/>.
		/// </summary>
		/// <param name="undoAction">The undo action.</param>
		public UndoEventArgs(UndoAction undoAction)
		{
			if (undoAction == null)
				throw new ArgumentNullException(nameof(undoAction));
			Action = undoAction;
		}

		/// <summary>
		/// Gets the undo action.
		/// </summary>
		public UndoAction Action { get; }
	}
}