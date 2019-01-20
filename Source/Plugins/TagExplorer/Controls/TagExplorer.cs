#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Editing;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Windows;

#endregion

namespace CodeImp.DoomBuilder.TagExplorer
{
	#region ================== Structs

	internal struct SortMode
	{
		public const string SORT_BY_INDEX = "By Index";
		public const string SORT_BY_TAG = "By Tag";
		public const string SORT_BY_ACTION = "By Action Special";
		public const string SORT_BY_POLYOBJ_NUMBER = "By Polyobject Number";
		public static readonly object[] SORT_MODES = new object[] { SORT_BY_INDEX, SORT_BY_TAG, SORT_BY_ACTION };
	}

	internal struct SelectedNode
	{
		public NodeInfoType Type;
		public int Index;
	}

	#endregion
	
	public sealed partial class TagExplorer : UserControl
	{
		private struct SectorEffectData
		{
			public int Effect; //1024
			public string CategoryName; // "Secret"
			public string EffectName; // "Secret: Yes"
			public bool IsGeneralized;
		}
		
		private const string DISPLAY_TAGS_AND_ACTIONS = "Tags and Action Specials";
		private const string DISPLAY_TAGS = "Tags";
		private const string DISPLAY_ACTIONS = "Action Specials";
		private const string DISPLAY_POLYOBJECTS = "Polyobjects";
		private readonly object[] DISPLAY_MODES = { DISPLAY_TAGS_AND_ACTIONS, DISPLAY_TAGS, DISPLAY_ACTIONS, DISPLAY_POLYOBJECTS };
		
		private string currentDisplayMode;
		private string currentSortMode;

		private const string CAT_THINGS = "Things:";
		private const string CAT_SECTORS = "Sectors:";
		private const string CAT_LINEDEFS = "Linedefs:";

		private readonly Color commentColor = Color.DarkMagenta;
		private SelectedNode selection;
		private readonly string nodetooltip;

		private static bool udmf;
		internal static bool UDMF { get { return udmf; } }

		#region ================== Constructor / Disposer

		public TagExplorer() 
		{
			InitializeComponent();

			selection = new SelectedNode();

			cbDisplayMode.Items.AddRange(DISPLAY_MODES);
			cbDisplayMode.SelectedIndex = General.Clamp(General.Settings.ReadPluginSetting("displaymode", 0), 0, DISPLAY_MODES.Length - 1);
			cbDisplayMode.SelectedIndexChanged += cbDisplayMode_SelectedIndexChanged;
			currentDisplayMode = cbDisplayMode.SelectedItem.ToString();

			cbSortMode.Items.AddRange(SortMode.SORT_MODES);
			cbSortMode.SelectedIndex = General.Clamp(General.Settings.ReadPluginSetting("sortmode", 0), 0, SortMode.SORT_MODES.Length - 1);
			cbSortMode.SelectedIndexChanged += cbSortMode_SelectedIndexChanged;
			currentSortMode = cbSortMode.SelectedItem.ToString();

			cbCenterOnSelected.Checked = General.Settings.ReadPluginSetting("centeronselected", false);
			cbSelectOnClick.Checked = General.Settings.ReadPluginSetting("doselect", false);

			udmf = (General.Map.Config.FormatInterface == "UniversalMapSetIO");

			string searchhint = "Enter '#' + tag number to show only specified tag. Example: #667" + Environment.NewLine
				+ "Enter '$' + effect number to show only specified effect. Example: $80" + Environment.NewLine
			    + "Enter '^' + polyobject number to filter by polyobject number. Example: ^22" + Environment.NewLine
			    + "Several wildcards can be combined.";

			if(udmf) 
			{
				cbCommentsOnly.Checked = General.Settings.ReadPluginSetting("commentsonly", false);
				toolTip1.SetToolTip(labelSearch, "Enter text to find comment" + Environment.NewLine + searchhint);
				nodetooltip = "Double-click item to edit item's comment\r\nRight-click item to open item's Properties";
			} 
			else 
			{
				cbCommentsOnly.Enabled = false;
				toolTip1.SetToolTip(labelSearch, searchhint);
				nodetooltip = "Right-click item to open item's Properties";
			}
		}

		// Disposer
		protected override void Dispose(bool disposing) 
		{
			General.Settings.WritePluginSetting("sortmode", cbSortMode.SelectedIndex);
			General.Settings.WritePluginSetting("displaymode", cbDisplayMode.SelectedIndex);
			General.Settings.WritePluginSetting("centeronselected", cbCenterOnSelected.Checked);
			General.Settings.WritePluginSetting("doselect", cbSelectOnClick.Checked);

			if(udmf) General.Settings.WritePluginSetting("commentsonly", cbCommentsOnly.Checked);
			if(disposing && (components != null)) components.Dispose();
			base.Dispose(disposing);
		}

		#endregion

		#region ================== Methods

		public void Setup() 
		{
			if(this.ParentForm != null) this.ParentForm.Activated += ParentForm_Activated;
			UpdateTree(true);
		}

