#region ======================== Namespaces

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

#endregion

// Based on http://www.codeproject.com/Articles/1106140/VisualStudio-Like-TabControl
namespace CodeImp.DoomBuilder.Controls
{
    public class VSTabControl : TabControl
    {
		#region ======================== Events

		public event EventHandler<TabControlEventArgs> OnCloseTabClicked;

		#endregion
		
		#region ======================== Properties

        public bool ShowClosingButton { get; set; }

        [Category("Colors"), Browsable(true), Description("The color of the selected page")]
        public Color ActiveColor { get { return activeColor; } set { activeColor = value; } }

        [Category("Colors"), Browsable(true), Description("The color of the highlighted page")]
        public Color HighlightColor { get { return highlightColor; } set { highlightColor = value; } }

        [Category("Colors"), Browsable(true), Description("The color of the background of the tab")]
        public Color BackTabColor { get { return backTabColor; } set { backTabColor = value; } }

        [Category("Colors"), Browsable(true), Description("The color of the border of the control")]
        public Color BorderColor { get { return borderColor; } set { borderColor = value; } }

        [Category("Colors"), Browsable(true), Description("The color of the title of the page")]
        public Color TextColor { get { return textColor; } set { textColor = value; } }

        [Category("Colors"), Browsable(true), Description("The color of the title of the page")]
        public Color SelectedTextColor { get { return selectedTextColor; } set { selectedTextColor = value; } }

	    #endregion

        #region ======================== Variables

        private readonly StringFormat centersringformat = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
        private TabPage predraggedTab;

        private Color textColor = SystemColors.WindowText;
        private Color selectedTextColor = SystemColors.HighlightText;
        private Color activeColor = SystemColors.HotTrack;
        private Color highlightColor = SystemColors.Highlight;
        private Color backTabColor = SystemColors.Control;
        private Color borderColor = SystemColors.Control;

        private int closebuttonmouseoverindex = -1; //mxd

        #endregion

        #region ======================== Constructor

