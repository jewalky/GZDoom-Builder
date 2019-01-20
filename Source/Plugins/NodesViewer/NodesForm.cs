#region === Copyright (c) 2010 Pascal van der Heiden ===

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Plugins.NodesViewer
{
	public partial class NodesForm : DelayedForm
	{
		#region ================== Variables

		private NodesViewerMode mode;

		#endregion

		#region ================== Properties

		public int SelectedTab { get { return tabs.SelectedIndex; } }
		public int ViewSplitIndex { get { return (int)splitindex.Value; } }
		public int ViewSubsectorIndex { get { return (int)ssectorindex.Value; } }
		public int ViewSegIndex { get { return viewsegbox.Checked ? (int)segindex.Value : -1; } }
		public bool ShowSegsVertices { get { return showsegsvertices.Checked; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public NodesForm()
		{
			InitializeComponent();
		}

		// Constructor
		public NodesForm(NodesViewerMode mode)
		{
			InitializeComponent();
			this.mode = mode;

			// Counts
			numsegs.Text = mode.Segs.Length.ToString();
			numsplits.Text = mode.Nodes.Length.ToString();
			numssectors.Text = mode.Subsectors.Length.ToString();
			numvertices.Text = mode.Vertices.Length.ToString();
			showsegsvertices.Text = "Show additional vertices (" + (mode.Vertices.Length - General.Map.Map.Vertices.Count) + " seg splits)";

			// Create stats on the tree
			List<int> leafdepths = new List<int>();
			DiveTree(mode.Nodes.Length - 1, 0, leafdepths);
			int maxdepth = 0, mindepth = int.MaxValue;
			foreach(int d in leafdepths)
			{
				if(d < mindepth) mindepth = d;
				if(d > maxdepth) maxdepth = d;
			}

			treedepth.Text = maxdepth.ToString();

			// Calculate the tree balance. The balance is 100% when all leafs are equal depth and
			// 0% when 1 leaf equals peakdepth and the others are at level 1.
			int balance = (int)(((float)mindepth / maxdepth) * 100f);
			treebalance.Text = balance + "%";

			// Start viewing root split
			splitindex.Maximum = mode.Nodes.Length - 1;
			splitindex.Value = mode.Nodes.Length - 1;
			splitindex_ValueChanged(null, EventArgs.Empty);
			
			// Viewing subsector
			ssectorindex.Maximum = mode.Subsectors.Length - 1;
			ssectorindex.Value = 0;
			ssectorindex_ValueChanged(null, EventArgs.Empty);
			segindex_ValueChanged(null, EventArgs.Empty);
		}

		// Clean up any resources being used.
		protected override void Dispose(bool disposing)
		{
			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		#region ================== Methods

		// This calculates the tree depth recursively
		private void DiveTree(int node, int level, List<int> leafdepths)
		{
			level++;
			
			// Process left side
			if(!mode.Nodes[node].leftsubsector)
				DiveTree(mode.Nodes[node].leftchild, level, leafdepths);
			else
				leafdepths.Add(level);

			// Process right side
			if(!mode.Nodes[node].rightsubsector)
				DiveTree(mode.Nodes[node].rightchild, level, leafdepths);
			else
				leafdepths.Add(level);			
		}

		// Show this form
		public void Show(Form owner)
		{
			// Position at left-top of owner
			this.Location = new Point(owner.Location.X + 20, owner.Location.Y + 90);

			// Show window
			base.Show(owner);
		}

		// Switch to a subsector
		public void ShowSubsector(int ssindex)
		{
			ssectorindex.Value = ssindex;
			tabs.SelectTab(tabsubsectors);
		}

		#endregion

		#region ================== Form / Overview Events

		// Exit this mode
		private void closebutton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		// Exit mode when dialog is closed
		private void NodesForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			General.Editing.CancelMode();
		}

		// Open tab changed
		private void tabs_SelectedIndexChanged(object sender, EventArgs e)
		{
			General.Interface.RedrawDisplay();
		}

		// Show segs vertices changed
		private void showsegsvertices_CheckedChanged(object sender, EventArgs e)
		{
			General.Interface.RedrawDisplay();
		}

		// (Re)build the nodes
		private void buildnodesbutton_Click(object sender, EventArgs e)
		{
			General.Map.RebuildNodes(General.Map.ConfigSettings.NodebuilderSave, true);

			// Restart the mode so that the new structures are loaded in.
			// This will automatically close and re-open this window.
			General.Editing.CancelMode();
			NodesViewerMode newmode = new NodesViewerMode();
			General.Editing.ChangeMode(newmode);
			newmode.Form.showsegsvertices.Checked = this.showsegsvertices.Checked;
			newmode.Form.Location = this.Location; //mxd
		}

		#endregion

		#region ================== Splits Events

		// Go to the root split
		private void rootbutton_Click(object sender, EventArgs e)
		{
			splitindex.Value = splitindex.Maximum;
		}

		// Go to the parent split
		private void parentbutton_Click(object sender, EventArgs e)
		{
			Node n = mode.Nodes[(int)splitindex.Value];
			if(n.parent > -1) splitindex.Value = n.parent;
		}

		// Go to the left split/subsector
		private void leftbutton_Click(object sender, EventArgs e)
		{
			Node n = mode.Nodes[(int)splitindex.Value];
			if(n.leftsubsector)
				ShowSubsector(n.leftchild);
			else
				splitindex.Value = n.leftchild;
		}

		// Go to the right split/subsector
		private void rightbutton_Click(object sender, EventArgs e)
		{
			Node n = mode.Nodes[(int)splitindex.Value];
			if(n.rightsubsector)
				ShowSubsector(n.rightchild);
			else
				splitindex.Value = n.rightchild;
		}

		// Split changes
		private void splitindex_ValueChanged(object sender, EventArgs e)
		{
			Node n = mode.Nodes[(int)splitindex.Value];
			if(n.parent == -1)
			{
				parentsplit.Text = "(root split)";
				parentsplit.Enabled = false;
				parentbutton.Enabled = false;
			}
			else
			{
				parentsplit.Text = n.parent.ToString();
				parentsplit.Enabled = true;
				parentbutton.Enabled = true;
			}

			leftarea.Text = "(" + n.leftbox.Left + ", " + n.leftbox.Top + ") - (" + n.leftbox.Right + ", " + n.leftbox.Bottom + ")";
			rightarea.Text = "(" + n.rightbox.Left + ", " + n.rightbox.Top + ") - (" + n.rightbox.Right + ", " + n.rightbox.Bottom + ")";
			leftindex.Text = n.leftchild.ToString();
			rightindex.Text = n.rightchild.ToString();

			if(n.leftsubsector)
			{
				lefttype.Text = "Subsector:";
				leftbutton.Text = "Go to subsector";
			}
			else
			{
				lefttype.Text = "Split:";
				leftbutton.Text = "Go to split";
			}

			if(n.rightsubsector)
			{
				righttype.Text = "Subsector:";
				rightbutton.Text = "Go to subsector";
			}
			else
			{
				righttype.Text = "Split:";
				rightbutton.Text = "Go to split";
			}

			General.Interface.RedrawDisplay();
		}

		#endregion

		#region ================== Subsectors Events

		// Go to parent split for this subsector
		private void ssparentsplit_Click(object sender, EventArgs e)
		{
			// Find parent split
			int ss = (int)ssectorindex.Value;
			int parentsplit = -1;
			for(int i = 0; i < mode.Nodes.Length; i++)
			{
				Node n = mode.Nodes[i];
				if((n.leftsubsector && (n.leftchild == ss)) || (n.rightsubsector && (n.rightchild == ss)))
				{
					parentsplit = i;
					break;
				}
			}
			if(parentsplit == -1) return;
			splitindex.Value = parentsplit;
			tabs.SelectTab(tabsplits);
		}

		// Subsector changes
		private void ssectorindex_ValueChanged(object sender, EventArgs e)
		{
			Subsector s = mode.Subsectors[(int)ssectorindex.Value];
			
			ssectornumsegs.Text = s.numsegs.ToString();
			segindex.Minimum = s.firstseg;
			segindex.Maximum = s.firstseg + s.numsegs - 1;
			segindex.Value = s.firstseg;
			
			General.Interface.RedrawDisplay();
		}

		// Segment changes
		private void segindex_ValueChanged(object sender, EventArgs e)
		{
			Seg sg = mode.Segs[(int)segindex.Value];
			Linedef ld = null; //mxd
			if(sg.lineindex != -1) ld = General.Map.Map.GetLinedefByIndex(sg.lineindex); //mxd

			lineindex.Text = sg.lineindex.ToString();
			startvertex.Text = sg.startvertex + "  (" + mode.Vertices[sg.startvertex].x + ", " + mode.Vertices[sg.startvertex].y + ")";
			endvertex.Text = sg.endvertex + "  (" + mode.Vertices[sg.endvertex].x + ", " + mode.Vertices[sg.endvertex].y + ")";
			segside.Text = sg.leftside ? "Back" : "Front";
			segangle.Text = Angle2D.RealToDoom(sg.angle) + "\u00B0";
			segoffset.Text = sg.offset + " mp";

			if(ld != null) //mxd
			{ 
				sideindex.Text = sg.leftside ? ld.Back.Index.ToString() : ld.Front.Index.ToString();
				sectorindex.Text = sg.leftside ? ld.Back.Sector.Index.ToString() : ld.Front.Sector.Index.ToString();
			}
			else
			{
				sideindex.Text = "None";
				sectorindex.Text = "None";
			}

			General.Interface.RedrawDisplay();
		}

		// View segment checkbox changes
		private void viewsegbox_CheckedChanged(object sender, EventArgs e)
		{
			General.Interface.RedrawDisplay();
		}

		#endregion
	}
}
