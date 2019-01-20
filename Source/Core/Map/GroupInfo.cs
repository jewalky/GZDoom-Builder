using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Map
{
	public class GroupInfo
	{
		private readonly int numSectors;
		private readonly int numLines;
		private readonly int numVerts;
		private readonly int numThings;

		private readonly int index;
		private readonly bool empty;

		public bool Empty { get { return empty; } }
		public int Index { get { return index; } }

		public GroupInfo(int index, int numSectors, int numLines, int numVerts, int numThings) 
		{
			this.index = index;
			this.numSectors = numSectors;
			this.numLines = numLines;
			this.numVerts = numVerts;
			this.numThings = numThings;

			empty = (numSectors == 0 && numLines == 0 && numVerts == 0 && numThings == 0);
		}

		public override string ToString() 
		{
			if(empty) return index + ": Empty";
			List<string> result = new List<string>();

			if(numSectors > 0) result.Add(numSectors + (numSectors > 1 ? " sectors" : " sector"));
			if(numLines > 0)   result.Add(numLines + (numLines > 1 ? " lines" : " line"));
			if(numVerts > 0)   result.Add(numVerts + (numVerts > 1 ? " vertices" : " vertex"));
			if(numThings > 0)  result.Add(numThings + (numThings > 1 ? " things" : " thing"));

			return index + ": " + string.Join(", ", result.ToArray());
		}
	}
}
