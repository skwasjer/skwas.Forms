using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace skwas.Forms
{
	/// <summary>
	/// Represents a undo/redo action manager.
	/// </summary>
	public class UndoManager
	{
		/// <summary>
		/// Used to prevent modification of the redo/undo stack while processing an undo/redo action.
		/// </summary>
		[SuppressMessage("ReSharper", "InconsistentNaming")]
		private readonly object LockObject = new object();

		/// <summary>
		/// Raised when a new action is added to the stack, the stack is cleared, or an action moves between the undo/redo stacks.
		/// </summary>
		public event EventHandler<EventArgs> Changed;
		
		/// <summary>
		/// Raised after an undo action has been executed.
		/// </summary>
		public event EventHandler<UndoEventArgs> AfterUndo;

		/// <summary>
		/// Raised before an undo action is executed.
		/// </summary>
		public event EventHandler<UndoEventArgs> BeforeUndo;

		/// <summary>
		/// Raised after a redo action has been executed.
		/// </summary>
		public event EventHandler<UndoEventArgs> AfterRedo;

		/// <summary>
		/// Raised before a redo action is executed.
		/// </summary>
		public event EventHandler<UndoEventArgs> BeforeRedo;


		/// <summary>
		/// Gets whether an undo operation is in progress.
		/// </summary>
		public bool IsUndoing { get; private set; }

		/// <summary>
		/// Gets whether a redo operation is in progress.
		/// </summary>
		public bool IsRedoing { get; private set; }

		/// <summary>
		/// Gets whether one or more undo actions are on the undo stack.
		/// </summary>
		public bool CanUndo
		{
			get 
			{
				lock (LockObject)
				{
					return UndoActions.Count > 0;
				}
			}
		}

		/// <summary>
		/// Gets whether one or more redo actions are on the redo stack.
		/// </summary>
		public bool CanRedo
		{
			get {
				lock (LockObject)
				{
					return RedoActions.Count > 0;
				}
			}
		}

		/// <summary>
		/// Gets the actions on the undo stack.
		/// </summary>
		public Stack<UndoAction> UndoActions { get; } = new Stack<UndoAction>();

		/// <summary>
		/// Gets the actions on the redo stack.
		/// </summary>
		public Stack<UndoAction> RedoActions { get; } = new Stack<UndoAction>();

		/// <summary>
		/// Adds a new action to the undo stack and clears the redo stack.
		/// </summary>
		/// <param name="action"></param>
		public void Add(UndoAction action)
		{
			lock (LockObject)
			{
				UndoActions.Push(action);
				RedoActions.Clear();
			}
			OnChanged(EventArgs.Empty);
		}

		/// <summary>
		/// Clears the undo and redo stack.
		/// </summary>
		public void Clear()
		{
			lock (LockObject)
			{
				UndoActions.Clear();
				RedoActions.Clear();
			}
			OnChanged(EventArgs.Empty);
		}

		/// <summary>
		/// Undo the number of actions specified by <paramref name="count"/>.
		/// </summary>
		/// <param name="count">The number of actions to undo.</param>
		public void Undo(int count = 1)
		{
			lock (LockObject)
			{
				if (count > UndoActions.Count)
					throw new ArgumentOutOfRangeException("Can't undo the number of actions specified.", nameof(count));

				IsUndoing = true;
				for (var i = 0; i < count; i++)
				{
					// Get the next action to undo.
					var action = UndoActions.Peek();
					OnBeforeUndo(new UndoEventArgs(action));
					action.Undo();

					// Add the action to the redo stack.
					RedoActions.Push(UndoActions.Pop());
					OnAfterUndo(new UndoEventArgs(action));
				}
				IsUndoing = false;
				OnChanged(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Redo the number of actions specified by <paramref name="count"/>.
		/// </summary>
		/// <param name="count">The number of actions to redo.</param>
		public void Redo(int count = 1)
		{
			lock (LockObject)
			{
				if (count > RedoActions.Count)
					throw new ArgumentOutOfRangeException("Can't redo the number of actions specified.", nameof(count));

				IsRedoing = true;
				for (var i = 0; i < count; i++)
				{
					// Get the next action to redo.
					var action = RedoActions.Peek();
					OnBeforeRedo(new UndoEventArgs(action));
					action.Redo();

					// Add the action to the undo stack.
					UndoActions.Push(RedoActions.Pop());
					OnAfterRedo(new UndoEventArgs(action));
				}
				IsRedoing = false;
				OnChanged(EventArgs.Empty);
			}
		}

		/// <summary>
		/// Raises the <see cref="Changed"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnChanged(EventArgs e)
		{
			Changed?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="BeforeUndo"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnBeforeUndo(UndoEventArgs e)
		{
			BeforeUndo?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="AfterUndo"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnAfterUndo(UndoEventArgs e)
		{
			AfterUndo?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="BeforeRedo"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnBeforeRedo(UndoEventArgs e)
		{
			BeforeRedo?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="AfterRedo"/> event.
		/// </summary>
		/// <param name="e">The event arguments.</param>
		protected virtual void OnAfterRedo(UndoEventArgs e)
		{
			AfterRedo?.Invoke(this, e);
		}
	}
}
