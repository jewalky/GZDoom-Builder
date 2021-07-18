#region ================== Copyright (c) 2020 Boris Iwanski

/*
 * This program is free software: you can redistribute it and/or modify
 *
 * it under the terms of the GNU General Public License as published by
 * 
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 * 
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * 
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.If not, see<http://www.gnu.org/licenses/>.
 */

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeImp.DoomBuilder.IO;
using Esprima;

namespace CodeImp.DoomBuilder.UDBScript
{
	public partial class ScriptDockerControl : UserControl
	{
		#region ================== Variables

		private ImageList images;

		#endregion

		#region ================== Properties

		public ImageList Images { get { return images; } }

		#endregion

		#region ================== Constructor

		public ScriptDockerControl(string foldername)
		{
			InitializeComponent();

			images = new ImageList();
			images.Images.Add("Folder", Properties.Resources.Folder);
			images.Images.Add("Script", Properties.Resources.Script);

			filetree.ImageList = images;

			// FillTree(foldername);
		}

		#endregion

		#region ================== Methods

		public void FillTree()
		{
			filetree.Nodes.Clear();
			filetree.Nodes.AddRange(AddToTree(BuilderPlug.Me.ScriptDirectoryStructure));
			filetree.ExpandAll();
		}

		private TreeNode[] AddToTree(ScriptDirectoryStructure sds)
		{
			List<TreeNode> newnodes = new List<TreeNode>();

			foreach (ScriptDirectoryStructure subsds in sds.Directories)
			{
				TreeNode tn = new TreeNode(subsds.Name, AddToTree(subsds));
				tn.SelectedImageKey = tn.ImageKey = "Folder";

				newnodes.Add(tn);
			}

			foreach(ScriptInfo si in sds.Scripts)
			{
				TreeNode tn = new TreeNode(si.Name);
				tn.Tag = si;
				tn.SelectedImageKey = tn.ImageKey = "Script";

				newnodes.Add(tn);
			}

			return newnodes.ToArray();
		}

		/// <summary>
		/// Ends editing the currently edited grid view cell. This is required so that the value is applied before running the script if the cell is currently
		/// being editing (i.e. typing in a value, then running the script without clicking somewhere else first)
		/// </summary>
		public void EndEdit()
		{
			scriptoptions.EndEdit();
		}

		#endregion

		#region ================== Events

		/// <summary>
		/// Sets up the the script options control for the currently selected script
		/// </summary>
		/// <param name="sender">the sender</param>
		/// <param name="e">the event</param>
		private void filetree_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Tag == null)
				return;

			if(e.Node.Tag is ScriptInfo)
			{
				BuilderPlug.Me.CurrentScript = (ScriptInfo)e.Node.Tag;
				scriptoptions.ParametersView.Rows.Clear();

				foreach (ScriptOption so in ((ScriptInfo)e.Node.Tag).Options)
				{
					int index = scriptoptions.ParametersView.Rows.Add();
					scriptoptions.ParametersView.Rows[index].Tag = so;
					scriptoptions.ParametersView.Rows[index].Cells["Value"].Value = so.value;
					scriptoptions.ParametersView.Rows[index].Cells["Description"].Value = so.description;
				}

				scriptoptions.EndAddingOptions();

				tbDescription.Text = ((ScriptInfo)e.Node.Tag).Description;
			}
			else
			{
				scriptoptions.ParametersView.Rows.Clear();
				scriptoptions.ParametersView.Refresh();
			}
		}

		/// <summary>
		/// Runs the currently selected script immediately
		/// </summary>
		/// <param name="sender">the sender</param>
		/// <param name="e">the event</param>
		private void btnRunScript_Click(object sender, EventArgs e)
		{
			BuilderPlug.Me.ScriptExecute();
		}

		/// <summary>
		/// Resets all options of the currently selected script to their default values
		/// </summary>
		/// <param name="sender">the sender</param>
		/// <param name="e">the event</param>
		private void btnResetToDefaults_Click(object sender, EventArgs e)
		{
			foreach (DataGridViewRow row in scriptoptions.ParametersView.Rows)
			{
				if (row.Tag is ScriptOption)
				{
					ScriptOption so = (ScriptOption)row.Tag;

					row.Cells["Value"].Value = so.defaultvalue.ToString();
					so.typehandler.SetValue(so.defaultvalue);

					General.Settings.DeletePluginSetting(BuilderPlug.Me.CurrentScript.GetScriptPathHash() + "." + so.name);
				}
			}
		}

		#endregion
	}
}
