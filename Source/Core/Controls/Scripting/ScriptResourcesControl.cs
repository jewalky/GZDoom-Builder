#region ================== Namespaces

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Data.Scripting;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal partial class ScriptResourcesControl : UserControl
	{

		#region ================== Nodes sorter

		private class ScriptNodesSorter : IComparer
		{
			private List<string> resourceorder;
			private HashSet<TextResourceNodeType> resourcetypes; 
			
			public ScriptNodesSorter()
			{
				// Needed to show resources in the map-defined order
				resourceorder = new List<string>();
				foreach(DataReader reader in General.Map.Data.Containers)
				{
					resourceorder.Add(reader.Location.location);
				}

				resourcetypes = new HashSet<TextResourceNodeType> { TextResourceNodeType.RESOURCE_DIRECTORY, TextResourceNodeType.RESOURCE_PK3, TextResourceNodeType.RESOURCE_WAD };
			}
			
			// Compare between two tree nodes
			public int Compare(object o1, object o2)
			{
				TreeNode n1 = o1 as TreeNode;
				TreeNode n2 = o2 as TreeNode;

				TextResourceNodeData d1 = (TextResourceNodeData)n1.Tag;
				TextResourceNodeData d2 = (TextResourceNodeData)n2.Tag;

				// Sort resources by load order
				if(resourcetypes.Contains(d1.NodeType) && resourcetypes.Contains(d2.NodeType))
				{
					int n1index = resourceorder.IndexOf(d1.ResourceLocation);
					int n2index = resourceorder.IndexOf(d2.ResourceLocation);

					if(n1index > n2index) return 1;
					if(n1index < n2index) return -1;
					return 0;
				}

				// Push map namespace resources before anything else
				if(d1.NodeType == TextResourceNodeType.RESOURCE_MAP && d2.NodeType != TextResourceNodeType.RESOURCE_MAP) return -1;
				if(d1.NodeType != TextResourceNodeType.RESOURCE_MAP && d2.NodeType == TextResourceNodeType.RESOURCE_MAP) return 1;

				// Push embedded WADs before anything else except map resources
				if(n1.Parent != null && n2.Parent != null)
				{
					if(d1.NodeType == TextResourceNodeType.RESOURCE_WAD && d2.NodeType != TextResourceNodeType.RESOURCE_WAD) return -1;
					if(d1.NodeType != TextResourceNodeType.RESOURCE_WAD && d2.NodeType == TextResourceNodeType.RESOURCE_WAD) return 1;
				}

				// Push script folders before script files
				if(d1.NodeType == TextResourceNodeType.DIRECTORY && d2.NodeType != TextResourceNodeType.DIRECTORY) return -1;
				if(d1.NodeType != TextResourceNodeType.DIRECTORY && d2.NodeType == TextResourceNodeType.DIRECTORY) return 1;

				// Sort by name
				return n1.Text.CompareTo(n2.Text);
			}
		}

		#endregion

		#region ================== Constants

		#endregion

		#region ================== Enums

		private enum TextResourceNodeType
		{
			RESOURCE_WAD,
			RESOURCE_DIRECTORY,
			RESOURCE_PK3,
			RESOURCE_MAP,
			DIRECTORY,
			NODE,
		}

		#endregion

		#region ================== Structs

		private struct TextResourceNodeData
		{
			public ScriptResource Resource;
			public string ResourceLocation; // Where PK3/WAD/Folder resource is located
			public string LocationInResource; // Path to text file inside resource
			public TextResourceNodeType NodeType;
			public ScriptType ScriptType;

			public override string ToString()
			{
				return (NodeType == TextResourceNodeType.NODE
					? Path.Combine(ResourceLocation, LocationInResource) + (Resource.LumpIndex != -1 ? ":" + Resource.LumpIndex : "") 
					: ResourceLocation);
			}
		}

		private struct ScriptTypeItem
		{
			public string Name;
			public ScriptType Type;
			public override string ToString() { return Name; }
		}

		#endregion

		#region ================== Variables

		private ScriptEditorPanel scriptpanel;
		private Dictionary<string, Dictionary<ScriptType, HashSet<ScriptResource>>> resourcesperlocation;
		private List<ScriptTypeItem> usedscripttypes;

		#endregion

		#region ================== Properties

		#endregion

		#region ================== Constructor / Setup

		public ScriptResourcesControl()
		{
			InitializeComponent();
		}

		public void Setup(ScriptEditorPanel scriptpanel, Dictionary<ScriptType, HashSet<ScriptResource>> resources)
		{
			this.scriptpanel = scriptpanel;
			projecttree.ImageList = scriptpanel.Icons.Icons; // Link icons
			resourcesperlocation = new Dictionary<string, Dictionary<ScriptType, HashSet<ScriptResource>>>();
			foreach(HashSet<ScriptResource> group in resources.Values)
			{
				foreach(ScriptResource resource in group)
				{
					string key = resource.Resource.Location.location;
					if(!resourcesperlocation.ContainsKey(key))
						resourcesperlocation.Add(key, new Dictionary<ScriptType, HashSet<ScriptResource>>());

					if(!resourcesperlocation[key].ContainsKey(resource.ScriptType))
						resourcesperlocation[key].Add(resource.ScriptType, new HashSet<ScriptResource>());

					resourcesperlocation[key][resource.ScriptType].Add(resource);
				}
			}

			// Add used script types to the filter combobox
			string curfilter = filterbytype.SelectedText;
			filterbytype.Items.Clear();
			filterbytype.Items.Add(new ScriptTypeItem { Name = "All", Type = ScriptType.UNKNOWN });
			
			usedscripttypes = new List<ScriptTypeItem>();
			foreach(ScriptType st in resources.Keys)
				usedscripttypes.Add(new ScriptTypeItem { Name = Enum.GetName(typeof(ScriptType), st), Type = st });
			usedscripttypes.Sort((i1, i2) => String.Compare(i1.Name, i2.Name, StringComparison.Ordinal));

			int toselect = 0;
			for(int i = 0; i < usedscripttypes.Count; i++)
			{
				if(usedscripttypes[i].Name == curfilter) toselect = i;
				filterbytype.Items.Add(usedscripttypes[i]);
			}
			filterbytype.SelectedIndex = toselect; // This will also trigger tree update
		}

		#endregion

		#region ================== Methods

		public void SelectItem(ScriptResourceDocumentTab tab)
		{
			if(tab == null) return;
			SelectItem(tab.Resource.Resource.Location.location, tab.Resource.Filename, 
				tab.Resource.LumpIndex, tab.Resource.ScriptType);
		}

		public void SelectItem(string resourcelocation, string lumpname, int lumpindex, ScriptType scripttype)
		{
			TreeNode target = FindItem(projecttree.Nodes, resourcelocation, lumpname, lumpindex, scripttype);
			if(target != null)
			{
				projecttree.SelectedNode = target;
			}
		}

		private static TreeNode FindItem(TreeNodeCollection nodes, string resourcelocation, string lumpname, int lumpindex, ScriptType scripttype)
		{
			foreach(TreeNode node in nodes)
			{
				// Is this the item we are looking for?
				TextResourceNodeData data = (TextResourceNodeData)node.Tag;

				if(data.NodeType == TextResourceNodeType.NODE && data.ResourceLocation == resourcelocation 
					&& data.ScriptType == scripttype && data.Resource.Filename == lumpname && data.Resource.LumpIndex == lumpindex)
				{
					// Found it!
					return node;
				}
				
				// Try children...
				if(node.Nodes.Count > 0)
				{
					TreeNode item = FindItem(node.Nodes, resourcelocation, lumpname, lumpindex, scripttype);
					if(item != null) return item;
				}
			}

			// No dice...
			return null;
		}

		private void UpdateResourcesTree()
		{
			ScriptType targettype = (filterbytype.SelectedIndex > -1 ? ((ScriptTypeItem)filterbytype.SelectedItem).Type : ScriptType.UNKNOWN);
			UpdateResourcesTree(targettype, filterproject.Text);
		}

		private void UpdateResourcesTree(ScriptType filtertype, string filterfilename)
		{
			TreeNode selected = projecttree.SelectedNode;
			TreeNode toselect = null;
			
			projecttree.BeginUpdate();
			projecttree.Nodes.Clear();

			char[] splitter = { Path.DirectorySeparatorChar };
			bool filenamefiltered = !string.IsNullOrEmpty(filterfilename);
			bool filteringapplied = (filenamefiltered || filtertype != ScriptType.UNKNOWN);

			// Create nodes
			foreach(KeyValuePair<string, Dictionary<ScriptType, HashSet<ScriptResource>>> group in resourcesperlocation)
			{
				foreach(ScriptTypeItem item in usedscripttypes)
				{
					// Filter by script type?
					if(filtertype != ScriptType.UNKNOWN && item.Type != filtertype) continue;

					// Current resource has this scrit type?
					if(!group.Value.ContainsKey(item.Type)) continue;

					HashSet<ScriptResource> resources = group.Value[item.Type];
					foreach(ScriptResource res in resources)
					{
						bool asreadonly = res.Resource.IsReadOnly;
						
						// Filter by filename?
						if(filenamefiltered && Path.GetFileName(res.Filename).IndexOf(filterfilename, StringComparison.OrdinalIgnoreCase) == -1)
							continue;

						// Resource type node added?
						TreeNode root;
						string key = res.Resource.Location.location;
						
						// WAD resource inside another resource?
						if(res.Resource is WADReader && ((WADReader)res.Resource).ParentResource != null)
						{
							WADReader wr = (WADReader)res.Resource;
							string parentkey = wr.ParentResource.Location.location;
							TreeNode parent = GetResourceNode(projecttree.Nodes, wr.ParentResource.Location.GetDisplayName(), parentkey, wr.ParentResource);

							if(parent.Nodes.ContainsKey(key))
							{
								root = parent.Nodes[key];
							}
							else
							{
								root = GetResourceNode(parent.Nodes, Path.GetFileName(wr.Location.GetDisplayName()), key, res.Resource);
								TrySelectNode(selected, root, ref toselect);
							}
						}
						else if(projecttree.Nodes.ContainsKey(key))
						{
							root = projecttree.Nodes[key];
						}
						else
						{
							root = GetResourceNode(projecttree.Nodes, res.Resource.Location.GetDisplayName(), key, res.Resource);
							TrySelectNode(selected, root, ref toselect);
						}

						// Single resource item or active filtering?
						int iconindex = scriptpanel.Icons.GetScriptIcon(res.ScriptType);
						if(filteringapplied || (resources.Count == 1 && res.ScriptType != ScriptType.ACS))
						{
							// Create new node
							var data = new TextResourceNodeData
									   {
										   ResourceLocation = key, 
										   LocationInResource = Path.GetDirectoryName(res.Filename), 
										   NodeType = TextResourceNodeType.NODE, 
										   Resource = res, 
										   ScriptType = res.ScriptType,
									   };
							TreeNode scriptnode = new TreeNode(res.ToString(), iconindex, iconindex) { Tag = data, ToolTipText = data.ToString() };
							if(asreadonly) scriptnode.ForeColor = SystemColors.GrayText;
							TrySelectNode(selected, scriptnode, ref toselect);

							// Add the node
							root.Nodes.Add(scriptnode);
						}
						else
						{
							// Script type added?
							string typename = "[" + Enum.GetName(typeof(ScriptType), res.ScriptType) + "]";
							int groupiconindex = scriptpanel.Icons.GetScriptFolderIcon(res.ScriptType, false);

							TreeNode scriptroot;
							if(root.Nodes.ContainsKey(typename))
							{
								scriptroot = root.Nodes[typename];
							}
							else
							{
								var rdata = new TextResourceNodeData
											{
												ResourceLocation = key,
												NodeType = TextResourceNodeType.DIRECTORY,
												ScriptType = res.ScriptType,
											};
								scriptroot = new TreeNode(typename, groupiconindex, groupiconindex)
											 {
												 Tag = rdata, 
												 Name = typename, 
												 ToolTipText = rdata.ToString()
											 };
								if(asreadonly) scriptroot.ForeColor = SystemColors.GrayText;
								root.Nodes.Add(scriptroot);
								
								TrySelectNode(selected, scriptroot, ref toselect);
							}

							// Add the resource path nodes if needed...
							string path = Path.GetDirectoryName(res.Filename);
							TreeNode pathnode = scriptroot;
							string localpath = string.Empty;

							if(!string.IsNullOrEmpty(path))
							{
								List<string> parts = new List<string>(path.Split(splitter, StringSplitOptions.RemoveEmptyEntries));
								while(parts.Count > 0)
								{
									if(pathnode.Nodes.ContainsKey(parts[0]))
									{
										pathnode = pathnode.Nodes[parts[0]];
									}
									else
									{
										localpath = Path.Combine(localpath, parts[0]);

										TreeNode child = new TreeNode(parts[0], groupiconindex, groupiconindex);
										child.Name = parts[0];
										var cdata = new TextResourceNodeData
										            {
											            ResourceLocation = key, 
														LocationInResource = path, 
														NodeType = TextResourceNodeType.DIRECTORY, 
														ScriptType = res.ScriptType
										            };
										child.Tag = cdata;
										child.ToolTipText = cdata.ToString();
										if(asreadonly) child.ForeColor = SystemColors.GrayText;
										pathnode.Nodes.Add(child);
										pathnode = child;
										
										TrySelectNode(selected, pathnode, ref toselect);
									}

									parts.RemoveAt(0);
								}
							}

							// Create new node
							TextResourceNodeData data = new TextResourceNodeData
							                            {
								                            ResourceLocation = key, 
															LocationInResource = path, 
															NodeType = TextResourceNodeType.NODE, 
															Resource = res,
															ScriptType = res.ScriptType,
							                            };
							string includepath = (res.ScriptType == ScriptType.ACS ? "\nInclude path: \"" + res.Filename + "\"" : "");
							TreeNode scriptnode = new TreeNode(res.ToString(), iconindex, iconindex) { Tag = data, ToolTipText = data + includepath };
							if(asreadonly) scriptnode.ForeColor = SystemColors.GrayText;
							TrySelectNode(selected, scriptnode, ref toselect);

							// Add the node
							pathnode.Nodes.Add(scriptnode);
						}
					}
				}
			}

			// If there's only one root node, shift all nodes up
			if(projecttree.Nodes.Count == 1 && projecttree.Nodes[0].Nodes.Count > 0)
			{
				TreeNode[] children = new TreeNode[projecttree.Nodes[0].Nodes.Count];
				projecttree.Nodes[0].Nodes.CopyTo(children, 0);

				projecttree.Nodes.Clear();
				projecttree.Nodes.AddRange(children);
			}

			// Sort the nodes
			projecttree.TreeViewNodeSorter = new ScriptNodesSorter();
			
			// Have valid selection?
			if(toselect != null) projecttree.SelectedNode = toselect;

			// Expand all nodes when filtered
			if(filenamefiltered) projecttree.ExpandAll();

			projecttree.EndUpdate();
		}

		private TreeNode GetResourceNode(TreeNodeCollection nodes, string title, string key, DataReader resource)
		{
			// Node already added?
			if(nodes.ContainsKey(key)) return nodes[key];
			
			// Create new node
			int resourceiconindex = scriptpanel.Icons.GetResourceIcon(resource.Location.type);

			TreeNode root = new TreeNode(title, resourceiconindex, resourceiconindex);
			root.Name = key;
			root.Tag = new TextResourceNodeData { ResourceLocation = key, NodeType = (TextResourceNodeType)resource.Location.type };
			root.ToolTipText = key;
			if(resource.IsReadOnly) root.ForeColor = SystemColors.GrayText;
			nodes.Add(root);

			return root;
		}

		private static void TrySelectNode(TreeNode oldselection, TreeNode node, ref TreeNode toselect)
		{
			if(oldselection == null || oldselection.Text != node.Text 
				|| oldselection.ToolTipText != node.ToolTipText 
				|| oldselection.Tag.ToString() != node.Tag.ToString())
				return;

			// Found match!
			toselect = node;
		}

		protected override void OnLoad(EventArgs e)
		{
			// Manual reposition required...
			if(MainForm.DPIScaler.Width != 1.0f || MainForm.DPIScaler.Height != 1.0f)
			{
				filterprojectclear.Left = this.Width - filterprojectclear.Width - filterprojectclear.Margin.Right;
				filterproject.Width = filterprojectclear.Left - filterprojectclear.Margin.Left - filterproject.Left;
				filterbytype.Left = filterproject.Left;
				filterbytype.Width = filterprojectclear.Right - filterproject.Left;
				projecttree.Width = this.Width - projecttree.Left - projecttree.Margin.Right;
				projecttree.Height = this.Height - projecttree.Top - projecttree.Margin.Bottom;
			}
			
			base.OnLoad(e);
		}

		#endregion

		#region ================== Events

		private void filterproject_TextChanged(object sender, EventArgs e)
		{
			UpdateResourcesTree();
		}

		private void filterprojectclear_Click(object sender, EventArgs e)
		{
			filterproject.Clear();
		}

		private void filterbytype_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateResourcesTree();
		}

		private void projecttree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			// Select node on Right-click
			projecttree.SelectedNode = e.Node;
		}

		private void projecttree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			//TODO: special handling for SCRIPTS/DIALOGUE
			if(!(e.Node.Tag is TextResourceNodeData)) throw new NotSupportedException("Tag must be TextResourceData!");
			TextResourceNodeData data = (TextResourceNodeData)e.Node.Tag;
			
			// Open file
			if(data.Resource != null) scriptpanel.OpenResource(data.Resource);
		}

		// Switch to opened resource icon
		private void projecttree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			if(!(e.Node.Tag is TextResourceNodeData)) throw new NotSupportedException("Tag must be TextResourceData!");
			TextResourceNodeData data = (TextResourceNodeData)e.Node.Tag;
			
			// Group node?
			if(data.NodeType == TextResourceNodeType.DIRECTORY)
			{
				e.Node.ImageIndex = scriptpanel.Icons.GetScriptFolderIcon(data.ScriptType, true);
				e.Node.SelectedImageIndex = e.Node.ImageIndex;
			}
		}

		// Switch to closed resource icon
		private void projecttree_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			if(!(e.Node.Tag is TextResourceNodeData)) throw new NotSupportedException("Tag must be TextResourceData!");
			TextResourceNodeData data = (TextResourceNodeData)e.Node.Tag;
			
			// Group node?
			if(data.NodeType == TextResourceNodeType.DIRECTORY)
			{
				e.Node.ImageIndex = scriptpanel.Icons.GetScriptFolderIcon(data.ScriptType, false);
				e.Node.SelectedImageIndex = e.Node.ImageIndex;
			}
		}

		#endregion
	}
}
