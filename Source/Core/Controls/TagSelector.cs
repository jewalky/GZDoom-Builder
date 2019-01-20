#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Map;
using System.Globalization;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal struct TagInfo
	{
		public readonly string Label;
		public readonly int Tag;

		public TagInfo(int tag, string label) 
		{
			Label = (string.IsNullOrEmpty(label) ? tag.ToString() : tag + " - " + label);
			Tag = tag;
		}

		public override string ToString() 
		{
			return Label;
		}
	}
	
	public partial class TagSelector : UserControl
	{
		#region ================== Variables

		private List<int> tags;
		private List<TagInfo> infos;
		private bool valid;
		private int tag;
		private UniversalType elementtype;
		private int rangemode; //0 - none, 1 - positive (>=), -1 - negative (<=)
		private int offsetmode; //0 - none, 1 - positive (++), -1 - negative (--)

		#endregion

		#region ================== Constructor

		public TagSelector() 
		{
			InitializeComponent();
		}

		#endregion

		#region ================== Methods

		public void Setup(UniversalType mapelementtype) 
		{
			tags = new List<int>();
			infos = new List<TagInfo>();
			this.elementtype = mapelementtype;

			//collect used tags from appropriate element type...
			switch(elementtype)
			{
				case UniversalType.SectorTag:
					foreach(Sector s in General.Map.Map.Sectors)
					{
						foreach(int t in s.Tags)
						{
							if(t == 0 || tags.Contains(t)) continue;
							tags.Add(t);
						}
					}
					break;

				case UniversalType.LinedefTag:
					if(General.Map.FormatInterface.HasLinedefTag)
					{
						foreach(Linedef l in General.Map.Map.Linedefs)
						{
							foreach(int t in l.Tags)
							{
								if(t == 0 || tags.Contains(t)) continue;
								tags.Add(t);
							}
						}
					}
					break;

				case UniversalType.ThingTag:
					if(General.Map.FormatInterface.HasThingTag)
					{
						foreach(Thing t in General.Map.Map.Things)
						{
							if(t.Tag == 0 || tags.Contains(t.Tag)) continue;
							tags.Add(t.Tag);
						}
					}
					break;
			}

			//now sort them in descending order
			tags.Sort((a, b) => -1 * a.CompareTo(b));

			//create tag infos
			foreach(int tag in tags)
			{
				if(General.Map.Options.TagLabels.ContainsKey(tag)) //tag labels
					infos.Add(new TagInfo(tag, General.Map.Options.TagLabels[tag]));
				else
					infos.Add(new TagInfo(tag, string.Empty));
			}

			foreach(TagInfo info in infos) cbTagPicker.Items.Add(info);
			cbTagPicker.DropDownWidth = DoomBuilder.Geometry.Tools.GetDropDownWidth(cbTagPicker);
		}

		public void SetTag(int newTag) 
		{
			if(tags.Contains(newTag)) 
			{
				cbTagPicker.SelectedIndex = tags.IndexOf(newTag);
			} 
			else 
			{
				cbTagPicker.SelectedIndex = -1;
				cbTagPicker.Text = newTag.ToString();
			}
			tag = newTag;
			valid = true;
		}

		public void ClearTag() 
		{
			cbTagPicker.SelectedIndex = -1;
			cbTagPicker.Text = string.Empty;
			rangemode = 0;
			offsetmode = 0;
			valid = false;
		}

		public int GetTag(int original) 
		{
			return (valid ? tag : original);
		}

		public int GetSmartTag(int original, int offset) 
		{
			if(!valid) return original;
			if(rangemode != 0) return tag + offset * rangemode;
			if(offsetmode != 0) return original + tag * offsetmode;
			return tag;
		}

		public void ValidateTag() 
		{
			rangemode = 0;
			offsetmode = 0;
			
			if(cbTagPicker.SelectedIndex != -1) 
			{
				tag = infos[cbTagPicker.SelectedIndex].Tag;
				valid = true;
				return;
			}

			//check text
			string text = cbTagPicker.Text.Trim().ToLowerInvariant();
			if(string.IsNullOrEmpty(text)) 
			{
				valid = false;
				return;
			}

			//incremental?
			if(text.Length > 2)
			{
				if(text.StartsWith(">=")) //range up
				{ 
					rangemode = 1;
					text = text.Substring(2, text.Length - 2);
				} 
				else if(text.StartsWith("<=")) //range down
				{ 
					rangemode = -1;
					text = text.Substring(2, text.Length - 2);
				} 
				else if(text.StartsWith("++")) //relative up
				{ 
					offsetmode = 1;
					text = text.Substring(2, text.Length - 2);
				} 
				else if(text.StartsWith("--")) //relative down
				{ 
					offsetmode = -1;
					text = text.Substring(2, text.Length - 2);
				}
			} 

			if(!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out tag)) 
			{
				//maybe it's user-pasted label?
				foreach(TagInfo info in infos) 
				{
					if(info.Label.ToLowerInvariant().Contains(text)) 
					{
						tag = info.Tag;
						valid = true;
						return;
					}
				}
				
				tag = General.Map.Map.GetNewTag();

				//create new tag label
				if(General.Map.Options.TagLabels.ContainsKey(tag))
					General.Map.Options.TagLabels[tag] = cbTagPicker.Text.Trim();
				else
					General.Map.Options.TagLabels.Add(tag, cbTagPicker.Text.Trim());
			}

			valid = true;
		}

		#endregion

		#region ================== Events

		private void newTag_Click(object sender, EventArgs e) 
		{
			//todo: check tag labels?
			tag = General.Map.Map.GetNewTag();
			cbTagPicker.SelectedIndex = -1;
			cbTagPicker.Text = tag.ToString();
			valid = true;
		}

		private void unusedTag_Click(object sender, EventArgs e) 
		{
			tag = General.Map.Map.GetNewTag(elementtype);
			cbTagPicker.SelectedIndex = -1;
			cbTagPicker.Text = tag.ToString();
			valid = true;
		}

		private void clear_Click(object sender, EventArgs e) 
		{
			tag = 0;
			cbTagPicker.SelectedIndex = -1;
			cbTagPicker.Text = tag.ToString();
			valid = true;
		}

		//mxd. Because anchor-based alignment fails when using high-Dpi settings...
		private void TagSelector_Resize(object sender, EventArgs e)
		{
			clear.Left = this.Width - clear.Width - clear.Margin.Right;
			unusedTag.Left = clear.Left - unusedTag.Margin.Right - unusedTag.Width;
			newTag.Left = unusedTag.Left - newTag.Margin.Right - newTag.Width;
			buttons.Left = newTag.Left - newTag.Margin.Left - buttons.Width;
			cbTagPicker.Width = buttons.Left - cbTagPicker.Margin.Right - cbTagPicker.Left;
		}

		//mxd
		private void buttons_ValueChanged(object sender, EventArgs e)
		{
			if(buttons.Value == 0) return;
			
			ValidateTag();

			if(!valid) tag = 0;
			else tag = General.Clamp(tag + (buttons.Value < 0 ? 1 : -1), General.Map.FormatInterface.MinTag, General.Map.FormatInterface.MaxTag);

			cbTagPicker.SelectedIndex = -1;
			cbTagPicker.Text = tag.ToString();
			buttons.Value = 0;
			valid = true;
		}

		#endregion
	}
}
