using System.Collections.Generic;

namespace CodeImp.DoomBuilder.GZBuilder.Data
{
	public sealed class SkyboxInfo
	{
		private readonly string name;
		public string Name { get { return name; } }
		public readonly List<string> Textures;
		public bool FlipTop;

		public SkyboxInfo(string name)
		{
			this.name = name;
			Textures = new List<string>();
		}
	}
}
