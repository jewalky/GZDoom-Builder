
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
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.Config
{
	internal interface IFilledTextureSet
	{
		// Properties
		string Name { get; }
		ICollection<ImageData> Textures { get; }
		ICollection<ImageData> Flats { get; }
	}
}
