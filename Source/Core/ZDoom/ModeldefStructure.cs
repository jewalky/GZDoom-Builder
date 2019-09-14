#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using SlimDX;
using CodeImp.DoomBuilder.GZBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.ZDoom 
{
	internal sealed class ModeldefStructure
	{
		#region ================== Constants

		private const int MAX_MODELS = 4; //maximum models per modeldef entry, zero-based

		#endregion
		
		#region ================== Structs

		internal struct FrameStructure
		{
			public string SpriteName; // Stays here for HashSet duplicate checks
			public int ModelIndex;
			public int FrameIndex;
			public string FrameName;
		}

		#endregion

		#region ================== Variables

		private string[] skinnames;
		private Dictionary<int, string>[] surfaceskinenames;
		private string[] modelnames;
		private string path;
		private Vector3 scale;
		private Vector3 offset;
		private float angleoffset;
		private float pitchoffset;
		private float rolloffset;
		private bool inheritactorpitch;
		private bool useactorpitch;
		private bool useactorroll;

		private Dictionary<string, HashSet<FrameStructure>> frames;

		#endregion

		#region ================== Properties

		public string[] SkinNames { get { return skinnames; } }
		public Dictionary<int, string>[] SurfaceSkinNames { get { return surfaceskinenames; } }
		public string[] ModelNames { get { return modelnames; } }
		public Vector3 Scale { get { return scale; } }
		public Vector3 Offset { get { return offset; } }
		public float AngleOffset { get { return angleoffset; } }
		public float PitchOffset { get { return pitchoffset; } }
		public float RollOffset { get { return rolloffset; } }
		public bool InheritActorPitch { get { return inheritactorpitch; } }
		public bool UseActorPitch { get { return useactorpitch; } }
		public bool UseActorRoll { get { return useactorroll; } }
		public string DataPath { get { return path; } } // biwa

		public Dictionary<string, HashSet<FrameStructure>> Frames { get { return frames; } }

		#endregion

		#region ================== Constructor

		internal ModeldefStructure()
		{
			path = string.Empty;
			skinnames = new string[MAX_MODELS];
			modelnames = new string[MAX_MODELS];
			frames = new Dictionary<string, HashSet<FrameStructure>>(StringComparer.OrdinalIgnoreCase);
			scale = new Vector3(1.0f, 1.0f, 1.0f);
			surfaceskinenames = new Dictionary<int, string>[MAX_MODELS];
			for(int i = 0; i < MAX_MODELS; i++)
			{
				surfaceskinenames[i] = new Dictionary<int, string>();
			}
		}

		#endregion

		#region ================== Parsing

		internal bool Parse(ModeldefParser parser)
		{
			// Read modeldef structure contents
			bool parsingfinished = false;
			while(!parsingfinished && parser.SkipWhitespace(true)) 
			{
				string token = parser.ReadToken().ToLowerInvariant();
				if(string.IsNullOrEmpty(token)) continue;

				switch(token)
				{
					case "path":
						parser.SkipWhitespace(true);
						path = parser.StripTokenQuotes(parser.ReadToken(false)).Replace("/", "\\"); // Don't skip newline
						if(string.IsNullOrEmpty(path))
						{
							parser.ReportError("Expected model path");
							return false;
						}
						break;

					case "model":
						parser.SkipWhitespace(true);

						// Model index
						int index = 0;
						token = parser.ReadToken();
						if(!parser.ReadSignedInt(token, ref index))
						{
							// Not numeric!
							parser.ReportError("Expected model index, but got \"" + token + "\"");
							return false;
						}

						if(index < 0 || index > MAX_MODELS - 1)
						{
							// Out of bounds
							parser.ReportError("Model index must be in [0.." + (MAX_MODELS - 1) + "] range");
							return false;
						}

						parser.SkipWhitespace(true);

						// Model path
						token = parser.StripTokenQuotes(parser.ReadToken(false)).ToLowerInvariant(); // Don't skip newline
						if(string.IsNullOrEmpty(token)) 
						{
							parser.ReportError("Expected model name");
							return false;
						} 

						// Check invalid path chars
						if(!parser.CheckInvalidPathChars(token)) return false;

						// Check extension
						string modelext = Path.GetExtension(token);
						if(string.IsNullOrEmpty(modelext)) 
						{
							parser.ReportError("Model \"" + token + "\" won't be loaded. Models without extension are not supported by GZDoom");
							return false;
						}

						if(modelext != ".md3" && modelext != ".md2" && modelext != ".3d" && modelext != ".obj") 
						{
							parser.ReportError("Model \"" + token + "\" won't be loaded. Only Unreal 3D, MD2, MD3, and OBJ models are supported");
							return false;
						}

						// GZDoom allows models with identical index, it uses the last one encountered
						modelnames[index] = Path.Combine(path, token).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
						break;

					case "skin":
						parser.SkipWhitespace(true);

						// Skin index
						int skinindex = 0;
						token = parser.ReadToken();
						if(!parser.ReadSignedInt(token, ref skinindex))
						{
							// Not numeric!
							parser.ReportError("Expected skin index, but got \"" + token + "\"");
							return false;
						}

						if(skinindex < 0 || skinindex >= MAX_MODELS)
						{
							// Out of bounds
							parser.ReportError("Skin index must be in [0.." + (MAX_MODELS - 1) + "] range");
							return false;
						}

						parser.SkipWhitespace(true);

						// Skin path
						token = parser.StripTokenQuotes(parser.ReadToken(false)).ToLowerInvariant(); // Don't skip newline
						if(string.IsNullOrEmpty(token)) 
						{
							parser.ReportError("Expected skin path");
							return false;
						} 

						// Check invalid path chars
						if(!parser.CheckInvalidPathChars(token)) return false;

						// GZDoom allows skins with identical index, it uses the last one encountered
						skinnames[skinindex] = Path.Combine(path, token);
						break;

					// SurfaceSkin <int modelindex> <int surfaceindex> <string skinfile>
					case "surfaceskin":
						parser.SkipWhitespace(true);

						// Model index
						int modelindex = 0;
						token = parser.ReadToken();
						if(!parser.ReadSignedInt(token, ref modelindex))
						{
							// Not numeric!
							parser.ReportError("Expected model index, but got \"" + token + "\"");
							return false;
						}

						if(modelindex < 0 || modelindex >= MAX_MODELS)
						{
							// Out of bounds
							parser.ReportError("Model index must be in [0.." + (MAX_MODELS - 1) + "] range");
							return false;
						}

						parser.SkipWhitespace(true);

						// Surfaceindex index
						int surfaceindex = 0;
						token = parser.ReadToken();
						if(!parser.ReadSignedInt(token, ref surfaceindex))
						{
							// Not numeric!
							parser.ReportError("Expected surface index, but got \"" + token + "\"");
							return false;
						}

						if(surfaceindex < 0)
						{
							// Out of bounds
							parser.ReportError("Surface index must be positive integer");
							return false;
						}

						parser.SkipWhitespace(true);

						// Skin path
						token = parser.StripTokenQuotes(parser.ReadToken(false)).ToLowerInvariant(); // Don't skip newline
						if(string.IsNullOrEmpty(token))
						{
							parser.ReportError("Expected skin path");
							return false;
						}

						// Check invalid path chars
						if(!parser.CheckInvalidPathChars(token)) return false;

						// Store
						surfaceskinenames[modelindex][surfaceindex] = Path.Combine(path, token).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
						break;

					case "scale":
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref scale.Y)) 
						{
							// Not numeric!
							parser.ReportError("Expected Scale X value, but got \"" + token + "\"");
							return false;
						}

						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref scale.X)) 
						{
							// Not numeric!
							parser.ReportError("Expected Scale Y value, but got \"" + token + "\"");
							return false;
						}

						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref scale.Z)) 
						{
							// Not numeric!
							parser.ReportError("Expected Scale Z value, but got \"" + token + "\"");
							return false;
						}
						break;

					case "offset":
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref offset.X)) 
						{
							// Not numeric!
							parser.ReportError("Expected Offset X value, but got \"" + token + "\"");
							return false;
						}

						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref offset.Y)) 
						{
							// Not numeric!
							parser.ReportError("Expected Offset Y value, but got \"" + token + "\"");
							return false;
						}

						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref offset.Z)) 
						{
							// Not numeric!
							parser.ReportError("Expected Offset Z value, but got \"" + token + "\"");
							return false;
						}
						break;

					case "zoffset":
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref offset.Z)) 
						{
							// Not numeric!
							parser.ReportError("Expected ZOffset value, but got \"" + token + "\"");
							return false;
						}
						break;

					case "angleoffset":
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref angleoffset)) 
						{
							// Not numeric!
							parser.ReportError("Expected AngleOffset value, but got \"" + token + "\"");
							return false;
						}
						break;

					case "pitchoffset":
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref pitchoffset)) 
						{
							// Not numeric!
							parser.ReportError("Expected PitchOffset value, but got \"" + token + "\"");
							return false;
						}
						break;

					case "rolloffset":
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(!parser.ReadSignedFloat(token, ref rolloffset)) 
						{
							// Not numeric!
							parser.ReportError("Expected RollOffset value, but got \"" + token + "\"");
							return false;
						}
						break;

					case "useactorpitch":
						inheritactorpitch = false;
						useactorpitch = true;
						break;

					case "useactorroll":
						useactorroll = true;
						break;

					case "inheritactorpitch":
						inheritactorpitch = true;
						useactorpitch = false;
						parser.LogWarning("INHERITACTORPITCH flag is deprecated. Consider using USEACTORPITCH flag instead");
						break;

					case "inheritactorroll": 
						useactorroll = true;
						parser.LogWarning("INHERITACTORROLL flag is deprecated. Consider using USEACTORROLL flag instead");
						break;

					//FrameIndex <XXXX> <X> <model index> <frame number>
					case "frameindex":
						// Sprite name
						parser.SkipWhitespace(true);
						string fispritename = parser.ReadToken();
						if(string.IsNullOrEmpty(fispritename))
						{
							parser.ReportError("Expected sprite name");
							return false;
						}
						if(fispritename.Length != 4)
						{
							parser.ReportError("Sprite name must be 4 characters long");
							return false;
						}

						// Sprite frame
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(string.IsNullOrEmpty(token))
						{
							parser.ReportError("Expected sprite frame");
							return false;
						}
						if(token.Length != 1)
						{
							parser.ReportError("Sprite frame must be 1 character long");
							return false;
						}

						// Make full name
						fispritename += token;

						// Model index
						parser.SkipWhitespace(true);
						int fimodelindnex = 0;
						token = parser.ReadToken();
						if(!parser.ReadSignedInt(token, ref fimodelindnex))
						{
							// Not numeric!
							parser.ReportError("Expected model index, but got \"" + token + "\"");
							return false;
						}
						if(fimodelindnex < 0 || fimodelindnex > MAX_MODELS - 1)
						{
							// Out of bounds
							parser.ReportError("Model index must be in [0.." + (MAX_MODELS - 1) + "] range");
							return false;
						}

						// Frame number
						parser.SkipWhitespace(true);
						int fiframeindnex = 0;
						token = parser.ReadToken();
						//INFO: setting frame index to a negative number disables model rendering in GZDoom
						if(!parser.ReadSignedInt(token, ref fiframeindnex))
						{
							// Not numeric!
							parser.ReportError("Expected frame index, but got \"" + token + "\"");
							return false;
						}

						// Add to collection
						FrameStructure fifs = new FrameStructure { FrameIndex = fiframeindnex, ModelIndex = fimodelindnex, SpriteName = fispritename };
						if(!frames.ContainsKey(fispritename))
						{
							frames.Add(fispritename, new HashSet<FrameStructure>());
							frames[fispritename].Add(fifs);
						}
						else if(frames[fispritename].Contains(fifs))
						{
							parser.LogWarning("Duplicate FrameIndex definition");
						}
						else
						{
							frames[fispritename].Add(fifs);
						}
						break;

					//Frame <XXXX> <X> <model index> <"frame name">
					case "frame":
						// Sprite name
						parser.SkipWhitespace(true);
						string spritename = parser.ReadToken();
						if(string.IsNullOrEmpty(spritename))
						{
							parser.ReportError("Expected sprite name");
							return false;
						}
						if(spritename.Length != 4)
						{
							parser.ReportError("Sprite name must be 4 characters long");
							return false;
						}

						// Sprite frame
						parser.SkipWhitespace(true);
						token = parser.ReadToken();
						if(string.IsNullOrEmpty(token))
						{
							parser.ReportError("Expected sprite frame");
							return false;
						}
						if(token.Length != 1)
						{
							parser.ReportError("Sprite frame must be 1 character long");
							return false;
						}

						// Make full name
						spritename += token;

						// Model index
						parser.SkipWhitespace(true);
						int modelindnex = 0;
						token = parser.ReadToken();
						if(!parser.ReadSignedInt(token, ref modelindnex))
						{
							// Not numeric!
							parser.ReportError("Expected model index, but got \"" + token + "\"");
							return false;
						}
						if(modelindnex < 0 || modelindnex > MAX_MODELS - 1)
						{
							// Out of bounds
							parser.ReportError("Model index must be in [0.." + (MAX_MODELS - 1) + "] range");
							return false;
						}

						// Frame name
						parser.SkipWhitespace(true);
						string framename = parser.StripTokenQuotes(parser.ReadToken());
						if(string.IsNullOrEmpty(framename))
						{
							parser.ReportError("Expected frame name");
							return false;
						}

						// Add to collection
						FrameStructure fs = new FrameStructure { FrameName = framename, ModelIndex = modelindnex, SpriteName = spritename };
						if(!frames.ContainsKey(spritename))
						{
							frames.Add(spritename, new HashSet<FrameStructure>());
							frames[spritename].Add(fs);
						}
						else if(frames[spritename].Contains(fs))
						{
							parser.LogWarning("Duplicate Frame definition");
						}
						else
						{
							frames[spritename].Add(fs);
						}
						break;

					case "{":
						parser.ReportError("Unexpected scope start");
						return false;

					// Structure ends here
					case "}":
						parsingfinished = true;
						break;
				}
			}

			// Perform some integrity checks
			if(!parsingfinished)
			{
				parser.ReportError("Unclosed structure scope");
				return false;
			}

			// Any models defined?
			bool valid = false;
			for(int i = 0; i < modelnames.Length; i++)
			{
				if(!string.IsNullOrEmpty(modelnames[i]))
				{
					//INFO: skin may be defined in the model itself, so we don't check it here
					valid = true;
					break;
				}
			}

			if(!valid)
			{
				parser.ReportError("Structure doesn't define any models");
				return false;
			}

			// Check skin-model associations
			for(int i = 0; i < skinnames.Length; i++)
			{
				if(!string.IsNullOrEmpty(skinnames[i]) && string.IsNullOrEmpty(modelnames[i]))
				{
					parser.ReportError("No model is defined for skin " + i + ":\"" + skinnames[i] + "\"");
					return false;
				}
			}

			// Check surfaceskin-model associations
			for(int i = 0; i < surfaceskinenames.Length; i++)
			{
				if(surfaceskinenames[i].Count > 0 && string.IsNullOrEmpty(modelnames[i]))
				{
					parser.ReportError("No model is defined for surface skin " + i);
					return false;
				}
			}

			return true;
		}

		#endregion
	}
}
