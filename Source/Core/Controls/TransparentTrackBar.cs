using System.Drawing;
using System.Windows.Forms;

// [ZZ] this is a copypasted version of TransparentPanel :)
//      implements the same functionality, except for a TrackBar, for use in tab controls.

namespace CodeImp.DoomBuilder.Controls
{
    public class TransparentTrackBar : TrackBar
    {
        #region ================== Constructor / Disposer

        // Constructor
        public TransparentTrackBar()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }

        #endregion

        #region ================== Methods

        protected override void OnCreateControl()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            if (Parent != null)
                BackColor = Parent.BackColor;

            base.OnCreateControl();
        }

        // Disable background drawing by overriding this
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (BackColor != Color.Transparent)
                e.Graphics.Clear(BackColor);
        }

        #endregion
    }
}
