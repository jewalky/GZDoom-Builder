
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion

#region ================== Namespaces

using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	public class NumericTextbox : AutoSelectTextbox
	{
		#region ================== Constants

		private const int ROUNDING_PRECISION = 4; //mxd

		#endregion

		#region ================== Variables

		private bool allownegative;		// Allow negative numbers
		private bool allowrelative;		// Allow ++, --, * and / prefix for relative changes
		private bool allowdecimal;		// Allow decimal (float) numbers
		private bool allowexpressions;  // mxd/mgr_inz_rafal. Allow expressions
		private bool controlpressed;
		private int incrementstep; //mxd. Step for +++ and  --- prefixes
		private ToolTip tooltip; //mxd

		//mxd. Used to compute expressions
		private static DataTable datatable = new DataTable();
		
		#endregion

		#region ================== Properties

		public bool AllowNegative { get { return allownegative; } set { allownegative = value; } }
		public bool AllowRelative { get { return allowrelative; } set { allowrelative = value; UpdateTextboxStyle(); } }
		public bool AllowDecimal  { get { return allowdecimal; } set { allowdecimal = value; } }
		public bool AllowExpressions { get { return allowexpressions; } set { allowexpressions = value; } } //mxd/mgr_inz_rafal

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		public NumericTextbox()
		{
			this.ImeMode = ImeMode.Off;
			this.incrementstep = 1; //mxd

			//mxd. Setup tooltip
			this.tooltip = new ToolTip { AutomaticDelay = 100, AutoPopDelay = 8000, InitialDelay = 100, ReshowDelay = 100 };
		}

		//mxd
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				tooltip.Dispose();
				tooltip = null;
			}
			base.Dispose(disposing);
		}

		#endregion

		#region ================== Methods

		// Key pressed
		protected override void OnKeyDown(KeyEventArgs e)
		{
			controlpressed = e.Control;
			base.OnKeyDown(e);
		}

		// Key released
		protected override void OnKeyUp(KeyEventArgs e)
		{
			controlpressed = e.Control;
			base.OnKeyUp(e);
		}
		
		// When a key is pressed
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			incrementstep = 1; //mxd
			string allowedchars = "0123456789\b";
			
			// Determine allowed chars
			if(allownegative) allowedchars += CultureInfo.CurrentCulture.NumberFormat.NegativeSign;
			if(allowrelative) allowedchars += "+-*/"; //mxd
			if(allowexpressions)
			{
				allowedchars += "()"; //mxd/mgr_inz_rafal
				if(!allowrelative) allowedchars += "+-*/"; //mxd
			}
			if(controlpressed) allowedchars += "\u0018\u0003\u0016";
			if(allowdecimal || allowexpressions || this.Text.StartsWith("*") || this.Text.StartsWith("/")) //mxd
				allowedchars += CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			
			// Check if key is not allowed
			if(allowedchars.IndexOf(e.KeyChar) == -1)
			{
				// Cancel this
				e.Handled = true;
			}
			else if(!allowexpressions)
			{
				//mxd. Check if * or / is pressed
				if(e.KeyChar == '*' || e.KeyChar == '/') 
				{
					if(this.SelectionStart - 1 > -1) e.Handled = true; //only valid when at the start of the text
				}
				// Check if + or - is pressed
				else if((e.KeyChar == '+') || (e.KeyChar == '-'))
				{
					string nonselectedtext;
					
					// Determine non-selected text
					if(this.SelectionLength > 0)
					{
						nonselectedtext = this.Text.Substring(0, this.SelectionStart) +
							this.Text.Substring(this.SelectionStart + this.SelectionLength);
					}
					else if(this.SelectionLength < 0)
					{
						nonselectedtext = this.Text.Substring(0, this.SelectionStart + this.SelectionLength) +
							this.Text.Substring(this.SelectionStart);
					}
					else
					{
						nonselectedtext = this.Text;
					}
					
					// Not at the start?
					int selectionpos = this.SelectionStart - 1;
					if(this.SelectionLength < 0) selectionpos = (this.SelectionStart + this.SelectionLength) - 1;
					if(selectionpos > -1)
					{
						// Find any other characters before the insert position
						string textpart = this.Text.Substring(0, selectionpos + 1);
						textpart = textpart.Replace("+", "");
						textpart = textpart.Replace("-", "");
						if(textpart.Length > 0)
						{
							// Cancel this
							e.Handled = true;
						}
					}

					// Determine other prefix
					char otherprefix = (e.KeyChar == '+' ? '-' : '+');
					
					// Limit the number of + and - allowed
					int numprefixes = nonselectedtext.Split(e.KeyChar, otherprefix).Length;
					if(numprefixes > 3)
					{
						// Can't have more than 3 prefixes (mxd)
						e.Handled = true;
					}
					else if(numprefixes > 1)
					{
						// Must have 2 or 3 same prefixes
						if(this.Text.IndexOf(e.KeyChar) == -1) e.Handled = true;

						// Double or triple prefix must be allowed
						if(!allowrelative) e.Handled = true;
					}
				}
			}
			
			// Call base
			base.OnKeyPress(e);
		}

		//mxd
		protected override void OnTextChanged(EventArgs e)
		{
			// Validate expression
			if(allowexpressions)
			{
				// Check if expression is valid. We may want "++" and "--" on their own...
				if(IsValidResult(StripPrefixes(this.Text)) || this.Text == "++" || this.Text == "--")
					this.ForeColor = (allowrelative ? SystemColors.HotTrack : SystemColors.WindowText);
				else
					this.ForeColor = Color.DarkRed;
			}
			
			base.OnTextChanged(e);
		}

		// Validate contents
		protected override void OnValidating(CancelEventArgs e)
		{
			//mxd. We may want "++" and "--" on their own...
			if(allowrelative && (this.Text == "++" || this.Text == "--"))
			{
				// Call base and bail out
				base.OnValidating(e);
				return;
			}

			if(allowexpressions) //mxd
			{
				if(!IsValidResult(StripPrefixes(this.Text)))
				{
					// Make the textbox empty
					this.Text = "";
				}
			}
			else
			{
				// Strip prefixes
				string textpart = this.Text.Replace("+", "").Replace("*", "").Replace("/", ""); //mxd
				if(!allownegative)
					textpart = textpart.Replace("-", "");

				// No numbers left?
				if(textpart.Length == 0)
				{
					// Make the textbox empty
					this.Text = "";
				}
			}
			
			// Call base
			base.OnValidating(e);
		}

		//mxd
		private string StripPrefixes(string input)
		{
			if(allowrelative)
			{
				// Strip prefixes
				if(input.StartsWith("+++") || input.StartsWith("---")) return input.Substring(3);
				if(input.StartsWith("++") || input.StartsWith("--")) return input.Substring(2);
				if(input.StartsWith("*") || input.StartsWith("/")) return input.Substring(1);
			}

			return input;
		}
		
		// This checks if the number is relative
		public bool CheckIsRelative()
		{
			// Prefixed with +++, ---, ++, --, * or /?
			return ( (this.Text.Length > 3 && (this.Text.StartsWith("+++") || this.Text.StartsWith("---"))) || //mxd
					 (this.Text.Length > 2 && (this.Text.StartsWith("++") || this.Text.StartsWith("--") )) || //mxd
					 (this.Text.Length > 1 && (this.Text.StartsWith("*") || this.Text.StartsWith("/"))) ); //mxd
		}
		
		//mxd. This determines the result value
		public int GetResult(int original)
		{
			return GetResult(original, incrementstep++);
		}

		//mxd. This determines the result value
		public int GetResult(int original, int step)
		{
			return (int)Math.Round(GetResultFloat(original, step));
		}

		//mxd. This determines the result value
		public float GetResultFloat(float original)
		{
			return GetResultFloat(original, incrementstep++);
		}

		// This determines the result value
		public float GetResultFloat(float original, int step)
		{
			// Strip prefixes
			string textpart = StripPrefixes(this.Text);

			// Any numbers left?
			if(textpart.Length > 0)
			{
				float result;
				if(allowrelative)
				{
					//mxd. Prefixed with +++?
					if(this.Text.StartsWith("+++"))
					{
						// Add number to original
						if(TryGetResultValue(textpart, out result))
							return original + result * step;

						// Keep original value
						return original;
					}

					//mxd. Prefixed with ---?
					if(this.Text.StartsWith("---"))
					{
						// Subtract number from original
						if(TryGetResultValue(textpart, out result))
						{
							float newvalue = original - result * step;
							return (!allownegative && (newvalue < 0)) ? original : newvalue;
						}

						// Keep original value
						return original;
					}

					// Prefixed with ++?
					if(this.Text.StartsWith("++"))
					{
						// Add number to original
						if(TryGetResultValue(textpart, out result))
							return original + result;

						// Keep original value
						return original;
					}

					// Prefixed with --?
					if(this.Text.StartsWith("--"))
					{
						// Subtract number from original
						if(TryGetResultValue(textpart, out result))
						{
							float newvalue = original - result;
							return (!allownegative && (newvalue < 0)) ? original : newvalue;
						}

						// Keep original value
						return original;
					}

					//mxd. Prefixed with *?
					if(this.Text.StartsWith("*"))
					{
						// Multiply original by number
						if(TryGetResultValue(textpart, out result))
						{
							float newvalue = (float)Math.Round(original * result, ROUNDING_PRECISION);
							return (!allownegative && (newvalue < 0f)) ? original : newvalue;
						}

						// Keep original value
						return original;
					}

					//mxd. Prefixed with /?
					if(this.Text.StartsWith("/"))
					{
						// Divide original by number
						if(TryGetResultValue(textpart, out result))
						{
							if(result == 0.0f) return original;
							float newvalue = (float)Math.Round(original / result, ROUNDING_PRECISION);
							return (!allownegative && (newvalue < 0f)) ? original : newvalue;
						}

						// Keep original value
						return original;
					}
				}

				//mxd. Return the new value
				if(TryGetResultValue(textpart, out result))
					return (!allownegative && (result < 0f)) ? original : result;
			}

			// Nothing given, keep original value
			return original;
		}

		//mxd
		private bool IsValidResult(string expression)
		{
			float unused;
			return TryGetResultValue(expression, out unused);
		}

		//mxd
		private bool TryGetResultValue(string expression, out float value)
		{
			//Compute expression
			if(allowexpressions)
			{
				try { expression = datatable.Compute(expression, null).ToString(); }
				catch
				{
					value = 0f;
					return false;
				}
			}

			// Parse result
			return float.TryParse(expression, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}

		//mxd
		public void UpdateTextboxStyle() { UpdateTextboxStyle(string.Empty); }
		public void UpdateTextboxStyle(string tip)
		{
			this.ForeColor = (allowrelative ? SystemColors.HotTrack : SystemColors.WindowText);
			if(allowrelative || allowexpressions)
			{
				string s = string.Empty;
				if(allowexpressions)
				{
					s += "You can use expressions. Example: (128+64)*2.5" + Environment.NewLine;
				}
				if(allowrelative)
				{
					s += "Use ++ or -- prefixes to change by given value." + Environment.NewLine +
						 "Use +++ or --- prefixes to incrementally change by given value." + Environment.NewLine +
						 "Use * or / prefixes to multiply or divide by given value." + Environment.NewLine;
				}
				
				tooltip.SetToolTip(this, s + tip);
			}
			else if(!string.IsNullOrEmpty(tip))
			{
				tooltip.SetToolTip(this, tip);
			}
			else
			{
				tooltip.RemoveAll();
			}
		}
		
		#endregion
	}
}
