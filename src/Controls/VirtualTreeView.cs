using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace skwas.Forms
{
	/// <summary>
	/// Displays a hierarchical collection of labeled items, each represented by a <see cref="VirtualTreeNode"/>.
	/// </summary>
	public class VirtualTreeView
		: TreeView
	{
		private int _updateCounter;
//		private VirtualTreeNodeCollection _nodes;
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
#if !NEWTREEVIEW
				if (_nodes != null)
				{
					_nodes.AfterInsert -= _nodes_AfterInsert;
					_nodes.AfterSet -= _nodes_AfterSet;
					_nodes.AfterRemove -= _nodes_AfterRemove;
					_nodes.BeforeClear -= _nodes_BeforeClear;
					_nodes.AfterClear -= _nodes_AfterClear;
					_nodes.AfterMove -= _nodes_AfterMove;
				}
#endif
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
#if NEWTREEVIEW
				value?.Nodes.LoadIntoTree();
#else
				if (value != null && !value.IsLoadedIntoTreeView) value.LoadParentVirtualNodes();
#endif
				base.SelectedNode = value; 
			}
		}

		/// <summary>
		/// Gets the collection of tree nodes that are assigned to the tree view control.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Behavior")]
		public new VirtualTreeNodeCollection Nodes
		{
			get
			{
				return InternalRoot.Nodes;
#if !NEWTREEVIEW
				if (_nodes == null)
				{
					_nodes = new VirtualTreeNodeCollection(_root);
					_nodes.AfterInsert += _nodes_AfterInsert;
					_nodes.AfterSet += _nodes_AfterSet;
					_nodes.AfterRemove += _nodes_AfterRemove;
					_nodes.BeforeClear += _nodes_BeforeClear;
					_nodes.AfterClear += _nodes_AfterClear;
					_nodes.AfterMove += _nodes_AfterMove;
			}
				return _nodes;
#endif
			}
		}

#if !NEWTREEVIEW
		void _nodes_BeforeClear(object sender, EventArgs e)
		{
			SetParents(Nodes);
		}

		void _nodes_AfterClear(object sender, EventArgs e)
		{
			base.Nodes.Clear();
		}

		void _nodes_AfterSet(object sender, IO.SetEventArgs<VirtualTreeNode> e)
		{
			SetParents(e.Value);
			SetParents(e.NewValue, this);
			base.Nodes[e.Index] = e.NewValue;
		}

		void _nodes_AfterRemove(object sender, IO.InsertRemoveEventArgs<VirtualTreeNode> e)
		{
			SetParents(e.Value);
			base.Nodes.Remove(e.Value);
		}

		void _nodes_AfterInsert(object sender, IO.InsertRemoveEventArgs<VirtualTreeNode> e)
		{
			SetParents(e.Value, this);
			base.Nodes.Insert(e.Index, e.Value);
		}

		void _nodes_AfterMove(object sender, IO.MoveEventArgs<VirtualTreeNode> e)
		{
			base.Nodes.RemoveAt(e.OldIndex);
			base.Nodes.Insert(e.NewIndex, e.Value);
		}
#endif

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.TreeView.BeforeExpand"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.TreeViewCancelEventArgs"/> that contains the event data. </param>
		protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
		{
#if NEWTREEVIEW
			(e.Node as VirtualTreeNode)?.Nodes.LoadIntoTree();
#else
			(e.Node as VirtualTreeNode)?.LoadVirtualNodes();
#endif
			base.OnBeforeExpand(e);
		}


#if !NEWTREEVIEW
		/// <summary>
		/// Assigns a new treeview and parent node to specified nodes.
		/// </summary>
		/// <param name="node">The node that is assigned to the treeview.</param>
		/// <param name="treeview">The treeview or null to remove the treeview reference.</param>
		/// <param name="parentNode">The parent node.</param>
		internal static void SetParents(VirtualTreeNode node, VirtualTreeView treeview = null, VirtualTreeNode parentNode = null)
		{
			SetParents(new[] {node}, treeview, parentNode);
		}

		/// <summary>
		/// Assigns a new treeview and parent node to specified nodes.
		/// </summary>
		/// <param name="nodes">The nodes that are assigned to the treeview.</param>
		/// <param name="treeview">The treeview or null to remove the treeview reference.</param>
		/// <param name="parentNode">The parent node.</param>
		internal static void SetParents(IEnumerable<VirtualTreeNode> nodes, VirtualTreeView treeview = null, VirtualTreeNode parentNode = null)
		{
			if (nodes == null) return;
			foreach (var node in nodes)
			{
				node.TreeView = treeview;
				node.Parent = parentNode;
			}
		}
#endif
	}
}
