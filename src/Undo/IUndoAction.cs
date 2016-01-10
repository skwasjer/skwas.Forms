namespace skwas.Forms
{
	/// <summary>
	/// Represents a replayable (redo) or revertable (undo) action.
	/// </summary>
	public interface IUndoAction
	{
		/// <summary>
		/// Gets a common description.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// Gets the undo description.
		/// </summary>
		string UndoDescription { get; }

		/// <summary>
		/// Gets the redo description.
		/// </summary>
		string RedoDescription { get; }

		/// <summary>
		/// Performs the undo. Calling this method directly will not modify the undo/redo stack, and can cause an action to be replayed twice.
		/// </summary>
		void Undo();

		/// <summary>
		/// Performs the redo. Calling this method directly will not modify the undo/redo stack, and can cause an action to be replayed twice.
		/// </summary>
		void Redo();
	}
}