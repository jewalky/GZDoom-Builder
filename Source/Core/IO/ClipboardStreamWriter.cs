#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Text;
using CodeImp.DoomBuilder.Map;
using System.IO;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Types;

#endregion

namespace CodeImp.DoomBuilder.IO
{
	internal class ClipboardStreamWriter
	{
		#region ================== Constants

		private const string UDMF_CONFIG_NAME = "UDMF.cfg";

		#endregion
		
		#region ================== Variables

		private readonly Configuration config;

		#endregion

		#region ================== Constructor / Disposer

		public ClipboardStreamWriter() 
		{
			// Make configurations
			config = new Configuration();

			// Find a resource named UDMF.cfg
			string[] resnames = General.ThisAssembly.GetManifestResourceNames();
			foreach(string rn in resnames) 
			{
				// Found it?
				if(rn.EndsWith(UDMF_CONFIG_NAME, StringComparison.OrdinalIgnoreCase)) 
				{
					// Get a stream from the resource
					Stream udmfcfg = General.ThisAssembly.GetManifestResourceStream(rn);
					StreamReader udmfcfgreader = new StreamReader(udmfcfg, Encoding.ASCII);

					// Load configuration from stream
					config.InputConfiguration(udmfcfgreader.ReadToEnd());

					// Now we add the linedef flags, activations and thing flags
					// to this list, so that these don't show up in the custom
					// fields list either. We use true as dummy value (it has no meaning)

					// Add linedef flags
					foreach(KeyValuePair<string, string> flag in General.Map.Config.LinedefFlags)
						config.WriteSetting("managedfields.linedef." + flag.Key, true);

					// Add linedef activations
					foreach(LinedefActivateInfo activate in General.Map.Config.LinedefActivates)
						config.WriteSetting("managedfields.linedef." + activate.Key, true);

					//mxd. Add sidedef flags
					foreach(KeyValuePair<string, string> flag in General.Map.Config.SidedefFlags)
						config.WriteSetting("managedfields.sidedef." + flag.Key, true);

					//mxd. Add sector flags
					foreach(KeyValuePair<string, string> flag in General.Map.Config.SectorFlags)
						config.WriteSetting("managedfields.sector." + flag.Key, true);
					foreach(KeyValuePair<string, string> flag in General.Map.Config.CeilingPortalFlags)
						config.WriteSetting("managedfields.sector." + flag.Key, true);
					foreach(KeyValuePair<string, string> flag in General.Map.Config.FloorPortalFlags)
						config.WriteSetting("managedfields.sector." + flag.Key, true);

					// Add thing flags
					foreach(KeyValuePair<string, string> flag in General.Map.Config.ThingFlags)
						config.WriteSetting("managedfields.thing." + flag.Key, true);

					// Done
					udmfcfgreader.Dispose();
					break;
				}
			}
		}

		#endregion

		#region ================== Writing

		public void Write(MapSet map, Stream stream) 
		{
			Write(map.Vertices, map.Linedefs, map.Sidedefs, map.Sectors, map.Things, stream);
		}

		public void Write(ICollection<Vertex> vertices, ICollection<Linedef> linedefs, ICollection<Sidedef> sidedefs, 
						  ICollection<Sector> sectors, ICollection<Thing> things, Stream stream) 
		{
			// Create collections
			Dictionary<Vertex, int> vertexids = new Dictionary<Vertex, int>();
			Dictionary<Sidedef, int> sidedefids = new Dictionary<Sidedef, int>();
			Dictionary<Sector, int> sectorids = new Dictionary<Sector, int>();

			// Index the elements in the data structures
			foreach(Vertex v in vertices) vertexids.Add(v, vertexids.Count);
			foreach(Sidedef sd in sidedefs) sidedefids.Add(sd, sidedefids.Count);
			foreach(Sector s in sectors) sectorids.Add(s, sectorids.Count);

			BinaryWriter writer = new BinaryWriter(stream);
			
			// Write the data structures to stream
			writer.Write(vertices.Count); //mxd
			writer.Write(sectors.Count); //mxd
			writer.Write(linedefs.Count); //mxd
			writer.Write(things.Count); //mxd
			WriteVertices(vertices, writer);
			WriteSectors(sectors, writer);
			WriteSidedefs(sidedefs, writer, sectorids);
			WriteLinedefs(linedefs, writer, sidedefids, vertexids);
			WriteThings(things, writer);
			writer.Flush();
		}

		private void WriteVertices(ICollection<Vertex> vertices, BinaryWriter writer) 
		{
			writer.Write(vertices.Count);
			
			foreach(Vertex v in vertices) 
			{
				//write "static" properties
				writer.Write(v.Position.x);
				writer.Write(v.Position.y);
				writer.Write(v.ZCeiling);
				writer.Write(v.ZFloor);

				// Write custom fields
				AddCustomFields(v.Fields, "vertex", writer);
			}
		}

		// This adds linedefs
		private void WriteLinedefs(ICollection<Linedef> linedefs, BinaryWriter writer, IDictionary<Sidedef, int> sidedefids, IDictionary<Vertex, int> vertexids) 
		{
			writer.Write(linedefs.Count);
			
			// Go for all linedefs
			foreach(Linedef l in linedefs) 
			{
				//write "static" properties
				writer.Write(vertexids[l.Start]);
				writer.Write(vertexids[l.End]);

				//sidedefs
				writer.Write((l.Front != null && sidedefids.ContainsKey(l.Front)) ? sidedefids[l.Front] : -1);
				writer.Write((l.Back != null && sidedefids.ContainsKey(l.Back)) ? sidedefids[l.Back] : -1);

				//action and args
				writer.Write(l.Action);
				for(int i = 0; i < l.Args.Length; i++) writer.Write(l.Args[i]);

				//mxd. Tags
				writer.Write(l.Tags.Count);
				for(int i = 0; i < l.Tags.Count; i++) writer.Write(l.Tags[i]);

				AddFlags(l.Flags, writer);
				AddCustomFields(l.Fields, "linedef", writer);
			}
		}

