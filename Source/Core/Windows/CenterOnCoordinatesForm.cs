using System;
using System.Windows.Forms;
using CodeImp.DoomBuilder.Geometry;

namespace CodeImp.DoomBuilder.Windows
{
	public partial class CenterOnCoordinatesForm : DelayedForm
	{
		private static Vector2D coordinates;
		public Vector2D Coordinates { get { return coordinates; } }

		public CenterOnCoordinatesForm() 
		{
			InitializeComponent();
			gotox.Text = coordinates.x.ToString();
			gotoy.Text = coordinates.y.ToString();
		}

		private void accept_Click(object sender, EventArgs e) 
		{
			coordinates.x = (float)Math.Round(General.Clamp(gotox.GetResult((int)coordinates.x), General.Map.FormatInterface.MinCoordinate, General.Map.FormatInterface.MaxCoordinate));
			coordinates.y = (float)Math.Round(General.Clamp(gotoy.GetResult((int)coordinates.y), General.Map.FormatInterface.MinCoordinate, General.Map.FormatInterface.MaxCoordinate));
			
			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void cancel_Click(object sender, EventArgs e) 
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}
	}
}
