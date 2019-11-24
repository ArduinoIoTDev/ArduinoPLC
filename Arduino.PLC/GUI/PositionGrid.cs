using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace HelloApps.GUI
{
    public class PositionGrid : PictureBox
    {
        public delegate void ChangeNotifyHandler(int x, int y);
        public ChangeNotifyHandler ChangeNotifyEvent = null;

        public int Pos_X = 0;
        public int Pos_Y = 0;

        public PositionGrid()
        {
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
        }

        public void SetPosition(float x, float y)
        {
            Pos_X = (int)x;
            Pos_Y = (int)(y * -1);
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            Pos_X = e.X - 100;
            Pos_Y = e.Y - 100;

            this.Invalidate();

            if (ChangeNotifyEvent != null)
                ChangeNotifyEvent(Pos_X, Pos_Y);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int cur_pos_x = Pos_X + 100;
            int cur_pos_y = Pos_Y + 100;

            for (int i = 0; i < 20; i++)
            {
                e.Graphics.DrawLine(Pens.Black, i * 10 - 1, 0, i * 10 - 1, 200);
                e.Graphics.DrawLine(Pens.Black, 0, i * 10 - 1, 200, i * 10 - 1);
            }

            e.Graphics.DrawLine(Pens.Black, 100 - 2, 0, 100 - 2, 200);
            e.Graphics.DrawLine(Pens.Black, 100, 0, 100, 200);

            e.Graphics.DrawLine(Pens.Black, 0, 100 - 2, 200, 100 - 2);
            e.Graphics.DrawLine(Pens.Black, 0, 100, 200, 100);

            e.Graphics.FillEllipse(Brushes.Green, new Rectangle(cur_pos_x - 11, cur_pos_y - 11, 21, 21));
        }
    }
}
