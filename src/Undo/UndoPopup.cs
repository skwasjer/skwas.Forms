using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace skwas.Forms
{
	/// <summary>
	/// Displays a selectable list of undo/redo actions.
	/// </summary>
	public class UndoPopup
		: ToolStripControlHost
	{
		/// <summary>
		/// Raised when the popup is clicked or triggered via enter.
		/// </summary>
		public event EventHandler<UndoPopupClickEventArgs> UndoPopupClick;

		private UndoPopupMode _mode;
		private UndoManager _undoManager;

		private ListBox _listBox;
		private ToolTip _tooltip;

		#region .ctor/cleanup

		/// <summary>
		/// Initializes a new instance of <see cref="UndoPopup"/>.
		/// </summary>
		/// <param name="mode">The mode the popup shows actions for.</param>
		public UndoPopup(UndoPopupMode mode)
			: this(null, mode)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="UndoPopup"/>.
		/// </summary>
		/// <param name="undoManager">The undo manager used to populate the action list.</param>
		/// <param name="mode">The mode the popup shows actions for.</param>
		public UndoPopup(UndoManager undoManager, UndoPopupMode mode)
			: base(new Control())
		{
			// Set some defaults on the host control.
			Control.Margin = Padding.Empty;
			Control.Size = new Size(400, 300);
			Control.TabStop = false;

			// Add the listbox.
			_listBox = new ListBox
			{
				Dock = DockStyle.Fill,
				BorderStyle = BorderStyle.None,
				IntegralHeight = false,
				SelectionMode = SelectionMode.MultiSimple,
				HorizontalScrollbar = true
			};
			_listBox.MouseDown += ListBox_MouseDown;
			_listBox.MouseMove += ListBox_MouseMove;

			Control.Controls.Add(_listBox);

			// Create tooltip.
			_tooltip = new ToolTip();

			_mode = mode;
			UndoManager = undoManager;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ToolStripControlHost"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					// Unregister events.
					if (UndoManager != null) UndoManager = null;

					_listBox?.Dispose();
					_listBox = null;
					_tooltip?.Dispose();
					_tooltip = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets mode the popup shows actions for.
		/// </summary>
		public UndoPopupMode Mode
		{
			get { return _mode; }
			set
			{
				if (value == _mode) return;
				_mode = value;

				RefreshListBox();
			}
		}

		/// <summary>
		/// Gets or sets the undo manager used to populate the action list.
		/// </summary>
		public UndoManager UndoManager
		{
			get { return _undoManager; }
			set 
			{
				if (value == _undoManager) return;

				if (_undoManager != null)
				{
					_undoManager.Changed -= UndoManager_Changed;
				}

				_undoManager = value;

				if (_undoManager != null)
				{
					_undoManager.Changed += UndoManager_Changed;
				}

				RefreshListBox();
			}
		}

		/// <summary>
		/// Raises the <see cref="UndoPopupClick"/> event.
		/// </summary>
		/// <param name="e">The event args.</param>
		protected virtual void OnUndoPopupClick(UndoPopupClickEventArgs e)
		{
			UndoPopupClick?.Invoke(this, e);
		}

		private void ListBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				var count = _listBox.SelectedIndices.Count + 1;
				PerformClick();
				OnUndoPopupClick(new UndoPopupClickEventArgs(count, _mode));
			}
		}

		private void ListBox_MouseMove(object sender, MouseEventArgs e)
		{
			var lastSelectedIndex = _listBox.IndexFromPoint(e.X, e.Y);
			var indexForTooltip = lastSelectedIndex;
			if (lastSelectedIndex == -1)
			{
				_tooltip.RemoveAll();
				lastSelectedIndex = _listBox.Items.Count - 1;
			}

			if (lastSelectedIndex == _listBox.SelectedIndices.Count - 1) return;

			if (indexForTooltip == -1)
				_tooltip.RemoveAll();
			else
				_tooltip.SetToolTip(_listBox, _listBox.Items[indexForTooltip].ToString());

			_listBox.BeginUpdate();
			_listBox.SelectedIndices.Clear();

			for (var i = 0; i <= lastSelectedIndex; i++)
			{
				_listBox.SelectedIndices.Add(i);
			}

			_listBox.EndUpdate();
		}

		private void UndoManager_Changed(object sender, EventArgs e)
		{
			RefreshListBox();
		}

		/// <summary>
		/// Refreshes the actions in the list box.
		/// </summary>
		private void RefreshListBox()
		{
			if (UndoManager == null || _listBox == null) return;

			var undoMode = _mode == UndoPopupMode.Undo;
			var items = (undoMode ? UndoManager.UndoActions : UndoManager.RedoActions)
				.Select(action => undoMode ? action.UndoDescription : action.RedoDescription)
				.ToArray<object>();

			_listBox.Items.Clear();
			if (items.Length > 0)
			{
				_listBox.Items.AddRange(items);
				_listBox.SelectedIndex = items.Length > 0 ? 0 : -1;
			}
		}


		#region Overrides of ToolStripItem

		/// <summary>
		/// Gets the default margin of an item.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Windows.Forms.Padding"/> representing the margin.
		/// </returns>
		protected override Padding DefaultMargin { get; } = Padding.Empty;

		#endregion

		#region Overrides of ToolStripControlHost

		/// <summary>
		/// Processes a command key.
		/// </summary>
		/// <returns>
		/// false in all cases.
		/// </returns>
		/// <param name="m">A <see cref="T:System.Windows.Forms.Message"/>, passed by reference, that represents the window message to process. </param>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys"/> values that represents the key to process. </param>
		protected override bool ProcessCmdKey(ref Message m, Keys keyData)
		{
			if (m.Msg == NativeMethods.WM_KEYDOWN)
			{
				var indices = _listBox.SelectedIndices;
				switch (keyData)
				{
					case Keys.Down:
						if (indices.Count != _listBox.Items.Count)
							indices.Add(indices.Count);

						return true;

					case Keys.Up:
						if (indices.Count != 1) indices.Remove(indices.Count - 1);
						return true;
				}
			}
			return base.ProcessCmdKey(ref m, keyData);
		}

		#endregion

	}
}