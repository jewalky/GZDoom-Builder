using System.Collections.Generic;
using SlimDX.Direct3D9;

namespace CodeImp.DoomBuilder.GZBuilder.MD3
{
	internal class GZModel 
	{
		internal readonly List<Mesh> Meshes;
		internal readonly List<Texture> Textures;
		internal float Radius;

		internal GZModel() 
		{
			Meshes = new List<Mesh>();
			Textures = new List<Texture>();
		}
	}
}