		// This adds sidedefs
		private void WriteSidedefs(ICollection<Sidedef> sidedefs, BinaryWriter writer, IDictionary<Sector, int> sectorids) 
		{
			writer.Write(sidedefs.Count);
			
			// Go for all sidedefs
			foreach(Sidedef s in sidedefs) 
			{
				//write "static" properties
				writer.Write(s.OffsetX);
				writer.Write(s.OffsetY);
				writer.Write(sectorids[s.Sector]);

				//textures
				writer.Write(s.HighTexture.Length);
				writer.Write(s.HighTexture.ToCharArray());
				writer.Write(s.MiddleTexture.Length);
				writer.Write(s.MiddleTexture.ToCharArray());
				writer.Write(s.LowTexture.Length);
				writer.Write(s.LowTexture.ToCharArray());

				AddFlags(s.Flags, writer);
				AddCustomFields(s.Fields, "sidedef", writer);
			}
		}

		// This adds sectors
		private void WriteSectors(ICollection<Sector> sectors, BinaryWriter writer) 
		{
			writer.Write(sectors.Count);
			
			// Go for all sectors
			foreach(Sector s in sectors) 
			{
				//write "static" properties
				writer.Write(s.Effect);
				writer.Write(s.FloorHeight);
				writer.Write(s.CeilHeight);
				writer.Write(s.Brightness);

				//mxd. Tags
				writer.Write(s.Tags.Count);
				for(int i = 0; i < s.Tags.Count; i++) writer.Write(s.Tags[i]);

				//textures
				writer.Write(s.FloorTexture.Length);
				writer.Write(s.FloorTexture.ToCharArray());
				writer.Write(s.CeilTexture.Length);
				writer.Write(s.CeilTexture.ToCharArray());

				//mxd. Slopes
				writer.Write(s.FloorSlopeOffset);
				writer.Write(s.FloorSlope.x);
				writer.Write(s.FloorSlope.y);
				writer.Write(s.FloorSlope.z);
				writer.Write(s.CeilSlopeOffset);
				writer.Write(s.CeilSlope.x);
				writer.Write(s.CeilSlope.y);
				writer.Write(s.CeilSlope.z);

				AddFlags(s.Flags, writer);
				AddCustomFields(s.Fields, "sector", writer);
			}
		}

		// This adds things
		private void WriteThings(ICollection<Thing> things, BinaryWriter writer) 
		{
			writer.Write(things.Count);
			
			// Go for all things
			foreach(Thing t in things) 
			{
				//write "static" properties
				writer.Write(t.Tag);
				writer.Write(t.Position.x);
				writer.Write(t.Position.y);
				writer.Write(t.Position.z);
				writer.Write(t.AngleDoom);
				writer.Write(t.Pitch); //mxd
				writer.Write(t.Roll); //mxd
				writer.Write(t.ScaleX); //mxd
				writer.Write(t.ScaleY); //mxd
				writer.Write(t.Type);
				writer.Write(t.Action);
				for(int i = 0; i < t.Args.Length; i++) writer.Write(t.Args[i]);

				AddFlags(t.Flags, writer);
				AddCustomFields(t.Fields, "thing", writer);
			}
		}

		private void AddCustomFields(UniFields elementFields, string elementname, BinaryWriter writer) 
		{
			Dictionary<string, UniValue> fields = new Dictionary<string, UniValue>(StringComparer.Ordinal);

			foreach(KeyValuePair<string, UniValue> f in elementFields) 
			{
				if(config.SettingExists("managedfields." + elementname + "." + f.Key)) continue;
				fields.Add(f.Key, f.Value);
			}

			writer.Write(fields.Count);

			foreach(KeyValuePair<string, UniValue> f in fields) 
			{
				writer.Write(f.Key.Length);
				writer.Write(f.Key.ToCharArray());

				writer.Write(f.Value.Type);
				if(f.Value.Value is bool) 
				{
					writer.Write((int)UniversalType.Boolean);
					writer.Write((bool)f.Value.Value);
				} 
				else if(f.Value.Value is float) 
				{
					writer.Write((int)UniversalType.Float);
					writer.Write((float)f.Value.Value);
				} 
				else if(f.Value.Value.GetType().IsPrimitive) 
				{
					writer.Write((int)UniversalType.Integer);
					writer.Write((int)f.Value.Value);
				} 
				else if(f.Value.Value is string) 
				{
					writer.Write((int)UniversalType.String);
					string s = (string)f.Value.Value;
					writer.Write(s.Length);
					writer.Write(s.ToCharArray());
				} 
				else //WOLOLO! ERRORS!
				{
					General.ErrorLogger.Add(ErrorType.Error, "Unable to copy Universal Field \"" + f.Key + "\" to clipboard: unknown value type \"" + f.Value.Type + "\"!");
				}
			}
		}

		private static void AddFlags(Dictionary<string, bool> flags, BinaryWriter writer) 
		{
			writer.Write(flags.Count);

			foreach(KeyValuePair<string, bool> group in flags) 
			{
				writer.Write(group.Key.Length);
				writer.Write(group.Key.ToCharArray());
				writer.Write(group.Value);
			}
		}

		#endregion
	}
}
