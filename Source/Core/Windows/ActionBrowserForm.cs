
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
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using System.Drawing;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class ActionBrowserForm : DelayedForm
	{
		// Constants
		private const int MAX_OPTIONS = 8;
		
		// Variables
		private int selectedaction;
		private readonly ComboBox[] options;
		private readonly Label[] optionlbls;
		private readonly TreeNode[] allNodes; //mxd
		private readonly bool addanyaction; //mxd
		
		// Properties
		public int SelectedAction { get { return selectedaction; } }
		
		// Constructor
		public ActionBrowserForm(int action, bool addanyaction)
		{
			// Initialize
			InitializeComponent();

			//mxd. Show "Any action" item?
			this.addanyaction = addanyaction;

			// Make array references for controls
			options = new[] { option0, option1, option2, option3, option4, option5, option6, option7 };
			optionlbls = new[] { option0label, option1label, option2label, option3label, option4label,
									   option5label, option6label, option7label };
			
			// Show prefixes panel only for doom type maps
			if(!General.Map.FormatInterface.HasBuiltInActivations)
			{
				prefixespanel.Visible = false;
				actions.Height += prefixespanel.Height + 3; //mxd
			}
			
			// Go for all predefined categories
			CreateActionCategories(action);
			allNodes = new TreeNode[actions.Nodes.Count];
			actions.Nodes.CopyTo(allNodes, 0);

			// Using generalized actions?
			if(General.Map.Config.GeneralizedActions)
			{
				// Add for all generalized categories to the combobox
				category.Items.AddRange(General.Map.Config.GenActionCategories.ToArray());

				// Given action is generalized?
				if(GameConfiguration.IsGeneralized(action, General.Map.Config.GenActionCategories))
				{
					// Open the generalized tab
					tabs.SelectedTab = tabgeneralized;

					// Select category
					foreach(GeneralizedCategory ac in category.Items)
					{
						if((action >= ac.Offset) && (action < (ac.Offset + ac.Length)))
						{
							category.SelectedItem = ac;
							break; //mxd
						}
					}

					// Anything selected?
					if(category.SelectedIndex > -1)
					{
						// Go for all options in selected category
						GeneralizedCategory sc = category.SelectedItem as GeneralizedCategory;
						int actionbits = action - sc.Offset;
						 
						// Go for all options, bigger steps first (mxd)
						// INFO: both GeneralizedOptions and GeneralizedBits are incrimentally sorted 
						for(int i = MAX_OPTIONS - 1; i > -1; i--)
						{
							// Option used?
							if(i < sc.Options.Count)
							{
								// Go for all bits, bigger bits first (mxd)
								for(int b = sc.Options[i].Bits.Count - 1; b > -1; b--)
								{
									// Select this setting if matches
									GeneralizedBit bit = sc.Options[i].Bits[b];
									if((actionbits & bit.Index) == bit.Index)
									{
										options[i].SelectedItem = bit;
										actionbits -= bit.Index; //mxd
										if(bit.Index > 0) break; //mxd
									}
								}
							}
						}
					}
				}

				//mxd. Make sure something is selected
				if(category.SelectedIndex == -1 && category.Items.Count > 0)
					category.SelectedIndex = 0;
			}
			else
			{
				// Remove generalized tab
				tabs.TabPages.Remove(tabgeneralized);
			}

            // [ZZ] we are drawing nodes manually. we need nice number padding.
            actions.DrawMode = TreeViewDrawMode.OwnerDrawText;
            actions.DrawNode += Actions_DrawNode;
		}

        private void Actions_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Rectangle nodeRect = Rectangle.FromLTRB(e.Bounds.Left + 1, e.Bounds.Top, e.Bounds.Right + 1, e.Bounds.Bottom);
            Rectangle nodeTextRect = Rectangle.FromLTRB(nodeRect.Left, nodeRect.Top + 1, nodeRect.Right, nodeRect.Bottom);
            string nodeText = e.Node.Text;
            string[] parts = nodeText.Split(new char[] { '\t' }, 2);

            Font nodeFont = e.Node.NodeFont;
            if (nodeFont == null) nodeFont = ((TreeView)sender).Font;

            int wd = (int)e.Graphics.MeasureString(parts[parts.Length - 1], nodeFont).Width;
            nodeRect.Width = wd;

            SizeF tagSize = e.Graphics.MeasureString("999 ", nodeFont);
            if (parts.Length == 2)
                nodeRect.Width += (int)tagSize.Width;

            Brush fgBrush = null;
            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                // fill selected bg
                e.Graphics.FillRectangle(SystemBrushes.Highlight, nodeRect);
                // draw the node text
                fgBrush = SystemBrushes.HighlightText;
            }
            else
            {
                // fill non selected bg (winforms does weird things here)
                // never understood the fact that "Window" color is actually white, but actual window background is counted as "3D element".
                e.Graphics.FillRectangle(SystemBrushes.Window, nodeRect);
                //
                fgBrush = SystemBrushes.WindowText;
            }

            StringFormat nodeStringFmt = new StringFormat
                {
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.None
                };

            if (parts.Length == 2)
            {
                e.Graphics.DrawString(parts[0], nodeFont, fgBrush, nodeTextRect, nodeStringFmt);
                nodeTextRect.X += (int)tagSize.Width;
                e.Graphics.DrawString(parts[1], nodeFont, fgBrush, nodeTextRect, nodeStringFmt);
            }
            else
            {
                e.Graphics.DrawString(parts[0], nodeFont, fgBrush, nodeTextRect, nodeStringFmt);
            }

            /*
            // If the node has focus, draw the focus rectangle large, making
            // it large enough to include the text of the node tag, if present.
            if ((e.State & TreeNodeStates.Focused) != 0)
            {
                using (Pen focusPen = SystemPens.In)
                {
                    focusPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                    Rectangle focusBounds = nodeRect;
                    focusBounds.Size = new Size(focusBounds.Width - 1, focusBounds.Height - 1);
                    e.Graphics.DrawRectangle(focusPen, focusBounds);
                }
            }*/
        }

        // This browses for an action
        // Returns the new action or the same action when cancelled
        public static int BrowseAction(IWin32Window owner, int action) { return BrowseAction(owner, action, false); } //mxd
		public static int BrowseAction(IWin32Window owner, int action, bool addanyaction)
		{
			ActionBrowserForm f = new ActionBrowserForm(action, addanyaction);
			if(f.ShowDialog(owner) == DialogResult.OK) action = f.SelectedAction;
			f.Dispose();
			return action;
		}

		//mxd
		private void CreateActionCategories(int action) 
		{
			actions.BeginUpdate();
			actions.ShowLines = true;

			// Add "Any action" item?
			if(addanyaction)
			{
				TreeNode aan = actions.Nodes.Add("Any action");
				aan.Tag = new LinedefActionInfo(-1, "Any action", false, false);
				if(action == -1)
				{
					actions.SelectedNode = aan;
					aan.EnsureVisible();
				}
			}

			foreach(LinedefActionCategory ac in General.Map.Config.ActionCategories)
			{
				// Empty category names will not be created
				// (those actions will go in the root of the tree)
				if(ac.Title.Length > 0)
				{
					// Create category
					TreeNode cn = actions.Nodes.Add(ac.Title);
					foreach(LinedefActionInfo ai in ac.Actions) 
					{
                        // Create action
                        TreeNode n = cn.Nodes.Add((ai.Index > 0) ? string.Format("{0}\t{1}", ai.Index, ai.Title) : ai.Title);
						n.Tag = ai;

						// This is the given action?
						if(ai.Index == action) 
						{
							// Select this and expand the category
							cn.Expand();
							actions.SelectedNode = n;
							n.EnsureVisible();
						}
					}
				}
				else 
				{
					// Put actions in the tree root
					foreach(LinedefActionInfo ai in ac.Actions) 
					{
						// Create action
						TreeNode n = actions.Nodes.Add((ai.Index > 0) ? string.Format("{0}\t{1}", ai.Index, ai.Title) : ai.Title);
                        n.Tag = ai;
					}
				}
			}
			actions.EndUpdate();
		}

		//mxd
		private void FilterActions(string text) 
		{
			List<TreeNode> filterednodes = new List<TreeNode>();
			HashSet<string> added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			// First add nodes, which titles start with given text
			foreach(TreeNode n in allNodes) 
			{
				foreach(TreeNode cn in n.Nodes) 
				{
					LinedefActionInfo ai = cn.Tag as LinedefActionInfo;
					if(ai != null && ai.Title.ToUpperInvariant().StartsWith(text))
					{
						filterednodes.Add(cn);
						added.Add(ai.Title);
					}
				}
			}

			// Then add nodes, which titles contain given text
			foreach(TreeNode n in allNodes)
			{
				foreach(TreeNode cn in n.Nodes)
				{
					LinedefActionInfo ai = cn.Tag as LinedefActionInfo;
					if(ai != null && !added.Contains(ai.Title) && ai.Title.ToUpperInvariant().Contains(text))
						filterednodes.Add(cn);
				}
			}

			actions.BeginUpdate();
			actions.Nodes.Clear();
			actions.ShowLines = false;
			actions.Nodes.AddRange(filterednodes.ToArray());
			actions.EndUpdate();
		}
		
		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Presume no result
			selectedaction = 0;
			
			// Predefined action?
			if(tabs.SelectedTab == tabactions)
			{
				// Action node selected?
				if(actions.SelectedNode != null && (actions.SelectedNode.Tag is LinedefActionInfo))
				{
					// Our result
					selectedaction = ((LinedefActionInfo)actions.SelectedNode.Tag).Index;
				}
			}
			// Generalized action
			else
			{
				// Category selected?
				if(category.SelectedIndex > -1)
				{
					// Add category bits and go for all options
					GeneralizedCategory sc = category.SelectedItem as GeneralizedCategory;
					selectedaction = sc.Offset;
					for(int i = 0; i < MAX_OPTIONS; i++)
					{
						// Option used?
						if(i < sc.Options.Count)
						{
							// Add selected bits
							if(options[i].SelectedIndex > -1)
								selectedaction += (options[i].SelectedItem as GeneralizedBit).Index;
						}
					}
				}
			}
			
			// Done
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// Leave
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Generalized category selected
		private void category_SelectedIndexChanged(object sender, EventArgs e)
		{
			// Category selected?
			if(category.SelectedIndex > -1)
			{
				// Get the category
				GeneralizedCategory ac = category.SelectedItem as GeneralizedCategory;

				// Go for all options
				for(int i = 0; i < MAX_OPTIONS; i++)
				{
					// Option used in selected category?
					if(i < ac.Options.Count)
					{
						// Setup controls
						optionlbls[i].Text = ac.Options[i].Name + ":";
						options[i].Items.Clear();
						options[i].Items.AddRange(ac.Options[i].Bits.ToArray());
						
						// Show option
						options[i].Visible = true;
						optionlbls[i].Visible = true;
					}
					else
					{
						// Hide option
						options[i].Visible = false;
						optionlbls[i].Visible = false;
					}
				}
			}
			else
			{
				// Hide all options
				for(int i = 0; i < MAX_OPTIONS; i++)
				{
					options[i].Visible = false;
					optionlbls[i].Visible = false;
				}
			}
		}
		
		// Double clicking on item
		private void actions_DoubleClick(object sender, EventArgs e)
		{
			// Action node selected?
			if((actions.SelectedNode != null) && (actions.SelectedNode.Tag is LinedefActionInfo))
			{
				if(apply.Enabled) apply_Click(this, EventArgs.Empty);
			}
		}

		//mxd
		private void tbFilter_TextChanged(object sender, EventArgs e) 
		{
			if(!string.IsNullOrEmpty(tbFilter.Text.Trim()))
			{
				FilterActions(tbFilter.Text.ToUpperInvariant());
			} 
			else
			{
				actions.Nodes.Clear();
				CreateActionCategories(actions.SelectedNode != null ? ((LinedefActionInfo)actions.SelectedNode.Tag).Index : 0);
			}
		}

		//mxd. Switch focus to actions list?
		private void tbFilter_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Down && actions.Nodes.Count > 0)
			{
				actions.SelectedNode = actions.Nodes[0];
				actions.Focus();
			}
		}

		//mxd
		private void btnClearFilter_Click(object sender, EventArgs e) 
		{
			tbFilter.Clear();
		}

		//mxd
		private void ActionBrowserForm_Shown(object sender, EventArgs e) 
		{
			if(tabs.SelectedTab == tabactions) tbFilter.Focus();
		}

		//mxd
		private void actions_MouseEnter(object sender, EventArgs e)
		{
			actions.Focus();
		}

		//mxd. Transfer focus to Filter textbox
		private void actions_KeyPress(object sender, KeyPressEventArgs e)
		{
			tbFilter.Focus();
			if(e.KeyChar == '\b') // Any better way to check for Backspace?..
			{
				if(!string.IsNullOrEmpty(tbFilter.Text) && tbFilter.SelectionStart > 0 && tbFilter.SelectionLength == 0)
				{
					int s = tbFilter.SelectionStart - 1;
					tbFilter.Text = tbFilter.Text.Remove(s, 1);
					tbFilter.SelectionStart = s;
				}
			}
			else
			{
				tbFilter.AppendText(e.KeyChar.ToString(CultureInfo.InvariantCulture));
			}
		}
	}
}
