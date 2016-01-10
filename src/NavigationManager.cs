using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace skwas.Forms
{
	/// <summary>
	/// The navigation manager keeps track the current position in a list, used to provide history back/forward functionality. The forward history is reset every time a new object is added to the list.
	/// </summary>
	public sealed class NavigationManager<T>
		: ICollection<T>
		where T : class
	{
		/// <summary>
		/// The internal history.
		/// </summary>
		private List<T> History { get; } = new List<T>();

		/// <summary>
		/// Tracks the current position.
		/// </summary>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private int _index = -1;
		
		/// <summary>
		/// Adds a new item of <typeparamref name="T"/> to the navigation history, and resets the forward history.
		/// </summary>
		/// <param name="item"></param>
		public void Add(T item)
		{
			lock (SyncRoot)
			{
				// Don't add same item twice...
				if (_index >= 0 && CurrentItem.Equals(item))
					return;

				if (CanGoForward)
				{
					// Remove excess items.
					History.RemoveRange(_index + 1, Count - _index - 1);
				}
				History.Add(item);

				_index++;
			}
		}

		private object SyncRoot => ((ICollection) History).SyncRoot;

		/// <summary>
		/// Returns the current index.
		/// </summary>
		public int CurrentIndex => _index;

		/// <summary>
		/// Indicates whether the current index is not at the start.
		/// </summary>
		public bool CanGoBack => _index > 0;

		/// <summary>
		/// Indicates whether the current index is not at the end.
		/// </summary>
		public bool CanGoForward => _index < Count - 1;

		/// <summary>
		/// Moves back one place and returns the new active item or null if at the start.
		/// </summary>
		/// <returns>The new active item or null if at the start.</returns>
		public T Back()
		{
			lock (SyncRoot)
			{
				if (!CanGoBack) return default(T);

				_index--;
				if (_index >= Count) // Out of range.
				{
					return Back(); // Push back recursively until we hit a valid entry.
				}

				return CurrentItem;
			}
		}

		/// <summary>
		/// Moves forward one place and returns the new active item or null if at the end.
		/// </summary>
		/// <returns>The new active item or null if at the end.</returns>
		public T Forward()
		{
			lock (SyncRoot)
			{
				if (!CanGoForward) return default(T);

				_index++;
				return CurrentItem;
			}
		}

		/// <summary>
		/// Gets the current item.
		/// </summary>
		public T CurrentItem
		{
			get
			{
				lock (SyncRoot)
				{
					return _index == -1 ? default(T) : History[_index];
				}
			}
		}

		/// <summary>
		/// Clears the history.
		/// </summary>
		public void Clear()
		{
			lock (SyncRoot)
			{
				History.Clear();
				_index = -1;
			}
		}

		/// <summary>
		/// Returns true if the item is in the history one or more times.
		/// </summary>
		/// <param name="item">The item to check.</param>
		/// <returns>true if the <paramref name="item"/> is in the history list.</returns>
		public bool Contains(T item)
		{
			return History.Contains(item);
		}

		void ICollection<T>.CopyTo(T[] array, int arrayIndex)
		{
			History.CopyTo(array, arrayIndex);
		}
		
		/// <summary>
		/// Returns the size of the history.
		/// </summary>
		public int Count => History.Count;

		bool ICollection<T>.IsReadOnly => false;

		/// <summary>
		/// Removes all occurrences of the item from the history. If the item was active, the previous item is selected.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(T item)
		{
			if (History.IndexOf(item) == -1) return false;

			lock (SyncRoot)
			{
				while (true)
				{
					var index = History.IndexOf(item);
					if (index == -1) break;
					if (index == _index)
					{
						if (CanGoBack) Back();
					}
					History.Remove(item);

					// If current index was the last item, step back 1.
					if (_index == Count) _index--;
				}

				// After removal, same items can follow eachother up, f.ex.: 
				//		history => [ object A, object B, object A ]
				//		object B is removed
				//		history is now => [ object A, object A ]
				//		so we clean history to become => [ object A ]

				for (var i = History.Count - 1; i >= 1; i--)
				{
					if (Equals(History[i], History[i - 1]))
						History.RemoveAt(i);
				}
			}

			return true;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="NavigationManager{T}"/>.
		/// </summary>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return History.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the <see cref="NavigationManager{T}"/>.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return History.GetEnumerator();
		}
	}
}
