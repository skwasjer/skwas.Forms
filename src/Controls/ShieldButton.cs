using System;
using System.ComponentModel;
using System.Drawing;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace skwas.Forms
{
	/// <summary>
	/// Represents a button that displays a shield to indicate elevated permissions are required. The shield is only displayed when the user does not have Administrator permissions.
	/// </summary>
	/// <remarks>Requires the <see cref="P:ShieldButton.FlatStyle"/> to be set to <see cref="F:FlatStyle.System"/></remarks>
	[ToolboxItem(true)]
	[ToolboxBitmap(typeof(ShieldButton), "Resources.ShieldButton.bmp")]
	public class ShieldButton
		: Button
	{		
		/// <summary>
		/// Initializes a new instance of <see cref="ShieldButton"/>.
		/// </summary>
		public ShieldButton()
		{
			if (RequiresElevation && FlatStyle != FlatStyle.System)
			{
				// Force the system style.
				FlatStyle = FlatStyle.System;
			}
		}

		/// <summary>
		/// Gets whether elevation is required for the current principal.
		/// </summary>
		public bool RequiresElevation
		{
			get
			{				
				var identity = WindowsIdentity.GetCurrent();
				var principal = new WindowsPrincipal(identity ?? WindowsIdentity.GetAnonymous());
				return !principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.HandleCreated"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
			
			if (RequiresElevation)
				UnsafeNativeMethods.SendMessage(Handle, NativeMethods.BCM_SETSHIELD, IntPtr.Zero, new IntPtr(-1));
		}
	}
}
