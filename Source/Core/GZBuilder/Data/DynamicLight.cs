using SlimDX;

namespace CodeImp.DoomBuilder.GZBuilder.Data
{
	public sealed class DynamicLightData 
	{
		public GZGeneral.LightData Type; //holds DynamicLightType
		public Color3 Color;
		public int PrimaryRadius;
		public int SecondaryRadius;
		public int Interval;
		public Vector3 Offset;
        public bool DontLightSelf;

		public DynamicLightData(GZGeneral.LightData type) 
		{
            Type = type;
			Color = new Color3();
			Offset = new Vector3();
		}
	}
}
