#region ================== Namespaces

using System;
using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.Geometry;
using CodeImp.DoomBuilder.GZBuilder.Data;
using SlimDX;

#endregion

namespace CodeImp.DoomBuilder.ZDoom
{
	public sealed class VoxeldefParser : ZDTextParser
	{
		internal override ScriptType ScriptType { get { return ScriptType.VOXELDEF; } }
		
		private Dictionary<string, ModelData> entries; //sprite name, entry
		internal Dictionary<string, ModelData> Entries { get { return entries; } }

		public override bool Parse(TextResourceData data, bool clearerrors) 
		{
			entries = new Dictionary<string, ModelData>(StringComparer.Ordinal);
			
			//mxd. Already parsed?
			if(!base.AddTextResource(data))
			{
				if(clearerrors) ClearError();
				return true;
			}

			// Cannot process?
			if(!base.Parse(data, clearerrors)) return false;

			List<string> spriteNames = new List<string>();
			string modelName = string.Empty;
			string prevToken = string.Empty;

			// Continue until at the end of the stream
			while(SkipWhitespace(true)) 
			{
				string token = ReadToken().ToLowerInvariant();
				if(string.IsNullOrEmpty(token)) continue;

				if(token == ",") //previous token was a sprite name
				{ 
					if(!string.IsNullOrEmpty(prevToken) && !spriteNames.Contains(prevToken)) spriteNames.Add(prevToken);
					prevToken = StripQuotes(token).ToUpperInvariant();
				} 
				else if(token == "=") //next token should be a voxel model name
				{ 
					if(!string.IsNullOrEmpty(prevToken) && !spriteNames.Contains(prevToken)) spriteNames.Add(prevToken);

					SkipWhitespace(true);
					token = ReadToken();

					if(string.IsNullOrEmpty(token)) 
					{
						ReportError("Expected voxel name");
						return false;
					}

					modelName = StripQuotes(token).ToUpperInvariant();
				} 
				else if(token == "{") //read the settings
				{
					ModelData mde = new ModelData { IsVoxel = true };
					float scale = 1.0f;

					while(SkipWhitespace(true)) 
					{
						token = ReadToken().ToLowerInvariant();
						if(string.IsNullOrEmpty(token)) continue;

						if(token == "}") //store data
						{ 
							if(!string.IsNullOrEmpty(modelName) && spriteNames.Count > 0) 
							{
								mde.ModelNames.Add(modelName);
								mde.SetTransform(Matrix.RotationZ(Angle2D.DegToRad(mde.AngleOffset)), Matrix.Identity, new Vector3(scale));

								foreach(string s in spriteNames)
								{
									//TODO: is this the proper behaviour?
									entries[s] = mde;
								}

								//reset local data
								modelName = string.Empty;
								prevToken = string.Empty;
								spriteNames.Clear();
							}

							break;
						} 
						else if(token == "overridepalette")
						{
							mde.OverridePalette = true;
						} 
						else if(token == "angleoffset")
						{
							if(!NextTokenIs("=")) return false;

							token = ReadToken();
							if(!ReadSignedFloat(token, ref mde.AngleOffset))
							{
								// Not numeric!
								ReportError("Expected AngleOffset value, but got \"" + token + "\"");
								return false;
							}
						} 
						else if(token == "scale") 
						{
							if(!NextTokenIs("=")) return false;

							token = ReadToken();
							if(!ReadSignedFloat(token, ref scale)) 
							{
								// Not numeric!
								ReportError("Expected Scale value, but got \"" + token + "\"");
								return false;
							}
						}

						prevToken = StripQuotes(token).ToUpperInvariant();
					}
				} 
				else 
				{
					prevToken = StripQuotes(token).ToUpperInvariant();
				}
			}

			return entries.Count > 0;
		}
	}
}
