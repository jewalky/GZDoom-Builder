
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
using System.Drawing;
using System.Windows.Forms;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class DockersControl : UserControl
	{
		#region ================== Constants
		
		#endregion

		#region ================== Delegates
		
		public event EventHandler MouseContainerEnter;
		public event EventHandler MouseContainerLeave;
		public event EventHandler Collapsed;
		public event EventHandler Expanded;
		public event EventHandler UserResize;
		
		#endregion
		
		#region ================== Variables
		
		// Behaviour
		private bool rightalign;
		private bool iscollapsed;
		
		// Collapsing
		private int expandedwidth;		// width when expanded
		private int expandedtab;		// selected tab index when expanded
		
		// Splitting
		private int splitstartoffset;
		
		// Selection
		private string currentselected;
		private string previousselected;
		private bool controlledselection;
		
		#endregion

		#region ================== Properties
		
		public bool IsCollpased { get { return iscollapsed; } }
		public string SelectedTabName { get { return (tabs.SelectedTab == null ? "None" : tabs.SelectedTab.Text); } } //mxd

		// This returns true when the focus is here, but not in some special cases
		public bool IsFocused
		{
			get
			{
				Control ac = FindActiveControl();
				
				// We have focus when we need the keyboard for input
				// Otherwise we don't want the focus and the docker may collapse
				return (ac is TextBox) || (ac is RichTextBox) || (ac is NumericUpDown) || (ac is ComboBox);
			}
		}
		
		#endregion
		
		#region ================== Constructor

		// Constructor
		public DockersControl()
		{
			InitializeComponent();
			expandedwidth = (int)(this.Width * (this.CurrentAutoScaleDimensions.Width / this.AutoScaleDimensions.Width));
		}
		
		#endregion
		
		#region ================== Methods
		
		// This returns the active child control
		private Control FindActiveControl()
		{
			Control c = this.ActiveControl;
			
			while(c is IContainerControl)
			{
				IContainerControl cc = (c as IContainerControl);
				if(cc.ActiveControl != null) c = cc.ActiveControl;
				else break;
			}
			
			return c;
		}
		
		// This sets up the controls for left or right alignment
		public void Setup(bool right)
		{
			int voffset = pinbutton.Bottom + pinbutton.Margin.Bottom; //mxd
			rightalign = right;
			if(rightalign)
			{
				splitter.Dock = DockStyle.Left;
				tabs.Alignment = TabAlignment.Right;
				tabs.Location = new Point(0, voffset);
			}
			else
			{
				splitter.Dock = DockStyle.Right;
				tabs.Alignment = TabAlignment.Left;
				tabs.Location = new Point(-2, voffset);
			}

			tabs.Size = new Size(this.ClientRectangle.Width + 2, this.ClientRectangle.Height - voffset);
			tabs.SendToBack();
			UpdatePinIcon(); //mxd
		}
		
		// This collapses the docker
		public void Collapse()
		{
			if(iscollapsed) return;

			controlledselection = true;
			splitter.Enabled = false;
			splitter.BackColor = SystemColors.Control;
			splitter.Width = (int)(2.0f * (this.CurrentAutoScaleDimensions.Width / this.AutoScaleDimensions.Width));
			expandedtab = tabs.SelectedIndex;
			expandedwidth = this.Width;
			tabs.SelectedIndex = -1;
			General.MainWindow.LockUpdate();
			if(rightalign) this.Left = this.Right - GetCollapsedWidth();
			this.Width = GetCollapsedWidth();
			General.MainWindow.UnlockUpdate();
			this.Invalidate(true);
			controlledselection = false;
			
			iscollapsed = true;
			
			if(Collapsed != null) Collapsed(this, EventArgs.Empty);
		}
		
		// This expands the docker
		public void Expand()
		{
			if(!iscollapsed) return;

			controlledselection = true;
			splitter.Enabled = true;
			splitter.BackColor = Color.Transparent;
			splitter.Width = (int)(4.0f * (this.CurrentAutoScaleDimensions.Width / this.AutoScaleDimensions.Width));
			General.MainWindow.LockUpdate();
			if(rightalign) this.Left = this.Right - expandedwidth;
			this.Width = expandedwidth;
			General.MainWindow.UnlockUpdate();
			tabs.SelectedIndex = expandedtab;
			tabs.Invalidate(true);
			controlledselection = false;
			
			iscollapsed = false;
			
			if(Expanded != null) Expanded(this, EventArgs.Empty);
		}
		
		// This calculates the collapsed width
		public int GetCollapsedWidth()
		{
			Rectangle r;
			if(tabs.TabPages.Count > 0)
				r = tabs.GetTabRect(0);
			else
				r = new Rectangle(0, 0, 26, 26);
			return r.Width + (int)(1.0f * (this.CurrentAutoScaleDimensions.Width / this.AutoScaleDimensions.Width));
		}
		
		// This adds a docker
		public void Add(Docker d, bool notify)
		{
			// Set up page
			TabPage page = new TabPage(d.Title);
			page.SuspendLayout();
			page.Font = this.Font;
			page.Tag = d;
			page.UseVisualStyleBackColor = false;
			page.Controls.Add(d.Control);
			d.Control.Dock = DockStyle.Fill;
			tabs.TabPages.Add(page);
			page.ResumeLayout(true);
			if(iscollapsed)
			{
				tabs.SelectedIndex = -1;
				if(notify) tabs.PlayNotifyAnimation(tabs.TabPages.Count - 1); //mxd
			}
			
			// Go for all controls to add events
			Queue<Control> todo = new Queue<Control>();
			todo.Enqueue(d.Control);
			while(todo.Count > 0)
			{
				Control c = todo.Dequeue();
				c.MouseEnter += RaiseMouseContainerEnter;
				c.MouseLeave += RaiseMouseContainerLeave;
				foreach(Control cc in c.Controls)
					todo.Enqueue(cc);
			}
		}
		
		// This removes a docker
		public bool Remove(Docker d)
		{
			foreach(TabPage page in tabs.TabPages)
			{
				if((page.Tag as Docker) == d)
				{
					// Go for all controls to remove events
					Queue<Control> todo = new Queue<Control>();
					todo.Enqueue(d.Control);
					while(todo.Count > 0)
					{
						Control c = todo.Dequeue();
						c.MouseEnter -= RaiseMouseContainerEnter;
						c.MouseLeave -= RaiseMouseContainerLeave;
						foreach(Control cc in c.Controls)
							todo.Enqueue(cc);
					}
					
					// Take down that page
					if(page == tabs.SelectedTab || tabs.SelectedTab == null) SelectPrevious();
					page.Controls.Clear();
					tabs.TabPages.Remove(page);
					return true;
				}
			}
			
			return false;
		}

		//mxd. This checks if given docker exists in this control
		public bool Contains(Docker d) 
		{
			foreach(TabPage page in tabs.TabPages) 
				if((page.Tag as Docker) == d) return true;
			return false;
		}
		
		// This selects a docker
		public bool SelectDocker(Docker d)
		{
			int index = 0;
			foreach(TabPage page in tabs.TabPages)
			{
				if((page.Tag as Docker) == d)
				{
					if(iscollapsed)
					{
						if(!string.IsNullOrEmpty(currentselected)) previousselected = currentselected; //mxd
						currentselected = d.FullName; //mxd
						expandedtab = index;
					}
					else
						tabs.SelectedTab = page;
					
					return true;
				}
				
				index++;
			}
			
			return false;
		}
		
		// This selectes the previous docker
		public void SelectPrevious()
		{
			//mxd. First one is better than nothing
			if(string.IsNullOrEmpty(previousselected) && tabs.TabPages.Count > 0)
			{
				previousselected = (tabs.TabPages[0].Tag as Docker).FullName;
			}
			
			if(!string.IsNullOrEmpty(previousselected))
			{
				int index = 0;
				foreach(TabPage page in tabs.TabPages)
				{
					if((page.Tag as Docker).FullName == previousselected)
					{
						if(iscollapsed)
						{
							currentselected = previousselected;
							expandedtab = index;
						}
						else
							tabs.SelectedTab = page;
						
						break;
					}
					
					index++;
				}
			}
		}
		
		// This sorts tabs by their full name
		public void SortTabs(IEnumerable<string> fullnames)
		{
			Dictionary<string, TabPage> pages = new Dictionary<string, TabPage>(tabs.TabPages.Count, StringComparer.Ordinal);
			foreach(TabPage p in tabs.TabPages) pages.Add((p.Tag as Docker).FullName, p);
			tabs.TabPages.Clear();
			
			// Add tabs in order as in fullnames
			foreach(string name in fullnames)
			{
				if(pages.ContainsKey(name))
				{
					tabs.TabPages.Add(pages[name]);
					pages.Remove(name);
				}
			}
			
			// Add remaining tabs
			foreach(KeyValuePair<string, TabPage> p in pages)
				tabs.TabPages.Add(p.Value);
		}

		//mxd
		private void UpdatePinIcon() 
		{
			if(tabs.Alignment == TabAlignment.Left)
				pinbutton.Image = (General.Settings.CollapseDockers ? Properties.Resources.DockerCollapse : Properties.Resources.DockerExpand);
			else
				pinbutton.Image = (General.Settings.CollapseDockers ? Properties.Resources.DockerExpand : Properties.Resources.DockerCollapse);
		}
		
		#endregion
		
		#region ================== Events
		
		// This raises the MouseContainerEnter event
		private void RaiseMouseContainerEnter(object sender, EventArgs e)
		{
			if(MouseContainerEnter != null)
				MouseContainerEnter(sender, e);
		}

		// This raises the MouseContainerLeave event
		private void RaiseMouseContainerLeave(object sender, EventArgs e)
		{
			if(MouseContainerLeave != null)
				MouseContainerLeave(sender, e);
		}
		
		// We don't want the focus
		private void tabs_Enter(object sender, EventArgs e) { General.MainWindow.FocusDisplay(); }
		private void tabs_MouseUp(object sender, MouseEventArgs e) { General.MainWindow.FocusDisplay(); }
		private void tabs_Selected(object sender, TabControlEventArgs e) { General.MainWindow.FocusDisplay(); }
		
		// Tab selected
		private void tabs_SelectedIndexChanged(object sender, EventArgs e)
		{
			if(!controlledselection)
			{
				// Keep track of previous selected tab
				if(!string.IsNullOrEmpty(currentselected)) previousselected = currentselected;

				if(tabs.SelectedTab != null)
				{
					Docker d = (tabs.SelectedTab.Tag as Docker);
					currentselected = d.FullName;
				}
				else
				{
					currentselected = null;
				}
			}
			
			General.MainWindow.FocusDisplay();
		}
		
		// Splitting begins
		private void splitter_MouseDown(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
			{
				splitstartoffset = e.X;
				splitter.BackColor = SystemColors.Highlight;
			}
		}
		
		// Splitting ends
		private void splitter_MouseUp(object sender, MouseEventArgs e)
		{
			splitter.BackColor = Color.Transparent;
			tabs.Invalidate(true);
			this.Update();
			General.MainWindow.RedrawDisplay();
			General.MainWindow.Update();
		}
		
		// Splitting dragged
		private void splitter_MouseMove(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Left)
			{
				General.MainWindow.LockUpdate();
				
				// Resize the control
				int delta = e.X - splitstartoffset;
				int collapsedwidth = GetCollapsedWidth();
				if(rightalign)
				{
					if((this.Width > collapsedwidth) || (delta < 0))
					{
						this.Left += delta;
						this.Width -= delta;
						if(this.Width < collapsedwidth)
						{
							this.Left -= collapsedwidth - this.Width;
							this.Width = collapsedwidth;
						}
					}
				}
				else
				{
					if((this.Width > collapsedwidth) || (delta > 0))
					{
						this.Width += delta;
						if(this.Width < collapsedwidth)
							this.Width = collapsedwidth;
					}
				}

				General.MainWindow.UnlockUpdate();
				this.Update();
				General.MainWindow.RedrawDisplay();
				General.MainWindow.Update();
				
				// Raise event
				if(UserResize != null) UserResize(this, EventArgs.Empty);
			}
		}

		//mxd
		private void pinbutton_Click(object sender, EventArgs e)
		{
			General.Settings.CollapseDockers = !General.Settings.CollapseDockers;
			General.MainWindow.SetupInterface();
			UpdatePinIcon();
		}

		//mxd
		private void DockersControl_Resize(object sender, EventArgs e)
		{
			pinbutton.Width = this.Width - pinbutton.Margin.Left - pinbutton.Margin.Right;
		}
		
		#endregion
	}
}
