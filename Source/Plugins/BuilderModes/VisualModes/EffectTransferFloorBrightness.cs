#region === Copyright (c) 2015 MaxED ===

using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.Rendering;

#endregion

namespace CodeImp.DoomBuilder.BuilderModes
{
	internal class EffectTransferFloorBrightness : SectorEffect
	{
		// Linedef that is used to create this effect
		private readonly Linedef linedef;

		// Constructor
		public EffectTransferFloorBrightness(SectorData data, Linedef sourcelinedef) : base(data)
		{
			linedef = sourcelinedef;

			// New effect added: This sector needs an update!
			if(data.Mode.VisualSectorExists(data.Sector))
			{
				BaseVisualSector vs = (BaseVisualSector)data.Mode.GetVisualSector(data.Sector);
				vs.UpdateSectorGeometry(false);
			}
		}
		
		// This makes sure we are updated with the source linedef information
		public override void Update() 
		{
			SectorData sd = data.Mode.GetSectorData(linedef.Front.Sector);
			if(!sd.Updated) sd.Update();
			sd.AddUpdateSector(data.Sector, false);

			// Transfer floor brightness, keep sector color
			PixelColor lightcolor = PixelColor.FromInt(data.Sector.Fields.GetValue("lightcolor", -1));
			PixelColor brightness = PixelColor.FromInt(General.Map.Renderer3D.CalculateBrightness(sd.Sector.Brightness));
			data.Floor.color = PixelColor.Modulate(lightcolor, brightness).WithAlpha(255).ToInt();
			data.Floor.brightnessbelow = sd.Floor.brightnessbelow;
		}
	}
}
