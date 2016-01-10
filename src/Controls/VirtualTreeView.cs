using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

// ReSharper disable once CheckNamespace
namespace skwas.Forms
{
	/// <summary>
	/// Displays a hierarchical collection of labeled items, each represented by a <see cref="VirtualTreeNode"/>. The tree nodes are not actually added into the actual tree view until needed (cached internally). This allows a large number of nodes to be added to the tree view very fast, as opposed to the stock <see cref="TreeView"/>.
	/// </summary>
	/// <remarks>All nodes are still required to be added to the tree view, the current implementation is not ment for 'dynamic' loading/binding.</remarks>
	public class VirtualTreeView
		: TreeView
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int _updateCounter;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		internal readonly VirtualTreeNode InternalRoot;

		/// <summary>
		/// Initializes a new instance of <see cref="VirtualTreeView"/>.
		/// </summary>
		public VirtualTreeView()
		{
			DrawMode = TreeViewDrawMode.OwnerDrawAll;
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);

			InternalRoot = new VirtualTreeNode {TreeView = this};
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.TreeView"/> and optionally releases the managed resources. 
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
			}
		}

		/// <summary>
		/// Disables any redrawing of the tree view.
		/// </summary>
		public new void BeginUpdate()
		{
			_updateCounter++;
			base.BeginUpdate();
		}

		/// <summary>
		/// Enables the redrawing of the tree view.
		/// </summary>
		public new void EndUpdate()
		{
			base.EndUpdate();
			_updateCounter--;
		}

		/// <summary>
		/// True when redrawing of the tree view is disabled.
		/// </summary>
		protected bool IsUpdating => _updateCounter > 0;

		/// <summary>
		/// Overrides <see cref="M:System.Windows.Forms.Control.OnHandleCreated(System.EventArgs)"/>.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (IsHandleCreated && VisualStyleInformation.IsSupportedByOS)
				SafeNativeMethods.SetWindowTheme(Handle, "explorer", null);
		}

		// TODO: move to VirtualTreeNodeCollection and support multiple in return set.
		/// <summary>
		/// Searches the treeview for a <paramref name="match"/>, optionally searching all child nodes.
		/// </summary>
		/// <param name="match">The function match.</param>
		/// <param name="searchAllChildren">true to search all child nodes.</param>
		/// <returns>Returns the first match.</returns>
		public VirtualTreeNode Find(Func<VirtualTreeNode, bool> match, bool searchAllChildren)
		{
			return Find(Nodes, match, searchAllChildren);
		}

		private static VirtualTreeNode Find(IEnumerable<VirtualTreeNode> nodes, Func<VirtualTreeNode, bool> match, bool searchAllChildren)
		{
			foreach (var node in nodes)
			{
				if (match.Invoke(node)) return node;
				if (!searchAllChildren) continue;

				var childFound = Find(node.Nodes, match, true);
				if (childFound != null) return childFound;
			}
			return null;
		}

		/// <summary>
		/// Overrides <see cref="M:System.Windows.Forms.Control.WndProc(System.Windows.Forms.Message@)"/>.
		/// </summary>
		/// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message"/> to process.</param>
		[DebuggerStepThrough]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case 0x0014:	// WM_ERASEBKGND
					m.Msg = 0;
					return;
			}
			base.WndProc(ref m);
		}

		/// <summary>
		/// Paints the background of the control.
		/// </summary>
		/// <param name="pevent">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains information about the control to paint. </param>
		[DebuggerStepThrough]
		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			//base.OnPaintBackground(pevent);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.TreeView.DrawNode"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.DrawTreeNodeEventArgs"/> that contains the event data. </param>
		[DebuggerStepThrough]
		protected override void OnDrawNode(DrawTreeNodeEventArgs e)
		{
			e.DrawDefault = true;
			base.OnDrawNode(e);
		}

		/// <summary>
		/// Retrieves the number of tree nodes, optionally including those in all subtrees, assigned to the tree view control.
		/// </summary>
		/// <param name="includeSubTrees">true to count the <see cref="VirtualTreeNode"/> items that the subtrees contain; otherwise, false. </param>
		/// <returns>
		/// The number of tree nodes, optionally including those in all subtrees, assigned to the tree view control.
		/// </returns>
		public new int GetNodeCount(bool includeSubTrees)
		{
			return GetNodeCount(Nodes, includeSubTrees);
		}

		/// <summary>
		/// Retrieves the number of tree nodes, optionally including those in all subtrees, assigned to the tree view control.
		/// </summary>
		/// <param name="nodes">The treenode collection to enumerate.</param>
		/// <param name="includeSubTrees">true to count the <see cref="VirtualTreeNode"/> items that the subtrees contain; otherwise, false. </param>
		/// <returns>
		/// The number of tree nodes, optionally including those in all subtrees, assigned to the tree view control.
		/// </returns>
		internal static int GetNodeCount(ICollection<VirtualTreeNode> nodes, bool includeSubTrees)
		{
			var cnt = nodes.Count;
			if (includeSubTrees)
				cnt += nodes.Sum(node => node.GetNodeCount(true));
			return cnt;
		}

		/// <summary>
		/// Gets or sets the tree node that is currently selected in the tree view control.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode SelectedNode
		{
			get { return (VirtualTreeNode)base.SelectedNode; }
			set 
			{
				value?.Nodes.LoadIntoTree();
				base.SelectedNode = value; 
			}
		}

		/// <summary>
		/// Gets the collection of tree nodes that are assigned to the tree view control.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Behavior")]
		public new VirtualTreeNodeCollection Nodes => InternalRoot.Nodes;

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.TreeView.BeforeExpand"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.TreeViewCancelEventArgs"/> that contains the event data. </param>
		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
			(e.Node as VirtualTreeNode)?.Nodes.LoadIntoTree();
			base.OnBeforeExpand(e);
		}
	}
}
