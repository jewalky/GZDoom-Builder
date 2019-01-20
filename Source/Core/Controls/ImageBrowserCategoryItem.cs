#region ================== Namespaces

using System;
using System.Drawing;
using CodeImp.DoomBuilder.Data;

#endregion

namespace CodeImp.DoomBuilder.Controls
{
	internal class ImageBrowserCategoryItem : ImageBrowserItem
	{
		#region ================== Variables

		private string groupname;
		private int groupnamewidth;

		#endregion

		#region ================== Properties

		public override bool IsPreviewLoaded { get { return true; } }
		public override string TextureName { get { return groupname; } }
		public override int TextureNameWidth { get { return groupnamewidth; } }

		#endregion

		#region ================== Constructors

		private ImageBrowserCategoryItem(ImageData icon, string tooltip, bool showfullname) : base(icon, tooltip, showfullname) { }
		public ImageBrowserCategoryItem(ImageBrowserItemType itemtype, string groupname)
		{
			this.groupname = groupname;
			this.itemtype = itemtype;
			
			switch(itemtype)
			{
				case ImageBrowserItemType.FOLDER:
					icon = General.Map.Data.FolderTexture;
					break;

				case ImageBrowserItemType.FOLDER_UP:
					icon = General.Map.Data.FolderUpTexture;
					break;

				default:
					throw new NotImplementedException("Unsupported ItemType");
			}

			// Calculate name width
			this.groupnamewidth = (int)Math.Ceiling(General.Interface.MeasureString(this.groupname, SystemFonts.MessageBoxFont, 10000, StringFormat.GenericTypographic).Width);
		}

		#endregion
	}
}
