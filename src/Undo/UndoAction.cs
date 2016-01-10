using System.Diagnostics;

namespace skwas.Forms
{
	/// <summary>
	/// Represents the base class for a replayable (redo) or revertable (undo) action.
	/// </summary>
	[DebuggerDisplay("Description = {Description}")]
	public abstract class UndoAction : IUndoAction
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UndoAction"/>.
		/// </summary>
		/// <param name="affectedInstance">The instance on which this action operates.</param>
		protected UndoAction(object affectedInstance)
			: this(null, affectedInstance)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="UndoAction"/>.
		/// </summary>
		/// <param name="description">A description for the action.</param>
		/// <param name="affectedInstance">The instance on which this action operates.</param>
		protected UndoAction(string description, object affectedInstance)
		{
			AffectedInstance = affectedInstance;
			Description = description;
		}

		/// <summary>
		/// Gets a common description.
		/// </summary>
		public virtual string Description { get; }

		/// <summary>
		/// Gets the undo description.
		/// </summary>
		public virtual string UndoDescription => "Undo " + Description;

		/// <summary>
		/// Gets the redo description.
		/// </summary>
		public virtual string RedoDescription => "Redo " + Description;

		/// <summary>
		/// Gets the instance on which this action operates.
		/// </summary>
		public object AffectedInstance { get; }

		/// <summary>
		/// Performs the undo. Calling this method directly will not modify the undo/redo stack, and can cause an action to be replayed twice.
		/// </summary>
		public void Undo()
		{
			OnUndo();
		}

		/// <summary>
		/// Performs the redo. Calling this method directly will not modify the undo/redo stack, and can cause an action to be replayed twice.
		/// </summary>
		public void Redo()
		{
			OnRedo();
		}

		/// <summary>
		/// Executes the undo action.
		/// </summary>
		protected abstract void OnUndo();

		/// <summary>
		/// Executes the redo action.
		/// </summary>
		protected abstract void OnRedo();

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return Description ?? base.ToString();
		}
	}
}