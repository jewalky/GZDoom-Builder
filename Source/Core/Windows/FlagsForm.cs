using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.Windows
{
	public partial class FlagsForm : DelayedForm
	{
		#region ================== Variables

		private string value;
		private IDictionary<string, string> flagdefs;

		#endregion

		#region ================== Properties

		public string Value { get { return value; } }

		#endregion

		#region ================== Methods

		public FlagsForm()
		{
			InitializeComponent();
		}

		// Setup from EnumList
		public void Setup(string value, IDictionary<string, string> inflags)
		{
			//setup = true;
			this.value = value;
			flagdefs = inflags;

			//mxd. Store current size...
			int flagswidth = flags.Width;
			int flagsheight = flags.Height;

			//mxd. How many columns will be required?
			flags.Columns = Math.Max(1, flagdefs.Count / 8);

			// Fill flags list
			foreach(KeyValuePair<string, string> tf in flagdefs)
			{
				CheckBox cb = flags.Add(tf.Value, tf.Key);
				cb.ThreeState = true; //mxd
				cb.CheckState = CheckState.Indeterminate; //mxd
			}

			//mxd. Resize window?
			int newflagswidth = flags.GetWidth();
			int newflagsheight = flags.GetHeight();

			if(flagswidth != newflagswidth) this.Width += (newflagswidth - flagswidth);
			if(flagsheight != newflagsheight) this.Height += (newflagsheight - flagsheight);

			// Parse the value string and check the boxes if necessary
			if(!string.IsNullOrEmpty(value.Trim()))
			{
				foreach(string s in value.Split(','))
				{
					string str = s.Trim();

					//mxd. Negative flag?
					CheckState setflag = CheckState.Checked;
					if(str.StartsWith("!"))
					{
						setflag = CheckState.Unchecked;
						str = str.Substring(1, str.Length - 1);
					}

					// Make sure the given flag actually exists
					if(!flagdefs.ContainsKey(str)) continue;

					foreach(CheckBox c in flags.Checkboxes)
					{
						if(c.Text == flagdefs[str]) c.CheckState = setflag;
					}
				}
			}
		}

		// This shows the dialog
		// Returns the flags or the same flags when cancelled
		public static string ShowDialog(IWin32Window owner, string value, IDictionary<string, string> inflags)
		{
			string result = value;
			FlagsForm f = new FlagsForm();
			f.Setup(value, inflags);
			if(f.ShowDialog(owner) == DialogResult.OK) result = f.Value;
			f.Dispose();
			return result;
		}

		#endregion


		private void apply_Click(object sender, EventArgs e)
		{
			value = "";

			foreach(CheckBox c in flags.Checkboxes)
			{
				if(c.CheckState == CheckState.Indeterminate) continue;

				foreach(KeyValuePair<string, string> lf in flagdefs)
				{
					if(lf.Value == c.Text)
					{
						if(!string.IsNullOrEmpty(value)) value += ",";
						value += (c.CheckState == CheckState.Unchecked ? "!" + lf.Key : lf.Key);
					}
				}
			}

			DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e)
		{
			// Close
			DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
