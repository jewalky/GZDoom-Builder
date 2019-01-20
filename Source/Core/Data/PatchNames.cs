
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

using System.IO;
using CodeImp.DoomBuilder.IO;

#endregion

namespace CodeImp.DoomBuilder.Data
{
	internal sealed class PatchNames
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private string[] pnames;

		#endregion

		#region ================== Properties

		public string this[int index] { get { return pnames[index]; } }
		public int Length { get { return pnames.Length; } }
		
		#endregion

		#region ================== Constructor / Disposer

		// Constructor for empty list
		public PatchNames()
		{
			// Create array
			pnames = new string[0];
		}

		// Constructor
		public PatchNames(Stream stream)
		{
			BinaryReader reader = new BinaryReader(stream);

			// Read length of array
			stream.Seek(0, SeekOrigin.Begin);
			uint length = reader.ReadUInt32();
			
			// Create array
			pnames = new string[length];

			// Read all patch names
			for(uint i = 0; i < length; i++)
			{
				byte[] bytes = reader.ReadBytes(8);
				pnames[i] = Lump.MakeNormalName(bytes, WAD.ENCODING).ToUpperInvariant();
			}
		}

		#endregion

		#region ================== Methods

		#endregion
	}
}
