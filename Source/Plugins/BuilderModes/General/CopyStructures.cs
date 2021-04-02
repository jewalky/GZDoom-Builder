
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
using System.Linq;
using System.Reflection;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	//mxd
	[AttributeUsage(AttributeTargets.Field)]
	public class FieldDescription : Attribute
	{
		private bool doom = true;
		private bool hexen = true;
		private bool udmf = true;
		private string description = "Unnamed field";
		private string fieldname1;
		private string fieldname2;

		public bool DOOM { get { return doom; } set { doom = value; } }
		public bool HEXEN { get { return hexen; } set { hexen = value; } }
		public bool UDMF { get { return udmf; } set { udmf = value; } }
		public string Description { get { return description; } set { description = value; } }
		public string Field1 { get { return fieldname1; } set { fieldname1 = value; } }
		public string Field2 { get { return fieldname2; } set { fieldname2 = value; } }

		public bool SupportsCurrentMapFormat
		{
			get
			{
				if(General.Map == null) return false;
				if(!string.IsNullOrEmpty(fieldname1) || !string.IsNullOrEmpty(fieldname2)) return General.Map.UDMF;
				return (General.Map.DOOM && doom || General.Map.HEXEN && hexen || General.Map.UDMF && udmf);
			}
		}
	}

	public abstract class MapElementPropertiesCopySettings
	{
		protected MapElementPropertiesCopySettings()
		{
			// Set all properties supported by currect map format to true
			FieldInfo[] props = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach(FieldInfo prop in props)
			{
				foreach(Attribute attr in Attribute.GetCustomAttributes(prop))
				{
					if(attr.GetType() != typeof(FieldDescription)) continue;
					FieldDescription fd = (FieldDescription)attr;
					prop.SetValue(this, fd.SupportsCurrentMapFormat);
				}
			}
		}
	}

	//mxd
	public abstract class MapElementProperties
	{
		private readonly UniFields fields;   // Only custom (e.g. not UI-managed) fields here
		private readonly UniFields uifields; // Only UI-managed fields here
		private readonly MapElementType elementtype;

		protected MapElementProperties(UniFields other, MapElementType type)
		{
			// Should we bother?
			if(!General.Map.UDMF) return;
			
			// Copy source fields except the ones controlled by the UI
			fields = new UniFields();
			uifields = new UniFields();
			elementtype = type;
			foreach(KeyValuePair<string, UniValue> group in other)
			{
				if(General.Map.FormatInterface.UIFields[elementtype].ContainsKey(group.Key))
					uifields.Add(group.Key, new UniValue(group.Value));
				else
					fields.Add(group.Key, new UniValue(group.Value));
			}
		}

		protected void ApplyUIFields<T>(ICollection<T> collection, MapElementPropertiesCopySettings settings) where T : MapElement
		{
			FieldInfo[] props = settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach(FieldInfo prop in props)
			{
				// Property set?
				if(!(bool)prop.GetValue(settings)) continue;
				foreach(Attribute attr in Attribute.GetCustomAttributes(prop))
				{
					if(attr.GetType() != typeof(FieldDescription)) continue;
					FieldDescription fd = (FieldDescription)attr;
					if(fd.SupportsCurrentMapFormat && !string.IsNullOrEmpty(fd.Field1))
					{
						if(!string.IsNullOrEmpty(fd.Field2))
							foreach(T me in collection) Apply(me.Fields, fd.Field1, fd.Field2);
						else
							foreach(T me in collection) Apply(me.Fields, fd.Field1);
					}
				}
			}
		}

		protected void Apply(UniFields other, string key)
		{
			if(uifields.ContainsKey(key)) other[key] = new UniValue(uifields[key]);
			else if(other.ContainsKey(key)) other.Remove(key);
		}

		protected void Apply(UniFields other, string key1, string key2)
		{
			Apply(other, key1);
			Apply(other, key2);
		}

		protected void ApplyCustomFields(UniFields other)
		{
			// Remove custom fields
			string[] keys = new string[other.Keys.Count];
			other.Keys.CopyTo(keys, 0);
			foreach(string key in keys)
			{
				if(!General.Map.FormatInterface.UIFields[elementtype].ContainsKey(key)) other.Remove(key);
			}

			// Replace with stored custom fields
			foreach(KeyValuePair<string, UniValue> group in fields)
			{
				other.Add(group.Key, new UniValue(group.Value));
			}
		}
	}

	#region ================== Vertex

	//mxd
	public class VertexPropertiesCopySettings : MapElementPropertiesCopySettings
	{
		[FieldDescription(Description = "Vertex floor height", Field1 = "zfloor")]
		public bool ZFloor = true;

		[FieldDescription(Description = "Vertex ceiling height", Field1 = "zceiling")]
		public bool ZCeiling = true;
		
		[FieldDescription(Description = "Custom fields", DOOM = false, HEXEN = false)]
		public bool Fields = true;
	}
	
	// Vertex
	public class VertexProperties : MapElementProperties
	{
		private static VertexPropertiesCopySettings defaultsettings = new VertexPropertiesCopySettings();
		public static VertexPropertiesCopySettings CopySettings = new VertexPropertiesCopySettings();
		
		private readonly double zceiling; //mxd
		private readonly double zfloor; //mxd

		public VertexProperties(Vertex v) : base(v.Fields, MapElementType.VERTEX)
		{
			zceiling = v.ZCeiling; //mxd
			zfloor = v.ZFloor; //mxd
		}

		//mxd. Applies coped properties
		public void Apply(ICollection<Vertex> verts, bool usecopysettings)
		{
			Apply(verts, (usecopysettings ? CopySettings : defaultsettings));
		}

		//mxd. Applies coped properties using selected settings
		public void Apply(ICollection<Vertex> verts, VertexPropertiesCopySettings settings)
		{
			foreach(Vertex v in verts)
			{
				if(settings.ZCeiling) v.ZCeiling = zceiling;
				if(settings.ZFloor) v.ZFloor = zfloor;
				if(settings.Fields)
				{
					v.Fields.BeforeFieldsChange();
					ApplyCustomFields(v.Fields);
				}
			}
		}
	}

	#endregion

	#region ================== Sector

	//mxd
	public class SectorPropertiesCopySettings : MapElementPropertiesCopySettings
	{
		[FieldDescription(Description = "Floor height")]
		public bool FloorHeight = true;
		
		[FieldDescription(Description = "Ceiling height")]
		public bool CeilingHeight = true;
		
		[FieldDescription(Description = "Floor texture")]
		public bool FloorTexture = true;
		
		[FieldDescription(Description = "Ceiling texture")]
		public bool CeilingTexture = true;

		[FieldDescription(Description = "Floor texture offsets", Field1 = "xpanningfloor", Field2 = "ypanningfloor")]
		public bool FloorTextureOffset = true;

		[FieldDescription(Description = "Ceiling texture offsets", Field1 = "xpanningceiling", Field2 = "ypanningceiling")]
		public bool CeilingTextureOffset = true;

		[FieldDescription(Description = "Floor texture scale", Field1 = "xscalefloor", Field2 = "yscalefloor")]
		public bool FloorTextureScale = true;

		[FieldDescription(Description = "Ceiling texture scale", Field1 = "xscaleceiling", Field2 = "yscaleceiling")]
		public bool CeilingTextureScale = true;

		[FieldDescription(Description = "Floor texture rotation", Field1 = "rotationfloor")]
		public bool FloorTextureRotation = true;

		[FieldDescription(Description = "Ceiling texture rotation", Field1 = "rotationceiling")]
		public bool CeilingTextureRotation = true;

		[FieldDescription(Description = "Floor alpha", Field1 = "alphafloor")]
		public bool FloorAlpha = true;

		[FieldDescription(Description = "Ceiling alpha", Field1 = "alphaceiling")]
		public bool CeilingAlpha = true;

		[FieldDescription(Description = "Floor portal alpha", Field1 = "portal_floor_alpha")]
		public bool FloorPortalAlpha = true;

		[FieldDescription(Description = "Ceiling portal alpha", Field1 = "portal_ceil_alpha")]
		public bool CeilingPortalAlpha = true;

		[FieldDescription(Description = "Sector brightness")]
		public bool Brightness = true;

		[FieldDescription(Description = "Floor brightness", Field1 = "lightfloor", Field2 = "lightfloorabsolute")]
		public bool FloorBrightness = true;

		[FieldDescription(Description = "Ceiling brightness", Field1 = "lightceiling", Field2 = "lightceilingabsolute")]
		public bool CeilingBrightness = true;

		[FieldDescription(Description = "Floor render style", Field1 = "renderstylefloor")]
		public bool FloorRenderStyle = true;

		[FieldDescription(Description = "Ceiling render style", Field1 = "renderstyleceiling")]
		public bool CeilingRenderStyle = true;

		[FieldDescription(Description = "Floor portal render style", Field1 = "portal_floor_overlaytype")]
		public bool FloorPortalRenderStyle = true;

		[FieldDescription(Description = "Ceiling portal render style", Field1 = "portal_ceil_overlaytype")]
		public bool CeilingPortalRenderStyle = true;
		
		[FieldDescription(Description = "Floor slope", DOOM = false, HEXEN = false)]
		public bool FloorSlope = true;

		[FieldDescription(Description = "Ceiling slope", DOOM = false, HEXEN = false)]
		public bool CeilingSlope = true;

		[FieldDescription(Description = "Floor terrain", Field1 = "floorterrain")]
		public bool FloorTerrain = true;

		[FieldDescription(Description = "Ceiling terrain", Field1 = "ceilingterrain")]
		public bool CeilingTerrain = true;
		
		[FieldDescription(Description = "Tags")]
		public bool Tag = true;
		
		[FieldDescription(Description = "Effect")]
		public bool Special = true;
		
		[FieldDescription(Description = "Flags", DOOM = false, HEXEN = false)]
		public bool Flags = true;

		[FieldDescription(Description = "Light color", Field1 = "lightcolor")]
		public bool LightColor = true;

		[FieldDescription(Description = "Fade color", Field1 = "fadecolor")]
		public bool FadeColor = true;

		[FieldDescription(Description = "Floor color", Field1 = "color_floor")]
		public bool FloorColor = true;

		[FieldDescription(Description = "Ceiling color", Field1 = "color_ceiling")]
		public bool CeilingColor = true;

		[FieldDescription(Description = "Top wall color", Field1 = "color_walltop")]
		public bool TopWallColor = true;

		[FieldDescription(Description = "Bottom wall color", Field1 = "color_wallbottom")]
		public bool BottomWallColor = true;

		[FieldDescription(Description = "Sprites color", Field1 = "color_sprites")]
		public bool SpritesColor = true;

		[FieldDescription(Description = "Floor glow", Field1 = "floorglowcolor", Field2 = "floorglowheight")]
		public bool FloorGlow = true;

		[FieldDescription(Description = "Ceiling glow", Field1 = "ceilingglowcolor", Field2 = "ceilingglowheight")]
		public bool CeilingGlow = true;

		[FieldDescription(Description = "Desaturation", Field1 = "desaturation")]
		public bool Desaturation = true;

		[FieldDescription(Description = "Damage type", Field1 = "damagetype")]
		public bool DamageType = true;

		[FieldDescription(Description = "Damage amount", Field1 = "damageamount")]
		public bool DamageAmount = true;

		[FieldDescription(Description = "Damage interval", Field1 = "damageinterval")]
		public bool DamageInterval = true;

		[FieldDescription(Description = "Damage leakiness", Field1 = "leakiness")]
		public bool DamageLeakiness = true;

		[FieldDescription(Description = "Sound sequence", Field1 = "soundsequence")]
		public bool SoundSequence = true;

		[FieldDescription(Description = "Gravity", Field1 = "gravity")]
		public bool Gravity = true;
		
		[FieldDescription(Description = "Custom fields", DOOM = false, HEXEN = false)]
		public bool Fields = true;

		[FieldDescription(Description = "Comment", Field1 = "comment")]
		public bool Comment = true;
	}

	// Sector
	public class SectorProperties : MapElementProperties
	{
		//mxd
		private static SectorPropertiesCopySettings defaultsettings = new SectorPropertiesCopySettings();
		public static SectorPropertiesCopySettings CopySettings = new SectorPropertiesCopySettings();
		
		private readonly int floorheight;
		private readonly int ceilheight;
		private readonly string floortexture;
		private readonly string ceilingtexture;
		private readonly int effect;
		private readonly int brightness;
		private readonly double ceilslopeoffset;
		private readonly double floorslopeoffset;
		private readonly Vector3D ceilslope;
		private readonly Vector3D floorslope;
		private readonly List<int> tags;
		private readonly Dictionary<string, bool> flags; //mxd
		
		public SectorProperties(Sector s) : base(s.Fields, MapElementType.SECTOR)
		{
			floorheight = s.FloorHeight;
			ceilheight = s.CeilHeight;
			floortexture = s.FloorTexture;
			ceilingtexture = s.CeilTexture;
			brightness = s.Brightness;
			effect = s.Effect;
			ceilslopeoffset = s.CeilSlopeOffset;
			floorslopeoffset = s.FloorSlopeOffset;
			ceilslope = s.CeilSlope;
			floorslope = s.FloorSlope;
			tags = new List<int>(s.Tags); //mxd
			flags = s.GetFlags(); //mxd
		}
		
		//mxd. Applies coped properties
		public void Apply(ICollection<Sector> sectors, bool usecopysettings)
		{
			Apply(sectors, (usecopysettings ? CopySettings : defaultsettings));
		}

		//mxd. Applies coped properties using selected settings
		public void Apply(ICollection<Sector> sectors, SectorPropertiesCopySettings settings)
		{
			foreach(Sector s in sectors)
			{
				if(settings.FloorHeight) s.FloorHeight = floorheight;
				if(settings.CeilingHeight) s.CeilHeight = ceilheight;
				if(settings.FloorTexture) s.SetFloorTexture(floortexture);
				if(settings.CeilingTexture) s.SetCeilTexture(ceilingtexture);
				if(settings.Brightness) s.Brightness = brightness;
				if(settings.Tag) s.Tags = new List<int>(tags); //mxd
				if(settings.Special) s.Effect = effect;
				if(settings.CeilingSlope)
				{
					s.CeilSlopeOffset = ceilslopeoffset;
					s.CeilSlope = ceilslope;
				}
				if(settings.FloorSlope)
				{
					s.FloorSlopeOffset = floorslopeoffset;
					s.FloorSlope = floorslope;
				}
				if(settings.Flags)
				{
					s.ClearFlags(); //mxd
					foreach(KeyValuePair<string, bool> f in flags) //mxd
						s.SetFlag(f.Key, f.Value);
				}
			}

			// Should we bother?
			if(!General.Map.UDMF) return;

			// Apply custom fields
			foreach(Sector s in sectors)
			{
				s.Fields.BeforeFieldsChange();
				if(settings.Fields) ApplyCustomFields(s.Fields);
			}

			// Apply UI fields
			ApplyUIFields(sectors, settings);
		}
	}

	#endregion

	#region ================== Sidedef

	//mxd
	public class SidedefPropertiesCopySettings : MapElementPropertiesCopySettings
	{
		[FieldDescription(Description = "Upper texture")]
		public bool UpperTexture = true;
		
		[FieldDescription(Description = "Middle texture")]
		public bool MiddleTexture = true;
		
		[FieldDescription(Description = "Lower texture")]
		public bool LowerTexture = true;
		
		[FieldDescription(Description = "Texture offset X")]
		public bool OffsetX = true;
		
		[FieldDescription(Description = "Texture offset Y")]
		public bool OffsetY = true;

		[FieldDescription(Description = "Upper texture offsets", Field1 = "offsetx_top", Field2 = "offsety_top")]
		public bool UpperTextureOffset = true;

		[FieldDescription(Description = "Middle texture offsets", Field1 = "offsetx_mid", Field2 = "offsety_mid")]
		public bool MiddleTextureOffset = true;

		[FieldDescription(Description = "Lower texture offsets", Field1 = "offsetx_bottom", Field2 = "offsety_bottom")]
		public bool LowerTextureOffset = true;

		[FieldDescription(Description = "Upper texture scale", Field1 = "scalex_top", Field2 = "scaley_top")]
		public bool UpperTextureScale = true;

		[FieldDescription(Description = "Middle texture scale", Field1 = "scalex_mid", Field2 = "scaley_mid")]
		public bool MiddleTextureScale = true;

		[FieldDescription(Description = "Lower texture scale", Field1 = "scalex_bottom", Field2 = "scaley_bottom")]
		public bool LowerTextureScale = true;

		[FieldDescription(Description = "Brightness", Field1 = "light", Field2 = "lightabsolute")]
		public bool Brightness = true;
		
		[FieldDescription(Description = "Flags", DOOM = false, HEXEN = false)]
		public bool Flags = true;
		
		[FieldDescription(Description = "Custom fields", DOOM = false, HEXEN = false)]
		public bool Fields = true;
	}

	// Sidedef
	public class SidedefProperties : MapElementProperties
	{
		//mxd
		internal static SidedefPropertiesCopySettings DefaultSettings = new SidedefPropertiesCopySettings();
		public static SidedefPropertiesCopySettings CopySettings = new SidedefPropertiesCopySettings();

		private readonly string hightexture;
		private readonly string middletexture;
		private readonly string lowtexture;
		private readonly int offsetx;
		private readonly int offsety;
		private readonly Dictionary<string, bool> flags; //mxd

		public SidedefProperties(Sidedef s) : base(s.Fields, MapElementType.SIDEDEF)
		{
			hightexture = s.HighTexture;
			middletexture = s.MiddleTexture;
			lowtexture = s.LowTexture;
			offsetx = s.OffsetX;
			offsety = s.OffsetY;
			flags = s.GetFlags(); //mxd
		}

		//mxd. Applies coped properties with all settings enabled
		public void Apply(ICollection<Sidedef> sides, bool usecopysettings)
		{
			Apply(sides, (usecopysettings ? CopySettings : DefaultSettings));
		}

		//mxd. Applies selected settings
		public void Apply(ICollection<Sidedef> sides, SidedefPropertiesCopySettings settings)
		{
			foreach(Sidedef s in sides)
			{
				if(settings.UpperTexture) s.SetTextureHigh(hightexture);
				if(settings.MiddleTexture) s.SetTextureMid(middletexture);
				if(settings.LowerTexture) s.SetTextureLow(lowtexture);
				if(settings.OffsetX) s.OffsetX = offsetx;
				if(settings.OffsetY) s.OffsetY = offsety;
				if(settings.Flags)
				{
					s.ClearFlags(); //mxd
					foreach(KeyValuePair<string, bool> f in flags) //mxd
						s.SetFlag(f.Key, f.Value);
				}
			}

			// Should we bother?
			if(!General.Map.UDMF) return;

			// Apply fields
			foreach(Sidedef s in sides)
			{
				s.Fields.BeforeFieldsChange();
				if(settings.Fields) ApplyCustomFields(s.Fields);
			}

			// Apply UI fields
			ApplyUIFields(sides, settings);
		}
	}

	#endregion

	#region ================== Linedef

	//mxd
	public class LinedefPropertiesCopySettings : MapElementPropertiesCopySettings
	{
		[FieldDescription(Description = "Action")]
		public bool Action = true;
		
		[FieldDescription(Description = "Action arguments", DOOM = false)]
		public bool Arguments = true;
		
		[FieldDescription(Description = "Activation", DOOM = false, UDMF = false)]
		public bool Activation = true;
		
		[FieldDescription(Description = "Tags", HEXEN = false)]
		public bool Tag = true;
		
		[FieldDescription(Description = "Flags")]
		public bool Flags = true;

		[FieldDescription(Description = "Alpha", Field1 = "alpha")]
		public bool Alpha = true;

		[FieldDescription(Description = "Render style", Field1 = "renderstyle")]
		public bool RenderStyle = true;

		[FieldDescription(Description = "Lock number", Field1 = "locknumber")]
		public bool LockNumber = true;
		
		[FieldDescription(Description = "Custom fields", DOOM = false, HEXEN = false)]
		public bool Fields = true;

		[FieldDescription(Description = "Comment", Field1 = "comment")]
		public bool Comment = true;
	}

	// Linedef
	public class LinedefProperties : MapElementProperties
	{
		//mxd
		private static LinedefPropertiesCopySettings defaultsettings = new LinedefPropertiesCopySettings();
		public static LinedefPropertiesCopySettings CopySettings = new LinedefPropertiesCopySettings();
		
		private readonly SidedefProperties front;
		private readonly SidedefProperties back;
		private readonly Dictionary<string, bool> flags;
		private readonly int action;
		private readonly int activate;
		private readonly List<int> tags;
		private readonly int[] args;

		public LinedefProperties(Linedef l) : base(l.Fields, MapElementType.LINEDEF)
		{
			front = (l.Front != null ? new SidedefProperties(l.Front) : null);
			back = (l.Back != null ? new SidedefProperties(l.Back) : null);

			flags = l.GetFlags();
			action = l.Action;
			activate = l.Activate;
			tags = new List<int>(l.Tags); //mxd
			args = (int[])(l.Args.Clone());
		}

		//mxd. Applies coped properties with all settings enabled
		public void Apply(ICollection<Linedef> lines, bool usecopysettings) { Apply(lines, usecopysettings, true); }
		public void Apply(ICollection<Linedef> lines, bool usecopysettings, bool applytosidedefs)
		{
			if(usecopysettings)
				Apply(lines, CopySettings, (applytosidedefs ? SidedefProperties.CopySettings : null));
			else
				Apply(lines, defaultsettings, (applytosidedefs ? SidedefProperties.DefaultSettings : null));
		}

		//mxd. Applies selected linededf and sidedef settings
		public void Apply(ICollection<Linedef> lines, LinedefPropertiesCopySettings settings, SidedefPropertiesCopySettings sidesettings)
		{
			List<Sidedef> frontsides = new List<Sidedef>(lines.Count);
			List<Sidedef> backsides = new List<Sidedef>(lines.Count);
			foreach(Linedef l in lines)
			{
				if(settings.Flags)
				{
					l.ClearFlags();
					foreach(KeyValuePair<string, bool> f in flags)
						l.SetFlag(f.Key, f.Value);
				}
				if(settings.Activation) l.Activate = activate;
				if(settings.Tag) l.Tags = new List<int>(tags); //mxd
				if(settings.Action) l.Action = action;
				if(settings.Arguments)
				{
					for(int i = 0; i < l.Args.Length; i++)
						l.Args[i] = args[i];
				}

				if(l.Front != null) frontsides.Add(l.Front);
				if(l.Back != null) backsides.Add(l.Back);
			}

			// Should we bother?
			if(General.Map.UDMF)
			{
				// Apply fields
				foreach(Linedef l in lines)
				{
					l.Fields.BeforeFieldsChange();

					// Apply string arguments
					if(settings.Arguments)
					{
						Apply(l.Fields, "arg0str");

						//TODO: re-enable when UI part is ready
						//Apply(l.Fields, "arg1str");
						//Apply(l.Fields, "arg2str");
						//Apply(l.Fields, "arg3str");
						//Apply(l.Fields, "arg4str");
					}

					// Apply custom fields
					if(settings.Fields) ApplyCustomFields(l.Fields);
				}

				// Apply UI fields
				ApplyUIFields(lines, settings);
			}

			// Apply sidedef settings
			if(sidesettings != null)
			{
				if(front != null) front.Apply(frontsides, sidesettings);
				if(back != null) back.Apply(backsides, sidesettings);
			}
		}
	}

	#endregion

	#region ================== Thing

	//mxd
	public class ThingPropertiesCopySettings : MapElementPropertiesCopySettings
	{
		[FieldDescription(Description = "Type")]
		public bool Type = true;
		
		[FieldDescription(Description = "Angle")]
		public bool Angle = true;

		[FieldDescription(Description = "Z-height", DOOM = false)]
		public bool ZHeight = true;
		
		[FieldDescription(Description = "Pitch", DOOM = false, HEXEN = false)]
		public bool Pitch = true;
		
		[FieldDescription(Description = "Roll", DOOM = false, HEXEN = false)]
		public bool Roll = true;
		
		[FieldDescription(Description = "Scale", DOOM = false, HEXEN = false)]
		public bool Scale = true;
		
		[FieldDescription(Description = "Action", DOOM = false)]
		public bool Action = true;
		
		[FieldDescription(Description = "Action arguments", DOOM = false)]
		public bool Arguments = true;
		
		[FieldDescription(Description = "Tag", DOOM = false)]
		public bool Tag = true;
		
		[FieldDescription(Description = "Flags")]
		public bool Flags = true;

		[FieldDescription(Description = "Conversation ID", Field1 = "conversation")]
		public bool Conversation = true;

		[FieldDescription(Description = "Gravity", Field1 = "gravity")]
		public bool Gravity = true;

		[FieldDescription(Description = "Health multiplier", Field1 = "health")]
		public bool Health = true;

		[FieldDescription(Description = "Score", Field1 = "score")]
		public bool Score = true;

		[FieldDescription(Description = "Float bob phase", Field1 = "floatbobphase")]
		public bool FloatBobPhase = true;

		[FieldDescription(Description = "Alpha", Field1 = "alpha")]
		public bool Alpha = true;

		[FieldDescription(Description = "Fill color", Field1 = "fillcolor")]
		public bool FillColor = true;

		[FieldDescription(Description = "Render style", Field1 = "renderstyle")]
		public bool RenderStyle = true;
		
		[FieldDescription(Description = "Custom fields", DOOM = false, HEXEN = false)]
		public bool Fields = true;

		[FieldDescription(Description = "Comment", Field1 = "comment")]
		public bool Comment = true;
	}
	
	// Thing
	public class ThingProperties : MapElementProperties
	{
		//mxd
		private static readonly ThingPropertiesCopySettings defaultsettings = new ThingPropertiesCopySettings();
		public static readonly ThingPropertiesCopySettings CopySettings = new ThingPropertiesCopySettings();
		
		private readonly int type;
		private readonly double angle;
		private readonly double zheight; //mxd
		private readonly int pitch; //mxd
		private readonly int roll; //mxd
		private readonly double scalex; //mxd
		private readonly double scaley; //mxd
		private readonly Dictionary<string, bool> flags;
		private readonly int tag;
		private readonly int action;
		private readonly int[] args;
		
		public ThingProperties(Thing t) : base(t.Fields, MapElementType.THING)
		{
			type = t.Type;
			angle = t.Angle;
			zheight = t.Position.z;
			pitch = t.Pitch;
			roll = t.Roll;
			scalex = t.ScaleX;
			scaley = t.ScaleY;
			flags = t.GetFlags();
			tag = t.Tag;
			action = t.Action;
			args = (int[])(t.Args.Clone());
		}

		//mxd. Applies coped properties with all settings enabled
		public void Apply(ICollection<Thing> things, bool usecopysettings)
		{
			Apply(things, (usecopysettings ? CopySettings : defaultsettings));
		}
		
		//mxd. Applies selected settings
		public void Apply(ICollection<Thing> things, ThingPropertiesCopySettings settings)
		{
			foreach(Thing t in things)
			{
				if(settings.Type) t.Type = type;

				// Update the setting from the configuration after changing the type. This makes sure all the following
				// actions work, since there can be problems when changing the type from or to thing that has a model defined
				t.UpdateConfiguration();

				if(settings.Angle) t.Rotate(angle);
				if(settings.ZHeight) t.Move(t.Position.x, t.Position.y, zheight);
				if(settings.Pitch) t.SetPitch(pitch);
				if(settings.Roll) t.SetRoll(roll);
				if(settings.Scale) t.SetScale(scalex, scaley);
				if(settings.Flags)
				{
					t.ClearFlags();
					foreach(KeyValuePair<string, bool> f in flags)
						t.SetFlag(f.Key, f.Value);
				}
				if(settings.Tag) t.Tag = tag;
				if(settings.Action) t.Action = action;
				if(settings.Arguments)
				{
					for(int i = 0; i < t.Args.Length; i++)
						t.Args[i] = args[i];
				}
			}

			// Should we bother?
			if(!General.Map.UDMF) return;

			foreach(Thing t in things)
			{
				// Apply fields
				t.Fields.BeforeFieldsChange();

				// Apply string arguments
				if(settings.Arguments)
				{
					Apply(t.Fields, "arg0str");

					//TODO: re-enable when UI part is ready
					//Apply(t.Fields, "arg1str");
					//Apply(t.Fields, "arg2str");
					//Apply(t.Fields, "arg3str");
					//Apply(t.Fields, "arg4str");
				}

				// Apply custom fields
				if(settings.Fields) ApplyCustomFields(t.Fields);
			}

			// Apply UI fields
			ApplyUIFields(things, settings);
		}
	}

	#endregion

	#region ================== Properties Comparer

	//mxd. A class, which checks whether source and target map element's properties match
	public static class PropertiesComparer
	{

		#region Vertex

		public static bool PropertiesMatch(VertexPropertiesCopySettings flags, Vertex source, Vertex target) 
		{
			if(!General.Map.UDMF) return true;
			
			// Built-in properties
			if(flags.ZCeiling && source.ZCeiling != target.ZCeiling) return false;
			if(flags.ZFloor && source.ZFloor != target.ZFloor) return false;

			// Custom fields
			return !flags.Fields || UniFields.CustomFieldsMatch(source.Fields, target.Fields);
		}

		#endregion

		#region Sector

		public static bool PropertiesMatch(SectorPropertiesCopySettings flags, Sector source, Sector target) 
		{
			// Built-in properties
			if(flags.FloorHeight && source.FloorHeight != target.FloorHeight) return false;
			if(flags.CeilingHeight && source.CeilHeight != target.CeilHeight) return false;
			if(flags.FloorTexture && source.FloorTexture != target.FloorTexture) return false;
			if(flags.CeilingTexture && source.CeilTexture != target.CeilTexture) return false;
			if(flags.Brightness && source.Brightness != target.Brightness) return false;
			if(flags.Tag && !TagsMatch(source.Tags, target.Tags)) return false;
			if(flags.Flags && !FlagsMatch(source.GetEnabledFlags(), target.GetEnabledFlags())) return false;

			// Generalized effects require more tender loving care...
			if(flags.Special && source.Effect != target.Effect)
			{
				if(!General.Map.Config.GeneralizedEffects || source.Effect == 0 || target.Effect == 0) return false;

				// Get effect bits...
				SectorEffectData sourcedata = General.Map.Config.GetSectorEffectData(source.Effect);
				SectorEffectData targetdata = General.Map.Config.GetSectorEffectData(target.Effect);
				
				// No bits match when at least one effect is not generalized, or when bits don't overlap 
				if(sourcedata.Effect != targetdata.Effect 
					|| sourcedata.GeneralizedBits.Count != targetdata.GeneralizedBits.Count 
					|| !sourcedata.GeneralizedBits.Overlaps(targetdata.GeneralizedBits)) return false;
			}

			if(!General.Map.UDMF) return true;

			// UI fields
			if(!UIFieldsMatch(flags, source, target)) return false;

			// Custom fields
			return !flags.Fields || UniFields.CustomFieldsMatch(source.Fields, target.Fields);
		}

		#endregion

		#region Linedef

		public static bool PropertiesMatch(LinedefPropertiesCopySettings linedefflags, SidedefPropertiesCopySettings sideflags, Linedef source, Linedef target) 
		{
			// Built-in properties
			if(linedefflags.Action && source.Action != target.Action) return false;
			if(linedefflags.Activation && source.Activate != target.Activate) return false;
			if(linedefflags.Tag && !TagsMatch(source.Tags, target.Tags)) return false;
			if(linedefflags.Arguments) 
			{
				// Classic args
				for(int i = 0; i < source.Args.Length; i++)
					if(source.Args[i] != target.Args[i]) return false;

				// String args
				if(General.Map.UDMF)
				{
					if(!UniFields.ValuesMatch("arg0str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg1str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg2str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg3str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg4str", source, target)) return false;
				}
			}
			if(linedefflags.Flags && !FlagsMatch(source.GetEnabledFlags(), target.GetEnabledFlags())) return false;

			if(General.Map.UDMF)
			{
				// UI fields
				if(!UIFieldsMatch(linedefflags, source, target)) return false;

				// Custom fields
				if(linedefflags.Fields && !UniFields.CustomFieldsMatch(source.Fields, target.Fields)) return false;
			}

			// Sidedef properties
			return (source.Front != null && target.Front != null && PropertiesMatch(sideflags, source.Front, target.Front)) ||
				   (source.Front != null && target.Back != null && PropertiesMatch(sideflags, source.Front, target.Back)) ||
				   (source.Back != null && target.Front != null && PropertiesMatch(sideflags, source.Back, target.Front)) ||
				   (source.Back != null && target.Back != null && PropertiesMatch(sideflags, source.Back, target.Back));
		}

		#endregion

		#region Sidedef

		public static bool PropertiesMatch(SidedefPropertiesCopySettings flags, Sidedef source, Sidedef target) 
		{
			// Built-in properties
			if(flags.OffsetX && source.OffsetX != target.OffsetX) return false;
			if(flags.OffsetY && source.OffsetY != target.OffsetY) return false;
			if(flags.UpperTexture && source.HighTexture != target.HighTexture) return false;
			if(flags.MiddleTexture && source.MiddleTexture != target.MiddleTexture) return false;
			if(flags.LowerTexture && source.LowTexture != target.LowTexture) return false;
			if(!General.Map.UDMF) return true;

			// UDMF-specific properties
			if(flags.Flags && !FlagsMatch(source.GetEnabledFlags(), target.GetEnabledFlags())) return false;

			// UI fields
			if(!UIFieldsMatch(flags, source, target)) return false;

			// Custom fields
			return !flags.Fields || UniFields.CustomFieldsMatch(source.Fields, target.Fields);
		}

		#endregion

		#region Thing

		public static bool PropertiesMatch(ThingPropertiesCopySettings flags, Thing source, Thing target) 
		{
			// Built-in properties
			if(flags.Type && source.Type != target.Type) return false;
			if(flags.Angle && source.AngleDoom != target.AngleDoom) return false;
			if(flags.Action && source.Action != target.Action) return false;
			if(flags.Arguments) 
			{
				// Classic args
				for(int i = 0; i < source.Args.Length; i++)
					if(source.Args[i] != target.Args[i]) return false;

				// String args
				if(General.Map.UDMF)
				{
					if(!UniFields.ValuesMatch("arg0str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg1str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg2str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg3str", source, target)) return false;
					if(!UniFields.ValuesMatch("arg4str", source, target)) return false;
				}
			}
			if(flags.Tag && source.Tag != target.Tag) return false;
			if(flags.Flags && !FlagsMatch(source.GetEnabledFlags(), target.GetEnabledFlags())) return false;
			if(!General.Map.UDMF) return true;

			// UDMF-specific properties
			if(flags.Pitch && source.Pitch != target.Pitch) return false;
			if(flags.Roll && source.Roll != target.Roll) return false;
			if(flags.Scale && (source.ScaleX != target.ScaleX) || (source.ScaleY != target.ScaleY)) return false;

			// UI fields
			if(!UIFieldsMatch(flags, source, target)) return false;

			// Custom fields
			return !flags.Fields || UniFields.CustomFieldsMatch(source.Fields, target.Fields);
		}

		#endregion

		#region Utility

		private static bool FlagsMatch(HashSet<string> flags1, HashSet<string> flags2)
		{
			if(flags1.Count != flags2.Count) return false;
			foreach(string flag in flags1)
			{
				if(!flags2.Contains(flag)) return false;
			}

			return true;
		}

		//mxd
		private static bool TagsMatch(List<int> tags1, List<int> tags2)
		{
			if(!General.Map.UDMF) return tags1[0] == tags2[0];
			
			if(tags1.Count != tags2.Count) return false;
			Dictionary<int, int> count = new Dictionary<int, int>();

			foreach(int s in tags1)
			{
				if(count.ContainsKey(s)) count[s]++;
				else count.Add(s, 1);
			}

			foreach(int s in tags2)
			{
				if(count.ContainsKey(s)) count[s]--;
				else return false;
			}

			return count.Values.All(c => c == 0);
		}

		private static bool UIFieldsMatch(MapElementPropertiesCopySettings settings, MapElement first, MapElement second)
		{
			FieldInfo[] props = settings.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
			foreach(FieldInfo prop in props)
			{
				// Property set?
				if(!(bool)prop.GetValue(settings)) continue;

				foreach(Attribute attr in Attribute.GetCustomAttributes(prop))
				{
					if(attr.GetType() != typeof(FieldDescription)) continue;
					FieldDescription fd = (FieldDescription)attr;
					if(fd.SupportsCurrentMapFormat && !string.IsNullOrEmpty(fd.Field1))
					{
						if(!string.IsNullOrEmpty(fd.Field2))
						{
							if(!UniFields.ValuesMatch(fd.Field1, fd.Field2, first, second)) return false;
						}
						else
						{
							if(!UniFields.ValuesMatch(fd.Field1, first, second)) return false;
						}
					}
				}
			}

			return true;
		}

		#endregion
	}

	#endregion

}
