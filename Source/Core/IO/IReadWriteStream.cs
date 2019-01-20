
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

using CodeImp.DoomBuilder.Geometry;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal interface IReadWriteStream
	{
		// Properties
		bool IsWriting { get; }
		
		// Management
		void Begin();
		void End();
		
		// Bidirectional members
		void rwInt(ref int v);
		void rwByte(ref byte v);
		void rwShort(ref short v);
		void rwString(ref string v);
		void rwLong(ref long v);
		void rwUInt(ref uint v);
		void rwUShort(ref ushort v);
		void rwULong(ref ulong v);
		void rwFloat(ref float v);
		void rwVector2D(ref Vector2D v);
		void rwVector3D(ref Vector3D v);
		void rwBool(ref bool v);

		// Write-only members
		void wInt(int v);
		void wByte(byte v);
		void wShort(short v);
		void wString(string v);
		void wLong(long v);
		void wUInt(uint v);
		void wUShort(ushort v);
		void wULong(ulong v);
		void wFloat(float v);
		void wVector2D(Vector2D v);
		void wVector3D(Vector3D v);
		void wBool(bool v);

		// Read-only members
		void rInt(out int v);
		void rByte(out byte v);
		void rShort(out short v);
		void rString(out string v);
		void rLong(out long v);
		void rUInt(out uint v);
		void rUShort(out ushort v);
		void rULong(out ulong v);
		void rFloat(out float v);
		void rVector2D(out Vector2D v);
		void rVector3D(out Vector3D v);
		void rBool(out bool v);
	}
}
