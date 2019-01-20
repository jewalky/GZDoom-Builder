#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data.Scripting;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Controls.Scripting
{
	public partial class FindUsagesControl : UserControl
	{
		#region ================== SearchData

		private class SearchData
		{
			private FindReplaceOptions options;
			private ScriptType scripttype;

			public FindReplaceOptions Options { get { return options; } }
			public ScriptType ScriptType { get { return scripttype; } }

			private SearchData(){}
			public SearchData(FindReplaceOptions options, ScriptType scripttype)
			{
				this.options = options;
				this.scripttype = scripttype;
			}
		}

		#endregion

		#region ================== Variabels

		private ScriptEditorPanel scriptpanel;
		private static List<TreeNode> persistentnodes;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor/Setup

		public FindUsagesControl()
		{
			InitializeComponent();
		}

		internal void Setup(ScriptEditorPanel scriptpanel)
		{
			this.scriptpanel = scriptpanel;
			findusagestree.ImageList = scriptpanel.Icons.Icons; // Link icons
			if(persistentnodes != null)
			{
				findusagestree.Nodes.AddRange(persistentnodes.ToArray());
			}
		}

		internal void OnClose()
		{
			if(persistentnodes != null)
			{
				//TECH: unlink persistentnodes from the TreeView, otherwise we'll get an exception 
				//when trying to add them to a "new" TreeView after reopening the form.
				foreach(TreeNode node in persistentnodes) node.Remove();
			}
		}

		#endregion

		#region ================== Methods

		internal bool FindUsages(FindReplaceOptions options, ScriptType scripttype)
		{
			if(string.IsNullOrEmpty(options.FindText)) return false;
			
			// If we have already searched for this text/scripttype, remove it now
			foreach(TreeNode node in findusagestree.Nodes)
			{
				SearchData sd = (SearchData)node.Tag;
				if(sd.ScriptType == scripttype && string.Compare(sd.Options.FindText, options.FindText, StringComparison.OrdinalIgnoreCase) == 0)
				{
					// Found it
					findusagestree.Nodes.Remove(node);
					break;
				}
			}

			// Perform search
			Dictionary<ScriptResource, List<FindUsagesResult>> results = new Dictionary<ScriptResource, List<FindUsagesResult>>();
			int matchescount = 0;
			int matchingfilescount = 0;
			if(scripttype == ScriptType.UNKNOWN)
			{
				// Search among all script types
				foreach(HashSet<ScriptResource> reslist in General.Map.Data.ScriptResources.Values)
				{
					foreach(ScriptResource res in reslist)
					{
						List<FindUsagesResult> matches = res.FindUsages(options);
						if(matches.Count > 0)
						{
							// Store
							results[res] = matches;
							matchescount += matches.Count;
							matchingfilescount++;
						}
					}
				}
			}
			else if(General.Map.Data.ScriptResources.ContainsKey(scripttype) 
				&& General.Map.Data.ScriptResources[scripttype].Count > 0)
			{
				// Search among given script type
				foreach(ScriptResource res in General.Map.Data.ScriptResources[scripttype])
				{
					List<FindUsagesResult> matches = res.FindUsages(options);
					if(matches.Count > 0)
					{
						// Store
						results[res] = matches;
						matchescount += matches.Count;
						matchingfilescount++;
					}
				}
			}
			else
			{
				// No dice...
				scriptpanel.DisplayStatus(ScriptStatusType.Warning, "No sutable text resources to search in!");
			}

			// Add to the list
			if(results.Count > 0)
			{
				// Create root node
				SearchData data = new SearchData(options, scripttype);
				string matchescountstr = matchescount + " match" + (matchescount > 1 ? "es" : "");
				string matchingfilescountstr = matchingfilescount + " file" + (matchingfilescount > 1 ? "s" : "");
				TreeNode root = new TreeNode("Find results for \"" + options.FindText + "\" (" + matchescountstr + " in " + matchingfilescountstr + ")", 1, 1);
				root.Tag = data;

				// Create result nodes
				foreach(KeyValuePair<ScriptResource, List<FindUsagesResult>> group in results)
				{
					// Create resource root node
					int iconindex = scriptpanel.Icons.GetScriptIcon(group.Key.ScriptType);
					matchescountstr = group.Value.Count + " match" + (group.Value.Count > 1 ? "es" : "");
					TreeNode resroot = new TreeNode("[" + Path.Combine(group.Key.Resource.Location.GetDisplayName(), group.Key.Filename) + "] (" + matchescountstr + ")", iconindex, iconindex);

					// Create a node for each match
					foreach(FindUsagesResult result in group.Value)
					{
						TreeNode resultnode = new TreeNode("Line " + result.LineIndex + ": " + result.Line.Trim(), 4, 4);
						resultnode.Tag = result;
						resroot.Nodes.Add(resultnode);
					}

					// Add to the root
					root.Nodes.Add(resroot);
				}

				// Add to the tree
				root.Expand();
				findusagestree.Nodes.Insert(0, root);

				// Store nodes
				persistentnodes = new List<TreeNode>();
				foreach(TreeNode node in findusagestree.Nodes)
					persistentnodes.Add(node);

				// Done
				return true;
			}

			// No dice...
			scriptpanel.DisplayStatus(ScriptStatusType.Warning, "No usages of \"" + options.FindText + "\" found!");

			// Done
			return false;
		}

		#endregion

		#region ================== Events

		private void findusagestree_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			if(!e.Node.IsVisible) return;

			FindUsagesResult result = e.Node.Tag as FindUsagesResult;
			Point location = new Point(e.Bounds.Location.X, e.Bounds.Location.Y + 1);
			if(result != null)
			{
				Size preferredsize = new Size(e.Bounds.Width, e.Bounds.Height);
				
				// Draw line number
				string linenum = "Line " + result.LineIndex + ": ";
				TextRenderer.DrawText(e.Graphics, linenum, findusagestree.Font, location, SystemColors.GrayText);

				//TECH: TextFormatFlags are ignored when using TextRenderer.MeasureText override without IDeviceContext
				location.X += TextRenderer.MeasureText(e.Graphics, linenum, findusagestree.Font, preferredsize, TextFormatFlags.NoPadding).Width;
				
				// Draw text before match
				string before = result.Line.Substring(0, result.MatchStart).TrimStart();
				if(!string.IsNullOrEmpty(before))
				{
					TextRenderer.DrawText(e.Graphics, before, findusagestree.Font, location, findusagestree.ForeColor);
					location.X += TextRenderer.MeasureText(e.Graphics, before, findusagestree.Font, preferredsize, TextFormatFlags.NoPadding).Width;
				}

				// Draw matching text
				string match = result.Line.Substring(result.MatchStart, result.MatchEnd - result.MatchStart);
				TextRenderer.DrawText(e.Graphics, match, findusagestree.Font, location, SystemColors.HotTrack);
				location.X += TextRenderer.MeasureText(e.Graphics, match, findusagestree.Font, preferredsize, TextFormatFlags.NoPadding).Width;

				// Draw text after match
				string after = result.Line.Substring(result.MatchEnd, result.Line.Length - result.MatchEnd).TrimEnd();
				if(!string.IsNullOrEmpty(after))
				{
					TextRenderer.DrawText(e.Graphics, after, findusagestree.Font, location, findusagestree.ForeColor);
				}
			}
			else
			{
				// Draw node text
				TextRenderer.DrawText(e.Graphics, e.Node.Text, findusagestree.Font, location, findusagestree.ForeColor);
			}
		}

		private void findusagestree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			// Select node on Right-click
			findusagestree.SelectedNode = e.Node;
		}

		private void findusagestree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			FindUsagesResult result = e.Node.Tag as FindUsagesResult;
			if(result != null)
			{
				ScriptResourceDocumentTab t = scriptpanel.OpenResource(result.Resource);
				if(t != null)
				{
					// Show target text
					t.MoveToLine(result.LineIndex);
					int pos = t.Editor.Scintilla.Lines[result.LineIndex].Position + result.MatchStart;
					t.SelectionStart = pos;
					t.SelectionEnd = pos;
					t.Focus();

					// Show in resources control
					scriptpanel.ScriptResourcesControl.SelectItem(t);
				}
			}
		}

		private void findusagestree_KeyUp(object sender, KeyEventArgs e)
		{
			if(findusagestree.SelectedNode == null) return;
			if(e.KeyCode == Keys.Delete)
			{
				persistentnodes.Remove(findusagestree.SelectedNode);
				findusagestree.Nodes.Remove(findusagestree.SelectedNode);
			}
		}

		private void menuremove_Click(object sender, EventArgs e)
		{
			if(findusagestree.SelectedNode != null)
			{
				// Remove parent node when the current one is it's only child node
				TreeNode target = findusagestree.SelectedNode;
				while(target.Parent != null && target.Parent.Nodes.Count == 1) 
					target = target.Parent;

				persistentnodes.Remove(target);
				findusagestree.Nodes.Remove(target);
			}
		}

		private void menuremoveall_Click(object sender, EventArgs e)
		{
			findusagestree.Nodes.Clear();
			persistentnodes.Clear();
		}

		private void menurepeat_Click(object sender, EventArgs e)
		{
			if(findusagestree.SelectedNode == null) return;
			
			// Find root node
			TreeNode node = findusagestree.SelectedNode;
			while(node.Parent != null) node = node.Parent;

			SearchData data = node.Tag as SearchData;
			if(data != null)
			{
				// Remove node
				findusagestree.Nodes.Remove(node);

				// Repeat search
				FindUsages(data.Options, data.ScriptType);
			}
		}

		private void contextmenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(findusagestree.Nodes.Count > 0)
			{
				menuremove.Enabled = (findusagestree.SelectedNode != null);
				menurepeat.Enabled = (findusagestree.SelectedNode != null);
			}
			else
			{
				// Don't show the menu when there are no valid options
				e.Cancel = true;
			}
		}

		private void findusagestree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			// Root node expanding?
			SearchData data = e.Node.Tag as SearchData;
			if(data != null)
			{
				e.Node.ImageIndex = General.Map.ScriptEditor.Editor.Icons.ScriptGroupOpenIconsOffset;
				e.Node.SelectedImageIndex = e.Node.ImageIndex;
			}
		}

		private void findusagestree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			// Root node collapsing?
			SearchData data = e.Node.Tag as SearchData;
			if(data != null)
			{
				e.Node.ImageIndex = General.Map.ScriptEditor.Editor.Icons.ScriptGroupIconsOffset;
				e.Node.SelectedImageIndex = e.Node.ImageIndex;
			}
		}

		#endregion

	}
}
