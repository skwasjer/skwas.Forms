using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace skwas.Forms
{
	/// <summary>
	/// Represents a tree node that supports a large number of children but only loads them into the treeview on first expand.
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[Serializable]
	public class VirtualTreeNode
		: TreeNode
	{
		/// <summary>
		/// The name of a dummy node, used to force the native treeview to draw a + sign, indicating the node can be expanded.
		/// </summary>
		private static readonly string VirtualExpandDummy = typeof(VirtualTreeNode).FullName + "_##dummy_treenode##";

		private VirtualTreeNodeCollection _nodes;
		private bool _hasExpandedOnce;
		private VirtualTreeView _treeview;
		
		/// <summary>
		/// Initializes a new instance of <see cref="VirtualTreeNode"/>.
		/// </summary>
		public VirtualTreeNode()
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="VirtualTreeNode"/> with specified label text.
		/// </summary>
		/// <param name="text">The label text of the node.</param>
		public VirtualTreeNode(string text)
			: base(text)
		{
		}
		
		/// <summary>
		/// Gets whether the node is already loaded into the treeview.
		/// </summary>
		internal bool IsLoadedIntoTreeView => base.TreeView != null;

		/// <summary>
		/// Expands all the child tree nodes.
		/// </summary>
		public new void ExpandAll()
		{
			foreach (var child in Nodes)
				child.ExpandAll();

			base.ExpandAll();
		}

		/// <summary>
		/// Ensures that the tree node is visible, expanding tree nodes and scrolling the tree view control as necessary.
		/// </summary>
		public new void EnsureVisible()
		{
#if NEWTREEVIEW
			Nodes.LoadIntoTree();
#else
			if (!IsLoadedIntoTreeView) LoadParentVirtualNodes();
#endif
			base.EnsureVisible();
		}

		/// <summary>
		/// Gets the first child tree node in the tree node collection.
		/// </summary>
		/// <returns>
		/// The first child <see cref="VirtualTreeNode"/> in the <see cref="Nodes"/> collection.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode FirstNode => Nodes.Count > 0 ? Nodes[0] : null;

		/// <summary>
		/// Gets the last child tree node in the tree node collection.
		/// </summary>
		/// <returns>
		/// The last child <see cref="VirtualTreeNode"/> in the <see cref="Nodes"/> collection.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode LastNode => Nodes.Count > 0 ? Nodes[Nodes.Count - 1] : null;

		/// <summary>
		/// Gets the next sibling tree node.
		/// </summary>
		/// <returns>
		/// A <see cref="VirtualTreeNode"/> that represents the next sibling tree node.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode NextNode
		{
			get 
			{
				var index = Index;
				var nodes = Parent == null ? TreeView.Nodes : Parent.Nodes;
				return index < nodes.Count - 1 ? nodes[index + 1] : null;
			}
		}

		/// <summary>
		/// Gets the previous sibling tree node.
		/// </summary>
		/// <returns>
		/// A <see cref="VirtualTreeNode"/> that represents the previous sibling tree node.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode PrevNode
		{
			get
			{
				var index = Index;
				var nodes = Parent == null ? TreeView.Nodes : Parent.Nodes;
				return index > 0 ? nodes[index - 1] : null;
			}
		}

		/// <summary>
		/// Gets the next visible tree node.
		/// </summary>
		/// <returns>
		/// A <see cref="VirtualTreeNode"/> that represents the next visible tree node.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode NextVisibleNode
		{
			get
			{
#if NEWTREEVIEW
				Nodes.LoadIntoTree();
#else
				if (!IsLoadedIntoTreeView) LoadParentVirtualNodes();
#endif
				return (VirtualTreeNode)base.NextVisibleNode;
			}
		}

		/// <summary>
		/// Gets the previous visible tree node.
		/// </summary>
		/// <returns>
		/// A <see cref="VirtualTreeNode"/> that represents the previous visible tree node.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode PrevVisibleNode
		{
			get
			{
#if NEWTREEVIEW
				Nodes.LoadIntoTree();
#else
				if (!IsLoadedIntoTreeView) LoadParentVirtualNodes();
#endif
				return (VirtualTreeNode)base.PrevVisibleNode;
			}
		}

		/// <summary>
		/// Gets the parent tree node of the current tree node.
		/// </summary>
		/// <returns>
		/// A <see cref="VirtualTreeNode"/> that represents the parent of the current tree node.
		/// </returns>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeNode Parent { get; internal set; }

		/// <summary>
		/// Gets the parent tree view that the tree node is assigned to.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new VirtualTreeView TreeView
		{
			get { return _treeview; }
			internal set 
			{
				if (_treeview == value) return;
				_treeview = value;
#if !NEWTREEVIEW
				VirtualTreeView.SetParents(Nodes, value);
#endif
			}
		}

		/// <summary>
		/// Gets the collection of <see cref="VirtualTreeNode"/> objects assigned to the current tree node.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		[Category("Behavior")]
		public new VirtualTreeNodeCollection Nodes
		{
			get
			{
				if (_nodes == null)
				{
					_nodes = new VirtualTreeNodeCollection(this);
#if !NEWTREEVIEW
					_nodes.AfterInsert += _nodes_AfterInsert;
					_nodes.AfterSet += _nodes_AfterSet;
					_nodes.AfterRemove += _nodes_AfterRemove;
					_nodes.BeforeInsert += _nodes_BeforeInsert;
					_nodes.BeforeClear += _nodes_BeforeClear;
					_nodes.AfterClear += _nodes_AfterClear;
					_nodes.AfterMove += _nodes_AfterMove;
#endif
				}
				return _nodes;
			}
		}

		/// <summary>
		/// Gets the path from the root tree node to the current tree node.
		/// </summary>
		public new string FullPath
		{
			get 
			{
#if NEWTREEVIEW
				Parent?.Nodes.LoadIntoTree();
#else
				if (!IsLoadedIntoTreeView) LoadParentVirtualNodes();
#endif
				return base.FullPath;
			}
		}

		/// <summary>
		/// Gets the handle of the tree node.
		/// </summary>
		public new IntPtr Handle
		{
			get
			{
#if NEWTREEVIEW
				Parent?.Nodes.LoadIntoTree();
#else
				if (!IsLoadedIntoTreeView) LoadParentVirtualNodes();
#endif
				return base.Handle;
			}			
		}

		/// <summary>
		/// Gets the position of the tree node in the tree node collection.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int Index => Parent?.Nodes.IndexOf(this) ?? TreeView?.Nodes.IndexOf(this) ?? -1;

		/// <summary>
		/// Gets the zero-based depth of the tree node in the <see cref="VirtualTreeView"/> control.
		/// </summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new int Level
		{
			get
			{
				if (IsLoadedIntoTreeView)
					return base.Level;

				return Parent?.Level + 1 ?? 0;
			}
		}

		/// <summary>
		/// Returns the number of child tree nodes.
		/// </summary>
		public new int GetNodeCount(bool includeSubTrees)
		{
			return IsLoadedIntoTreeView 
				? base.GetNodeCount(includeSubTrees) 
				: VirtualTreeView.GetNodeCount(Nodes, includeSubTrees);
		}
	
		/// <summary>
		/// Removes the tree node from the tree node collection.
		/// </summary>
		public new void Remove()
		{
			if (Parent == null)
				TreeView.Nodes.Remove(this);
			else
				Parent.Nodes.Remove(this);
		}

#if !NEWTREEVIEW
		void _nodes_BeforeClear(object sender, EventArgs e)
		{
			VirtualTreeView.SetParents(Nodes);
		}

		void _nodes_AfterClear(object sender, EventArgs e)
		{
			if (_hasExpandedOnce)
				base.Nodes.Clear();
		}

		void _nodes_AfterSet(object sender, IO.SetEventArgs<VirtualTreeNode> e)
		{
			VirtualTreeView.SetParents(e.Value);
			VirtualTreeView.SetParents(e.NewValue, _treeview, this);

			if (_hasExpandedOnce)
				base.Nodes[e.Index] = e.NewValue;
		}

		void _nodes_AfterRemove(object sender, IO.InsertRemoveEventArgs<VirtualTreeNode> e)
		{
			if (_hasExpandedOnce)
				base.Nodes.Remove(e.Value);
			else if (_nodes.Count == 0)
			{
				// Remove + if it is set.
				if (base.FirstNode != null && base.FirstNode.Name == VirtualExpandDummy)
					base.FirstNode.Remove();
			}
			VirtualTreeView.SetParents(e.Value);
		}

		private void _nodes_BeforeInsert(object sender, IO.InsertRemoveEventArgs<VirtualTreeNode> e)
		{
			VirtualTreeView.SetParents(e.Value, _treeview, this);
		}


		void _nodes_AfterInsert(object sender, IO.InsertRemoveEventArgs<VirtualTreeNode> e)
		{
			if (_hasExpandedOnce)
				base.Nodes.Insert(e.Index, e.Value);
			else if (_nodes.Count == 1)
			{
				// This is the first added item. Add a + sign.
				base.Nodes.Add(VirtualExpandDummy, VirtualExpandDummy);
			}
		}

		void _nodes_AfterMove(object sender, IO.MoveEventArgs<VirtualTreeNode> e)
		{
			if (_hasExpandedOnce)
			{
				base.Nodes.RemoveAt(e.OldIndex);
				base.Nodes.Insert(e.NewIndex, e.Value);
			}
		}

		internal void LoadParentVirtualNodes()
		{
			if (Parent == null) return;
			Parent.LoadVirtualNodes();
			Parent.LoadParentVirtualNodes();
		}

		internal void LoadVirtualNodes()
		{
			if (_hasExpandedOnce) return;

			var interactive = TreeView != null && Nodes.Count > 50;
			var busyCursor = false;
			if (interactive)
			{
				busyCursor = Nodes.Count > 200;
				if (busyCursor) TreeView.Cursor = Cursors.AppStarting;
				TreeView.BeginUpdate();
			}

			try
			{
				// Remove dummy.
				base.Nodes.RemoveByKey(VirtualExpandDummy);

				// Copy nodes from the virtual collection to the underlying base collection.
				base.Nodes.AddRange(Nodes.ToArray<TreeNode>());
			}
			finally
			{
				_hasExpandedOnce = true;
				if (interactive)
				{
					TreeView.EndUpdate();
					if (busyCursor) TreeView.Cursor = Cursors.Default;
				}
			}
		}
#endif


		/// <summary>
		/// Saves the state of the <see cref="T:System.Windows.Forms.TreeNode"/> to the specified <see cref="T:System.Runtime.Serialization.SerializationInfo"/>. 
		/// </summary>
		/// <param name="si">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that describes the <see cref="T:System.Windows.Forms.TreeNode"/>.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that indicates the state of the stream during serialization</param>
		protected override void Serialize(SerializationInfo si, StreamingContext context)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Loads the state of the <see cref="T:System.Windows.Forms.TreeNode"/> from the specified <see cref="T:System.Runtime.Serialization.SerializationInfo"/>.
		/// </summary>
		/// <param name="serializationInfo">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that describes the <see cref="T:System.Windows.Forms.TreeNode"/>.</param>
		/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that indicates the state of the stream during deserialization.</param>
		protected override void Deserialize(SerializationInfo serializationInfo, StreamingContext context)
		{
			throw new NotSupportedException();
		}

		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString()
		{
			return GetType().Name + ": " + Text;
		}
	}
}