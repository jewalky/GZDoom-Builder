using CodeImp.DoomBuilder.Data;
using CodeImp.DoomBuilder.UDBScript.Wrapper;

namespace CodeImp.DoomBuilder.UDBScript.API
{
	struct ImageInfo
	{
		private string _name;
		private string _fullname;
		private int _width;
		private int _height;
		private Vector2DWrapper _scale;
		private bool _isflat;

		/// <summary>
		/// Name of the image.
		/// </summary>
		public string name
		{
			get { return _name; }
		}

		/// <summary>
		/// Width of the image.
		/// </summary>
		public int width
		{
			get { return _width; }
		}

		/// <summary>
		/// Height of the image.
		/// </summary>
		public int height
		{
			get { return _height; }
		}

		/// <summary>
		/// Scale of the image as `Vector2D`.
		/// </summary>
		public Vector2DWrapper scale
		{
			get { return _scale; }
		}

		/// <summary>
		/// If the image is a flat (`true`) or not (`false`).
		/// </summary>
		public bool isFlat
		{
			get { return _isflat; }
		}

		internal ImageInfo(ImageData image)
		{
			_name = image.ShortName;
			_fullname = image.Name;
			_width = image.Width;
			_height = image.Height;
			_scale = new Vector2DWrapper(image.Scale);
			_isflat = image.IsFlat;
		}
	}
}