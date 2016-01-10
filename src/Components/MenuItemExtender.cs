using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// ReSharper disable once CheckNamespace
namespace skwas.Forms
{
	/// <summary>
	/// Provides ImageList/ImageIndex support to the <see cref="MainMenu"/> and <see cref="ContextMenu"/> components.
	/// </summary>
	[ProvideProperty("ImageIndex", typeof(MenuItem))]
	[ToolboxItem(true)]
	public class MenuItemExtender
		: Component, IExtenderProvider 
	{
		/// <summary>
		/// Tracks menu items and the associated item info (image index, native handle).
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly Dictionary<MenuItem, ItemInfo> _items = new Dictionary<MenuItem, ItemInfo>();

		/// <summary>
		/// The image list to use.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private ImageList _imageList;

		/// <summary>
		/// true when images are supported on menu items.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private readonly bool _isSupported;

		#region .ctor/cleanup

		/// <summary>
		/// Initializes a new instance of the MenuItemExtender class.
		/// </summary>
		public MenuItemExtender()
		{
			var os = Environment.OSVersion;
			_isSupported = os.Version.Major >= 6 && os.Platform == PlatformID.Win32NT;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Component"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
		protected override void Dispose(bool disposing)
		{
			foreach (var info in _items.Values)
				RemoveMenuItem(info.Item);

			base.Dispose(disposing);
		}

		#endregion

		#region Component properties

		/// <summary>
		/// Gets or sets the <see cref="System.Windows.Forms.ImageList"/> to use.
		/// </summary>
		[DefaultValue(null)]
		public ImageList ImageList
		{
			get { return _imageList; }
			set 
			{
				if (value == _imageList) return;
				_imageList = value; 

				// Update each menu item.
				foreach (var item in _items.Keys)
					SetBitmap(item);
			}
		}

		#endregion

		#region Implementation of IExtenderProvider 

		/// <summary>
		/// Specifies whether this object can provide its extender properties to the specified object.
		/// </summary>
		/// <returns>
		/// true if this object can provide extender properties to the specified object; otherwise, false.
		/// </returns>
		/// <param name="extendee">The <see cref="T:System.Object"/> to receive the extender properties. </param>
		protected virtual bool CanExtend(object extendee)
		{
			return extendee is MenuItem;
		}

		/// <summary>
		/// Specifies whether this object can provide its extender properties to the specified object.
		/// </summary>
		/// <returns>
		/// true if this object can provide extender properties to the specified object; otherwise, false.
		/// </returns>
		/// <param name="extendee">The <see cref="T:System.Object"/> to receive the extender properties. </param>
		bool IExtenderProvider.CanExtend(object extendee)
		{
			return CanExtend(extendee);
		}

		#region Extender properties

		/// <summary>
		/// Gets the image index, if any, associated with specified menu item.
		/// </summary>
		/// <param name="menuItem"></param>
		/// <returns></returns>
		[DefaultValue(-1)]
		public int GetImageIndex(MenuItem menuItem)
		{
			if (_items.ContainsKey(menuItem))
				return _items[menuItem].ImageIndex;
			return -1;
		}

		/// <summary>
		/// Sets the image index for specified menu item.
		/// </summary>
		/// <param name="menuItem"></param>
		/// <param name="value"></param>
		public void SetImageIndex(MenuItem menuItem, int value)
		{
			if (value < 0)
				RemoveMenuItem(menuItem);
			else
				AddMenuItem(menuItem, value); 
		}

		#endregion

		#endregion

		private void AddMenuItem(MenuItem item, int imageIndex)
		{
			if (_items.ContainsKey(item))
			{
				var info = _items[item];
				// Reset previous used bitmap handle.
				info.NativeHandle = IntPtr.Zero;
				info.ImageIndex = imageIndex;
			}
			else
			{
				_items.Add(item, new ItemInfo(item, imageIndex, IntPtr.Zero));
				// Hook up to Disposed event. We want to remove menu items that get disposed, otherwise we have a leak.
				item.Disposed += menuItem_Disposed;
			}

			SetBitmap(item);
		}

		private void RemoveMenuItem(MenuItem item)
		{
			if (!_items.ContainsKey(item)) return;

			var info = _items[item];
			// Make sure to set ImageIndex to -1. This destroys the native bitmap handle. Otherwise, we have a leak.
			info.ImageIndex = -1;
			SetBitmap(item);
			_items.Remove(item);
			item.Disposed -= menuItem_Disposed;
		}

		/// <summary>
		/// Sets the native bitmap handle on a menu item.
		/// </summary>
		/// <param name="item"></param>
		private void SetBitmap(MenuItem item)
		{
			if (!_isSupported || DesignMode || _imageList == null) return;

			var itemInfo = _items[item];
			if (itemInfo.ImageIndex >= 0 && itemInfo.ImageIndex < _imageList.Images.Count && itemInfo.NativeHandle == IntPtr.Zero)
			{
				using (var bmp = (Bitmap)_imageList.Images[itemInfo.ImageIndex])
					itemInfo.NativeHandle = bmp.GetHbitmap(Color.FromArgb(0, 0, 0, 0));
			}

			UpdateBitmap(itemInfo);
		}


		/// <summary>
		/// Updates a menu item with the native bitmap handle.
		/// </summary>
		/// <param name="info"></param>
		private void UpdateBitmap(ItemInfo info)
		{
			if (!_isSupported) return;

			if (info.Item.Parent == null) return;

			if (!SafeNativeMethods.SetMenuItemBitmaps(info.Item.Parent.Handle, info.Item.Parent.MenuItems.IndexOf(info.Item), NativeMethods.MF_BYPOSITION, info.NativeHandle, info.NativeHandle))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		/// <summary>
		/// Updates the bitmap on all tracked menu items.
		/// </summary>
		public void Refresh()
		{
			foreach (var itemInfo in _items.Values)
				UpdateBitmap(itemInfo);
		}


		/// <summary>
		/// Called when a menu item is disposed. Removes the item from our collection.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void menuItem_Disposed(object sender, EventArgs e)
		{
			RemoveMenuItem((MenuItem)sender);

			// Problem: once a menu item is disposed, other menu items that belong to the same menu seem to loose their icon. This is probably since the menu is recreated. So lets refresh here (no need to recreate bitmap handles, just resend the handle).
			Refresh();
		}


		private class ItemInfo
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private int _imageIndex;
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			private IntPtr _nativeHandle;

			public ItemInfo(MenuItem item, int imageIndex, IntPtr nativeHandle)
			{
				Item = item;
				_imageIndex = imageIndex;
				_nativeHandle = nativeHandle;
			}

			/// <summary>
			/// Gets or sets the menu item.
			/// </summary>
			public MenuItem Item { get; set; }

			/// <summary>
			/// Gets or sets the image index.
			/// </summary>
			public int ImageIndex
			{
				get { return _imageIndex; }
				set
				{
					if (_imageIndex == value) return;
					_imageIndex = value;

					// If image index is invalid, delete the native handle.
					if (value < 0 && _nativeHandle != IntPtr.Zero)
						NativeHandle = IntPtr.Zero;
				}
			}

			/// <summary>
			/// The native bitmap handle of the icon.
			/// </summary>
			public IntPtr NativeHandle
			{
				get { return _nativeHandle; }
				set
				{
					if (_nativeHandle == value) return;
					if (_nativeHandle != IntPtr.Zero)
						UnsafeNativeMethods.DeleteObject(_nativeHandle);
					_nativeHandle = value;
				}
			}
		}
	}
}
