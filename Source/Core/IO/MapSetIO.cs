
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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal abstract class MapSetIO : IMapSetIO
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		// WAD File
		protected WAD wad;

		// Map manager
		protected MapManager manager;

		//mxd
		protected Dictionary<MapElementType, Dictionary<string, UniversalType>> uifields;

		#endregion

		#region ================== Properties

		public abstract int MaxSidedefs { get; }
		public abstract int MaxVertices { get; }
		public abstract int MaxLinedefs { get; }
		public abstract int MaxSectors { get; }
		public abstract int MaxThings { get; }
		public abstract int MinTextureOffset { get; }
		public abstract int MaxTextureOffset { get; }
		public abstract int VertexDecimals { get; }
		public abstract string DecimalsFormat { get; }
		public abstract bool HasLinedefTag { get; }
		public abstract bool HasThingTag { get; }
		public abstract bool HasThingAction { get; }
		public abstract bool HasCustomFields { get; }
		public abstract bool HasThingHeight { get; }
		public abstract bool HasActionArgs { get; }
		public abstract bool HasMixedActivations { get; }
		public abstract bool HasPresetActivations { get; }
		public abstract bool HasBuiltInActivations { get; }
		public abstract bool HasNumericLinedefFlags { get; }
		public abstract bool HasNumericThingFlags { get; }
		public abstract bool HasNumericLinedefActivations { get; }
		public abstract int MaxTag { get; }
		public abstract int MinTag { get; }
		public abstract int MaxAction { get; }
		public abstract int MinAction { get; }
		public abstract int MaxArgument { get; }
		public abstract int MinArgument { get; }
		public abstract int MaxEffect { get; }
		public abstract int MinEffect { get; }
		public abstract int MaxBrightness { get; }
		public abstract int MinBrightness { get; }
		public abstract int MaxThingType { get; }
		public abstract int MinThingType { get; }
		public abstract float MaxCoordinate { get; }
		public abstract float MinCoordinate { get; }
		public abstract int MaxThingAngle { get; }
		public abstract int MinThingAngle { get; }
		public abstract Dictionary<MapElementType, Dictionary<string, UniversalType>> UIFields { get; } //mxd
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		protected MapSetIO(WAD wad, MapManager manager)
		{
			// Initialize
			this.wad = wad;
			this.manager = manager;
			this.uifields = new Dictionary<MapElementType, Dictionary<string, UniversalType>>(); //mxd
		}
		
		#endregion

		#region ================== Static Methods

		// This returns and instance of the specified IO class
		public static MapSetIO Create(string classname)
		{
			return Create(classname, null, null);
		}
		
		// This returns and instance of the specified IO class
		public static MapSetIO Create(string classname, WAD wadfile, MapManager manager)
		{
			try
			{
				// Create arguments
				object[] args = new object[2];
				args[0] = wadfile;
				args[1] = manager;
				
				// Make the full class name
				string fullname = "CodeImp.DoomBuilder.IO." + classname;
				
				// Create IO class
				MapSetIO result = (MapSetIO)General.ThisAssembly.CreateInstance(fullname, false,
					BindingFlags.Default, null, args, CultureInfo.CurrentCulture, new object[0]);
				
				// Check result
				if(result != null)
				{
					// Success
					return result;
				}
				else
				{
					// No such class
					throw new ArgumentException("No such map format interface found: \"" + classname + "\"");
				}
			}
			// Catch errors
			catch(TargetInvocationException e)
			{
				// Throw the actual exception
				Debug.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString());
				Debug.WriteLine(e.InnerException.Source + " throws " + e.InnerException.GetType().Name + ":");
				Debug.WriteLine(e.InnerException.Message);
				Debug.WriteLine(e.InnerException.StackTrace);
				throw e.InnerException;
			}
		}
		
		#endregion
		
		#region ================== Methods

		// Required implementations
		public abstract MapSet Read(MapSet map, string mapname);
		public abstract void Write(MapSet map, string mapname, int position);

		//mxd.
		public string GetElementName(MapElementType elementtype)
		{
			switch(elementtype)
			{
				case MapElementType.VERTEX:  return "vertex";
				case MapElementType.LINEDEF: return "linedef";
				case MapElementType.SIDEDEF: return "sidedef";
				case MapElementType.SECTOR:  return "sector";
				case MapElementType.THING:   return "thing";
				default: throw new NotSupportedException("Tried to get element name of unsupported map element type!");
			}
		}

		public MapElementType GetElementType(string elementname)
		{
			switch(elementname)
			{
				case "vertex":  return MapElementType.VERTEX;
				case "linedef": return MapElementType.LINEDEF;
				case "sidedef": return MapElementType.SIDEDEF;
				case "sector":  return MapElementType.SECTOR;
				case "thing":   return MapElementType.THING;
				default: throw new NotSupportedException("Tried to get element type of unsupported map element type!");
			}
		}
		
		#endregion
	}
}
