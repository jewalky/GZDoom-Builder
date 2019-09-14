using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Windows;
using CodeImp.DoomBuilder.Types;
using CodeImp.DoomBuilder.Map;

namespace CodeImp.DoomBuilder.TagRange
{
	public partial class TagRangeForm : DelayedForm
	{
		private UniversalType selectiontype;
		private int selectioncount;
		Dictionary<int, bool> usedtags;
		private List<int> initialtags; //mxd

		//mxd. Persistent settings
		private static int storedstep = 1;
		private static bool storedrelative;
		
		public int SelectionCount { get { return selectioncount; } }
		
		// Constructor
		public TagRangeForm()
		{
			InitializeComponent();
		}
		
		// This sets up the form
		public void Setup()
		{
			General.Map.Map.ClearAllMarks(false);
			
			switch(General.Editing.Mode.GetType().Name)
			{
				case "SectorsMode":
				{
					General.Map.Map.MarkSelectedSectors(true, true); //mxd
					selectiontype = UniversalType.SectorTag;
					ICollection<Sector> list = General.Map.Map.GetSelectedSectors(true);
					initialtags = new List<int>(list.Count); //mxd
					foreach(Sector element in list) initialtags.Add(element.Tag); //mxd
					selectioncount = list.Count;
					this.Text = "Create tag range for " + selectioncount + (selectioncount > 1 ? " sectors" : " sector");
				}
				break;

				case "LinedefsMode":
				{
					General.Map.Map.MarkSelectedLinedefs(true, true); //mxd
					selectiontype = UniversalType.LinedefTag;
					ICollection<Linedef> list = General.Map.Map.GetSelectedLinedefs(true);
					initialtags = new List<int>(list.Count); //mxd
					foreach(Linedef element in list) initialtags.Add(element.Tag); //mxd
					selectioncount = list.Count;
					this.Text = "Create tag range for " + selectioncount + (selectioncount > 1 ? " linedefs" : " linedef");
				}
				break;

				case "ThingsMode":
				{
					General.Map.Map.MarkSelectedThings(true, true); //mxd
					selectiontype = UniversalType.ThingTag;
					ICollection<Thing> list = General.Map.Map.GetSelectedThings(true);
					initialtags = new List<int>(list.Count); //mxd
					foreach(Thing element in list) initialtags.Add(element.Tag); //mxd
					selectioncount = list.Count;
					this.Text = "Create tag range for " + selectioncount + (selectioncount > 1 ? " things" : " thing");
				}
				break;
			}
			
			// Find out which tags are used
			usedtags = new Dictionary<int, bool>();
			General.Map.Map.ForAllTags(TagHandler, false, usedtags);
			
			// Find the first unused tag to use as range start
			int starttag = General.Map.Map.GetNewTag();
			rangestart.Text = starttag.ToString();

			//mxd. Apply saved settings
			rangestep.Text = storedstep.ToString();
			relativemode.Checked = storedrelative;

			//mxd. Do useless stuff
			if(General.Random(0, 255) > 230) bglabel.Text = "Creating tag ranges is fun! ^_^";
		}
		
		// Handler for finding a new tag
		private static void TagHandler(MapElement element, bool actionargument, UniversalType type, ref int value, Dictionary<int, bool> usedtags)
		{
			if(value > 0) usedtags[value] = true; //mxd
		}
		
