using System;
using System.Drawing;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace skwas.Forms.Tests
{
	[TestClass]
	public class WindowTests
	{
		[TestMethod]
		public void it_gets_and_sets_window_styles()
		{
			var expected = WindowStyles.MaximizeBox | WindowStyles.ClipChildren | WindowStyles.ClipSiblings | WindowStyles.Visible | WindowStyles.Child;
			var expectedAfterRemove = expected & ~WindowStyles.MaximizeBox;
			var expectedAfterAdd = expectedAfterRemove | WindowStyles.Popup;

			using (var ctl = new TextBox())
			{
				var win = (Window) ctl;
				var ws = ctl.GetWindowStyles();
				var cs = ctl.GetControlStyles();
				cs.Should().NotBe(0, "because text boxes have control styles");

				ws.Should().Be(expected).And.Subject.Should().Be(win.WindowStyles);

				win.WindowStyles &= ~WindowStyles.MaximizeBox;

				ws = ctl.GetWindowStyles();
				ws.Should().Be(expectedAfterRemove, "because MaximizeBox should be removed").And.Subject.Should().Be(win.WindowStyles);

				win.WindowStyles |= WindowStyles.Popup;

				ws = ctl.GetWindowStyles();
				ws.Should().Be(expectedAfterAdd, "because Popup should be added").And.Subject.Should().Be(win.WindowStyles);

				win.ControlStyles.Should().Be(cs, "because control styles (lower 16 bits) should be left untouched");
			}
		}

		[TestMethod]
		public void it_will_ignore_invalid_window_styles()
		{
			var expected = WindowStyles.MaximizeBox | WindowStyles.ClipChildren | WindowStyles.ClipSiblings | WindowStyles.Visible | WindowStyles.Child;

			using (var ctl = new TextBox())
			{
				var ws = ctl.GetWindowStyles();
				ctl.SetWindowStyles(ws | (WindowStyles) 15);	// Add invalid flags.
				ctl.GetWindowStyles().Should().Be(expected);
			}
		}

		[TestMethod]
		public void it_gets_and_sets_control_styles()
		{
			var expected = 192;
			var expectedAfterRemove = expected & ~128;
			var expectedAfterAdd = expectedAfterRemove | 0xFFFF;

			using (var ctl = new TextBox())
			{
				var win = (Window)ctl;
				var ws = ctl.GetWindowStyles();
				var cs = ctl.GetControlStyles();
				cs.Should().Be(expected, "because text boxes have control styles");

				ws.Should().NotBe(0, "because text boxes have window styles");

				win.ControlStyles &= ~128;

				cs = ctl.GetControlStyles();
				cs.Should().Be(expectedAfterRemove, "because bit 8 should be removed").And.Subject.Should().Be(win.ControlStyles);

				win.ControlStyles |= 0xFFFF;

				cs = ctl.GetControlStyles();
				cs.Should().Be(expectedAfterAdd, "because Popup should be added").And.Subject.Should().Be(win.ControlStyles);

				win.WindowStyles.Should().Be(ws, "because window styles (upper 16 bits) should be left untouched");
			}
		}

		[TestMethod]
		public void it_will_ignore_invalid_control_styles()
		{
			var expected = 192;

			using (var ctl = new TextBox())
			{
				var cs = ctl.GetControlStyles();
				ctl.SetControlStyles(cs | (int)WindowStyles.MaximizeBox); // Add invalid flags.
				ctl.GetControlStyles().Should().Be(expected);
			}
		}

		[TestMethod]
		public void it_gets_and_sets_extended_window_styles()
		{
			var expected = ExtendedWindowStyles.ClientEdge;
			var expectedAfterRemove = expected & ~ExtendedWindowStyles.ClientEdge;
			var expectedAfterAdd = expectedAfterRemove | ExtendedWindowStyles.ContextHelp | ExtendedWindowStyles.RightScrollbar;

			using (var ctl = new TextBox())
			{
				var win = (Window)ctl;
				var ews = ctl.GetExtendedWindowStyles();

				ews.Should().Be(expected, "because text boxes have extended window styles").And.Subject.Should().Be(win.ExtendedWindowStyles);

				win.ExtendedWindowStyles &= ~ExtendedWindowStyles.ClientEdge;

				ews = ctl.GetExtendedWindowStyles();
				ews.Should().Be(expectedAfterRemove, "because ClientEdge should be removed").And.Subject.Should().Be(win.ExtendedWindowStyles);

				win.ExtendedWindowStyles |= ExtendedWindowStyles.ContextHelp | ExtendedWindowStyles.RightScrollbar;

				ews = ctl.GetExtendedWindowStyles();
				ews.Should().Be(expectedAfterAdd, "because ContextHelp and RightScrollBar should be added").And.Subject.Should().Be(win.ExtendedWindowStyles);
			}
		}

		[TestMethod]
		public void it_gets_location_in_screen_or_client_space()
		{
			using (var ctl = new Control())
			using (var form = new Form())
			{
				form.Controls.Add(ctl);
				form.StartPosition = FormStartPosition.Manual;
				form.Location = ctl.Location = new Point(100, 100);

				// For top level windows, it should be in screen space.
				var win = (Window)form;

				var loc = form.GetLocation();
				loc.Should().Be(form.Location);
				loc.Should().Be(win.Location);

				// For controls, it should be in client space.
				win = (Window) ctl;

				loc = ctl.GetLocation(form);	// Get relative to form.
				loc.Should().Be(ctl.Location);
				loc.Should().Be(win.Location);
			}
		}
	}
}
