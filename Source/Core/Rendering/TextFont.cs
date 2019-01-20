
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
/*
#region ================== Namespaces

using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using SlimDX;

using Configuration = CodeImp.DoomBuilder.IO.Configuration;

#endregion

namespace CodeImp.DoomBuilder.Rendering
{
	internal class TextFont : IDisposable
	{
		#region ================== Structures
		
		// This structure defines character properties
		private struct FontCharacter
		{
			// Variables
			public float width;
			public float height;
			public float u1;
			public float v1;
			public float u2;
			public float v2;
		}

		#endregion
		
		#region ================== Constants

		// Font resource name
		private const string FONT_RESOURCE = "Font.cfg";
		
		// Spacing adjustments
		private const float ADJUST_SPACING = -0.08f;
		
		#endregion

		#region ================== Variables

		// Characters
		private FontCharacter[] characters;
		
		// Disposing
		private bool isdisposed;

		#endregion

		#region ================== Properties

		public bool IsDisposed { get { return isdisposed; } }

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal TextFont()
		{
			// Initialize
			characters = new FontCharacter[256];

			// Make chars configuration
			Configuration cfg = new Configuration();
			
			// Find a resource named Font.cfg
			string[] resnames = General.ThisAssembly.GetManifestResourceNames();
			foreach(string rn in resnames)
			{
				// Found it?
				if(rn.EndsWith(FONT_RESOURCE, StringComparison.OrdinalIgnoreCase))
				{
					// Get a stream from the resource
					Stream fontdata = General.ThisAssembly.GetManifestResourceStream(rn);
					StreamReader fontreader = new StreamReader(fontdata, Encoding.ASCII);

					// Load configuration from stream
					cfg.InputConfiguration(fontreader.ReadToEnd());

					// Done
					fontreader.Dispose();
					break;
				}
			}
			
			// Get the charset from configuration
			IDictionary cfgchars = cfg.ReadSetting("chars", new Hashtable());

			// Go for all defined chars
			foreach(DictionaryEntry item in cfgchars)
			{
				// Get the character Hashtable
				IDictionary chr = (IDictionary)item.Value;
				int i = Convert.ToInt32(item.Key);

				// This is ancient code of mine.
				// The charater sizes were based on 800x600 resolution.
				characters[i].width = (int)chr["width"] / 40f;
				characters[i].height = (int)chr["height"] / 30f;
				characters[i].u1 = (float)chr["u1"];
				characters[i].v1 = (float)chr["v1"];
				characters[i].u2 = (float)chr["u2"];
				characters[i].v2 = (float)chr["v2"];
			}
			
			// We have no destructor
			GC.SuppressFinalize(this);
		}

		// Diposer
		public void Dispose()
		{
			// Not already disposed?
			if(!isdisposed)
			{
				// Done
				isdisposed = true;
			}
		}
		
		#endregion
		
		#region ================== Methods

		// This sets up vertices for a specific character
		// also advances vertsoffset and textx
		public void SetupVertices(DataStream stream, byte c, float scale, int color,
						ref float textx, float texty, float textheight, float offsetv)
		{
			FlatVertex vert = new FlatVertex();

			// Get the character information
			FontCharacter cinfo = characters[c];
			float cwidth = cinfo.width * scale;

			// Create lefttop vertex
			vert.c = color;
			vert.u = cinfo.u1;
			vert.v = cinfo.v1 * 0.5f + offsetv;
			vert.x = textx;
			vert.y = texty;
			stream.Write(vert);

			// Create leftbottom vertex
			vert.c = color;
			vert.u = cinfo.u1;
			vert.v = cinfo.v2 * 0.5f + offsetv;
			vert.x = textx;
			vert.y = texty + textheight;
			stream.Write(vert);

			// Create righttop vertex
			vert.c = color;
			vert.u = cinfo.u2;
			vert.v = cinfo.v1 * 0.5f + offsetv;
			vert.x = textx + cwidth;
			vert.y = texty;
			stream.Write(vert);

			// Create leftbottom vertex
			vert.c = color;
			vert.u = cinfo.u1;
			vert.v = cinfo.v2 * 0.5f + offsetv;
			vert.x = textx;
			vert.y = texty + textheight;
			stream.Write(vert);

			// Create righttop vertex
			vert.c = color;
			vert.u = cinfo.u2;
			vert.v = cinfo.v1 * 0.5f + offsetv;
			vert.x = textx + cwidth;
			vert.y = texty;
			stream.Write(vert);

			// Create rightbottom vertex
			vert.c = color;
			vert.u = cinfo.u2;
			vert.v = cinfo.v2 * 0.5f + offsetv;
			vert.x = textx + cwidth;
			vert.y = texty + textheight;
			stream.Write(vert);

			textx += (cwidth + (ADJUST_SPACING * scale));
		}
		
		// This checks if the given character exists in the charset
		public bool Contains(char c)
		{
			// Convert character to ASCII
			byte[] keybyte = Encoding.ASCII.GetBytes(c.ToString());
			return Contains(keybyte[0]);
		}
		
		// This checks if the given character exists in the charset
		public bool Contains(byte b)
		{
			// Check if the character has been set
			return ((characters[b].width > 0.000000001f) ||
					(characters[b].height > 0.000000001f));
		}

		// This calculates the size of a text string at a given scale
		public SizeF GetTextSize(string text, float scale)
		{
			// Size items
			float sizex = 0, sizey = 0;

			// Get the ASCII bytes for the text
			byte[] btext = Encoding.ASCII.GetBytes(text);

			// Go for all chars in text to calculate final text size
			foreach(byte b in btext)
			{
				// Add to the final size
				sizex += characters[b].width * scale;
				sizey = characters[b].height * scale;
			}

			// Return size
			return new SizeF(sizex, sizey);
		}
		
		#endregion
	}
}
*/