        public VSTabControl()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw
                | ControlStyles.OptimizedDoubleBuffer,
                true);
            DoubleBuffered = true;
            SizeMode = TabSizeMode.Normal;
            ItemSize = new Size(240, 16);
            AllowDrop = true;
        }

        #endregion

        #region ======================== Event overrides

        protected override void CreateHandle()
        {
            base.CreateHandle();
            Alignment = TabAlignment.Top;
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            //mxd. Collect used tab page types...
			var tabpagetypes = new HashSet<Type>();
	        foreach(var page in TabPages) tabpagetypes.Add(page.GetType());

			//mxd. Identify dragged tab type...
	        TabPage draggedTab = null;
	        foreach(var T in tabpagetypes)
	        {
				draggedTab = (TabPage)drgevent.Data.GetData(T);
				if(draggedTab != null) break;
	        }

            var pointedTab = GetPointedTab();

            if(ReferenceEquals(draggedTab, predraggedTab) && pointedTab != null)
            {
                drgevent.Effect = DragDropEffects.Move;
                if(!ReferenceEquals(pointedTab, draggedTab)) ReplaceTabPages(draggedTab, pointedTab);
            }

            base.OnDragOver(drgevent);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            predraggedTab = GetPointedTab();

            //mxd. MEMO: OnMouseUp is not fired when clicking on inactive tab...
            if(ShowClosingButton)
            {
                for(var i = 0; i < TabCount; i++)
                {
                    Rectangle r = GetCloseButtonRect(GetTabRect(i));
                    if(r.Contains(e.Location))
                    {
						// Raise event...
						if(OnCloseTabClicked != null)
						{
							TabControlEventArgs te = new TabControlEventArgs(TabPages[i], i, TabControlAction.Selected);
							OnCloseTabClicked(this, te);
							closebuttonmouseoverindex = -1;
						}
                        break;
                    }
                }
            }

            base.OnMouseDown(e);
        }

	    protected override void OnMouseMove(MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left && predraggedTab != null)
            {
                DoDragDrop(predraggedTab, DragDropEffects.Move);
            }
            //mxd. Closing button highlight needs updating?
            else if(ShowClosingButton)
            {
                int index = GetPointedTabIndex();
                if(index != -1)
                {
                    Rectangle cr = GetCloseButtonRect(GetTabRect(index));
                    bool inside = cr.Contains(PointToClient(Cursor.Position));

                    if(inside && closebuttonmouseoverindex == -1)
                    {
                        closebuttonmouseoverindex = index;
                        Refresh();
                    }
                    else if(!inside && closebuttonmouseoverindex != -1)
                    {
                        closebuttonmouseoverindex = -1;
                        Refresh();
                    }
                }
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            predraggedTab = null;
            base.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.Clear(backTabColor);

            //mxd
            int highlightedtabindex = GetPointedTabIndex();
            for(var i = 0; i < TabCount; i++)
            {
                var tabrect = GetTabRect(i); //mxd
                var Header = new Rectangle(
                    new Point(tabrect.Location.X + 2, tabrect.Location.Y),
                    new Size(tabrect.Width, tabrect.Height));
                var HeaderSize = new Rectangle(Header.Location, new Size(Header.Width, Header.Height));
                var TextSize = new Rectangle(HeaderSize.Location.X, HeaderSize.Location.Y, HeaderSize.Width, HeaderSize.Height - 2);

                //mxd
                if(TabPages[i].ImageIndex != -1)
                {
                    int offset = ImageList.Images[TabPages[i].ImageIndex].Width + 2;
                    HeaderSize.X += offset;
                    HeaderSize.Width -= offset;

                    TextSize.X += offset + 2;
                    TextSize.Width -= offset + 2;
                }

                if(i == SelectedIndex || i == highlightedtabindex)
                {
                    // Draws the back of the color when it is selected
                    var tabbgrect = new Rectangle(tabrect.X, tabrect.Y - 2, tabrect.Width, tabrect.Height + 2);
                    using(var bgbrush = new SolidBrush(i == SelectedIndex ? activeColor : highlightColor))
                        g.FillRectangle(bgbrush, tabbgrect);

                    // Draws the title of the page
                    using(var textbrush = new SolidBrush(selectedTextColor))
                    {
                        g.DrawString(TabPages[i].Text, Font, textbrush, TextSize, centersringformat);

                        // Draws the closing button
                        if(ShowClosingButton)
                        {
                            //mxd. Draw bg rect?
                            var bgrect = GetCloseButtonRect(tabrect);
                            if(closebuttonmouseoverindex == i)
                            {
                                using(var bgbrush = new SolidBrush(Color.FromArgb(96, selectedTextColor)))
                                    g.FillRectangle(bgbrush, bgrect);
                            }

                            //mxd. Draw X
                            using(var closepen = new Pen(selectedTextColor, 1f))
                            {
                                const int offset = 4;
                                int tlx = bgrect.X + offset;
                                int tly = bgrect.Y + offset;
                                int brx = bgrect.X + bgrect.Width - offset;
                                int bry = bgrect.Y + bgrect.Height - offset;

                                g.DrawLine(closepen, tlx, tly, brx, bry);
                                g.DrawLine(closepen, tlx, bry, brx, tly);
                            }
                        }
                    }
                }
                else
                {
                    // Simply draw the header when it is not selected
                    using(var textbrush = new SolidBrush(textColor))
                    {
                        g.DrawString(TabPages[i].Text, Font, textbrush, TextSize, centersringformat);
                    }
                }

                //mxd. Draw icon?
                if(TabPages[i].ImageIndex != -1)
                {
                    g.DrawImage(ImageList.Images[TabPages[i].ImageIndex], Header.X + 2, 3);
                }
            }

            // Draw the background of the tab control
            using(var backtabbrush = new SolidBrush(backTabColor))
                g.FillRectangle(backtabbrush, new Rectangle(0, ItemSize.Height, Width, Height - ItemSize.Height));

            // Draw the border of the TabControl
            using(var borderpen = new Pen(borderColor, 2))
				g.DrawRectangle(borderpen, new Rectangle(0, ItemSize.Height, Width, Height - ItemSize.Height));

			// Draw the horizontal line
			using(var linepen = new Pen(activeColor, 2))
				g.DrawLine(linepen, new Point(0, ItemSize.Height - 1), new Point(Width, ItemSize.Height - 1));
        }

        #endregion

        #region ======================== Methods

        //mxd
        private static Rectangle GetCloseButtonRect(Rectangle tabrect)
        {
            int height = tabrect.Height - 6;
            return new Rectangle(tabrect.Right - height - 3, 3, height, height);
        }

        private TabPage GetPointedTab()
        {
            for(var i = 0; i < TabPages.Count; i++)
            {
                if(GetTabRect(i).Contains(PointToClient(Cursor.Position)))
                {
                    return TabPages[i];
                }
            }

            return null;
        }

        //mxd
        private int GetPointedTabIndex()
        {
            for(var i = 0; i < TabPages.Count; i++)
            {
                if(GetTabRect(i).Contains(PointToClient(Cursor.Position)))
                {
                    return i;
                }
            }

            return -1;
        }

        private void ReplaceTabPages(TabPage Source, TabPage Destination)
        {
            var SourceIndex = TabPages.IndexOf(Source);
            var DestinationIndex = TabPages.IndexOf(Destination);

            TabPages[DestinationIndex] = Source;
            TabPages[SourceIndex] = Destination;

            if(SelectedIndex == SourceIndex)
            {
                SelectedIndex = DestinationIndex;
            }
            else if(SelectedIndex == DestinationIndex)
            {
                SelectedIndex = SourceIndex;
            }

            Refresh();
        }

        #endregion
    }
}
