
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
using CodeImp.DoomBuilder.Geometry;
using System.IO;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal sealed class DeserializerStream : IReadWriteStream, IDisposable
	{
		#region ================== Constants

		#endregion

		#region ================== Variables

		private Stream stream;
		private readonly BinaryReader reader;
		private string[] stringstable;
		private int stringtablepos;
		private bool isdisposed; //mxd

		#endregion

		#region ================== Properties

		public bool IsWriting { get { return false; } }

		public int EndPosition { get { return stringtablepos; } }

		#endregion

		#region ================== Constructor / Destructor

		// Constructor
		public DeserializerStream(Stream stream)
		{
			// Initialize
			this.stream = stream;
			this.reader = new BinaryReader(stream);
		}

		//mxd
		public void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				if(reader != null) reader.Close();
				if(stream != null)
				{
					stream.Dispose();
					stream = null;
				}

				isdisposed = true;
			}
		}

		#endregion

		#region ================== Methods

		// Management
		public void Begin()
		{
			// First 4 bytes are reserved for the offset of the strings table
			stringtablepos = reader.ReadInt32();
			stream.Seek(stringtablepos, SeekOrigin.Begin);
			
			// Read the strings
			List<string> strings = new List<string>();
			while(stream.Position < (int)stream.Length)
				strings.Add(reader.ReadString());
			stringstable = strings.ToArray();

			// Back to start
			stream.Seek(4, SeekOrigin.Begin);
		}
		
		public void End()
		{
		}

		// Bidirectional
		public void rwInt(ref int v) { v = reader.ReadInt32(); }

		public void rwByte(ref byte v) { v = reader.ReadByte(); }

		public void rwShort(ref short v) { v = reader.ReadInt16(); }

		public void rwString(ref string v)
		{
			ushort index = reader.ReadUInt16();
			v = stringstable[index];
		}

		public void rwLong(ref long v) { v = reader.ReadInt64(); }

		public void rwUInt(ref uint v) { v = reader.ReadUInt32(); }

		public void rwUShort(ref ushort v) { v = reader.ReadUInt16(); }

		public void rwULong(ref ulong v) { v = reader.ReadUInt64(); }

		public void rwFloat(ref float v) { v = reader.ReadSingle(); }

		public void rwBool(ref bool v) { v = reader.ReadBoolean(); }

		public void rwVector2D(ref Vector2D v)
		{
			v.x = reader.ReadSingle();
			v.y = reader.ReadSingle();
		}

		public void rwVector3D(ref Vector3D v)
		{
			v.x = reader.ReadSingle();
			v.y = reader.ReadSingle();
			v.z = reader.ReadSingle();
		}

		// Write-only is not supported
		public void wInt(int v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wByte(byte v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wShort(short v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wString(string v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wLong(long v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wUInt(uint v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wUShort(ushort v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wULong(ulong v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wFloat(float v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wBool(bool v) { General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support."); }

		public void wVector2D(Vector2D v)
		{
			General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support.");
		}

		public void wVector3D(Vector3D v)
		{
			General.Fail("Write-only is not supported on deserialization stream. Consider passing the element by reference for bidirectional support.");
		}

		// Read-only
		public void rInt(out int v) { v = reader.ReadInt32(); }

		public void rByte(out byte v) { v = reader.ReadByte(); }

		public void rShort(out short v) { v = reader.ReadInt16(); }

		public void rString(out string v)
		{
			ushort index = reader.ReadUInt16();
			v = stringstable[index];
		}

		public void rLong(out long v) { v = reader.ReadInt64(); }

		public void rUInt(out uint v) { v = reader.ReadUInt32(); }

		public void rUShort(out ushort v) { v = reader.ReadUInt16(); }

		public void rULong(out ulong v) { v = reader.ReadUInt64(); }

		public void rFloat(out float v) { v = reader.ReadSingle(); }

		public void rBool(out bool v) { v = reader.ReadBoolean(); }

		public void rVector2D(out Vector2D v)
		{
			v = new Vector2D();
			v.x = reader.ReadSingle();
			v.y = reader.ReadSingle();
		}

		public void rVector3D(out Vector3D v)
		{
			v = new Vector3D();
			v.x = reader.ReadSingle();
			v.y = reader.ReadSingle();
			v.z = reader.ReadSingle();
		}
		
		#endregion
	}
}
