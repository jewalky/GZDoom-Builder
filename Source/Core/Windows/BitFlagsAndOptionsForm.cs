#region ================== Namespaces

using System;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	public partial class BitFlagsAndOptionsForm : DelayedForm
	{
		#region ================== Variables

		private bool blockupdate;
		private int value;

		#endregion

		#region ================== Properties

		public int Value { get { return value; } }

		#endregion

		#region ================== Constructor

		public BitFlagsAndOptionsForm()
		{
			InitializeComponent();
		}

		#endregion

		#region ================== Events

		// When a flags checkbox is clicked
		private void flagsbox_CheckedChanged(object sender, EventArgs e)
		{
			if(!blockupdate)
			{
				// Now setting up
				blockupdate = true;

				// Get this checkbox
				CheckBox thisbox = (sender as CheckBox);

				// Checking or unchecking?
				if(thisbox.Checked)
				{
					// Go for all other options
					foreach(CheckBox b in flags.Checkboxes)
					{
						// Not the same box?
						if(b != thisbox)
						{
							// Overlapping bit flags? mxd: box with flag 0 requires special handling...
							if((int)b.Tag == 0 || (int)thisbox.Tag == 0 || (((int)b.Tag & (int)thisbox.Tag) != 0))
							{
								// Uncheck the other
								b.Checked = false;
							}
						}
					}
				}

				// Done
				blockupdate = false;
			}
		}

		// When a options checkbox is clicked
		private void optionsbox_CheckedChanged(object sender, EventArgs e)
		{
			if(!blockupdate)
			{
				// Now setting up
				blockupdate = true;

				// Get this checkbox
				CheckBox thisbox = (sender as CheckBox);

				// Checking or unchecking?
				if(thisbox.Checked)
				{
					// Uncheck all other options
					foreach(CheckBox b in options.Checkboxes)
					{
						// Not the same box?
						if(b != thisbox)
						{
							// Uncheck the other
							b.Checked = false;
						}
					}
				}

				// Done
				blockupdate = false;
			}
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Close
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			this.value = 0;

			// Go for all checkboxes to make the final value
			// Options are exclusive
			foreach(CheckBox b in options.Checkboxes)
			{
				if(b.Checked)
				{
					value = (int)b.Tag;
					break;
				}
			}

			// Flags must be combined
			foreach(CheckBox b in flags.Checkboxes) if(b.Checked) value |= (int)b.Tag;

			// Done
			DialogResult = DialogResult.OK;
			this.Close();
		}

		#endregion

		#region ================== Methods

		// Setup from EnumList
		public void Setup(EnumList optionslist, EnumList flagslist, int value)
		{
			blockupdate = true;
			this.value = value;
			int optionsheight = options.Height;
			int flagsheight = flags.Height;
			int optionsvalue = value;

			// First make a checkbox for each flags item
			foreach(EnumItem item in flagslist)
			{
				// Make the checkbox
				int flag = item.GetIntValue();
				CheckBox box = flags.Add(flag + ": " + item.Title, item.GetIntValue());

				// Bind checking event
				box.CheckedChanged += flagsbox_CheckedChanged;

				// Checking the box?
				if((value & (int)box.Tag) == (int)box.Tag)
				{
					box.Checked = true;
					optionsvalue -= (int)box.Tag;

					// Go for all other checkboxes
					foreach(CheckBox b in flags.Checkboxes)
					{
						// Not the same box?
						if(b != box)
						{
							// Overlapping bit flags? mxd: box with flag 0 requires special handling...
							if(((int)b.Tag == 0 && value != 0) || ((int)b.Tag & (int)box.Tag) != 0)
							{
								// Uncheck the other
								b.Checked = false;
							}
						}
					}
				}
			}

			// Make a checkbox for each options item
			foreach(EnumItem item in optionslist)
			{
				// Make the checkbox
				int option = item.GetIntValue();
				CheckBox box = options.Add(option + ": " + item.Title, item.GetIntValue());

				// Bind checking event
				box.CheckedChanged += optionsbox_CheckedChanged;

				// Checking the box?
				box.Checked = ((int)box.Tag == optionsvalue);
			}

			// Update window size
			int optionsheightdiff = (optionsheight - options.GetHeight());
			int flagsheightdiff = (flagsheight - flags.GetHeight());
			groupoptions.Height -= optionsheightdiff;
			groupflags.Location = new Point(groupflags.Location.X, groupflags.Location.Y - optionsheightdiff);
			groupflags.Height -= flagsheightdiff;
			this.Height -= optionsheightdiff + flagsheightdiff;
			int targetwidth = Math.Max(options.GetWidth(), flags.GetWidth());
			if(targetwidth > options.Width) this.Width += (targetwidth - options.Width);

			blockupdate = false;
		}

		// This shows the dialog
		// Returns the flags or the same flags when cancelled
		public static int ShowDialog(IWin32Window owner, EnumList options, EnumList flags, int value)
		{
			int result = value;
			BitFlagsAndOptionsForm f = new BitFlagsAndOptionsForm();
			f.Setup(options, flags, value);
			if(f.ShowDialog(owner) == DialogResult.OK) result = f.Value;
			f.Dispose();
			return result;
		}

		#endregion
	}
}
