
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
using System.IO;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	public class TextureOrFlatName
	{
		public readonly string LumpName;
		public readonly Boolean IsFlat;

		public TextureOrFlatName(string lumpName, Boolean isFlat)
		{
			LumpName = lumpName;
			IsFlat = isFlat;
		}
	}
}