		public void Terminate() 
		{
			if(this.ParentForm != null) this.ParentForm.Activated -= ParentForm_Activated;
		}

		// This sets the timer to update the list very soon (because we'll have problems if we just call updateTree now)
		public void UpdateTreeSoon() 
		{
			updatetimer.Stop();
			updatetimer.Start();
		}

		private void UpdateTree(bool focusDisplay) 
		{
			bool showTags = (currentDisplayMode == DISPLAY_TAGS || currentDisplayMode == DISPLAY_TAGS_AND_ACTIONS);
			bool showActions = (currentDisplayMode == DISPLAY_ACTIONS || currentDisplayMode == DISPLAY_TAGS_AND_ACTIONS);
			bool hasComment;
			string comment = "";
			string serachStr = tbSearch.Text.ToLowerInvariant();

			HashSet<int> filteredtags = new HashSet<int>();
			HashSet<int> filteredactions = new HashSet<int>();
			HashSet<int> filteredpolyobjects = new HashSet<int>();
			GetSpecialValues(serachStr, ref filteredtags, ref filteredactions, ref filteredpolyobjects);

			if(!udmf || filteredtags.Count > 0 || filteredactions.Count > 0 || filteredpolyobjects.Count > 0)
				serachStr = "";

			TreeNode selectedNode = null;

			this.SuspendLayout();
			treeView.Nodes.Clear();

			List<TreeNode> nodes = new List<TreeNode>();

//add things
			if(General.Map.FormatInterface.HasThingAction || General.Map.FormatInterface.HasThingTag) 
			{
				ICollection<Thing> things = General.Map.Map.Things;

				if(!(things is MapElementCollection<Thing>)) //don't want to enumerate when array is locked
				{ 
					foreach(Thing t in things) 
					{
						if((showTags && t.Tag != 0) || (showActions && t.Action > 0)) 
						{
							if(filteredtags.Count > 0 && !filteredtags.Contains(t.Tag)) continue;
							if(filteredactions.Count > 0 && !filteredactions.Contains(t.Action)) continue;

							NodeInfo info = new NodeInfo(t);
							if(filteredpolyobjects.Count > 0 && !filteredpolyobjects.Contains(info.PolyobjectNumber)) continue;

							string name = info.GetName(ref comment, currentSortMode);
							hasComment = comment.Length > 0;

							if(!hasComment && cbCommentsOnly.Checked) continue;

							if(!udmf || serachStr.Length == 0 || (hasComment && comment.ToLowerInvariant().IndexOf(serachStr) != -1)) 
							{
								TreeNode node = new TreeNode(name, 1, 1) { Tag = info, ToolTipText = nodetooltip };
								if(hasComment) node.ForeColor = commentColor;
								nodes.Add(node);

								if(info.Index == selection.Index && info.Type == selection.Type)
									selectedNode = node;
							}
						}
						else if(currentDisplayMode == DISPLAY_POLYOBJECTS)
						{
							NodeInfo info = new NodeInfo(t);
							if(info.PolyobjectNumber != int.MinValue && (filteredpolyobjects.Count == 0 || filteredpolyobjects.Contains(info.PolyobjectNumber)))
							{
								string name = info.GetName(ref comment, SortMode.SORT_BY_POLYOBJ_NUMBER);
								TreeNode node = new TreeNode(name, 1, 1) { Tag = info, ToolTipText = nodetooltip };
								nodes.Add(node);
							}
						}
					}

					//sort nodes
					Sort(ref nodes, currentSortMode);

					//add "things" category
					if(nodes.Count > 0) 
					{
						switch(currentSortMode)
						{
							case SortMode.SORT_BY_ACTION:
							{ 
								Dictionary<int, TreeNode> categories = new Dictionary<int, TreeNode>();
								TreeNode noAction = new TreeNode("No Action", 0, 0);

								foreach(TreeNode node in nodes) 
								{
									NodeInfo nodeInfo = node.Tag as NodeInfo;

									if(nodeInfo.Action == 0) 
									{
										noAction.Nodes.Add(node);
										continue;
									}

									LinedefActionInfo lai = General.Map.Config.GetLinedefActionInfo(nodeInfo.Action);

									if(!categories.ContainsKey(lai.Index))
										categories.Add(lai.Index, new TreeNode(lai.Index + " - " + lai.Name, 0, 0, new[] { node }));
									else
										categories[lai.Index].Nodes.Add(node);
								}

								TreeNode[] catNodes = new TreeNode[categories.Values.Count];
								categories.Values.CopyTo(catNodes, 0);

								TreeNode category = new TreeNode(CAT_THINGS, 0, 0, catNodes);
								if(noAction.Nodes.Count > 0) category.Nodes.Add(noAction);

								treeView.Nodes.Add(category);

							}
							break;

							case SortMode.SORT_BY_INDEX:
							{
								if(currentDisplayMode == DISPLAY_POLYOBJECTS)
								{
									TreeNode category = new TreeNode(CAT_THINGS, 0, 0);
									category.Nodes.AddRange(nodes.ToArray());
									treeView.Nodes.Add(category);
								}
								else
								{
									Dictionary<string, TreeNode> categories = new Dictionary<string, TreeNode>(StringComparer.Ordinal);
									foreach(TreeNode node in nodes)
									{
										NodeInfo nodeInfo = node.Tag as NodeInfo;
										ThingTypeInfo tti = General.Map.Data.GetThingInfoEx(General.Map.Map.GetThingByIndex(nodeInfo.Index).Type);

										if(tti != null)
										{
											if(!categories.ContainsKey(tti.Category.Title)) categories.Add(tti.Category.Title, new TreeNode(tti.Category.Title, 0, 0, new[] {node}));
											else categories[tti.Category.Title].Nodes.Add(node);
										}
										else
										{
											if(!categories.ContainsKey("UNKNOWN")) categories.Add("UNKNOWN", new TreeNode("UNKNOWN", 0, 0, new[] {node}));
											else categories["UNKNOWN"].Nodes.Add(node);
										}
									}
									TreeNode[] catNodes = new TreeNode[categories.Values.Count];
									categories.Values.CopyTo(catNodes, 0);

									treeView.Nodes.Add(new TreeNode(CAT_THINGS, 0, 0, catNodes));
								}
							}
							break;

							case SortMode.SORT_BY_TAG:
							{ 
								Dictionary<int, TreeNode> categories = new Dictionary<int, TreeNode>();
								TreeNode noTag = new TreeNode("No Tag", 0, 0);

								foreach(TreeNode node in nodes) 
								{
									NodeInfo nodeInfo = node.Tag as NodeInfo;

									if(nodeInfo.Tag == 0) 
									{
										noTag.Nodes.Add(node);
										continue;
									}

									if(!categories.ContainsKey(nodeInfo.Tag)) 
									{
										string title = "Tag " + nodeInfo.Tag;
										if(General.Map.Options.TagLabels.ContainsKey(nodeInfo.Tag))
											title += ": " + General.Map.Options.TagLabels[nodeInfo.Tag];

										categories.Add(nodeInfo.Tag, new TreeNode(title, 0, 0, new[] { node }));
									} 
									else 
									{
										categories[nodeInfo.Tag].Nodes.Add(node);
									}
								}

								TreeNode[] catNodes = new TreeNode[categories.Values.Count];
								categories.Values.CopyTo(catNodes, 0);

								TreeNode category = new TreeNode(CAT_THINGS, 0, 0, catNodes);
								if(noTag.Nodes.Count > 0) category.Nodes.Add(noTag);

								treeView.Nodes.Add(category);
							}
							break;
						}
					}
				}
			}

//add sectors
			nodes = new List<TreeNode>();
			ICollection<Sector> sectors = General.Map.Map.Sectors;

			if(currentDisplayMode != DISPLAY_POLYOBJECTS && !(sectors is MapElementCollection<Sector>)) //don't want to enumerate when array is locked
			{ 
				foreach(Sector s in sectors) 
				{
					if((showTags && s.Tag != 0) || (showActions && s.Effect > 0)) 
					{
						if(filteredactions.Count > 0 && !filteredactions.Contains(s.Effect)) continue;
						for(int i = 0; i < s.Tags.Count; i++)
						{
							if(filteredtags.Count > 0 && !filteredtags.Contains(s.Tags[i])) continue;

							NodeInfo info = new NodeInfo(s, i);
							string name = info.GetName(ref comment, currentSortMode);
							hasComment = comment.Length > 0;

							if(!hasComment && cbCommentsOnly.Checked) continue;
							if(!udmf || serachStr.Length == 0 || (hasComment && comment.ToLowerInvariant().IndexOf(serachStr) != -1))
							{
								TreeNode node = new TreeNode(name, 3, 3) { Tag = info, ToolTipText = nodetooltip };
								if(hasComment) node.ForeColor = commentColor;
								nodes.Add(node);

								if(info.Index == selection.Index && info.Type == selection.Type)
									selectedNode = node;
							}
						}
					}
				}

				// Add category
				if(nodes.Count > 0) 
				{
					switch(currentSortMode)
					{
						case SortMode.SORT_BY_ACTION:
						{
							Dictionary<int, TreeNode> categories = new Dictionary<int, TreeNode>();
							TreeNode noAction = new TreeNode("No Effect", 2, 2);

							foreach(TreeNode node in nodes) 
							{
								NodeInfo info = node.Tag as NodeInfo;

								if(info.Action == 0) 
								{
									noAction.Nodes.Add(node);
									continue;
								}

								List<SectorEffectData> effects = GetSectorEffects(info.Action);
								if(effects.Count == 1 && !effects[0].IsGeneralized)
								{
									// The first node is already has all relevant data. Just add it
									if(!categories.ContainsKey(effects[0].Effect))
									{
										TreeNode catnode = new TreeNode(effects[0].Effect + " - " + effects[0].CategoryName, 2, 2, new[] {node});
										catnode.Tag = effects[0].Effect;
										categories.Add(effects[0].Effect, catnode);
									}
									else
									{
										categories[effects[0].Effect].Nodes.Add(node);
									}
								}
								else
								{
									// Add generalized effects
									foreach(SectorEffectData data in effects)
									{
										// Create NodeInfo for each effect... 
										NodeInfo geninfo = new NodeInfo(info, data.Effect);
										
										// Copy the initial node, otherwise it won't be added several times
										string name = geninfo.GetName(ref comment, currentSortMode);
										TreeNode nodecopy = new TreeNode(name, node.ImageIndex, node.SelectedImageIndex)
										{
											Tag = geninfo,
											ToolTipText = node.ToolTipText
										};

										if(!categories.ContainsKey(data.Effect))
										{
											TreeNode catnode = new TreeNode(data.Effect + " - " + data.CategoryName, 2, 2, new[] {nodecopy});
											catnode.Tag = data.Effect;
											categories.Add(data.Effect, catnode);
										}
										else
										{
											categories[data.Effect].Nodes.Add(nodecopy);
										}
									}
								}
							}

							// Because of generalized effects shenanigans, category and nodes resortings are required...
							TreeNode[] catnodes = new TreeNode[categories.Values.Count];
							categories.Values.CopyTo(catnodes, 0);

							// Sort categories
							Array.Sort(catnodes, delegate(TreeNode t1, TreeNode t2)
							{
								int effect1 = (int)t1.Tag;
								int effect2 = (int)t2.Tag;

								if(effect1 > effect2)  return 1;
								if(effect1 == effect2) return 0;
								return -1;
							});

							// Sort nodes
							foreach(TreeNode n in catnodes)
							{
								if(n.Nodes.Count > 0)
								{
									List<TreeNode> tosort = new List<TreeNode>(n.Nodes.Count);
									foreach(TreeNode nn in n.Nodes) tosort.Add(nn);
									Sort(ref tosort, currentSortMode);

									n.Nodes.Clear();
									n.Nodes.AddRange(tosort.ToArray());
								}
							}

							TreeNode category = new TreeNode(CAT_SECTORS, 2, 2, catnodes);
							if(noAction.Nodes.Count > 0) category.Nodes.Add(noAction);

							treeView.Nodes.Add(category);
						}
						break;

						case SortMode.SORT_BY_TAG:
						{
							Dictionary<int, TreeNode> categories = new Dictionary<int, TreeNode>();
							TreeNode noTag = new TreeNode("No Tag", 2, 2);

							// Sort nodes
							Sort(ref nodes, currentSortMode);

							foreach(TreeNode node in nodes) 
							{
								NodeInfo nodeInfo = node.Tag as NodeInfo;

								if(nodeInfo.Tag == 0) 
								{
									noTag.Nodes.Add(node);
									continue;
								}

								if(!categories.ContainsKey(nodeInfo.Tag)) 
								{
									string title = "Tag " + nodeInfo.Tag;
									if(General.Map.Options.TagLabels.ContainsKey(nodeInfo.Tag))
										title += ": " + General.Map.Options.TagLabels[nodeInfo.Tag];

									categories.Add(nodeInfo.Tag, new TreeNode(title, 2, 2, new[] { node }));
								} 
								else 
								{
									categories[nodeInfo.Tag].Nodes.Add(node);
								}
							}
							TreeNode[] catNodes = new TreeNode[categories.Values.Count];
							categories.Values.CopyTo(catNodes, 0);

							TreeNode category = new TreeNode(CAT_SECTORS, 2, 2, catNodes);
							if(noTag.Nodes.Count > 0) category.Nodes.Add(noTag);

							treeView.Nodes.Add(category);
						}
						break;

						default:
							// Sort nodes
							Sort(ref nodes, currentSortMode);

							treeView.Nodes.Add(new TreeNode(CAT_SECTORS, 2, 2, nodes.ToArray()));
							break;
					}
				}
			}

//add linedefs
			nodes = new List<TreeNode>();
			ICollection<Linedef> linedefs = General.Map.Map.Linedefs;

			if(!(linedefs is MapElementCollection<Linedef>)) //don't want to enumerate when array is locked
			{ 
				foreach(Linedef l in linedefs) 
				{
					if((showTags && l.Tag != 0) || (showActions && l.Action > 0)) 
					{
						if(filteredactions.Count > 0 && !filteredactions.Contains(l.Action)) continue;

						NodeInfo firstinfo = new NodeInfo(l, 0);
						if(filteredpolyobjects.Count > 0 && !filteredpolyobjects.Contains(firstinfo.PolyobjectNumber)) continue;

						for(int i = 0; i < l.Tags.Count; i++)
						{
							if(filteredtags.Count > 0 && !filteredtags.Contains(l.Tags[i])) continue;

							NodeInfo info = new NodeInfo(l, i);
							string name = info.GetName(ref comment, currentSortMode);
							hasComment = comment.Length > 0;

							if(!hasComment && cbCommentsOnly.Checked) continue;
							if(!udmf || serachStr.Length == 0 || (hasComment && comment.ToLowerInvariant().IndexOf(serachStr) != -1))
							{
								TreeNode node = new TreeNode(name, 5, 5) { Tag = info, ToolTipText = nodetooltip };
								if(hasComment) node.ForeColor = commentColor;
								nodes.Add(node);

								if(info.Index == selection.Index && info.Type == selection.Type)
									selectedNode = node;
							}
						}
					} 
					else if(currentDisplayMode == DISPLAY_POLYOBJECTS) 
					{
						NodeInfo info = new NodeInfo(l, 0);
						if(info.PolyobjectNumber != int.MinValue && (filteredpolyobjects.Count == 0 || filteredpolyobjects.Contains(info.PolyobjectNumber))) 
						{
							string name = info.GetName(ref comment, SortMode.SORT_BY_POLYOBJ_NUMBER);
							TreeNode node = new TreeNode(name, 1, 1) { Tag = info, ToolTipText = nodetooltip };
							nodes.Add(node);
						}
					}
				}

				//sort nodes
				Sort(ref nodes, currentSortMode);

				//add category
				if(nodes.Count > 0) 
				{
					switch(currentSortMode)
					{
						case SortMode.SORT_BY_ACTION:
						{
							Dictionary<int, TreeNode> categories = new Dictionary<int, TreeNode>();
							TreeNode noAction = new TreeNode("No Action", 4, 4);

							foreach(TreeNode node in nodes)
							{
								NodeInfo nodeInfo = node.Tag as NodeInfo;

								if(nodeInfo.Action == 0)
								{
									noAction.Nodes.Add(node);
									continue;
								}

								LinedefActionInfo lai = General.Map.Config.GetLinedefActionInfo(nodeInfo.Action);

								if(!categories.ContainsKey(lai.Index)) categories.Add(lai.Index, new TreeNode(lai.Index + " - " + lai.Title, 4, 4, new[] { node }));
								else categories[lai.Index].Nodes.Add(node);
							}
							TreeNode[] catNodes = new TreeNode[categories.Values.Count];
							categories.Values.CopyTo(catNodes, 0);

							TreeNode category = new TreeNode(CAT_LINEDEFS, 4, 4, catNodes);
							if(noAction.Nodes.Count > 0) category.Nodes.Add(noAction);

							treeView.Nodes.Add(category);
						}
						break;

						case SortMode.SORT_BY_TAG:
						{
							Dictionary<int, TreeNode> categories = new Dictionary<int, TreeNode>();
							TreeNode noTag = new TreeNode("No Tag", 4, 4);

							foreach(TreeNode node in nodes) 
							{
								NodeInfo nodeInfo = node.Tag as NodeInfo;

								if(nodeInfo.Tag == 0) 
								{
									noTag.Nodes.Add(node);
									continue;
								}

								if(!categories.ContainsKey(nodeInfo.Tag)) 
								{
									string title = "Tag " + nodeInfo.Tag;
									if(General.Map.Options.TagLabels.ContainsKey(nodeInfo.Tag))
										title += ": " + General.Map.Options.TagLabels[nodeInfo.Tag];

									categories.Add(nodeInfo.Tag, new TreeNode(title, 4, 4, new[] { node }));
								} 
								else 
								{
									categories[nodeInfo.Tag].Nodes.Add(node);
								}
							}
							TreeNode[] catNodes = new TreeNode[categories.Values.Count];
							categories.Values.CopyTo(catNodes, 0);

							TreeNode category = new TreeNode(CAT_LINEDEFS, 4, 4, catNodes);
							if(noTag.Nodes.Count > 0) category.Nodes.Add(noTag);

							treeView.Nodes.Add(category);
						}
						break;

						default:
							treeView.Nodes.Add(new TreeNode(CAT_LINEDEFS, 4, 4, nodes.ToArray()));
							break;
					}
				}
			}

			//expand top level nodes
			foreach(TreeNode t in treeView.Nodes) t.Expand();

			if(selectedNode != null)
				treeView.SelectedNode = selectedNode;
			else if(treeView.Nodes.Count > 0)
				treeView.SelectedNode = treeView.Nodes[0];

			this.ResumeLayout();

			//update Export button
			bExportToFile.Enabled = (treeView.Nodes != null && treeView.Nodes.Count > 0);

			// Loose focus when the main windows is active
			if(focusDisplay) General.Interface.FocusDisplay();
		}

//tag/action search
		private static void GetSpecialValues(string serachstr, ref HashSet<int> filteredtags, ref HashSet<int> filteredactions, ref HashSet<int> filteredpolyobjects) 
		{
			if(serachstr.Length == 0) return;

			string[] tags = serachstr.Split(new[]{ '#' });
			foreach(string tagstr in tags)
			{
				int tag = ReadNumber(tagstr);
				if(tag != int.MinValue) filteredtags.Add(tag);
			}

			string[] actions = serachstr.Split(new[] { '$' });
			foreach(string actionstr in actions) 
			{
				int action = ReadNumber(actionstr);
				if(action != int.MinValue) filteredactions.Add(action);
			}

			string[] ponums = serachstr.Split(new[] { '^' });
			foreach(string postr in ponums) 
			{
				int ponum = ReadNumber(postr);
				if(ponum != int.MinValue) filteredpolyobjects.Add(ponum);
			}
		}

