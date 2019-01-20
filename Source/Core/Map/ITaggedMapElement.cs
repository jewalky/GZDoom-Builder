using System.Collections.Generic;

namespace CodeImp.DoomBuilder.Map
{
	public interface ITaggedMapElement
	{
		int Tag { get; set; }
	}

	public interface IMultiTaggedMapElement : ITaggedMapElement
	{
		List<int> Tags { get; set; }
	}
}
