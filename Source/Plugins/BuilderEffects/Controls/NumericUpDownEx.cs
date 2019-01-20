// ============================================================================ '
// NumericUpDownEx - v.1.3                                                      '
// ============================================================================ '
// Author:   Claudio Nicora                                                     '
// WebSite:  http://coolsoft.altervista.org                                     '
// CodeProject: http://www.codeproject.com/KB/edit/NumericUpDownEx.aspx         '
// License:  CodeProject Open License                                           '
//           http://www.codeproject.com/info/cpol10.aspx                        '
// Feel free to contribute here: http://coolsoft.altervista.org                 '
// ============================================================================ '

using System;
using System.Reflection;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.BuilderEffects
{
	[DesignerCategory("code")]
	public class NumericUpDownEx : NumericUpDown
	{
		// reference to the underlying TextBox control
		private TextBox _textbox;

		// reference to the underlying UpDownButtons control
		private Control _upDownButtons;

		/// <summary>
		/// object creator
		/// </summary>
		public NumericUpDownEx()
		{
			// extract a reference to the underlying TextBox field
			_textbox = GetPrivateField<TextBox>(this, "upDownEdit");
			if(_textbox == null)
			{
				throw new ArgumentNullException(this.GetType().FullName + ": Can't find internal TextBox field.");
			}
			// extract a reference to the underlying UpDownButtons field
			_upDownButtons = GetPrivateField<Control>(this, "upDownButtons");
			if(_upDownButtons == null)
			{
				throw new ArgumentNullException(this.GetType().FullName + ": Can't find internal UpDown buttons field.");
			}
			// add handlers (MouseEnter and MouseLeave events of NumericUpDown
			// are not working properly)
			_textbox.MouseEnter += _mouseEnterLeave;
			_textbox.MouseLeave += _mouseEnterLeave;
			_upDownButtons.MouseEnter += _mouseEnterLeave;
			_upDownButtons.MouseLeave += _mouseEnterLeave;
			base.MouseEnter += _mouseEnterLeave;
			base.MouseLeave += _mouseEnterLeave;
		}


		/// <summary>
		/// Extracts a reference to a private underlying field
		/// </summary>
		protected static internal T GetPrivateField<T>(NumericUpDownEx ctrl, string fieldName) where T : Control
		{
			// find internal TextBox
			FieldInfo fi = typeof(NumericUpDown).GetField(fieldName, BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance);
			// take some caution... they could change field name in the future!
			if(fi == null)
			{
				return null;
			}
			else
			{
				return fi.GetValue(ctrl) as T;
			}
		}


		protected override void OnPaint(PaintEventArgs e)
		{
			if(_upDownButtons.Visible == false)
			{
				e.Graphics.Clear(this.BackColor);
			}
			base.OnPaint(e);
		}


		/// <summary>
		/// WndProc override to kill WN_MOUSEWHEEL message
		/// </summary>
		protected override void WndProc(ref Message m)
		{
			const int WM_MOUSEWHEEL = 0x20a;

			if(m.Msg == WM_MOUSEWHEEL)
			{
				switch(_interceptMouseWheel)
				{
					case InterceptMouseWheelMode.Always:
						// standard message
						base.WndProc(ref m);
						break;
					case InterceptMouseWheelMode.WhenMouseOver:
						if(_mouseOver)
						{
							// standard message
							base.WndProc(ref m);
						}
						break;
					case InterceptMouseWheelMode.Never:
						// kill the message
						return;
				}
			}
			else
			{
				base.WndProc(ref m);
			}

		}


#region New properties

		[DefaultValue(false)]
		[Category("Behavior")]
		[Description("Automatically select control text when it receives focus.")]
		public bool AutoSelect
		{
			get { return _autoSelect; }
			set { _autoSelect = value; }
		}

		private bool _autoSelect;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart
		{
			get { return _textbox.SelectionStart; }
			set { _textbox.SelectionStart = value; }
		}


		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionLength
		{
			get { return _textbox.SelectionLength; }
			set { _textbox.SelectionLength = value; }
		}


		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string SelectedText
		{
			get { return _textbox.SelectedText; }
			set { _textbox.SelectedText = value; }
		}


		[DefaultValue(typeof(InterceptMouseWheelMode), "Always")]
		[Category("Behavior")]
		[Description("Enables MouseWheel only under certain conditions.")]
		public InterceptMouseWheelMode InterceptMouseWheel
		{
			get { return _interceptMouseWheel; }
			set { _interceptMouseWheel = value; }
		}

		private InterceptMouseWheelMode _interceptMouseWheel = InterceptMouseWheelMode.Always;

		public enum InterceptMouseWheelMode
		{
			/// <summary>MouseWheel always works (defauld behavior)</summary>
			Always,
			/// <summary>MouseWheel works only when mouse is over the (focused) control</summary>
			WhenMouseOver,
			/// <summary>MouseWheel never works</summary>
			Never
		}


		[DefaultValue(typeof(ShowUpDownButtonsMode), "Always")]
		[Category("Behavior")]
		[Description("Set UpDownButtons visibility mode.")]
		public ShowUpDownButtonsMode ShowUpDownButtons
		{
			get { return _showUpDownButtons; }
			set
			{
				_showUpDownButtons = value;
				// update UpDownButtons visibility
				UpdateUpDownButtonsVisibility();
			}
		}

		private ShowUpDownButtonsMode _showUpDownButtons = ShowUpDownButtonsMode.Always;

		public enum ShowUpDownButtonsMode
		{
			/// <summary>UpDownButtons are always visible (defauld behavior)</summary>
			Always,
			/// <summary>UpDownButtons are visible only when mouse is over the control</summary>
			WhenMouseOver
		}

		/// <summary>
		/// If set, incrementing value will cause it to restart from Minimum 
		/// when Maximum is reached (and viceversa).
		/// </summary>
		[DefaultValue(false)]
		[Category("Behavior")]
		[Description("If set, incrementing value will cause it to restart from Minimum when Maximum is reached (and viceversa).")]
		public bool WrapValue
		{
			get { return _wrapValue; }
			set { _wrapValue = value; }
		}
		private bool _wrapValue;
	
#endregion


#region Text selection

		// select all the text on focus enter
		protected override void OnGotFocus(EventArgs e)
		{
			if(_autoSelect)
			{
				_textbox.SelectAll();
			}
			base.OnGotFocus(e);
		}


		// MouseUp will kill the SelectAll made on GotFocus.
		// Will restore it, but only if user have not made a partial text selection.
		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			if(_autoSelect && _textbox.SelectionLength == 0)
			{
				_textbox.SelectAll();
			}
			base.OnMouseUp(mevent);
		}

#endregion


#region Additional events

		// these events will be raised correctly, when mouse enters on the textbox
		public new event EventHandler<EventArgs> MouseEnter;
		public new event EventHandler<EventArgs> MouseLeave;

		// Events raised BEFORE value decrement(increment
		public event CancelEventHandler BeforeValueDecrement;
		public event CancelEventHandler BeforeValueIncrement;

		// flag to track mouse position
		private bool _mouseOver;

		// this handler is called at each mouse Enter/Leave movement
		private void _mouseEnterLeave(object sender, EventArgs e)
		{
			Rectangle cr = RectangleToScreen(ClientRectangle);
			Point mp = MousePosition;

			// actual state
			bool isOver = cr.Contains(mp);

			// test if status changed
			if(_mouseOver ^ isOver)
			{
				// update state
				_mouseOver = isOver;
				if(_mouseOver)
				{
					if(MouseEnter != null)
					{
						MouseEnter(this, EventArgs.Empty);
					}
				}
				else
				{
					if(MouseLeave != null)
					{
						MouseLeave(this, EventArgs.Empty);
					}
				}
			}

			// update UpDownButtons visibility
			if(_showUpDownButtons != ShowUpDownButtonsMode.Always)
			{
				UpdateUpDownButtonsVisibility();
			}

		}

#endregion


#region Value increment/decrement management

		// raises the two new events
		public override void DownButton()
		{
			CancelEventArgs e = new CancelEventArgs();
			if(BeforeValueDecrement != null) 
			{
				BeforeValueDecrement(this, e);
			}
			if(e.Cancel) return;

			if(_wrapValue && Value - Increment < Minimum) 
			{
				Value = Maximum;
			} 
			else 
			{
				base.DownButton();
			}
		}
		public override void UpButton()
		{
			CancelEventArgs e = new CancelEventArgs();
			if(BeforeValueIncrement != null) 
			{
				BeforeValueIncrement(this, e);
			}
			if(e.Cancel) return;

			if(_wrapValue && Value + Increment > Maximum) 
			{
				Value = Minimum;
			} 
			else 
			{
				base.UpButton();
			}
		}

#endregion


#region UpDownButtons visibility management

		/// <summary>
		/// Show or hide the UpDownButtons, according to ShowUpDownButtons property value
		/// </summary>
		public void UpdateUpDownButtonsVisibility()
		{
			// test new state
			bool newVisible;
			switch(_showUpDownButtons)
			{
				case ShowUpDownButtonsMode.WhenMouseOver:
					newVisible = _mouseOver;
					break;
				default:
					newVisible = true;
					break;
			}

			// assign only if needed
			if(_upDownButtons.Visible != newVisible)
			{
				if(newVisible)
				{
					_textbox.Width = this.ClientRectangle.Width - _upDownButtons.Width;
				}
				else
				{
					_textbox.Width = this.ClientRectangle.Width;
				}
				_upDownButtons.Visible = newVisible;
				OnTextBoxResize(_textbox, EventArgs.Empty);
				this.Invalidate();
			}

		}


		/// <summary>
		/// Custom textbox size management
		/// </summary>
		protected override void OnTextBoxResize(object source, EventArgs e)
		{
			if(_textbox == null)
				return;
			if(_showUpDownButtons == ShowUpDownButtonsMode.Always)
			{
				// standard management
				base.OnTextBoxResize(source, e);
			}
			else
			{
				// custom management

				// change position if RTL
				bool fixPos = this.RightToLeft == RightToLeft.Yes ^ this.UpDownAlign == LeftRightAlignment.Left;

				if(_mouseOver)
				{
					_textbox.Width = this.ClientSize.Width - _textbox.Left - _upDownButtons.Width - 2;
					if(fixPos)
						_textbox.Location = new Point(16, _textbox.Location.Y);
				}
				else
				{
					if(fixPos)
						_textbox.Location = new Point(2, _textbox.Location.Y);
					_textbox.Width = this.ClientSize.Width - _textbox.Left - 2;
				}

			}

		}

#endregion

	}
}