		private static int ReadNumber(string str) 
		{
			string token = "";
			int pos = 0;

			while(pos < str.Length && Configuration.NUMBERS.IndexOf(str[pos]) != -1) 
			{
				token += str[pos];
				pos++;
			}

			if(token.Length > 0) 
			{
				int result;
				if(int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
					return result;
			}

			return int.MinValue;
		}

		#endregion

		#region ================== Sorting

		private void Sort(ref List<TreeNode> nodes, string sortMode)
		{
			switch(sortMode)
			{
				case SortMode.SORT_BY_ACTION:
					nodes.Sort(SortByAction);
					break;

				case SortMode.SORT_BY_TAG:
					nodes.Sort(SortByTag);
					break;

				case SortMode.SORT_BY_INDEX:
					if(currentDisplayMode == DISPLAY_POLYOBJECTS) 
						nodes.Sort(SortByPolyobjectNumber);
					else
						nodes.Sort(SortByIndex);
					break;

				default:
					throw new NotImplementedException("Tag Explorer: Sort mode \"" + sortMode + "\" is not implemented!");
			}
		}

		private static int SortByAction(TreeNode t1, TreeNode t2) 
		{
			NodeInfo i1 = t1.Tag as NodeInfo;
			NodeInfo i2 = t2.Tag as NodeInfo;

			if(i1.Action == i2.Action) return SortByTag(t1, t2);
			if(i1.Action == 0) return 1; //push items with no action to the end of the list
			if(i2.Action == 0) return -1; //push items with no action to the end of the list
			if(i1.Action > i2.Action) return 1;
			return -1; //should be i1 < i2
		}

		private static int SortByTag(TreeNode t1, TreeNode t2) 
		{
			NodeInfo i1 = t1.Tag as NodeInfo;
			NodeInfo i2 = t2.Tag as NodeInfo;

			if(i1.Tag == i2.Tag) return SortByIndex(t1, t2);
			if(i1.Tag == 0) return 1; //push items with no tag to the end of the list
			if(i2.Tag == 0) return -1; //push items with no tag to the end of the list
			if(i1.Tag > i2.Tag) return 1;
			return -1; //should be i1 < i2
		}

		private static int SortByIndex(TreeNode t1, TreeNode t2) 
		{
			NodeInfo i1 = t1.Tag as NodeInfo;
			NodeInfo i2 = t2.Tag as NodeInfo;

			if(i1.Index > i2.Index) return 1;
			if(i1.Index == i2.Index) return 0;
			return -1;
		}

		private static int SortByPolyobjectNumber(TreeNode t1, TreeNode t2) 
		{
			NodeInfo i1 = t1.Tag as NodeInfo;
			NodeInfo i2 = t2.Tag as NodeInfo;

			if(i1.PolyobjectNumber > i2.PolyobjectNumber) return 1;
			if(i1.PolyobjectNumber == i2.PolyobjectNumber)
				return String.CompareOrdinal(i1.DefaultName, i2.DefaultName);
			return -1;
		}

		// This returns information on a sector effect
		private static List<SectorEffectData> GetSectorEffects(int effect)
		{
			List<SectorEffectData> result = new List<SectorEffectData>(1);
			
			// No effect?
			if(effect == 0)
			{
				result.Add(new SectorEffectData { Effect = 0, CategoryName = "None", EffectName = "None" });
			}
			// Known effect?
			else if(General.Map.Config.SectorEffects.ContainsKey(effect))
			{
				string title = General.Map.Config.SectorEffects[effect].Title;
				result.Add(new SectorEffectData { Effect = effect, CategoryName = title, EffectName = title });
			}
			else
			{
				// Generalized effect(s)?
				int initialeffect = effect;

				// Check all effects, in bigger to smaller order
				for(int i = General.Map.Config.GenEffectOptions.Count - 1; i > -1; i--)
				{
					for(int j = General.Map.Config.GenEffectOptions[i].Bits.Count - 1; j > -1; j--)
					{
						GeneralizedBit bit = General.Map.Config.GenEffectOptions[i].Bits[j];
						if(bit.Index > 0 && (effect & bit.Index) == bit.Index)
						{
							initialeffect -= bit.Index;
							result.Add(new SectorEffectData
							           {
								           Effect = bit.Index, 
										   CategoryName = General.Map.Config.GenEffectOptions[i].Name, 
										   EffectName = General.Map.Config.GenEffectOptions[i].Name + ": " + bit.Title, 
										   IsGeneralized = true,
							           });
							break;
						}
					}
				}

				// Add combined effect?
				if(result.Count > 0 && initialeffect > 0)
				{
					string combinedeffect = General.Map.Config.GetGeneralizedSectorEffectName(effect);
					result.Add(new SectorEffectData { Effect = effect, CategoryName = combinedeffect, EffectName = combinedeffect + ": " + effect, });
				}

				if(initialeffect > 0)
				{
					// Insert non-generalized effect as the first one
					if(General.Map.Config.SectorEffects.ContainsKey(initialeffect))
					{
						string title = General.Map.Config.SectorEffects[initialeffect].Title;
						result.Insert(0, new SectorEffectData { Effect = initialeffect, CategoryName = title, EffectName = title });
					}
					else
					{
						// Unknown effect...
						result.Insert(0, new SectorEffectData { Effect = initialeffect, CategoryName = "Unknown", EffectName = "Unknown" });
					}
				}
				else if(result.Count == 0)
				{
					// Unknown effect...
					result.Add(new SectorEffectData { Effect = effect, CategoryName = "Unknown", EffectName = "Unknown" });
				}
			}

			return result;
		}

		#endregion

		#region ================== Events

		private void cbDisplayMode_SelectedIndexChanged(object sender, EventArgs e) 
		{
			currentDisplayMode = cbDisplayMode.SelectedItem.ToString();
			UpdateTree(true);
		}

		private void cbSortMode_SelectedIndexChanged(object sender, EventArgs e) 
		{
			currentSortMode = cbSortMode.SelectedItem.ToString();
			UpdateTree(true);
		}

		private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e) 
		{
			NodeInfo info = e.Node.Tag as NodeInfo;
			if(info == null) return;

			//store selection
			selection.Type = info.Type;
			selection.Index = info.Index;

			if(e.Button == MouseButtons.Right) //open element properties
			{ 
				bool updateDisplay = false;
				
				switch(info.Type) 
				{
					case NodeInfoType.THING:
						Thing t = General.Map.Map.GetThingByIndex(info.Index);
						updateDisplay = (t != null && General.Interface.ShowEditThings(new List<Thing>() { t }) == DialogResult.OK);
						break;

					case NodeInfoType.SECTOR:
						Sector s = General.Map.Map.GetSectorByIndex(info.Index);
						updateDisplay = (s != null && General.Interface.ShowEditSectors(new List<Sector>() { s }) == DialogResult.OK);
						break;

					case NodeInfoType.LINEDEF:
						Linedef l = General.Map.Map.GetLinedefByIndex(info.Index);
						updateDisplay = (l != null && General.Interface.ShowEditLinedefs(new List<Linedef>() { l }) == DialogResult.OK);
						break;

					default:
						General.ErrorLogger.Add(ErrorType.Warning, "Tag Explorer: got unknown category: " + info.Type);
						break;
				}

				if(updateDisplay) 
				{
					// Update entire display
					General.Map.Map.Update();
					General.Interface.RedrawDisplay();
					UpdateTree(true);
				}

			} 
			else 
			{
				//select element?
				if(cbSelectOnClick.Checked) 
				{
					// Leave any volatile mode
					General.Editing.CancelVolatileMode();
					General.Map.Map.ClearAllSelected();

					//make selection
					switch(info.Type)
					{
						case NodeInfoType.THING:
						{
							if(General.Editing.Mode.GetType().Name != "ThingsMode") General.Editing.ChangeMode("ThingsMode");
							Thing t = General.Map.Map.GetThingByIndex(info.Index);
							if(t != null) t.Selected = true;
						}
						break;

						case NodeInfoType.LINEDEF:
						{
							if(General.Editing.Mode.GetType().Name != "LinedefsMode") General.Editing.ChangeMode("LinedefsMode");
							Linedef l = General.Map.Map.GetLinedefByIndex(info.Index);
							if(l != null) l.Selected = true;
						}
						break;

						default:
						{
							if(General.Editing.Mode.GetType().Name != "SectorsMode") General.Editing.ChangeMode("SectorsMode");
							Sector s = General.Map.Map.GetSectorByIndex(info.Index);
							if(s != null) 
							{
								((ClassicMode)General.Editing.Mode).SelectMapElement(s);
								foreach(Sidedef sd in s.Sidedefs) sd.Line.Selected = true;
							}
						}
						break;
					}
				}

				//focus on element?
				if(cbCenterOnSelected.Checked) 
				{
					List<Vector2D> points = new List<Vector2D>();
					RectangleF area = MapSet.CreateEmptyArea();

					switch(info.Type)
					{
						case NodeInfoType.LINEDEF:
						{
							Linedef l = General.Map.Map.GetLinedefByIndex(info.Index);
							points.Add(l.Start.Position);
							points.Add(l.End.Position);
						}
						break;

						case NodeInfoType.SECTOR:
						{
							Sector s = General.Map.Map.GetSectorByIndex(info.Index);
							foreach(Sidedef sd in s.Sidedefs) 
							{
								points.Add(sd.Line.Start.Position);
								points.Add(sd.Line.End.Position);
							}
						}
						break;

						case NodeInfoType.THING:
						{
							Thing t = General.Map.Map.GetThingByIndex(info.Index);
							Vector2D p = t.Position;
							points.Add(p);
							points.Add(p + new Vector2D(t.Size * 2.0f, t.Size * 2.0f));
							points.Add(p + new Vector2D(t.Size * 2.0f, -t.Size * 2.0f));
							points.Add(p + new Vector2D(-t.Size * 2.0f, t.Size * 2.0f));
							points.Add(p + new Vector2D(-t.Size * 2.0f, -t.Size * 2.0f));
						}
						break;

						default:
							General.Fail("Tag Explorer: unknown object type given to zoom in on!");
							break;
					}

					// Make a view area from the points
					foreach(Vector2D p in points) area = MapSet.IncreaseArea(area, p);

					// Make the area square, using the largest side
					if(area.Width > area.Height) 
					{
						float delta = area.Width - area.Height;
						area.Y -= delta * 0.5f;
						area.Height += delta;
					} 
					else 
					{
						float delta = area.Height - area.Width;
						area.X -= delta * 0.5f;
						area.Width += delta;
					}

					// Add padding
					area.Inflate(100f, 100f);

					// Zoom to area
					ClassicMode editmode = (General.Editing.Mode as ClassicMode);
					editmode.CenterOnArea(area, 0.6f);
				}

				// Update info and view
				General.Editing.Mode.UpdateSelectionInfo();
				General.Interface.RedrawDisplay();
			}
		}

