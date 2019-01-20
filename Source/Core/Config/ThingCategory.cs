
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CodeImp.DoomBuilder.IO;
using CodeImp.DoomBuilder.ZDoom;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	public class ThingCategory
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// Things
		private List<ThingTypeInfo> things;
		
		// Category properties
		private readonly string name;
		private readonly string title;
		private readonly bool sorted;
		private readonly List<ThingCategory> children; //mxd 

		// Thing properties for inheritance
		private readonly string sprite;
		private readonly int color;
		private readonly float alpha; //mxd
		private readonly string renderstyle; //mxd
		private readonly int arrow;
		private readonly float radius;
		private readonly float height;
		private readonly int hangs;
		private readonly int blocking;
		private readonly int errorcheck;
		private readonly bool fixedsize;
		private readonly bool fixedrotation; //mxd
		private readonly bool absolutez;
		private readonly float spritescale;
		
		// Disposing
		private bool isdisposed;

		//mxd. Validity
		private bool isinvalid;

        // [ZZ]
        private bool optional;
		
		#endregion

		#region ================== Properties

		public string Name { get { return name; } }
		public string Title { get { return title; } }
		public string Sprite { get { return sprite; } }
		public bool Sorted { get { return sorted; } }
		public List<ThingCategory> Children { get { return children; } } //mxd
		public int Color { get { return color; } }
		public float Alpha { get { return alpha; } } //mxd
		public string RenderStyle { get { return renderstyle; } } //mxd
		public int Arrow { get { return arrow; } }
		public float Radius { get { return radius; } }
		public float Height { get { return height; } }
		public int Hangs { get { return hangs; } }
		public int Blocking { get { return blocking; } }
		public int ErrorCheck { get { return errorcheck; } }
		public bool FixedSize { get { return fixedsize; } }
		public bool FixedRotation { get { return fixedrotation; } } //mxd
		public bool IsDisposed { get { return isdisposed; } }
		public bool IsValid { get { return !isinvalid; } } //mxd
		public bool AbsoluteZ { get { return absolutez; } }
		public float SpriteScale { get { return spritescale; } }
		public List<ThingTypeInfo> Things { get { return things; } }
        public bool Optional { get { return optional; } }

		#endregion

		#region ================== Constructor / Disposer
		
		// Constructor
		internal ThingCategory(ThingCategory parent, string name, string title, DecorateCategoryInfo catinfo)
		{
			// Initialize
			this.name = name;
			this.title = title;
			this.things = new List<ThingTypeInfo>();
			this.children = new List<ThingCategory>();
			
			//mxd. Copy properties from the parent
			if(parent != null)
			{
				this.sprite = parent.sprite;
				this.sorted = parent.sorted;
				this.color = parent.color;
				this.alpha = parent.alpha;
				this.renderstyle = parent.renderstyle;
				this.arrow = parent.arrow;
				this.radius = parent.radius;
				this.height = parent.height;
				this.hangs = parent.hangs;
				this.blocking = parent.blocking;
				this.errorcheck = parent.errorcheck;
				this.fixedsize = parent.fixedsize;
				this.fixedrotation = parent.fixedrotation;
				this.absolutez = parent.absolutez;
				this.spritescale = parent.spritescale;
                this.optional = parent.optional;
			}
			// Set default properties
			else
			{
				this.sprite = "";
				this.sorted = true;
				this.color = 18;
				this.alpha = 1f; //mxd
				this.renderstyle = "normal"; //mxd
				this.arrow = 1;
				this.radius = 10;
				this.height = 20;
				this.hangs = 0;
				this.blocking = 0;
				this.errorcheck = 1;
				this.fixedsize = false;
				this.fixedrotation = false; //mxd
				this.absolutez = false;
				this.spritescale = 1.0f;
                this.optional = false;
			}

			//mxd. Apply DecorateCategoryInfo overrides...
			if(catinfo != null && catinfo.Properties.Count > 0)
			{
				this.sprite = catinfo.GetPropertyValueString("$sprite", 0, this.sprite);
				this.sorted = (catinfo.GetPropertyValueInt("$sort", 0, (this.sorted ? 1 : 0)) != 0);
				this.color = catinfo.GetPropertyValueInt("$color", 0, this.color);
				this.arrow = catinfo.GetPropertyValueInt("$arrow", 0, this.arrow);
				this.errorcheck = catinfo.GetPropertyValueInt("$error", 0, this.errorcheck);
				this.fixedsize = catinfo.GetPropertyValueBool("$fixedsize", 0, this.fixedsize);
				this.fixedrotation = catinfo.GetPropertyValueBool("$fixedrotation", 0, this.fixedrotation);
				this.absolutez = catinfo.GetPropertyValueBool("$absolutez", 0, this.absolutez);
			}
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}
		
		// Constructor
		internal ThingCategory(Configuration cfg, ThingCategory parent, string name, IDictionary<string, EnumList> enums)
		{
			// Initialize
			this.name = name;
			this.things = new List<ThingTypeInfo>();
			this.children = new List<ThingCategory>();
			
			// Read properties
			this.title = cfg.ReadSetting("thingtypes." + name + ".title", name);

			//mxd. If current block has no settings, it should be a grouping block, not a thing category.
			if(this.title == name)
			{
				string[] props = new[] { "sprite", "sort", "color", "alpha", "renderstyle", "arrow", "width", 
					"height", "hangs", "blocking", "error", "fixedsize", "fixedrotation", "absolutez", "spritescale" };

				isinvalid = true;
				foreach(string prop in props)
				{
					if(cfg.SettingExists("thingtypes." + name + "." + prop))
					{
						isinvalid = false;
						break;
					}
				}

				if(isinvalid) return;
			}

			if(parent != null) //mxd
			{
				this.sprite = cfg.ReadSetting("thingtypes." + name + ".sprite", parent.sprite);
				this.sorted = (cfg.ReadSetting("thingtypes." + name + ".sort", (parent.sorted ? 1 : 0)) != 0);
				this.color = cfg.ReadSetting("thingtypes." + name + ".color", parent.color);
				this.alpha = cfg.ReadSetting("thingtypes." + name + ".alpha", parent.alpha);
				this.renderstyle = cfg.ReadSetting("thingtypes." + name + ".renderstyle", parent.renderstyle).ToLower();
				this.arrow = cfg.ReadSetting("thingtypes." + name + ".arrow", parent.arrow);
				this.radius = cfg.ReadSetting("thingtypes." + name + ".width", parent.radius);
				this.height = cfg.ReadSetting("thingtypes." + name + ".height", parent.height);
				this.hangs = cfg.ReadSetting("thingtypes." + name + ".hangs", parent.hangs);
				this.blocking = cfg.ReadSetting("thingtypes." + name + ".blocking", parent.blocking);
				this.errorcheck = cfg.ReadSetting("thingtypes." + name + ".error", parent.errorcheck);
				this.fixedsize = cfg.ReadSetting("thingtypes." + name + ".fixedsize", parent.fixedsize);
				this.fixedrotation = cfg.ReadSetting("thingtypes." + name + ".fixedrotation", parent.fixedrotation);
				this.absolutez = cfg.ReadSetting("thingtypes." + name + ".absolutez", parent.absolutez);
				this.spritescale = cfg.ReadSetting("thingtypes." + name + ".spritescale", parent.spritescale);
                this.optional = cfg.ReadSetting("thingtypes." + name + ".optional", parent.optional);
            }
			else
			{
				this.sprite = cfg.ReadSetting("thingtypes." + name + ".sprite", "");
				this.sorted = (cfg.ReadSetting("thingtypes." + name + ".sort", 0) != 0);
				this.color = cfg.ReadSetting("thingtypes." + name + ".color", 0);
				this.alpha = General.Clamp(cfg.ReadSetting("thingtypes." + name + ".alpha", 1f), 0f, 1f);
				this.renderstyle = cfg.ReadSetting("thingtypes." + name + ".renderstyle", "normal").ToLower();
				this.arrow = cfg.ReadSetting("thingtypes." + name + ".arrow", 0);
				this.radius = cfg.ReadSetting("thingtypes." + name + ".width", 10);
				this.height = cfg.ReadSetting("thingtypes." + name + ".height", 20);
				this.hangs = cfg.ReadSetting("thingtypes." + name + ".hangs", 0);
				this.blocking = cfg.ReadSetting("thingtypes." + name + ".blocking", 0);
				this.errorcheck = cfg.ReadSetting("thingtypes." + name + ".error", 1);
				this.fixedsize = cfg.ReadSetting("thingtypes." + name + ".fixedsize", false);
				this.fixedrotation = cfg.ReadSetting("thingtypes." + name + ".fixedrotation", false); //mxd
				this.absolutez = cfg.ReadSetting("thingtypes." + name + ".absolutez", false);
				this.spritescale = cfg.ReadSetting("thingtypes." + name + ".spritescale", 1.0f);
                this.optional = cfg.ReadSetting("thingtypes." + name + ".optional", false);
            }
			
			// Safety
			if(this.radius < 4f) this.radius = 8f;
			
			// Go for all items in category
			IDictionary dic = cfg.ReadSetting("thingtypes." + name, new Hashtable());
			Dictionary<string, ThingCategory> cats = new Dictionary<string, ThingCategory>(StringComparer.Ordinal); //mxd
			foreach(DictionaryEntry de in dic)
			{
				// Check if the item key is numeric
				int index;
				if(int.TryParse(de.Key.ToString(), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out index))
				{
					// Check if the item value is a structure
					if(de.Value is IDictionary)
					{
						// Create this thing
						things.Add(new ThingTypeInfo(this, index, cfg, enums));
					}
					// Check if the item value is a string
					else if(de.Value is string)
					{
						// Interpret this as the title
						things.Add(new ThingTypeInfo(this, index, de.Value.ToString()));
					}
				}
				//mxd. This should be a child category 
				else if(de.Value is IDictionary)
				{
					ThingCategory child = new ThingCategory(cfg, this, name + "." + de.Key, enums);
					if(child.IsValid && child.things.Count > 0)
					{
						if(cats.ContainsKey(child.title.ToLowerInvariant()))
							General.ErrorLogger.Add(ErrorType.Warning, "Thing Category \"" + child.title + "\" is double defined in " + this.title);
						cats[child.title.ToLowerInvariant()] = child;
					}
				}
			}

			//mxd. Add to main collection
			foreach(ThingCategory tc in cats.Values) children.Add(tc);

			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Disposer
		internal void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Clean up
				things = null;

				//mxd. Dispose children (oh so cruel!!11)
				foreach(ThingCategory tc in children) tc.Dispose();

				// Done
				isdisposed = true;
			}
		}
		
		#endregion

		#region ================== Methods

		// This sorts the category, if preferred
		internal void SortIfNeeded()
		{
			if(sorted) things.Sort();

			//mxd. Sort children as well
			foreach(ThingCategory tc in children) tc.SortIfNeeded();
		}
		
		// This adds a thing to the category
		internal void AddThing(ThingTypeInfo t)
		{
			// Add
			things.Add(t);
		}

		//mxd. This removes a thing from the category
		internal void RemoveThing(ThingTypeInfo t) 
		{
			// Remove
			if(things.Contains(t)) things.Remove(t);
		}

		// String representation
		public override string ToString()
		{
			return title;
		}
		
		#endregion
	}
}

