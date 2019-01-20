#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
	public struct CameraTextureData
	{
		public string Name;
		public int Width;
		public int Height;
		public float ScaleX;
		public float ScaleY;
		public bool WorldPanning;
		public bool FitTexture;
	}
	
	//mxd. Currently this only parses cameratextures
	internal sealed class AnimdefsParser : ZDTextParser
	{
		#region ================== Variables

		private readonly Dictionary<string, CameraTextureData> cameratextures;

		#endregion

		#region ================== Properties

		internal override ScriptType ScriptType { get { return ScriptType.ANIMDEFS; } }

		public Dictionary<string, CameraTextureData> CameraTextures { get { return cameratextures; } }

		#endregion

		#region ================== Constructor

		internal AnimdefsParser()
		{
			cameratextures = new Dictionary<string, CameraTextureData>();
		}

		#endregion

		#region ================== Parsing

		public override bool Parse(TextResourceData data, bool clearerrors)
		{
			// Already parsed?
			if(!base.AddTextResource(data))
			{
				if(clearerrors) ClearError();
				return true;
			}

			// Cannot process?
			if(!base.Parse(data, clearerrors)) return false;

			// Continue until at the end of the stream
			while(SkipWhitespace(true))
			{
				string token = ReadToken();
				if(string.IsNullOrEmpty(token) || string.Compare(token, "CAMERATEXTURE", true) != 0) continue;

				// Texture name
				SkipWhitespace(true);
				string texturename = StripQuotes(ReadToken(false));
				if(string.IsNullOrEmpty(texturename))
				{
					ReportError("Expected camera texture name");
					return false;
				}

                /*
				// Camera texture names are limited to 8 chars
				if(texturename.Length > DataManager.CLASIC_IMAGE_NAME_LENGTH)
				{
					ReportError("Camera texture names must be no longer than " + DataManager.CLASIC_IMAGE_NAME_LENGTH + " chars");
					return false;
				}*/
                // [ZZ] not really. I've successfully tried it with >8 chars and it works.

				// Width
				int width = -1;
				SkipWhitespace(true);
				if(!ReadSignedInt(ref width) || width < 1)
				{
					ReportError("Expected camera texture width");
					return false;
				}

				// Height
				int height = -1;
				SkipWhitespace(true);
				if(!ReadSignedInt(ref height) || height < 1)
				{
					ReportError("Expected camera texture height");
					return false;
				}

				// "Fit" keyword?
				bool worldpanning = false;
				bool fit = false;
				float scalex = 1.0f;
				float scaley = 1.0f;

				if(NextTokenIs("fit", false))
				{
					fit = true;
					int fitwidth = width;
					int fitheight = height;
					
					// Fit width
					SkipWhitespace(true);
					if(!ReadSignedInt(ref fitwidth) || fitwidth < 1)
					{
						ReportError("Expected camera texture fit width");
						return false;
					}

					// Fit height
					SkipWhitespace(true);
					if(!ReadSignedInt(ref fitheight) || fitheight < 1)
					{
						ReportError("Expected camera texture fit height");
						return false;
					}

					// Update scale
					scalex = (float)fitwidth / width;
					scaley = (float)fitheight / height;

					// WorldPanning
					worldpanning = NextTokenIs("worldpanning", false);
				}
				else if(NextTokenIs("worldpanning", false))
				{
					worldpanning = true;
				}

				// Check results
				if(cameratextures.ContainsKey(texturename.ToUpperInvariant()))
				{
					ReportError("Camera texture \"" + texturename + "\" is defined more than once");
					return false;
				}

				// Store results
				texturename = texturename.ToUpperInvariant();
				cameratextures[texturename] = new CameraTextureData { Name = texturename, Width = width, Height = height, 
																	  ScaleX = scalex, ScaleY = scaley, 
																	  WorldPanning = worldpanning, FitTexture = fit };
			}

			return true;
		}

		#endregion
	}
}
