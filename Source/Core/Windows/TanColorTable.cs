
#region ================== Copyright (c) 2007 Pascal vd Heiden

/*
 * Copyright (c) 2007 Pascal vd Heiden, www.codeimp.com
 * This program is released under GNU General Public License
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 */

#endregion
/*
#region ================== Namespaces

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

#endregion

namespace CodeImp.DoomBuilder.Windows
{
	// Taken from http://blogs.msdn.com/jfoscoding/archive/2005/11/06/489641.aspx
	
	internal class TanColorTable : ProfessionalColorTable
	{
		// Methods
		public TanColorTable()
		{
		}

		internal Color FromKnownColor(KnownColors color)
		{
			return this.ColorTable[color];
		}

		internal static void InitTanLunaColors(ref Dictionary<KnownColors, Color> rgbTable)
		{
			rgbTable[KnownColors.GripDark] = Color.FromArgb(0xc1, 190, 0xb3);
			rgbTable[KnownColors.SeparatorDark] = Color.FromArgb(0xc5, 0xc2, 0xb8);
			rgbTable[KnownColors.MenuItemSelected] = Color.FromArgb(0xc1, 210, 0xee);
			rgbTable[KnownColors.ButtonPressedBorder] = Color.FromArgb(0x31, 0x6a, 0xc5);
			rgbTable[KnownColors.CheckBackground] = Color.FromArgb(0xe1, 230, 0xe8);
			rgbTable[KnownColors.MenuItemBorder] = Color.FromArgb(0x31, 0x6a, 0xc5);
			rgbTable[KnownColors.CheckBackgroundMouseOver] = Color.FromArgb(0x31, 0x6a, 0xc5);
			rgbTable[KnownColors.MenuItemBorderMouseOver] = Color.FromArgb(0x4b, 0x4b, 0x6f);
			rgbTable[KnownColors.ToolStripDropDownBackground] = Color.FromArgb(0xfc, 0xfc, 0xf9);
			rgbTable[KnownColors.MenuBorder] = Color.FromArgb(0x8a, 0x86, 0x7a);
			rgbTable[KnownColors.SeparatorLight] = Color.FromArgb(0xff, 0xff, 0xff);
			rgbTable[KnownColors.ToolStripBorder] = Color.FromArgb(0xa3, 0xa3, 0x7c);
			rgbTable[KnownColors.MenuStripGradientBegin] = Color.FromArgb(0xe5, 0xe5, 0xd7);
			rgbTable[KnownColors.MenuStripGradientEnd] = Color.FromArgb(0xf4, 0xf2, 0xe8);
			rgbTable[KnownColors.ImageMarginGradientBegin] = Color.FromArgb(0xfe, 0xfe, 0xfb);
			rgbTable[KnownColors.ImageMarginGradientMiddle] = Color.FromArgb(0xec, 0xe7, 0xe0);
			rgbTable[KnownColors.ImageMarginGradientEnd] = Color.FromArgb(0xbd, 0xbd, 0xa3);
			rgbTable[KnownColors.OverflowButtonGradientBegin] = Color.FromArgb(0xf3, 0xf2, 240);
			rgbTable[KnownColors.OverflowButtonGradientMiddle] = Color.FromArgb(0xe2, 0xe1, 0xdb);
			rgbTable[KnownColors.OverflowButtonGradientEnd] = Color.FromArgb(0x92, 0x92, 0x76);
			rgbTable[KnownColors.MenuItemPressedGradientBegin] = Color.FromArgb(0xfc, 0xfc, 0xf9);
			rgbTable[KnownColors.MenuItemPressedGradientEnd] = Color.FromArgb(0xf6, 0xf4, 0xec);
			rgbTable[KnownColors.ImageMarginRevealedGradientBegin] = Color.FromArgb(0xf7, 0xf6, 0xef);
			rgbTable[KnownColors.ImageMarginRevealedGradientMiddle] = Color.FromArgb(0xf2, 240, 0xe4);
			rgbTable[KnownColors.ImageMarginRevealedGradientEnd] = Color.FromArgb(230, 0xe3, 210);
			rgbTable[KnownColors.ButtonCheckedGradientBegin] = Color.FromArgb(0xe1, 230, 0xe8);
			rgbTable[KnownColors.ButtonCheckedGradientMiddle] = Color.FromArgb(0xe1, 230, 0xe8);
			rgbTable[KnownColors.ButtonCheckedGradientEnd] = Color.FromArgb(0xe1, 230, 0xe8);
			rgbTable[KnownColors.ButtonSelectedGradientBegin] = Color.FromArgb(0xc1, 210, 0xee);
			rgbTable[KnownColors.ButtonSelectedGradientMiddle] = Color.FromArgb(0xc1, 210, 0xee);
			rgbTable[KnownColors.ButtonSelectedGradientEnd] = Color.FromArgb(0xc1, 210, 0xee);
			rgbTable[KnownColors.ButtonPressedGradientBegin] = Color.FromArgb(0x98, 0xb5, 0xe2);
			rgbTable[KnownColors.ButtonPressedGradientMiddle] = Color.FromArgb(0x98, 0xb5, 0xe2);
			rgbTable[KnownColors.ButtonPressedGradientEnd] = Color.FromArgb(0x98, 0xb5, 0xe2);
			rgbTable[KnownColors.GripLight] = Color.FromArgb(0xff, 0xff, 0xff);
		}

		public override Color ButtonCheckedGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonCheckedGradientBegin);
				}
				return base.ButtonCheckedGradientBegin;
			}
		}

		public override Color ButtonCheckedGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonCheckedGradientEnd);
				}
				return base.ButtonCheckedGradientEnd;
			}
		}

		public override Color ButtonCheckedGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonCheckedGradientMiddle);
				}
				return base.ButtonCheckedGradientMiddle;
			}
		}


		public override Color ButtonPressedBorder
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonPressedBorder);
				}
				return base.ButtonPressedBorder;
			}
		}

		public override Color ButtonPressedGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonPressedGradientBegin);
				}
				return base.ButtonPressedGradientBegin;
			}
		}

		public override Color ButtonPressedGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonPressedGradientEnd);
				}
				return base.ButtonPressedGradientEnd;
			}
		}


		public override Color ButtonPressedGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonPressedGradientMiddle);
				}
				return base.ButtonPressedGradientMiddle;
			}
		}

		public override Color ButtonSelectedBorder
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonPressedBorder);
				}
				return base.ButtonSelectedBorder;
			}
		}


		public override Color ButtonSelectedGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonSelectedGradientBegin);
				}
				return base.ButtonSelectedGradientBegin;
			}
		}
		public override Color ButtonSelectedGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonSelectedGradientEnd);
				}
				return base.ButtonSelectedGradientEnd;
			}
		}
		public override Color ButtonSelectedGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonSelectedGradientMiddle);
				}
				return base.ButtonSelectedGradientMiddle;
			}
		}

		public override Color CheckBackground
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.CheckBackground);
				}
				return base.CheckBackground;
			}
		}

		public override Color CheckPressedBackground
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.CheckBackgroundMouseOver);
				}
				return base.CheckPressedBackground;
			}
		}

		public override Color CheckSelectedBackground
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.CheckBackgroundMouseOver);
				}
				return base.CheckSelectedBackground;
			}
		}


		internal static string ColorScheme
		{
			get
			{
				return DisplayInformation.ColorScheme;
			}
		}

		private Dictionary<KnownColors, Color> ColorTable
		{
			get
			{
				if(this.tanRGB == null)
				{
					this.tanRGB = new Dictionary<KnownColors, Color>((int)KnownColors.LastKnownColor);
					InitTanLunaColors(ref this.tanRGB);
				}
				return this.tanRGB;
			}
		}

		public override Color GripDark
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.GripDark);
				}
				return base.GripDark;
			}
		}

		public override Color GripLight
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.GripLight);
				}
				return base.GripLight;
			}
		}
		public override Color ImageMarginGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginGradientBegin);
				}
				return base.ImageMarginGradientBegin;
			}
		}

		public override Color ImageMarginGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginGradientEnd);
				}
				return base.ImageMarginGradientEnd;
			}
		}

		public override Color ImageMarginGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginGradientMiddle);
				}
				return base.ImageMarginGradientMiddle;
			}
		}

		public override Color ImageMarginRevealedGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginRevealedGradientBegin);
				}
				return base.ImageMarginRevealedGradientBegin;
			}
		}

		public override Color ImageMarginRevealedGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginRevealedGradientEnd);
				}
				return base.ImageMarginRevealedGradientEnd;
			}
		}

		public override Color ImageMarginRevealedGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginRevealedGradientMiddle);
				}
				return base.ImageMarginRevealedGradientMiddle;
			}
		}

		public override Color MenuBorder
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuBorder);
				}
				return base.MenuItemBorder;
			}
		}


		public override Color MenuItemBorder
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuItemBorder);
				}
				return base.MenuItemBorder;
			}
		}

		public override Color MenuItemPressedGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuItemPressedGradientBegin);
				}
				return base.MenuItemPressedGradientBegin;
			}
		}

		public override Color MenuItemPressedGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuItemPressedGradientEnd);
				}
				return base.MenuItemPressedGradientEnd;
			}
		}

		public override Color MenuItemPressedGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginRevealedGradientMiddle);
				}
				return base.MenuItemPressedGradientMiddle;
			}
		}

		public override Color MenuItemSelected
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuItemSelected);
				}
				return base.MenuItemSelected;
			}
		}

		public override Color MenuItemSelectedGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonSelectedGradientBegin);
				}
				return base.MenuItemSelectedGradientBegin;
			}
		}

		public override Color MenuItemSelectedGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ButtonSelectedGradientEnd);
				}
				return base.MenuItemSelectedGradientEnd;
			}
		}


		public override Color MenuStripGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuStripGradientBegin);
				}
				return base.MenuStripGradientBegin;
			}
		}

		public override Color MenuStripGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuStripGradientEnd);
				}
				return base.MenuStripGradientEnd;
			}
		}

		public override Color OverflowButtonGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.OverflowButtonGradientBegin);
				}
				return base.OverflowButtonGradientBegin;
			}
		}

		public override Color OverflowButtonGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.OverflowButtonGradientEnd);
				}
				return base.OverflowButtonGradientEnd;
			}
		}

		public override Color OverflowButtonGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.OverflowButtonGradientMiddle);
				}
				return base.OverflowButtonGradientMiddle;
			}
		}


		public override Color RaftingContainerGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuStripGradientBegin);
				}
				return base.RaftingContainerGradientBegin;
			}
		}

		public override Color RaftingContainerGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.MenuStripGradientEnd);
				}
				return base.RaftingContainerGradientEnd;
			}
		}


		public override Color SeparatorDark
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.SeparatorDark);
				}
				return base.SeparatorDark;
			}
		}

		public override Color SeparatorLight
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.SeparatorLight);
				}
				return base.SeparatorLight;
			}
		}

		public override Color ToolStripBorder
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ToolStripBorder);
				}
				return base.ToolStripBorder;
			}
		}


		public override Color ToolStripDropDownBackground
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ToolStripDropDownBackground);
				}
				return base.ToolStripDropDownBackground;
			}
		}

		public override Color ToolStripGradientBegin
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginGradientBegin);
				}
				return base.ToolStripGradientBegin;
			}
		}

		public override Color ToolStripGradientEnd
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginGradientEnd);
				}
				return base.ToolStripGradientEnd;
			}
		}

		public override Color ToolStripGradientMiddle
		{
			get
			{
				if(!this.UseBaseColorTable)
				{
					return this.FromKnownColor(KnownColors.ImageMarginGradientMiddle);
				}
				return base.ToolStripGradientMiddle;
			}
		}

		private bool UseBaseColorTable
		{
			get
			{
				bool flag1 = !DisplayInformation.IsLunaTheme ||
								((ColorScheme != oliveColorScheme) &&
								 (ColorScheme != blueColorScheme));
				if(flag1 && (this.tanRGB != null))
				{
					this.tanRGB.Clear();
					this.tanRGB = null;
				}
				return flag1;
			}
		}



		// Fields
		private const string blueColorScheme = "NormalColor";
		private const string oliveColorScheme = "HomeStead";
		//private const string silverColorScheme = "Metallic";
		private Dictionary<KnownColors, Color> tanRGB;

		// Nested Types
		private static class DisplayInformation
		{
			// Methods
			static DisplayInformation()
			{
				SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;
				SetScheme();
			}

			private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
			{
				SetScheme();
			}

			private static void SetScheme()
			{
				isLunaTheme = false;
				if(VisualStyleRenderer.IsSupported)
				{
					colorScheme = VisualStyleInformation.ColorScheme;

					if(!VisualStyleInformation.IsEnabledByUser)
					{
						return;
					}
					StringBuilder builder1 = new StringBuilder(0x200);
					GetCurrentThemeName(builder1, builder1.Capacity, null, 0, null, 0);
					string text1 = builder1.ToString();
					isLunaTheme = string.Equals(lunaFileName, Path.GetFileName(text1), StringComparison.InvariantCultureIgnoreCase);
				}
				else
				{
					colorScheme = null;
				}
			}


			// Properties

			public static string ColorScheme
			{
				get
				{
					return colorScheme;
				}
			}

			internal static bool IsLunaTheme
			{
				get
				{
					return isLunaTheme;
				}
			}

			// Fields
			[ThreadStatic]
			private static string colorScheme;
			[ThreadStatic]
			private static bool isLunaTheme;
			private const string lunaFileName = "luna.msstyles";

			[DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
			public static extern int GetCurrentThemeName(StringBuilder pszThemeFileName, int dwMaxNameChars, StringBuilder pszColorBuff, int dwMaxColorChars, StringBuilder pszSizeBuff, int cchMaxSizeChars);

		}

		internal enum KnownColors
		{
			ButtonPressedBorder,
			MenuItemBorder,
			MenuItemBorderMouseOver,
			MenuItemSelected,
			CheckBackground,
			CheckBackgroundMouseOver,
			GripDark,
			GripLight,
			MenuStripGradientBegin,
			MenuStripGradientEnd,
			ImageMarginRevealedGradientBegin,
			ImageMarginRevealedGradientEnd,
			ImageMarginRevealedGradientMiddle,
			MenuItemPressedGradientBegin,
			MenuItemPressedGradientEnd,
			ButtonPressedGradientBegin,
			ButtonPressedGradientEnd,
			ButtonPressedGradientMiddle,
			ButtonSelectedGradientBegin,
			ButtonSelectedGradientEnd,
			ButtonSelectedGradientMiddle,
			OverflowButtonGradientBegin,
			OverflowButtonGradientEnd,
			OverflowButtonGradientMiddle,
			ButtonCheckedGradientBegin,
			ButtonCheckedGradientEnd,
			ButtonCheckedGradientMiddle,
			ImageMarginGradientBegin,
			ImageMarginGradientEnd,
			ImageMarginGradientMiddle,
			MenuBorder,
			ToolStripDropDownBackground,
			ToolStripBorder,
			SeparatorDark,
			SeparatorLight,
			LastKnownColor = SeparatorLight,

		}
	}

}*/