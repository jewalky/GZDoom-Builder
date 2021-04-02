using System.Collections.Generic;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.GZBuilder.Data 
{
	public sealed class ThingCopyData 
	{
		// Properties
		private readonly int type;
		private readonly Vector3D pos;
		private readonly int angledoom;	// Angle as entered / stored in file
		private readonly Dictionary<string, bool> flags;
		private readonly int tag;
		private readonly int action;
		private readonly int[] args;
		private readonly UniFields fields;

		public Vector3D Position { get { return pos; } }
		
		public ThingCopyData(Thing t) 
		{
			type = t.Type;
			angledoom = t.AngleDoom;
			pos = t.Position;
			flags = new Dictionary<string, bool>(t.Flags);
			tag = t.Tag;
			action = t.Action;
			args = (int[])t.Args.Clone();
			fields = new UniFields(t, t.Fields);
		}

		public void ApplyTo(Thing t) 
		{
			t.Type = type;
			t.Rotate(angledoom);
			t.Move(pos);

			foreach(KeyValuePair<string, bool> group in flags)
				t.SetFlag(group.Key, group.Value);

			t.Tag = tag;
			t.Action = action;

			for(int i = 0; i < args.Length; i++) 
				t.Args[i] = args[i];

			foreach(KeyValuePair<string, UniValue> group in fields) 
			{
				if(t.Fields.ContainsKey(group.Key))
					t.Fields[group.Key] = group.Value;
				else
					t.Fields.Add(group.Key, group.Value);
			}

			t.UpdateConfiguration();
		}
	}
}
