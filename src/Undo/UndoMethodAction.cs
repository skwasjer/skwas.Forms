using System;

namespace skwas.Forms
{
	/// <summary>
	/// Represents an undo action that will execute a callback for redo/undo operations.
	/// </summary>
	public class UndoMethodAction
		: UndoAction
	{
		private readonly Action<UndoMethodAction> _undo, _redo;

		/// <summary>
		/// Initializes a new instance of <see cref="UndoMethodAction"/>.
		/// </summary>
		/// <param name="affectedInstance">The instance on which this action operates.</param>
		/// <param name="undo">The undo action.</param>
		/// <param name="redo">The redo action.</param>
		/// <param name="description">A description for the action.</param>
		/// <param name="arguments">Optional arguments.</param>
		public UndoMethodAction(
			object affectedInstance, 
			Action<UndoMethodAction> undo, 
			Action<UndoMethodAction> redo, 
			string description, 
			params object[] arguments
			)
			: base(description, affectedInstance)
		{
			if (undo == null)
				throw new ArgumentNullException(nameof(undo));
			if (redo == null)
				throw new ArgumentNullException(nameof(redo));

			Arguments = arguments;

			_undo = undo;
			_redo = redo;
		}

		/// <summary>
		/// Gets the undo description.
		/// </summary>
		public override string UndoDescription => "Undo " + _redo.Method.Name + " " + Description;

		/// <summary>
		/// Gets the redo description.
		/// </summary>
		public override string RedoDescription => "Redo " + _redo.Method.Name + " " + Description;

		/// <summary>
		/// Gets or sets the arguments associated with the action.
		/// </summary>
		public object[] Arguments { get; set; }

		/// <summary>
		/// Executes the undo action.
		/// </summary>
		protected override void OnUndo()
		{
			_undo.DynamicInvoke(this);
		}

		/// <summary>
		/// Executes the redo action.
		/// </summary>
		protected override void OnRedo()
		{
			_redo.DynamicInvoke(this);
		}
	}
}