		// This creates a range
		private List<int> CreateRange(int starttag, int increment, bool relative, bool skipusedtags, out bool tagsused, out bool outoftags) 
		{
			List<int> newtags = new List<int>(selectioncount);
			outoftags = false;
			tagsused = false;

			//mxd. Get relative tag range
			if(relative) 
			{
				int addtag = 0; // biwa

				// Go for the number of tags we need
				for(int i = 0; i < selectioncount; i++) 
				{
					int newtag = initialtags[i] + starttag + addtag;

					if(newtag > General.Map.FormatInterface.MaxTag || newtag < General.Map.FormatInterface.MinTag) 
					{
						outoftags = true;
						return newtags;
					}

					if(skipusedtags) 
					{
						// Find next unused tag
						while(usedtags.ContainsKey(newtag)) 
						{
							if(newtag >= General.Map.FormatInterface.MaxTag || newtag <= General.Map.FormatInterface.MinTag) 
							{
								outoftags = true;
								return newtags;
							}

							newtag += increment; //mxd // biwa
							addtag += increment; // biwa
						}
					} 
					else 
					{
						tagsused |= usedtags.ContainsKey(newtag);
					}

					newtags.Add(newtag);
					addtag += increment;
				}
			} 
			else //mxd. Get absolute tag range
			{
				// Go for the number of tags we need
				for(int i = 0; i < selectioncount; i++) 
				{
					if(starttag > General.Map.FormatInterface.MaxTag || starttag < General.Map.FormatInterface.MinTag) 
					{
						outoftags = true;
						return newtags;
					}

					if(skipusedtags) 
					{
						// Find next unused tag
						while(usedtags.ContainsKey(starttag)) 
						{
							if(starttag >= General.Map.FormatInterface.MaxTag || starttag <= General.Map.FormatInterface.MinTag) 
							{
								outoftags = true;
								return newtags;
							}

							starttag += increment; //mxd
						}
					} 
					else 
					{
						tagsused |= usedtags.ContainsKey(starttag);
					}

					newtags.Add(starttag);
					starttag += increment; //mxd
				}
			}
			
			return newtags;
		}
		
		// This updates the calculated range
		private void UpdateChanges()
		{
			bool outoftags, tagsused;
			int starttag = rangestart.GetResult(0);
			int step = rangestep.GetResult(1); //mxd
			
			List<int> tags = CreateRange(starttag, step, relativemode.Checked, skipdoubletags.Checked, out tagsused, out outoftags); //mxd

			outoftagswarning.Visible = outoftags;
			okbutton.Enabled = !outoftags;
			doubletagwarning.Visible = tagsused && !outoftags;
			skipdoubletags.Visible = tagsused && !outoftags;
			skipdoubletags.BringToFront();
			
			if(tags.Count > 0) endtaglabel.Text = tags[tags.Count - 1].ToString();
		}
		
		// Range start changes
		private void rangestart_WhenTextChanged(object sender, EventArgs e)
		{
			UpdateChanges();
		}
		
		// OK clicked
		private void okbutton_Click(object sender, EventArgs e)
		{
			bool outoftags, tagsused;
			int starttag = rangestart.GetResult(0);
			int step = rangestep.GetResult(1);

			//mxd
			List<int> tags = CreateRange(starttag, step, relativemode.Checked, skipdoubletags.Checked, out tagsused, out outoftags);

			if(outoftags)
			{
				General.ShowWarningMessage("The range exceeds the maximum allowed tags and cannot be created.", MessageBoxButtons.OK);
			}
			else
			{
				// Apply tags!
				if(selectiontype == UniversalType.SectorTag)
				{
					General.Map.UndoRedo.CreateUndo("Set " + selectioncount + " sector tags");
					ICollection<Sector> list = General.Map.Map.GetSelectedSectors(true);
					int index = 0;
					foreach(Sector s in list) s.Tag = tags[index++];
				}
				else if(selectiontype == UniversalType.LinedefTag)
				{
					General.Map.UndoRedo.CreateUndo("Set " + selectioncount + " linedef tags");
					ICollection<Linedef> list = General.Map.Map.GetSelectedLinedefs(true);
					int index = 0;
					foreach(Linedef l in list) l.Tag = tags[index++];
				}
				else if(selectiontype == UniversalType.ThingTag)
				{
					General.Map.UndoRedo.CreateUndo("Set " + selectioncount + " thing tags");
					ICollection<Thing> list = General.Map.Map.GetSelectedThings(true);
					int index = 0;
					foreach(Thing t in list) t.Tag = tags[index++];
				}

				//mxd. Store settings
				storedstep = rangestep.GetResult(1);
				storedrelative = relativemode.Checked;

				//We are done here.
				this.Close();
			}
		}
		
		// Cancel clicked
		private void cancelbutton_Click(object sender, EventArgs e)
		{
			this.Close();
		}
		
		// Skip over used tags changed
		private void skipdoubletags_CheckedChanged(object sender, EventArgs e)
		{
			UpdateChanges();
		}

		//mxd
		private void rangestep_WhenTextChanged(object sender, EventArgs e) 
		{
			UpdateChanges();
		}

		//mxd
		private void relativemode_CheckedChanged(object sender, EventArgs e) 
		{
			rangestart.AllowNegative = relativemode.Checked;
			UpdateChanges();
		}
	}
}

