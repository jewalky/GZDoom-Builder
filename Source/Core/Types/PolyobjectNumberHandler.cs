#region ================== Namespaces

using System.Collections.Generic;
using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.Map;

#endregion

namespace CodeImp.DoomBuilder.Types
{
	[TypeHandler(UniversalType.PolyobjectNumber, "Polyobject Number", false)]
	internal class PolyobjectNumberHandler : SectorTagHandler
	{
		#region ================== Setup

		protected override EnumList CreateEnumList()
		{
			// Collect polyobjects
			HashSet<int> ponums = new HashSet<int>();
			EnumList polist = new EnumList();

			foreach(Thing t in General.Map.Map.Things)
			{
				ThingTypeInfo info = General.Map.Data.GetThingInfoEx(t.Type);
				if(info == null || info.ClassName.ToLowerInvariant() != "$polyanchor") continue;
				ponums.Add(t.AngleDoom);
			}

			// Now sort them in descending order
			List<int> ponumslist = new List<int>(ponums);
			ponumslist.Sort((a, b) => -1 * a.CompareTo(b));

			// Create enum items
			foreach(int ponum in ponums)
				polist.Add(new EnumItem(ponum.ToString(), ponum.ToString()));

			return polist;
		}

		#endregion

		#region ================== Methods

		public override string GetStringValue()
		{
			return (this.value != null ? this.value.Title : "0: 0");
		}

		#endregion
	}
}