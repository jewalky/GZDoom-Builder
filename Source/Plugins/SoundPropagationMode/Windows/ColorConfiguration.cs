using System;
using CodeImp.DoomBuilder.Rendering;
using CodeImp.DoomBuilder.Windows;

namespace CodeImp.DoomBuilder.SoundPropagationMode
{
	public partial class ColorConfiguration : DelayedForm
	{
		public ColorConfiguration()
		{
			InitializeComponent();

			highlightcolor.Color = BuilderPlug.Me.HighlightColor;
			level1color.Color = BuilderPlug.Me.Level1Color;
			level2color.Color = BuilderPlug.Me.Level2Color;
			nosoundcolor.Color = BuilderPlug.Me.NoSoundColor;
			blocksoundcolor.Color = BuilderPlug.Me.BlockSoundColor;
		}

		private void okbutton_Click(object sender, EventArgs e)
		{
			BuilderPlug.Me.HighlightColor = highlightcolor.Color;
			BuilderPlug.Me.Level1Color = level1color.Color;
			BuilderPlug.Me.Level2Color = level2color.Color;
			BuilderPlug.Me.NoSoundColor = nosoundcolor.Color;
			BuilderPlug.Me.BlockSoundColor = blocksoundcolor.Color;

			General.Settings.WritePluginSetting("highlightcolor", highlightcolor.Color.ToInt());
			General.Settings.WritePluginSetting("level1color", level1color.Color.ToInt());
			General.Settings.WritePluginSetting("level2color", level2color.Color.ToInt());
			General.Settings.WritePluginSetting("nosoundcolor", nosoundcolor.Color.ToInt());
			General.Settings.WritePluginSetting("blocksoundcolor", blocksoundcolor.Color.ToInt());

			this.Close();
		}

		private void resetcolors_Click(object sender, EventArgs e)
		{
			highlightcolor.Color = new PixelColor(255, 0, 192, 0);
			level1color.Color = new PixelColor(255, 0, 255, 0);
			level2color.Color = new PixelColor(255, 255, 255, 0);
			nosoundcolor.Color = new PixelColor(255, 160, 160, 160);
			blocksoundcolor.Color = new PixelColor(255, 255, 0, 0);
		}
	}
}
