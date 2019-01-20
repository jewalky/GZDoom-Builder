using System.IO;

namespace CodeImp.DoomBuilder.ZDoom
{
	public class AmbientSoundInfo
	{
		#region ================== Enums

		public enum AmbientType
		{
			NONE,
			POINT,
			SURROUND,
			WORLD
		}

		public enum AmbientMode
		{
			NONE,
			CONTINUOUS,
			RANDOM,
			PERIODIC
		}

		#endregion

		#region ================== Variables

		private string soundname;
		private int index = -1;

		private AmbientType type = AmbientType.NONE;
		private AmbientMode mode = AmbientMode.NONE;
		private float volume = 1.0f;

		private float attenuation = 1.0f;

		private float minsecs;
		private float maxsecs;
		private float secs;

		// Editor sound radii
		private float minradius;
		private float maxradius;

		#endregion

		#region ================== Properties

		public string SoundName { get { return soundname; } }
		public int Index { get { return index; } } // Ambient sound index

		// Sound settings
		public AmbientType AmbientSoundType { get { return type; } }
		public AmbientMode AmbientSoundMode { get { return mode; } }
		public float Volume { get { return volume; } }

		// Can be set when AmbientType == POINT
		public float Attenuation { get { return attenuation; } }

		// Used when AmbientMode == RANDOM
		public float SecondsMin { get { return minsecs; } }
		public float SecondsMax { get { return maxsecs; } }

		// Used when AmbientMode == PERIODIC
		public float Seconds { get { return secs; } }

		// Editor sound radii
		public float MinimumRadius { get { return minradius; } }
		public float MaximumRadius { get { return maxradius; } }

		#endregion

		#region ================== Methods

		internal bool Setup(SndInfoParser parser)
		{
			// Read index
			if(!parser.ReadSignedInt(ref index))
			{
				// Not numeric!
				parser.ReportError("Expected $ambient <index> value");
				return false;
			}

			// Check index
			//INFO: Up to 64 ambient sounds can be used in the Doom map format and 256 in Hexen format. UDMF maps have no limit.
			//TODO: can index be zero/negative in UDMF?
			if(General.Map.DOOM && (index < 1 || index > 64))
			{
				parser.ReportError("$ambient <index> must be in [1..64] range");
				return false;
			}
			if(General.Map.HEXEN && (index < 1 || index > 256))
			{
				parser.ReportError("$ambient <index> must be in [1..256] range");
				return false;
			}

			// Read name
			if(!parser.SkipWhitespace(true)) return false;
			soundname = parser.StripTokenQuotes(parser.ReadToken(false));
			if(string.IsNullOrEmpty(soundname))
			{
				parser.ReportError("Expected $ambient <logicalsound> value");
				return false;
			}

			// Next token can be either [type] or <mode>...
			if(!parser.SkipWhitespace(true)) return false;
			string token = parser.ReadToken(false).ToLowerInvariant();

			// Can be [type]
			if(token == "point" || token == "surround" || token == "world")
			{
				// Next token may be attenuation...
				if(token == "point")
				{
					if(!parser.SkipWhitespace(false)) return false;
					string next = parser.ReadToken(false);
					if(!parser.ReadSignedFloat(next, ref attenuation) || attenuation < 0f)
					{
						// Rewind so this structure can be read again
						parser.DataStream.Seek(-next.Length - 1, SeekOrigin.Current);
					}
				}

				// Store type
				switch(token)
				{
					case "point": type = AmbientType.POINT; break;
					case "surround": type = AmbientType.SURROUND; break;
					case "world": type = AmbientType.WORLD; break;
				}

				// Read next token
				if(!parser.SkipWhitespace(false)) return false;
				token = parser.ReadToken(false).ToLowerInvariant();
			}
				
			// Sould be <mode>
			if(token == "continuous" || token == "random" || token == "periodic")
			{
				// Next 2 tokens must be minsecs and maxsecs
				if(token == "random")
				{
					if(!parser.SkipWhitespace(false)) return false;
					if(!parser.ReadSignedFloat(ref minsecs) || minsecs < 0f)
					{
						parser.ReportError("Expected $ambient <minsecs> value");
						return false;
					}

					if(!parser.SkipWhitespace(false)) return false;
					if(!parser.ReadSignedFloat(ref maxsecs) || maxsecs < 0f)
					{
						parser.ReportError("Expected $ambient <maxsecs> value");
						return false;
					}
				}
				// Next token must be secs
				else if(token == "periodic")
				{
					if(!parser.SkipWhitespace(false)) return false;
					if(!parser.ReadSignedFloat(ref secs) || secs < 0f)
					{
						parser.ReportError("Expected $ambient <secs> value");
						return false;
					}
				}

				// Store mode
				switch(token)
				{
					case "continuous": mode = AmbientMode.CONTINUOUS; break;
					case "random": mode = AmbientMode.RANDOM; break;
					case "periodic": mode = AmbientMode.PERIODIC; break;
				}
			}
			else
			{
				parser.ReportError("Expected ambient sound <mode> or [type]");
				return false;
			}

			// Read volume
			if(!parser.SkipWhitespace(false)) return false;
			if(!parser.ReadSignedFloat(ref volume) || volume < 0f)
			{
				parser.ReportError("Expected ambient sound <volume> value");
				return false;
			}

			return true;
		}

		internal void SetupSound(SoundInfo info)
		{
			// Store radii
			minradius = info.MinimumDistance / info.Attenuation;

			if(info.Rolloff == SoundInfo.RolloffType.LOG)
			{
				// Calculate from RolloffFactor
				maxradius = info.MinimumDistance + info.RolloffFactor * info.MinimumDistance;
			}
			else
			{
				maxradius = info.MaximumDistance;
			}
			maxradius /= info.Attenuation;
		}

		#endregion
	}
}
