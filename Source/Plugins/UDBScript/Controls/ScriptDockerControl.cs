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

			FillTree(foldername);
		}

		#endregion

		#region ================== Methods

		public ExpandoObject GetScriptOptions()
		{
			return scriptoptions.GetScriptOptions();
		}

		/// <summary>
		/// Gets the name option from the script configuration template literal from the script file.
		/// </summary>
		/// <param name="filename">File to read the configuration from</param>
		/// <returns>Name of the script</returns>
		private string GetScriptNameFromFile(string filename)
		{
			Scanner scanner = new Scanner(File.ReadAllText(filename));
			Token token;
			string configstring = string.Empty;

			// Try to get the configuration from the script file
			do
			{
				scanner.ScanComments();
				token = scanner.Lex();

				if (token.Type == TokenType.Template)
				{
					string tokenstring = token.Value.ToString();
					if (tokenstring.ToLowerInvariant().StartsWith("#scriptconfiguration"))
					{
						configstring = tokenstring.Remove(0, "#scriptconfiguration".Length);
						break;
					}
				}
			} while (token.Type != TokenType.EOF);

			if (!string.IsNullOrWhiteSpace(configstring))
			{
				Configuration cfg = new Configuration();
				cfg.InputConfiguration(configstring, true);

				if (cfg.ErrorResult)
					return Path.GetFileNameWithoutExtension(filename);

				return cfg.ReadSetting("name", Path.GetFileNameWithoutExtension(filename));
			}

			return Path.GetFileNameWithoutExtension(filename);
		}

		/// <summary>
		/// Starts adding files to the file tree, starting from the "scripts" subfolders
		/// </summary>
		/// <param name="foldername">folder name inside the application directory to use as a base</param>
		private void FillTree(string foldername)
		{
			string path = Path.Combine(General.AppPath, foldername, "scripts");

			filetree.Nodes.AddRange(AddFiles(path));
			filetree.ExpandAll();
		}

		/// <summary>
		/// Adds elements (script files) to the file tree, based on the given path. Subfolders are processed recursively
		/// </summary>
		/// <param name="path">path to start at</param>
		/// <returns>Array of TreeNode</returns>
		private TreeNode[] AddFiles(string path)
		{
			List<TreeNode> newnodes = new List<TreeNode>();

			// Add files (and subfolders) in folders recursively
			foreach (string directory in Directory.GetDirectories(path))
			{
				TreeNode tn = new TreeNode(Path.GetFileName(directory), AddFiles(directory));
				tn.SelectedImageKey = tn.ImageKey = "Folder";

				newnodes.Add(tn);
			}

			// Add files
			foreach (string filename in Directory.GetFiles(path))
			{
				// Only add files with the .js extension. Us the file name as the node name. TODO: use a setting in the .cfg file (if there is one) as a name
				// The file name is stored in the Tag
				if (Path.GetExtension(filename).ToLowerInvariant() == ".js")
				{
					//TreeNode tn = new TreeNode(BuilderPlug.GetScriptName(filename));
					TreeNode tn = new TreeNode(GetScriptNameFromFile(filename));
					tn.Tag = filename;
					tn.SelectedImageKey = tn.ImageKey = "Script";

					newnodes.Add(tn);
				}
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

			// The Tag contains the file name of the script, so only continue if its set (and not if a folder is selected)
			if (e.Node.Tag is string)
			{
				BuilderPlug.Me.CurrentScriptFile = (string)e.Node.Tag;

				Scanner scanner = new Scanner(File.ReadAllText(BuilderPlug.Me.CurrentScriptFile));
				Token token;
				string configstring = string.Empty;

				// Try to get the configuration from the script file
				do
				{
					scanner.ScanComments();
					token = scanner.Lex();
					
					if(token.Type == TokenType.Template)
					{
						string tokenstring = token.Value.ToString();
						if (tokenstring.ToLowerInvariant().StartsWith("#scriptconfiguration"))
						{
							configstring = tokenstring.Remove(0, "#scriptconfiguration".Length);
							break;
						}
					}
				} while (token.Type != TokenType.EOF);
				
				if(!string.IsNullOrWhiteSpace(configstring))
				{
					Configuration cfg = new Configuration();
					cfg.InputConfiguration(configstring, true);

					if(cfg.ErrorResult)
					{
						string errordesc = "Error in script configuration of file " + BuilderPlug.Me.CurrentScriptFile + " on line " + cfg.ErrorLine + ": " + cfg.ErrorDescription;
						General.ErrorLogger.Add(ErrorType.Error, errordesc);
						General.WriteLogLine(errordesc);

						scriptoptions.ParametersView.Rows.Clear();
						scriptoptions.ParametersView.Refresh();

						return;
					}

					IDictionary options = cfg.ReadSetting("options", new Hashtable());

					scriptoptions.ParametersView.Rows.Clear();

					foreach (DictionaryEntry de in options)
					{
						string description = cfg.ReadSetting(string.Format("options.{0}.description", de.Key), "no description");
						int type = cfg.ReadSetting(string.Format("options.{0}.type", de.Key), 0);
						string defaultvaluestr = cfg.ReadSetting(string.Format("options.{0}.default", de.Key), string.Empty);
						IDictionary enumvalues = cfg.ReadSetting(string.Format("options.{0}.enumvalues", de.Key), new Hashtable());

						if(Array.FindIndex(ScriptOption.ValidTypes, t => (int)t == type) == -1)
						{
							string errordesc = "Error in script configuration of file " + BuilderPlug.Me.CurrentScriptFile + ": option " + de.Key + " has invalid type " + type;
							General.ErrorLogger.Add(ErrorType.Error, errordesc);
							General.WriteLogLine(errordesc);
							continue;
						}

						ScriptOption so = new ScriptOption((string)de.Key, description, type, enumvalues, defaultvaluestr);

						// Try to read a saved script option value from the config
						string savedvalue = General.Settings.ReadPluginSetting(BuilderPlug.Me.GetScriptPathHash() + "." + so.name, so.defaultvalue.ToString());

						if (string.IsNullOrWhiteSpace(savedvalue))
							so.value = so.defaultvalue;
						else
							so.value = savedvalue;

						so.typehandler.SetValue(so.value);

						int index = scriptoptions.ParametersView.Rows.Add(); 
						scriptoptions.ParametersView.Rows[index].Tag = so;
						scriptoptions.ParametersView.Rows[index].Cells["Value"].Value = so.value;
						scriptoptions.ParametersView.Rows[index].Cells["Description"].Value = description;
					}

					// Make sure the browse button is shown if the first option has it
					scriptoptions.EndAddingOptions();
				}
				else
				{
					scriptoptions.ParametersView.Rows.Clear();
					scriptoptions.ParametersView.Refresh();
				}
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

					General.Settings.DeletePluginSetting(BuilderPlug.Me.GetScriptPathHash() + "." + so.name);

					row.Tag = so;
				}
			}
		}

		#endregion
	}
}