		private void treeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e) 
		{
			//edit comment
			if(udmf && currentDisplayMode != DISPLAY_POLYOBJECTS) 
			{
				NodeInfo info = e.Node.Tag as NodeInfo;
				if(info == null) return;

				treeView.LabelEdit = true;
				e.Node.Text = info.Comment; //set node text to comment
				e.Node.BeginEdit(); //begin editing
			}
		}

		//we don't want to edit categories if we are in UDMF
		private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e) 
		{
			if(!udmf || !treeView.LabelEdit || e.Node.Tag == null) e.CancelEdit = true;
		}

		//map should be in UDMF format, or we wouldn't be here
		private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e) 
		{
			NodeInfo info = e.Node.Tag as NodeInfo;
			string comment = "";

			//to set comment manually we actually have to cancel edit...
			e.CancelEdit = true;
			e.Node.EndEdit(true);
			treeView.LabelEdit = false;

			//apply comment
			if(e.Label != null)	info.Comment = e.Label;
			e.Node.Text = info.GetName(ref comment, currentSortMode);
			e.Node.ForeColor = string.IsNullOrEmpty(info.Comment) ? Color.Black : commentColor;

			// Because of multiple tags, several nodes can link to the same sector/linedef
			UpdateTree(false);
		}

		//It is called every time a dialog window closes.
		private void ParentForm_Activated(object sender, EventArgs e)
		{
			UpdateTreeSoon();
		}

		private void btnClearSearch_Click(object sender, EventArgs e) 
		{
			tbSearch.Clear();
		}

		private void tbSearch_TextChanged(object sender, EventArgs e) 
		{
			if(tbSearch.Text.Length > 1 || tbSearch.Text.Length == 0) UpdateTree(false);
		}

		private void cbCommentsOnly_CheckedChanged(object sender, EventArgs e) 
		{
			UpdateTree(true);
		}

		private void updatetimer_Tick(object sender, EventArgs e) 
		{
			updatetimer.Stop();
			UpdateTree(Form.ActiveForm == General.Interface);
		}

		private void bExportToFile_Click(object sender, EventArgs e) 
		{
			if(treeView.Nodes == null || treeView.Nodes.Count == 0) return;

			// Show save dialog
			string path = Path.GetDirectoryName(General.Map.FilePathName);
			saveFileDialog.InitialDirectory = path;
			saveFileDialog.FileName = Path.GetFileNameWithoutExtension(General.Map.FileTitle) + "_info.txt";
			if(saveFileDialog.ShowDialog(this) == DialogResult.Cancel) return;

			// Generate stuff
			StringBuilder sb = new StringBuilder();

			//top level
			foreach(TreeNode n in treeView.Nodes) 
			{
				if(n.Nodes.Count == 0) continue;

				if(sb.Length > 0) sb.AppendLine(Environment.NewLine);
				sb.AppendLine(n.Text.Replace(":", " (" + currentSortMode.ToLowerInvariant() + "):"));

				//second level
				foreach(TreeNode cn in n.Nodes) 
				{
					//third level
					if(cn.Nodes.Count > 0) 
					{
						sb.AppendLine("  " + cn.Text + ":");
						foreach(TreeNode ccn in cn.Nodes) sb.AppendLine("    " + ccn.Text);
					} 
					else 
					{
						sb.AppendLine("  " + cn.Text);
					}
				}
			}
			
			// Save to file
			try
			{
				using(StreamWriter sw = File.CreateText(saveFileDialog.FileName)) sw.Write(sb.ToString());
				General.Interface.DisplayStatus(StatusType.Info, "Tag info successfully saved.");
			}
			catch(Exception ex)
			{
				General.ErrorLogger.Add(ErrorType.Error, "Failed to save tag info: " + ex.Message);
				General.Interface.DisplayStatus(StatusType.Info, "Failed to save tag info...");
			}
		}

		#endregion
	}
}