using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;

namespace skwas.Forms
{
	public class VirtualTreeNodeCollection
		: ObservableCollection<VirtualTreeNode>
	{
		/// <summary>
		/// The name of a dummy node, used to force the native treeview to draw a + sign, indicating the node can be expanded.
		/// </summary>
		private static readonly string VirtualExpandDummy = typeof(VirtualTreeNodeCollection).FullName + "_##dummy_treenode##";

		private readonly VirtualTreeNode _owner;
		private TreeNodeCollection _baseCollection;
		private bool _hasExpandedOnce;

		internal VirtualTreeNodeCollection(VirtualTreeNode owner)
		{
			_owner = owner;
		}

		public VirtualTreeNode Add(string text)
		{
			var node = new VirtualTreeNode(text);
			Add(node);
			return node;
		}

		public void Move(VirtualTreeNode node, int offset)
		{
			var oldIndex = IndexOf(node);
			Move(oldIndex, oldIndex + offset);
		}

		/// <summary>
		/// Gets whether the node is already loaded into the treeview.
		/// </summary>
		bool IsLoadedIntoTreeView => ((TreeNode)_owner).TreeView != null;

		internal void LoadIntoTree()
		{
			var nodes = (_owner.Parent?.Nodes ?? _owner.TreeView.Nodes);
			if (nodes != null && nodes != this)
				nodes.LoadIntoTree();

//			if (IsLoadedIntoTreeView) return;
			if (_hasExpandedOnce) return;

			var interactive = IsLoadedIntoTreeView && Count > 50;
			var busyCursor = false;
			if (interactive)
			{
				busyCursor = Count > 200;
				if (busyCursor) _owner.TreeView.Cursor = Cursors.AppStarting;
				_owner.TreeView.BeginUpdate();
			}

			_baseCollection = _owner == _owner.TreeView.InternalRoot
				? ((TreeView)_owner.TreeView).Nodes 
				: ((TreeNode)_owner).Nodes;
			try
			{

				// Remove dummy.
				_baseCollection.RemoveByKey(VirtualExpandDummy);

				// Copy nodes from the virtual collection to the underlying base collection.
				_baseCollection.AddRange(this.ToArray<TreeNode>());

				SetParents(this, _owner.TreeView, _owner);
				foreach (var node in this)
					node.Nodes.AddDummy();
			}
			finally
			{
				_hasExpandedOnce = true;
				if (interactive)
				{
					_owner.TreeView.EndUpdate();
					if (busyCursor) _owner.TreeView.Cursor = Cursors.Default;
				}
			}

		}
		
		private bool IsRootCollection => _owner.Parent == null && _owner == _owner.TreeView?.InternalRoot;

		private void AddDummy()
		{
			// This is the first added item. Add a + sign.
			var dummy = new VirtualTreeNode(VirtualExpandDummy) { Name = VirtualExpandDummy };
			_baseCollection = IsRootCollection ? ((TreeView)_owner.TreeView)?.Nodes : ((TreeNode)_owner).Nodes;
			if (Count > 0 && (_baseCollection?.Count ?? -1) == 0)
				_baseCollection?.Add(dummy);
		}

		#region Overrides of ObservableCollection<VirtualTreeNode>

			/// <summary>
			/// Inserts an element into the <see cref="T:System.Collections.ObjectModel.Collection`1"/> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert. The value can be null for reference types.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void InsertItem(int index, VirtualTreeNode item)
		{
			if (item == null)
				throw new ArgumentNullException(nameof(item));

			SetParents(item, _owner.TreeView, _owner);
			base.InsertItem(index, item);

			if (_hasExpandedOnce)
				_baseCollection.Insert(index, item);
			else if (Count == 1) AddDummy();
		}

		/// <summary>
		/// Replaces the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to replace.</param><param name="item">The new value for the element at the specified index. The value can be null for reference types.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void SetItem(int index, VirtualTreeNode item)
		{
			SetParents(this[index]);
			base.SetItem(index, item);
			SetParents(item, _owner.TreeView, _owner);

			if (_hasExpandedOnce)
				_baseCollection[index] = item;
		}

		protected override void MoveItem(int oldIndex, int newIndex)
		{
			var item = this[oldIndex];
			base.MoveItem(oldIndex, newIndex);
			if (_hasExpandedOnce)
			{
				_baseCollection.RemoveAt(oldIndex);
				_baseCollection.Insert(newIndex, item);
			}
		}

		/// <summary>
		/// Removes the element at the specified index of the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is less than zero.-or-<paramref name="index"/> is equal to or greater than <see cref="P:System.Collections.ObjectModel.Collection`1.Count"/>.</exception>
		protected override void RemoveItem(int index)
		{
			var removedItem = this[index];
			base.RemoveItem(index);

			if (_hasExpandedOnce)
				_baseCollection.Remove(removedItem);
			else if (Count == 0)
			{
				// Remove + if it is set.
				TreeNode firstNode;
				if (_baseCollection.Count > 0 && (firstNode = _baseCollection[0]).Name == VirtualExpandDummy)
					firstNode.Remove();
			}
			SetParents(removedItem);
		}


		/// <summary>
		/// Removes all elements from the <see cref="T:System.Collections.ObjectModel.Collection`1"/>.
		/// </summary>
		protected override void ClearItems()
		{
			SetParents(this);

			base.ClearItems();

			if (_hasExpandedOnce) _baseCollection.Clear();
			_hasExpandedOnce = false;
		}

		#endregion

		/// <summary>
		/// Assigns a new treeview and parent node to specified nodes.
		/// </summary>
		/// <param name="node">The node that is assigned to the treeview.</param>
		/// <param name="treeview">The treeview or null to remove the treeview reference.</param>
		/// <param name="parentNode">The parent node.</param>
		void SetParents(VirtualTreeNode node, VirtualTreeView treeview = null, VirtualTreeNode parentNode = null)
		{
			SetParents(new[] { node }, treeview, parentNode);
		}

		/// <summary>
		/// Assigns a new treeview and parent node to specified nodes.
		/// </summary>
		/// <param name="nodes">The nodes that are assigned to the treeview.</param>
		/// <param name="treeview">The treeview or null to remove the treeview reference.</param>
		/// <param name="parentNode">The parent node.</param>
		void SetParents(IEnumerable<VirtualTreeNode> nodes, VirtualTreeView treeview = null, VirtualTreeNode parentNode = null)
		{
			if (nodes == null) return;
			foreach (var node in nodes)
			{
				node.TreeView = treeview;
				node.Parent = (parentNode as TreeNode)?.Parent == null && IsRootCollection ? null : parentNode; 
			}
		}
	}
}