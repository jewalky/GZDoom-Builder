using System;

namespace CodeImp.DoomBuilder.ColorPicker.Controls 
{
	public class ColorPickerSliderEventArgs : EventArgs 
	{
		private int value;
		public int Value { get { return value; } }

		public ColorPickerSliderEventArgs(int value) 
		{
			this.value = value;
		}
	}
}
