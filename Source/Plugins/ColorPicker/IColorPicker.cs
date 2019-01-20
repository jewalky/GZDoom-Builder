using System.Drawing;
using System.Windows.Forms;

namespace CodeImp.DoomBuilder.ColorPicker {
	interface IColorPicker : IWin32Window {
		Point Location { get; set; }
		bool Visible { get; set; }
		int Width { get; set; }

		ColorPickerType Type { get; }

		bool Setup(string editingModeName);
		//void OnBeforeClose();
		//void UpdateSelection();
		//void UpdateSelection(string editingModeName);

		DialogResult ShowDialog(IWin32Window w);
		//void Show(IWin32Window w);
		void Close();
		void Dispose();
		//void Activate();

		event FormClosedEventHandler FormClosed;
	}
}
