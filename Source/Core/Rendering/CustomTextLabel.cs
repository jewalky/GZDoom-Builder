using System.Drawing;
using SlimDX.Direct3D9;
using Font = System.Drawing.Font;

namespace CodeImp.DoomBuilder.Rendering
{
	//mxd. TextLabel wrapper
	public abstract class CustomTextLabel : ITextLabel
	{
		protected TextLabel label; // Derived classes must create this!
		
		// Required to render text label
		public bool SkipRendering { get { return label.SkipRendering; } }
		public Texture Texture { get { return label.Texture; } }
		public VertexBuffer VertexBuffer { get { return label.VertexBuffer; } }
		public Font Font { get { return label.Font; } set { label.Font = value; } }
		public string Text { get { return label.Text; } set { label.Text = value; } }

		// Access/setup
		public TextLabel TextLabel { get { return label; } }
		public PixelColor Color { get { return label.Color; } set { label.Color = value; } }
		public PixelColor BackColor { get { return label.BackColor; } set { label.BackColor = value; } }
		public SizeF TextSize { get { return label.TextSize; } }

		public void Update(float translatex, float translatey, float scalex, float scaley)
		{
			label.Update(translatex, translatey, scalex, scaley);
		}
	}
}
