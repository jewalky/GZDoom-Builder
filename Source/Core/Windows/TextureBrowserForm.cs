
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
using System.Windows.Forms;
using CodeImp.DoomBuilder.Controls;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Config;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	internal partial class TextureBrowserForm : DelayedForm
	{
		//mxd. Constants
		private const string ALL_IMAGES = "[All]";
		
		//mxd. Structs
		private struct TreeNodeData
		{
			public IFilledTextureSet Set;
			public string FolderName;
		}
		
		// Variables
		private string selectedname;
		private TreeNode selectedset; //mxd
		private long selecttextureonfill; //mxd. Was string, which wasn't reliable whem dealing with long texture names
		private readonly bool browseflats; //mxd
		
		// Properties
		public string SelectedName { get { return selectedname; } }
		
		// Constructor
		public TextureBrowserForm(string selecttexture, bool browseflats)
		{
			Cursor.Current = Cursors.WaitCursor;
			General.Interface.DisableProcessing(); //mxd

			TreeNode item; //mxd
			long longname = Lump.MakeLongName(selecttexture ?? "");
            // [ZZ] check if this name is even ok.
            if (browseflats && General.Map.Data.GetFlatImage(longname) == General.Map.Data.UnknownImage)
                longname = Lump.MakeLongName("");
			longname = (browseflats ? General.Map.Data.GetFullLongFlatName(longname) : General.Map.Data.GetFullLongTextureName(longname)); //mxd
			int count; //mxd
			selectedset = null; //mxd
			this.browseflats = browseflats; //mxd
			
			// Initialize
			InitializeComponent();

			//mxd. Set titles
			string imagetype = (browseflats ? "flats" : "textures");
			this.Text = "Browse " + imagetype;
			browser.ElementName = imagetype;

			// Setup texture browser
			browser.ApplySettings("windows." + configname, browseflats);
			
			// Update the used textures
			General.Map.Data.UpdateUsedTextures();

			tvTextureSets.BeginUpdate(); //mxd

			//mxd. Texture longname to select when list is filled
			selecttextureonfill = longname;

			//mxd. Fill texture sets list with normal texture sets
			foreach(IFilledTextureSet ts in General.Map.Data.TextureSets) 
			{
				count = (browseflats ? ts.Flats.Count : ts.Textures.Count);
				if((count == 0 && !General.Map.Config.MixTexturesFlats) || (ts.Flats.Count == 0 && ts.Textures.Count == 0)) 
					continue;

				item = tvTextureSets.Nodes.Add(ts.Name + " [" + count + "]");
				item.Name = ts.Name;
				item.Tag = new TreeNodeData { Set = ts, FolderName = ts.Name };
				item.ImageIndex = 0;
			}

			//mxd. Add container-specific texture sets
			foreach(ResourceTextureSet ts in General.Map.Data.ResourceTextureSets)
			{
				count = (browseflats ? ts.Flats.Count : ts.Textures.Count);
				if((count == 0 && !General.Map.Config.MixTexturesFlats) || (ts.Flats.Count == 0 && ts.Textures.Count == 0))
					continue;

				item = tvTextureSets.Nodes.Add(ts.Name);
				item.Name = ts.Name;
				item.Tag = new TreeNodeData { Set = ts, FolderName = ts.Name };
				item.ImageIndex = 2 + ts.Location.type;
				item.SelectedImageIndex = item.ImageIndex;

				CreateNodes(item);
				item.Expand();
			}

			//mxd. Add "All" texture set
			count = (browseflats ? General.Map.Data.AllTextureSet.Flats.Count : General.Map.Data.AllTextureSet.Textures.Count);
			item = tvTextureSets.Nodes.Add(General.Map.Data.AllTextureSet.Name + " [" + count + "]");
			item.Name = General.Map.Data.AllTextureSet.Name;
			item.Tag = new TreeNodeData { Set = General.Map.Data.AllTextureSet, FolderName = General.Map.Data.AllTextureSet.Name };
			item.ImageIndex = 1;
			item.SelectedImageIndex = item.ImageIndex;

			//mxd. Should we bother finding the correct texture set?
			if(General.Settings.LocateTextureGroup)
			{
				//mxd. Get the previously selected texture set
				string prevtextureset = General.Settings.ReadSetting("windows." + configname + ".textureset", "");
				TreeNode match;

				// When texture set name is empty, select "All" texture set
				if(string.IsNullOrEmpty(prevtextureset))
				{
					match = tvTextureSets.Nodes[tvTextureSets.Nodes.Count - 1];
				}
				else
				{
					match = FindNodeByName(tvTextureSets.Nodes, prevtextureset);
				}

				if(match != null)
				{
					IFilledTextureSet set = ((TreeNodeData)match.Tag).Set;
					foreach(ImageData img in (browseflats ? set.Flats : set.Textures))
					{
						if(img.LongName == longname)
						{
							selectedset = match;
							break;
						}
					}
				}

				//mxd. If the selected texture was not found in the last-selected set, try finding it in the other sets
				if(selectedset == null && selecttexture != "-")
				{
					foreach(TreeNode n in tvTextureSets.Nodes)
					{
						selectedset = FindTextureByLongName(n, longname);
						if(selectedset != null) break;
					}
				}

				//mxd. Texture still not found? Then just select the last used set
				if(selectedset == null && match != null) selectedset = match;
			}

			//mxd. Select the found set or "All", if none were found
			if(tvTextureSets.Nodes.Count > 0)
			{
				if(selectedset == null) selectedset = tvTextureSets.Nodes[tvTextureSets.Nodes.Count - 1];
				tvTextureSets.SelectedNodes.Clear();
				tvTextureSets.SelectedNodes.Add(selectedset);
				selectedset.EnsureVisible();
			}

			tvTextureSets.EndUpdate(); //mxd

			//mxd. Set splitter position and state (doesn't work when layout is suspended)
			if(General.Settings.ReadSetting("windows." + configname + ".splittercollapsed", false))
				splitter.IsCollapsed = true;

			//mxd. Looks like SplitterDistance is unaffected by DPI scaling. Let's fix that...
			int splitterdistance = General.Settings.ReadSetting("windows." + configname + ".splitterdistance", int.MinValue);
			if(splitterdistance == int.MinValue)
			{
				splitterdistance = 210;
				if(MainForm.DPIScaler.Width != 1.0f)
				{
					splitterdistance = (int)Math.Round(splitterdistance * MainForm.DPIScaler.Width);
				}
			}

			splitter.SplitPosition = splitterdistance;
		}

		//mxd
		private static int SortImageData(ImageData img1, ImageData img2) 
		{
			return String.Compare(img1.Name, img2.Name, StringComparison.OrdinalIgnoreCase);
		}

		//mxd
		private static int SortTreeNodes(TreeNode n1, TreeNode n2)
		{
			return String.Compare(n1.Text, n2.Text, StringComparison.InvariantCultureIgnoreCase);
		}

		//mxd
		private TreeNode FindTextureByLongName(TreeNode node, long longname) 
		{
			if(node.Name == ALL_IMAGES) return null; // Skip "All images" set
			
			//first search in child nodes
			foreach(TreeNode n in node.Nodes)
			{
				TreeNode match = FindTextureByLongName(n, longname);
				if(match != null) return match;
			}

			//then - in current node
			IFilledTextureSet set = ((TreeNodeData)node.Tag).Set;

			foreach(ImageData img in (browseflats ? set.Flats : set.Textures))
				if(img.LongName == longname) return node;

			return null;
		}

		//mxd
		private static TreeNode FindNodeByName(TreeNodeCollection nodes, string selectname) 
		{
			foreach(TreeNode n in nodes) 
			{
				if(n.Name == selectname) return n;

				TreeNode match = FindNodeByName(n.Nodes, selectname);
				if(match != null) return match;
			}
			return null;
		}

		//mxd
		private void CreateNodes(TreeNode root)
		{
			TreeNodeData rootdata = (TreeNodeData)root.Tag;
			ResourceTextureSet set = rootdata.Set as ResourceTextureSet;
			if(set == null) 
			{
				General.ErrorLogger.Add(ErrorType.Error, "Resource " + root.Name + " doesn't have TextureSet!");
				return;
			}

			int imageIndex = set.Location.type + 5;
			char[] separator = { Path.AltDirectorySeparatorChar };
			
			ImageData[] images;
			if(browseflats)
			{
				images = new ImageData[set.Flats.Count];
				set.Flats.CopyTo(images, 0);
			}
			else
			{
				images = new ImageData[set.Textures.Count];
				set.Textures.CopyTo(images, 0);
			}
			Array.Sort(images, SortImageData);

			List<ImageData> rootimages = new List<ImageData>();
			foreach(ImageData image in images) 
			{
				string[] parts = image.VirtualName.Split(separator, StringSplitOptions.RemoveEmptyEntries);
				TreeNode curNode = root;

				if(parts.Length == 1)
				{
					rootimages.Add(image);
					continue;
				}

				int localindex = ((parts[0] == "[TEXTURES]" || image is TEXTURESImage) ? 8 : imageIndex);
				string category = set.Name;
				for(int i = 0; i < parts.Length - 1; i++) 
				{
					category += (Path.DirectorySeparatorChar + parts[i]);
					
					//already got such category?
					if(curNode.Nodes.Count > 0 && curNode.Nodes.ContainsKey(category)) 
					{
						curNode = curNode.Nodes[category];
					} 
					else //create a new one
					{
						TreeNode n = new TreeNode(parts[i]) { Name = category, ImageIndex = localindex, SelectedImageIndex = localindex };
						curNode.Nodes.Add(n);
						curNode = n;
						curNode.Tag = new TreeNodeData { Set = new ResourceTextureSet(category, set.Location), FolderName = parts[i] };
					}

					// Add to current node
					if(i == parts.Length - 2) 
					{
						ResourceTextureSet curTs = ((TreeNodeData)curNode.Tag).Set as ResourceTextureSet;
						if(image.IsFlat)
							curTs.AddFlat(image);
						else
							curTs.AddTexture(image);
					}
				}
			}

			// Shift the tree up when only single child node was added
			if(root.Nodes.Count == 1 && root.Nodes[0].Nodes.Count > 0) 
			{
				TreeNode[] children = new TreeNode[root.Nodes[0].Nodes.Count];
				root.Nodes[0].Nodes.CopyTo(children, 0);
				root.Nodes.Clear();
				root.Nodes.AddRange(children);
			}

			// Add "All set textures" node
			if(General.Map.Config.MixTexturesFlats && root.Nodes.Count > 1)
			{
				TreeNode allnode = new TreeNode(ALL_IMAGES)
				{
					Name = ALL_IMAGES,
					ImageIndex = imageIndex,
					SelectedImageIndex = imageIndex,
					Tag = new TreeNodeData { Set = set, FolderName = ALL_IMAGES }
				};
				root.Nodes.Add(allnode);
			}

			// Sort immediate child nodes...
			TreeNode[] rootnodes = new TreeNode[root.Nodes.Count];
			root.Nodes.CopyTo(rootnodes, 0);
			Array.Sort(rootnodes, SortTreeNodes);
			root.Nodes.Clear();
			root.Nodes.AddRange(rootnodes);

			// Re-add root images
			ResourceTextureSet rootset = new ResourceTextureSet(set.Name, set.Location);
			if(browseflats)
			{
				foreach(ImageData data in rootimages) rootset.AddFlat(data);
			}
			else
			{
				foreach(ImageData data in rootimages) rootset.AddTexture(data);
			}
			if(General.Map.Config.MixTexturesFlats) rootset.MixTexturesAndFlats();

			// Store root data
			rootdata.Set = rootset;
			root.Tag = rootdata;

			// Set root images count
			var rootsetimages = (browseflats ? rootset.Flats : rootset.Textures);
			if(rootsetimages.Count > 0) root.Text += " [" + rootsetimages.Count + "]";

			// Add image count to node titles
			foreach(TreeNode n in root.Nodes) SetItemsCount(n);
		}

		//mxd
		private void SetItemsCount(TreeNode node) 
		{
			ResourceTextureSet ts = ((TreeNodeData)node.Tag).Set as ResourceTextureSet;
			if(ts == null) throw new Exception("Expected ResourceTextureSet, but got null...");
			
			if(node.Parent != null && General.Map.Config.MixTexturesFlats)
			{
				ts.MixTexturesAndFlats();
				if(ts.Textures.Count > 0) node.Text += " [" + ts.Textures.Count + "]";
			} 
			else
			{
				int texcount = (browseflats ? ts.Flats.Count : ts.Textures.Count);
				if(texcount > 0) node.Text += " [" + texcount + "]";
			}

			foreach(TreeNode child in node.Nodes) SetItemsCount(child);
		}

		// Selection changed
		private void browser_SelectedItemChanged(ImageBrowserItem item)
		{
			apply.Enabled = (item != null && item.ItemType == ImageBrowserItemType.IMAGE);
		}

		// OK clicked
		private void apply_Click(object sender, EventArgs e)
		{
			// Set selected name and close
			if(browser.SelectedItem != null && browser.SelectedItem.ItemType == ImageBrowserItemType.IMAGE)
			{
				selectedname = browser.SelectedItem.TextureName;
				DialogResult = DialogResult.OK;
			}
			else
			{
				selectedname = "";
				DialogResult = DialogResult.Cancel;
			}
			this.Close();
		}

		// Cancel clicked
		private void cancel_Click(object sender, EventArgs e)
		{
			// No selection, close
			selectedname = "";
			DialogResult = DialogResult.Cancel;
			this.Close();
		}

		// Activated
		private void TextureBrowserForm_Activated(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.Default;
			General.Interface.EnableProcessing(); //mxd
		}

		// Closing
		private void TextureBrowserForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Save window settings
			General.Settings.WriteSetting("windows." + configname + ".splitterdistance", splitter.SplitPosition); //mxd
			General.Settings.WriteSetting("windows." + configname + ".splittercollapsed", splitter.IsCollapsed); //mxd

			//mxd. Save last selected texture set
			if(this.DialogResult == DialogResult.OK && tvTextureSets.SelectedNodes.Count > 0)
				General.Settings.WriteSetting("windows." + configname + ".textureset", tvTextureSets.SelectedNodes[0].Name);
			
			// Clean up
			browser.OnClose("windows." + configname);
		}

		// Static method to browse for texture or flat.
		public static string Browse(IWin32Window parent, string select, bool browseflats)
		{
			TextureBrowserForm browser = new TextureBrowserForm(select, browseflats);
			return (browser.ShowDialog(parent) == DialogResult.OK ? browser.SelectedName : select);
		}

		// Item double clicked
		private void browser_SelectedItemDoubleClicked(ImageBrowserItem item)
		{
			if(item == null) return;
			switch(item.ItemType)
			{
				case ImageBrowserItemType.IMAGE:
					if(selectedset == null) throw new NotSupportedException("selectedset required!");
					if(apply.Enabled) apply_Click(this, EventArgs.Empty);
					break;

				case ImageBrowserItemType.FOLDER_UP:
					if(selectedset == null) throw new NotSupportedException("selectedset required!");
					if(selectedset.Parent != null)
					{
						// Select the node
						tvTextureSets.SelectedNodes.Clear();
						tvTextureSets.SelectedNodes.Add(selectedset.Parent);
						selectedset.Parent.EnsureVisible();

						// Update textures list
						selectedset = selectedset.Parent;
					}
					else
					{
						tvTextureSets.SelectedNodes.Clear();
						selectedset = null;
					}
					FillImagesList();
					break;

				case ImageBrowserItemType.FOLDER:
					// selectedset is null when at root level
					TreeNodeCollection nodes = (selectedset == null ? tvTextureSets.Nodes : selectedset.Nodes); 
					foreach(TreeNode child in nodes)
					{
						TreeNodeData data = (TreeNodeData)child.Tag;
						if(data.FolderName == item.TextureName)
						{
							// Select the node
							tvTextureSets.SelectedNodes.Clear();
							tvTextureSets.SelectedNodes.Add(child);
							child.EnsureVisible();

							// Update textures list
							selectedset = child;
							FillImagesList();
							break;
						}
					}
					break;

				default: throw new NotImplementedException("Unsupported ImageBrowserItemType");
			}
		}

		// This fills the list of textures, depending on the selected texture set
		private void FillImagesList()
		{
			//mxd. Show root items
			if(selectedset == null)
			{
				FillCategoriesList();
				return;
			}
			
			// Get the selected texture set
			IFilledTextureSet set = ((TreeNodeData)selectedset.Tag).Set;

			// Start adding
			browser.BeginAdding(false);

			//mxd. Add "Browse up" item
			if(selectedset.Parent != null)
			{
				TreeNodeData data = (TreeNodeData)selectedset.Parent.Tag;
				browser.AddFolder(ImageBrowserItemType.FOLDER_UP, data.FolderName);
			}
			else
			{
				browser.AddFolder(ImageBrowserItemType.FOLDER_UP, "All Texture Sets");
			}

			//mxd. Add folders
			foreach(TreeNode child in selectedset.Nodes)
			{
				TreeNodeData data = (TreeNodeData)child.Tag;
				browser.AddFolder(ImageBrowserItemType.FOLDER, data.FolderName);
			}

			// Add textures
			if(browseflats) 
			{
				// Add all available flats
				foreach(ImageData img in set.Flats) browser.AddItem(img);
			}
			else
			{
				// Add all available textures
				foreach(ImageData img in set.Textures) browser.AddItem(img);
			}

			browser.MakeTexturesUnique(); // biwa

			// Done adding
			browser.EndAdding();
		}

		private void FillCategoriesList()
		{
			// Start adding
			browser.BeginAdding(false);

			foreach(TreeNode node in tvTextureSets.Nodes)
			{
				TreeNodeData data = (TreeNodeData)node.Tag;
				browser.AddFolder(ImageBrowserItemType.FOLDER, data.FolderName);
			}

			// Done adding
			browser.EndAdding();
		}

		// Help
		private void TextureBrowserForm_HelpRequested(object sender, HelpEventArgs e)
		{
			General.ShowHelp("w_imagesbrowser.html");
			e.Handled = true;
		}

		private void TextureBrowserForm_Shown(object sender, EventArgs e)
		{
			//mxd. Calling FillImagesList() from constructor results in TERRIBLE load times. Why? I have no sodding idea...
			if(selectedset != null) FillImagesList();
			
			// Select texture
			if(selecttextureonfill != 0)
			{
				browser.SelectItem(selecttextureonfill);
				selecttextureonfill = 0;
			}

			//mxd. Focus the textures list. Calling this from TextureBrowserForm_Activated (like it's done in DB2) fails when the form is maximized. Again, I've no idea why...
			browser.FocusList();
		}

		//mxd
		private void tvTextureSets_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) 
		{
			selectedset = e.Node;
			FillImagesList();
		}

		//mxd
		private void tvTextureSets_KeyUp(object sender, KeyEventArgs e) 
		{
			if(tvTextureSets.SelectedNodes.Count > 0 && tvTextureSets.SelectedNodes[0] != selectedset) 
			{
				selectedset = tvTextureSets.SelectedNodes[0];
				FillImagesList();
			}
		}
	}
}