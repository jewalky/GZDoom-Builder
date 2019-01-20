using System.Windows.Forms;
using System.Drawing;

namespace CodeImp.DoomBuilder.Controls
{
	public class ColoredComboBox : ComboBox
	{
		public ColoredComboBox() 
		{ 
			this.DrawMode = DrawMode.OwnerDrawFixed;
            this.KeyPress += ColoredComboBox_KeyPress;
		}

        private void ColoredComboBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                e.Handled = true;
            }
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
		{
			base.OnDrawItem(e);
			e.DrawBackground();
			ColoredComboBoxItem item = (ColoredComboBoxItem)this.Items[e.Index];
			using(Brush brush = new SolidBrush(((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? Color.White : item.ForeColor))
			{
				e.Graphics.DrawString(item.Text, this.Font, brush, e.Bounds.X, e.Bounds.Y);
			}
		}
	}

	public class ColoredComboBoxItem
	{
		private string text = "";
		private object value;
		Color forecolor = Color.Black;

		public string Text { get { return text; } set { text = value; } }
		public object Value { get { return value; } set { this.value = value; } }
		public Color ForeColor { get { return forecolor; } set { forecolor = value; } }
		
		public ColoredComboBoxItem() { }

		public ColoredComboBoxItem(object value) 
		{
			this.text = value.ToString();
			this.value = value;
		}

		public ColoredComboBoxItem(object value, Color forecolor) 
		{
			this.text = value.ToString();
			this.value = value;
			this.forecolor = forecolor;
		}

		public override string ToString() 
		{
			return text;
		}
	}
}
