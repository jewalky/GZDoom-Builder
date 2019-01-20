
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
using System.Globalization;
using System.IO;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
	public sealed class PatchStructure
	{
		#region ================== Constants

		// Some odd things in ZDoom
		private const string IGNORE_SPRITE = "TNT1A0";

		#endregion

		#region ================== Variables

		// Declaration
		private readonly string name;
		private readonly int offsetx;
		private readonly int offsety;
		private readonly bool flipx;
		private readonly bool flipy;
		private readonly float alpha;
		private readonly int rotation; //mxd
		private readonly TexturePathRenderStyle renderstyle; //mxd
		private readonly PixelColor blendcolor; //mxd
		private readonly TexturePathBlendStyle blendstyle; //mxd
		private static readonly string[] renderStyles = { "copy", "translucent", "add", "subtract", "reversesubtract", "modulate", "copyalpha", "copynewalpha", "overlay" }; //mxd
		private readonly bool skip; //mxd

		#endregion

		#region ================== Properties

		public string Name { get { return name; } }
		public int OffsetX { get { return offsetx; } }
		public int OffsetY { get { return offsety; } }
		public bool FlipX { get { return flipx; } }
		public bool FlipY { get { return flipy; } }
		public float Alpha { get { return alpha; } }
		public int Rotation { get { return rotation; } } //mxd
		public TexturePathRenderStyle RenderStyle { get { return renderstyle; } } //mxd
		public TexturePathBlendStyle BlendStyle { get { return blendstyle; } }
		public PixelColor BlendColor { get { return blendcolor; } }//mxd
		public bool Skip { get { return skip; } } //mxd

		#endregion

		#region ================== Constructor / Disposer

		// Constructor
		internal PatchStructure(TexturesParser parser)
		{
			// Initialize
			alpha = 1.0f;
			renderstyle = TexturePathRenderStyle.COPY;//mxd
			blendstyle = TexturePathBlendStyle.NONE; //mxd
			
			// There should be 3 tokens separated by 2 commas now:
			// Name, Width, Height

			// First token is the class name
			parser.SkipWhitespace(true);
			if(!parser.ReadTextureName(out name, "patch")) return; //mxd
			if(string.IsNullOrEmpty(name))
			{
				parser.ReportError("Expected patch name");
				return;
			}

			//mxd. Skip what must be skipped
			skip = (name.ToUpperInvariant() == IGNORE_SPRITE);

			//mxd
			name = name.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

			// Now we should find a comma
			if(!parser.NextTokenIs(",")) return; //mxd

			// Next is the patch width
			parser.SkipWhitespace(true);
			string tokenstr = parser.ReadToken();
			if(string.IsNullOrEmpty(tokenstr) || !int.TryParse(tokenstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out offsetx))
			{
				parser.ReportError("Expected offset in pixels");
				return;
			}

			// Now we should find a comma again
			if(!parser.NextTokenIs(",")) return; //mxd

			// Next is the patch height
			parser.SkipWhitespace(true);
			tokenstr = parser.ReadToken();
			if(string.IsNullOrEmpty(tokenstr) || !int.TryParse(tokenstr, NumberStyles.Integer, CultureInfo.InvariantCulture, out offsety))
			{
				parser.ReportError("Expected offset in pixels");
				return;
			}

			// Next token is the beginning of the texture scope. If not, then the patch info ends here.
			if(!parser.NextTokenIs("{", false)) return; //mxd

			// Now parse the contents of texture structure
			bool done = false; //mxd
			while(!done && parser.SkipWhitespace(true))
			{
				string token = parser.ReadToken();
				token = token.ToLowerInvariant();

				switch(token) 
				{
					case "flipx":
						flipx = true;
						break;

					case "flipy":
						flipy = true;
						break;

					case "alpha":
						if(!ReadTokenFloat(parser, token, out alpha)) return;
						alpha = General.Clamp(alpha, 0.0f, 1.0f);
						break;

					case "rotate":
						if(!ReadTokenInt(parser, token, out rotation)) return;
						rotation = rotation % 360; //Coalesce multiples
						if(rotation < 0) rotation += 360; //Force positive

						if(rotation != 0 && rotation != 90 && rotation != 180 && rotation != 270) 
						{
							parser.LogWarning("Unsupported rotation (" + rotation + ") in patch \"" + name + "\"");
							rotation = 0;
						}
						break;

					case "style": //mxd
						string s;
						if(!ReadTokenString(parser, token, out s)) return;
						int index = Array.IndexOf(renderStyles, s.ToLowerInvariant());
						renderstyle = index == -1 ? TexturePathRenderStyle.COPY : (TexturePathRenderStyle) index;
						break;

					case "blend": //mxd
						parser.SkipWhitespace(false);
						PixelColor color = new PixelColor();

						// Blend <string color>[,<float alpha>] block?
						token = parser.ReadToken(false);
						if(!parser.ReadByte(token, ref color.r))
						{
							if(!ZDTextParser.GetColorFromString(token, out color))
							{
								parser.ReportError("Unsupported patch blend definition");
								return;
							}
						}
						// That's Blend <int r>,<int g>,<int b>[,<float alpha>] block
						else
						{
							if(!parser.SkipWhitespace(false) ||
								!parser.NextTokenIs(",", false) || !parser.SkipWhitespace(false) || !parser.ReadByte(ref color.g) ||
								!parser.NextTokenIs(",", false) || !parser.SkipWhitespace(false) || !parser.ReadByte(ref color.b))
							{
								parser.ReportError("Unsupported patch blend definition");
								return;
							}
						}

						// Alpha block?
						float blendalpha = -1f;
						parser.SkipWhitespace(false);
						if(parser.NextTokenIs(",", false))
						{
							parser.SkipWhitespace(false);
							if(!ReadTokenFloat(parser, token, out blendalpha))
							{
								parser.ReportError("Unsupported patch blend alpha value");
								return;
							}
						}

						// Blend may never be 0 when using the Tint effect
						if(blendalpha > 0.0f)
						{
							color.a = (byte)General.Clamp((int)(blendalpha * 255), 1, 254);
							blendstyle = TexturePathBlendStyle.TINT;

						}
						else if(blendalpha < 0.0f)
						{
							color.a = 255;
							blendstyle = TexturePathBlendStyle.BLEND;
						}
						else
						{
							// Ignore Blend when alpha == 0
							parser.LogWarning("Blend with zero alpha will be ignored by ZDoom");
							break;
						}

						// Store the color
						blendcolor = color;
						break;

					case "}":
						// Patch scope ends here,
						// break out of this parse loop
						done = true;
						break;
				}
			}
		}

		#endregion

		#region ================== Methods

		// This reads the next token and sets a floating point value, returns false when failed
		private static bool ReadTokenFloat(TexturesParser parser, string propertyname, out float value)
		{
			// Next token is the property value to set
			parser.SkipWhitespace(true);
			string strvalue = parser.ReadToken();
			if(!string.IsNullOrEmpty(strvalue))
			{
				// Try parsing as value
				if(!float.TryParse(strvalue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				{
					parser.ReportError("Expected numeric value for property \"" + propertyname + "\"");
					return false;
				}
				// Success
				return true;
			}

			// Can't find the property value!
			parser.ReportError("Expected a value for property \"" + propertyname + "\"");
			value = 0.0f;
			return false;
		}

		// This reads the next token and sets an integral value, returns false when failed
		private static bool ReadTokenInt(TexturesParser parser, string propertyname, out int value)
		{
			// Next token is the property value to set
			parser.SkipWhitespace(true);
			string strvalue = parser.ReadToken();
			if(!string.IsNullOrEmpty(strvalue))
			{
				// Try parsing as value
				if(!int.TryParse(strvalue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
				{
					parser.ReportError("Expected integral value for property \"" + propertyname + "\"");
					return false;
				}

				// Success
				return true;
			}

			// Can't find the property value!
			parser.ReportError("Expected a value for property \"" + propertyname + "\"");
			value = 0;
			return false;
		}

		//mxd. This reads the next token and sets a string value, returns false when failed
		private static bool ReadTokenString(TexturesParser parser, string propertyname, out string value) 
		{
			parser.SkipWhitespace(true);
			value = parser.StripTokenQuotes(parser.ReadToken());
			
			if(string.IsNullOrEmpty(value)) 
			{
				// Can't find the property value!
				parser.ReportError("Expected a value for property \"" + propertyname + "\"");
				return false;
			}

			return true;
		}

		#endregion
	}
}
