using System;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Types;

namespace CodeImp.DoomBuilder.TagExplorer
{
	internal sealed class NodeInfo
	{
		private readonly NodeInfoType type;

		private readonly int index;
		private readonly int action;
		private readonly int tag;
		private readonly int polyobjnumber;
		private readonly string defaultname;

		public int Index { get { return index; } }
		public int Tag { get { return tag; } }
		public int PolyobjectNumber { get { return polyobjnumber; } }
		public int Action { get { return action; } }
		public string DefaultName { get { return defaultname; } }
		public NodeInfoType Type { get { return type; } }
		public string Comment { get { return GetComment(); } set { SetComment(value); } }

		//constructor
		public NodeInfo(Thing t) 
		{
			type = NodeInfoType.THING;
			index = t.Index;
			action = t.Action;
			tag = t.Tag;
			polyobjnumber = ((t.Type > 9299 && t.Type < 9304) ? t.AngleDoom : int.MinValue);
			ThingTypeInfo tti = General.Map.Data.GetThingInfoEx(t.Type);
			defaultname = (tti != null ? tti.Title : "Thing");
		}

		public NodeInfo(Sector s, int tagindex)
		{
			type = NodeInfoType.SECTOR;
			index = s.Index;
			action = s.Effect;
			tag = s.Tags[tagindex];
			
			if(General.Map.Config.SectorEffects.ContainsKey(action))
				defaultname = General.Map.Config.SectorEffects[action].Title;
			else if(action > 0)
				defaultname = General.Map.Config.GetGeneralizedSectorEffectName(action);
			else
				defaultname = "Sector";
		}

		public NodeInfo(Linedef l, int tagindex) 
		{
			type = NodeInfoType.LINEDEF;
			index = l.Index;
			action = l.Action;
			tag = l.Tags[tagindex];
			polyobjnumber = ((l.Action > 0 && l.Action < 9) ? l.Args[0] : int.MinValue);
			
			if(General.Map.Config.LinedefActions.ContainsKey(action))
				defaultname = General.Map.Config.LinedefActions[action].Title;
			else if(action > 0 && GameConfiguration.IsGeneralized(action))
				defaultname = "Generalized (" + General.Map.Config.GetGeneralizedActionCategory(action) + ")";
			else
				defaultname = "Linedef";
		}

		// Copy constructor
		public NodeInfo(NodeInfo other, int neweffect)
		{
			this.type = other.type;
			this.index = other.index;
			this.action = neweffect;
			this.tag = other.tag;
			this.polyobjnumber = other.polyobjnumber;

			switch(type)
			{
				case NodeInfoType.LINEDEF:
					if(General.Map.Config.LinedefActions.ContainsKey(action))
						defaultname = General.Map.Config.LinedefActions[action].Title;
					else if(action > 0 && GameConfiguration.IsGeneralized(action))
						defaultname = "Generalized (" + General.Map.Config.GetGeneralizedActionCategory(action) + ")";
					else
						defaultname = "Linedef";
					break;

				case NodeInfoType.SECTOR:
					if(General.Map.Config.SectorEffects.ContainsKey(action))
						defaultname = General.Map.Config.SectorEffects[action].Title;
					else if(action > 0)
						defaultname = General.Map.Config.GetGeneralizedSectorEffectName(action);
					else
						defaultname = "Sector";
					break;

				default: throw new NotImplementedException("Not implemented...");
			}
		}

		//methods
		private UniFields GetFields() 
		{
			if(type == NodeInfoType.THING) 
			{
				Thing t = General.Map.Map.GetThingByIndex(index);
				return (t == null ? null : t.Fields);
			}

			if(type == NodeInfoType.SECTOR) 
			{
				Sector s = General.Map.Map.GetSectorByIndex(index);
				return (s == null ? null : s.Fields);
			}

			Linedef l = General.Map.Map.GetLinedefByIndex(index);
			return (l == null ? null : l.Fields);
		}

//comment
		private void SetComment(string comment) 
		{
			UniFields fields = GetFields();

			if(comment.Length == 0) 
			{
				if(fields.ContainsKey("comment")) 
				{
					General.Map.UndoRedo.CreateUndo("Remove comment");
					fields.BeforeFieldsChange();
					fields.Remove("comment");
				}
				return;
			}

			//create undo stuff
			General.Map.UndoRedo.CreateUndo("Set comment");
			fields.BeforeFieldsChange();

			if(!fields.ContainsKey("comment"))
				fields.Add("comment", new UniValue((int)UniversalType.String, comment));
			else
				fields["comment"].Value = comment;
		}

		private string GetComment() 
		{
			UniFields fields = GetFields();
			if(fields == null) return "";
			if(!fields.ContainsKey("comment")) return "";
			return fields["comment"].Value.ToString();
		}

//naming
		public string GetName(ref string comment, string sortMode) 
		{
			if(type == NodeInfoType.THING) 
			{
				Thing t = General.Map.Map.GetThingByIndex(index);
				if(t == null) return "<invalid thing>";
				return GetThingName(t, ref comment, sortMode);
			}

			if(type == NodeInfoType.SECTOR) 
			{
				Sector s = General.Map.Map.GetSectorByIndex(index);
				if(s == null) return "<invalid sector>";
				return GetSectorName(s, ref comment, sortMode);
			}

			Linedef l = General.Map.Map.GetLinedefByIndex(index);
			if(l == null) return "<invalid linedef>";
			return GetLinedefName(l, ref comment, sortMode);
		}

		private string GetThingName(Thing t, ref string comment, string sortmode) 
		{
			comment = ((TagExplorer.UDMF && t.Fields.ContainsKey("comment")) ? t.Fields["comment"].Value.ToString() : string.Empty);
			return CombineName(comment, sortmode);
		}

		private string GetSectorName(Sector s, ref string comment, string sortmode) 
		{
			comment = ((TagExplorer.UDMF && s.Fields.ContainsKey("comment")) ? s.Fields["comment"].Value.ToString() : string.Empty);
			return CombineName(comment, sortmode);
		}

		private string GetLinedefName(Linedef l, ref string comment, string sortmode) 
		{
			if(polyobjnumber != int.MinValue) return CombineName(string.Empty, sortmode);
			comment = ((TagExplorer.UDMF && l.Fields.ContainsKey("comment")) ? l.Fields["comment"].Value.ToString() : string.Empty);
			return CombineName(comment, sortmode);
		}

		private string CombineName(string comment, string sortmode)
		{
			string name = (!string.IsNullOrEmpty(comment) ? comment : defaultname);

			switch(sortmode) 
			{
				case SortMode.SORT_BY_ACTION: //action name is already shown as category name, so we'll show tag here
					return (tag > 0 ? "Tag " + tag + ": " : "") + name + ", Index " + index;

				case SortMode.SORT_BY_INDEX:
					return index + ": " + name + (tag > 0 ? ", Tag " + tag : "") + (action > 0 ? ", Action " + action : "");

				case SortMode.SORT_BY_TAG: //tag is already shown as category name, so we'll show action here
					return (action > 0 ? "Action " + action + ": " : "") + name + ", Index " + index;

				case SortMode.SORT_BY_POLYOBJ_NUMBER:
					return "PO " + polyobjnumber + ": " + defaultname + ", Index " + index;

				default:
					return name;
			}
		}
	}

	internal enum NodeInfoType
	{
		THING,
		SECTOR,
		LINEDEF
	}
}
