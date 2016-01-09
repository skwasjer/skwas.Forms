using System;
using System.Reflection;
using System.Windows.Forms;

namespace skwas.Forms
{
	/// <summary>
	/// Represents an undo action that will set a property on an instance.
	/// </summary>
	public class UndoSetValueAction<T>
		: UndoAction
	{
		/// <summary>
		/// Initializes a new instance of <see cref="UndoSetValueAction{T}"/>.
		/// </summary>
		/// <param name="affectedInstance">The instance on which this action operates.</param>
		/// <param name="description">A description for the action.</param>
		/// <param name="propertyName">The name of the property on <paramref name="affectedInstance"/> to change.</param>
		/// <param name="oldValue">The old/current value of <paramref name="propertyName"/> on <paramref name="affectedInstance"/>.</param>
		/// <param name="newValue">The new value for <paramref name="propertyName"/> to be set on <paramref name="affectedInstance"/>.</param>
		/// <param name="focusedControl">OBSOLETE: the control to focus when the undo/redo action executes.</param>
		public UndoSetValueAction(object affectedInstance, string propertyName, T oldValue, T newValue, Control focusedControl, string description)
			: base(description, affectedInstance)
		{
			if (affectedInstance == null)
				throw new ArgumentNullException(nameof(affectedInstance));
			if (propertyName == null)
				throw new ArgumentNullException(nameof(propertyName));

			Property = affectedInstance.GetType().GetProperty(propertyName);
			if (Property == null)
				throw new InvalidOperationException(string.Format("Property '{0}' not found.", propertyName));
			// TODO: perform type check of property with type T.

			PropertyName = propertyName;
			OldValue = oldValue;
			NewValue = newValue;
			FocusedControl = focusedControl;
		}

		/// <summary>
		/// Gets the control to focus.
		/// </summary>
		[Obsolete("remove focused control. This is implementation specific for S3D and shouldn't be here.")]
		public Control FocusedControl { get; }

		/// <summary>
		/// Gets the property name.
		/// </summary>
		public string PropertyName { get; }

		/// <summary>
		/// Gets the property.
		/// </summary>
		private PropertyInfo Property { get; }

		/// <summary>
		/// Gets the new value to redo to.
		/// </summary>
		public T NewValue { get; set; }	// TODO: remove setter.

		/// <summary>
		/// Gets the old value to undo to.
		/// </summary>
		public T OldValue { get; }

		/// <summary>
		/// Gets a common description.
		/// </summary>
		public override string Description
		{
			get
			{
				return base.Description + string.Format(", {0} = {2}{1}{2}", PropertyName, NewValue, (OldValue is string) || (NewValue is string) ? "\"" : "");
			}
		}

		/// <summary>
		/// Executes the undo action.
		/// </summary>
		protected override void OnUndo()
		{
			Property.SetValue(AffectedInstance, OldValue);
		}

		/// <summary>
		/// Executes the redo action.
		/// </summary>
		protected override void OnRedo()
		{
			Property.SetValue(AffectedInstance, NewValue);
		}
	}
}