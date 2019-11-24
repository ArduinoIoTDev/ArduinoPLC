using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HelloApps.GUI
{
    public class BlockMaxSize
    {
        public int Width = 0;
        public int Height = 0;
    }


    public class GlobalInstance
    {
        public static string LastAddedOp1 = string.Empty;
        public static string LastAddedOp2 = string.Empty;
    }

    public enum BlockListType
    {
        None = 0, Editor = 1, Item = 2, Block = 3, Option = 4, Paste = 5
    }

    public enum DragOverType
    {
        None = 0, Upper = 1, Inner = 2, Bottom = 3
    }

    public enum InstaceType
    {
        None = 0, Editor = 1, Item = 2, Block = 3, Option = 4
    }

}
