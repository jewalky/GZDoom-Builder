
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

using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	public interface IMapSetIO
	{
		int MaxSidedefs { get; }
		int VertexDecimals { get; }
		string DecimalsFormat { get; }
		int MaxVertices { get; }
		int MaxLinedefs { get; }
		int MaxSectors { get; }
		int MaxThings { get; }
		int MinTextureOffset { get; }
		int MaxTextureOffset { get; }
		bool HasLinedefTag { get; }
		bool HasThingTag { get; }
		bool HasThingAction { get; }
		bool HasCustomFields { get; }
		bool HasThingHeight { get; }
		bool HasActionArgs { get; }
		bool HasMixedActivations { get; }
		bool HasPresetActivations { get; }
		bool HasBuiltInActivations { get; }
		bool HasNumericLinedefFlags { get; }
		bool HasNumericThingFlags { get; }
		bool HasNumericLinedefActivations { get; }
		int MaxTag { get; }
		int MinTag { get; }
		int MaxAction { get; }
		int MinAction { get; }
		int MaxArgument { get; }
		int MinArgument { get; }
		int MaxEffect { get; }
		int MinEffect { get; }
		int MaxBrightness { get; }
		int MinBrightness { get; }
		int MaxThingType { get; }
		int MinThingType { get; }
		float MaxCoordinate { get; }
		float MinCoordinate { get; }
		int MaxThingAngle { get; }
		int MinThingAngle { get; }
		Dictionary<MapElementType, Dictionary<string, UniversalType>> UIFields { get; } //mxd. Element type (like "sector"), list of uifields.
		string GetElementName(MapElementType elementtype); //mxd
		MapElementType GetElementType(string elementname); //mxd
	}
}
