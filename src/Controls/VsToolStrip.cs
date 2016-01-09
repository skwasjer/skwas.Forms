using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms.VisualStyles;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace skwas.Forms
{
	/// <summary>
	/// Provides a container for Windows toolbar objects, and is drawn using a 3D like style, if visual styles is supported and enabled on the operating system.
	/// </summary>
	public class VsToolStrip
		: ToolStrip
	{
		private Rectangle _backgroundRect, _linearRect1, _linearRect2;

		private LinearGradientBrush _backgroundBrush1, _backgroundBrush2;
		private readonly bool _isSupported;

		#region .ctor/cleanup

		/// <summary>
		/// Initializes a new instance of <see cref="VsToolStrip"/>.
		/// </summary>
		public VsToolStrip()
		{
			var os = Environment.OSVersion;
			_isSupported = os.Platform == PlatformID.Win32NT && os.Version.Major >= 6;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="T:System.Windows.Forms.ToolStrip"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_backgroundBrush1")]
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_backgroundBrush1?.Dispose();
				_backgroundBrush1 = null;

				_backgroundBrush2.Dispose();
				_backgroundBrush2 = null;
			}

			base.Dispose(disposing);
		}

		#endregion

		/// <summary>
		/// Initialize paint objects.
		/// </summary>
		private void InitPaint()
		{
			_backgroundBrush1?.Dispose();
			_backgroundBrush2?.Dispose();

			// Can't create gradient brushes if we don't have a valid rect. Size.IsEmpty won't suffice, we need both height and width.
			if (_backgroundRect.Width == 0 || _backgroundRect.Height == 0) return;

			var color = SystemColors.ActiveCaption;

			_backgroundBrush1 = new LinearGradientBrush(_linearRect1, Color.FromArgb(20, color), Color.FromArgb(80, color), LinearGradientMode.Vertical);
			_backgroundBrush2 = new LinearGradientBrush(_linearRect2, Color.FromArgb(140, color), Color.FromArgb(60, color), LinearGradientMode.Vertical);
		}

		#region Overrides of Control

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.SystemColorsChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data. </param>
		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);

			InitPaint();
			Invalidate();
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the event data. </param>
		protected override void OnLayout(LayoutEventArgs e)
		{
			if (e.AffectedProperty == "Bounds" || e.AffectedProperty == "BackColor")
			{
				_backgroundRect = ClientRectangle;
				_linearRect1 = _backgroundRect;
				_linearRect1.Height = _backgroundRect.Height / 2 + 1;
				_linearRect2 = _backgroundRect;
				_linearRect2.Height -= _linearRect1.Height - 1;
				_linearRect2.Y = _linearRect1.Height - 1;

				if (_backgroundRect.Width > 0 && _backgroundRect.Height > 0)
				{
					InitPaint();
					Invalidate();
				}
			}

			base.OnLayout(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains the event data. </param>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			// Draw the bottom 2 pixels of the background again...
			var rc = _backgroundRect;
			rc.Y = rc.Bottom - 2;
			rc.Height = 2;
			e.Graphics.SetClip(rc);

			DrawBackground(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event for the <see cref="T:System.Windows.Forms.ToolStrip"/> background.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs"/> that contains information about the control to paint. </param>
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			if (!DrawBackground(e))
				base.OnPaintBackground(e);
		}

		#endregion

		/// <summary>
		/// Draws the background using a 3D like style, if visual styles is supported and enabled.
		/// </summary>
		/// <param name="e">The event args.</param>
		/// <returns>true if the background is drawn using <see cref="VisualStyleRenderer"/>, and false otherwise.</returns>
		private bool DrawBackground(PaintEventArgs e)
		{
			if (!_isSupported || !VisualStyleRenderer.IsSupported || !VisualStyleInformation.IsEnabledByUser) return false;

			e.Graphics.FillRectangle(Brushes.White, _backgroundRect);

			e.Graphics.FillRectangle(_backgroundBrush1, _linearRect1);
			e.Graphics.FillRectangle(_backgroundBrush2, _linearRect2);

			e.Graphics.DrawLine(SystemPens.ActiveCaption, 0, _backgroundRect.Bottom - 1, _backgroundRect.Width, _backgroundRect.Bottom - 1);

			return true;
		}

	}
}
