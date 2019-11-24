using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace HelloApps.GUI
{
    public class CommandEditorClass : ContainerControl
    {
        public delegate void LogInfoHandler(string log);
        public LogInfoHandler LogInfoEvent = null;

        public delegate void RequestAutoExecutionHandler();
        public RequestAutoExecutionHandler RequestAutoExecutionEvent = null;

        public delegate void RequestColorUpdateHandler(string object_name, string color);
        public RequestColorUpdateHandler RequestColorUpdateEvent = null;


        bool _Korean_Mode = false;

        public string FilePath = string.Empty;
        public bool UpdatedFlag = false;
        public CustomTabControl ParentTabControl = null;        

        string _name = string.Empty;

        Font _default_bold_font = new System.Drawing.Font("Microsoft Sans Serif", 12.0F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Font _default_normal_font = new System.Drawing.Font("Microsoft Sans Serif", 14.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        string _drag_guide_msg1 = string.Empty;
        string _drag_guide_msg2 = string.Empty;


        public Dictionary<string, int> _stringToIntMapping = new Dictionary<string, int>();

        public int LadderCellWidth = 130;
        public int LadderLineWidth = 30;
        public int LadderOutputWidth = 180;
        public int LadderCellHeight = 95;

        public int LadderXSize = 11;

        public int LadderYSize = 30;
        public int LadderLeftOffset = 30;
        public int LadderTopOffset = 30;

        public string ArduinoMainScript = string.Empty;

        public LadderCellItem[,] LadderCellList = new LadderCellItem[30, 15];


        public Dictionary<string, string> CmdNameTextList = new Dictionary<string, string>();
        public Dictionary<string, string> CmdNamePinList = new Dictionary<string, string>();
        public Dictionary<string, string> CmdNameArduinoMappingList = new Dictionary<string, string>();


        public CommandEditorClass()
        {
            _Korean_Mode = false;

            this.AutoScroll = true;
            this.AutoScrollMargin = new Size(0, 20);
            this.Margin = new Padding(0, 0, 0, 0);
            this.Padding = new Padding(20, 20, 20, 20);

            this.BackColor = Color.DimGray;

            this.ImeMode = ImeMode.Alpha;

            this.Initialize();
        }



        void Initialize()
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            ((Control)this).AllowDrop = true;

            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.OnDragOver);
            this.DragLeave += new System.EventHandler(this.OnDragLeave);

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);


            this.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.OnMouseWheel);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);

            this.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);

            _drag_guide_msg1 = "Please drag and drop command icon here.";
            _drag_guide_msg2 = "";

            if (_Korean_Mode)
            {
                _drag_guide_msg1 = "Please drag and drop";
                _drag_guide_msg2 = "command icon here.";
            }



            for (int y = 0; y < LadderYSize; y++)
            {

                int last_cell_pos_x = 0;

                for (int x = 0; x < LadderXSize; x++)
                {
                    LadderCellList[y, x] = new LadderCellItem();

                    LadderCellList[y, x].CELL_X = x;
                    LadderCellList[y, x].CELL_Y = y;
                    LadderCellList[y, x].Cell_ID = x.ToString() + "_" + y.ToString();

                    LadderCellList[y, x].LogInfoEvent = new LadderCellItem.LogInfoHandler(this.LogInfo);


                    int point_x = 0;
                    int point_y = 0;

                    if (x == 0)
                    {
                        LadderCellList[y, x].IsFirstCell = true;
                        LadderCellList[y, x].Width = LadderCellWidth;

                        point_x = LadderLeftOffset;
                        last_cell_pos_x = point_x + LadderCellWidth;
                    }
                    else if (x == (LadderXSize - 1))
                    {
                        LadderCellList[y, x].IsOutputCell = true;
                        LadderCellList[y, x].Width = LadderOutputWidth;


                        point_x = last_cell_pos_x;
                        last_cell_pos_x = last_cell_pos_x + LadderOutputWidth;
                    }
                    else
                    {
                        if ((x % 2) == 0)
                        {
                            LadderCellList[y, x].IsInputCell = true;
                            LadderCellList[y, x].Width = LadderCellWidth;

                            point_x = last_cell_pos_x;
                            last_cell_pos_x = last_cell_pos_x + LadderCellWidth;
                        }
                        else
                        {
                            LadderCellList[y, x].IsLineCell = true;
                            LadderCellList[y, x].Width = LadderLineWidth;

                            point_x = last_cell_pos_x;
                            last_cell_pos_x = last_cell_pos_x + LadderLineWidth;
                        }
                    }


                    point_y = LadderTopOffset + y * LadderCellHeight;


                    LadderCellList[y, x].CellRectangle = new Rectangle(point_x, point_y, LadderCellWidth, LadderCellHeight);

                    LadderCellList[y, x].CellPath = new GraphicsPath();
                    LadderCellList[y, x].CellPath.AddRectangle(LadderCellList[y, x].CellRectangle);
                    LadderCellList[y, x].CellPath.CloseFigure();
                    LadderCellList[y, x].CellRegion = new Region(LadderCellList[y, x].CellPath);

                    LadderCellList[y, x].Left = point_x;
                    LadderCellList[y, x].Top = point_y;
                    LadderCellList[y, x].Height = LadderCellHeight;
                    

                    if (LadderCellList[y, x].IsFirstCell || LadderCellList[y, x].IsInputCell)
                    {
                        LadderCellList[y, x].BgColor = new SolidBrush(Color.FromArgb(0, 128, 0));
                        LadderCellList[y, x].ShadowColor = new SolidBrush(Color.FromArgb(0, 98, 0));
                    }
                    else if (LadderCellList[y, x].IsOutputCell)
                    {
                        LadderCellList[y, x].BgColor = new SolidBrush(Color.FromArgb(255, 128, 0));
                        LadderCellList[y, x].ShadowColor = new SolidBrush(Color.FromArgb(225, 98, 0));
                    }

                    LadderCellList[y, x].ParentEditorClass = this;

                    LadderCellList[y, x].UpdatePaintInfo();

                    this.Controls.Add(LadderCellList[y, x]);
                }
            }
        }


        public void UpdateCmdNameList(LadderCellItem ladder_cell_item)
        {
            if (string.IsNullOrEmpty(ladder_cell_item.CmdName))
                return;

            if (CmdNameTextList.ContainsKey(ladder_cell_item.CmdName))
            {
                CmdNameTextList[ladder_cell_item.CmdName] = ladder_cell_item.CmdText;
                CmdNamePinList[ladder_cell_item.CmdName] = ladder_cell_item.ArduinoPin;
                CmdNameArduinoMappingList[ladder_cell_item.CmdName] = ladder_cell_item.ArduinoMappingMode;
            }
            else
            {
                CmdNameTextList.Add(ladder_cell_item.CmdName, ladder_cell_item.CmdText);
                CmdNamePinList.Add(ladder_cell_item.CmdName, ladder_cell_item.ArduinoPin);
                CmdNameArduinoMappingList.Add(ladder_cell_item.CmdName, ladder_cell_item.ArduinoMappingMode);
            }


            for (int y = 0; y < LadderYSize; y++)
            {
                for (int x = 0; x < LadderXSize; x++)
                {
                    if (LadderCellList[y, x].CmdName == ladder_cell_item.CmdName)
                    {
                        LadderCellList[y, x].CmdText = ladder_cell_item.CmdText;
                        LadderCellList[y, x].Invalidate();
                    }
                }
            }


            LogInfoEvent("#UpdateCmdNameList");
        }



        private void LogInfo(string log)
        {
            if (log == "#Invalidate")
            {
                for (int y = 0; y < LadderYSize; y++)
                {
                    for (int x = 0; x < LadderXSize; x++)
                    {
                        LadderCellList[y, x].Invalidate();
                    }
                }
            }
            else
            {
                if (LogInfoEvent != null)
                    LogInfoEvent(log);
            }
        }



        int _for_loop_index_iteration = 0;
        int _for_loop_index_name_order = -1;
        string[] _for_loop_index_names = new string[] { "i", "j", "k", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };

        public string GetNextForLoopIndexName()
        {
            string res = string.Empty;

            _for_loop_index_name_order = _for_loop_index_name_order + 1;

            if (_for_loop_index_name_order >= _for_loop_index_names.Length)
            {
                _for_loop_index_name_order = 0;
                _for_loop_index_iteration = _for_loop_index_iteration + 1;
            }

            if (_for_loop_index_iteration == 0)
            {
                res = _for_loop_index_names[_for_loop_index_name_order];
            }
            else
            {
                res = _for_loop_index_names[_for_loop_index_name_order] + _for_loop_index_iteration.ToString();
            }

            return res;
        }


        public void SetUpdatedFlag()
        {
            if (!UpdatedFlag)
            {
                UpdatedFlag = true;

                if (ParentTabControl != null)
                {
                    string title = ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text;

                    if (!title.EndsWith("*"))
                        ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text = ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text + "*";
                }
            }
        }

        public void SetSavedFlag()
        {
            //if (UpdatedFlag)
            {
                UpdatedFlag = false;

                if (ParentTabControl != null)
                {
                    string title = ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text;

                    if (!string.IsNullOrEmpty(FilePath))
                        ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text = GetTabTitle(FilePath);
                    else if (title.EndsWith("*"))
                        ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text = ParentTabControl.TabPages[ParentTabControl.SelectedIndex].Text.TrimEnd('*');
                }
            }
        }


        public string GetTabTitle(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                string filename = System.IO.Path.GetFileName(filepath);

                if (filename.Length > 33)
                {
                    string prefix = filename.Substring(0, 10);
                    string postfix = filename.Substring(filename.Length - 20, 20);

                    return prefix + "..." + postfix;
                }
                return filename;
            }
            else
                return "untitled";
        }



        public string GetScriptOfChilds(string indentation)
        {
            string script_lines = string.Empty;

            script_lines += "[LadderXSize] " + this.LadderXSize.ToString() + System.Environment.NewLine;
            script_lines += "[LadderYSize] " + this.LadderYSize.ToString() + System.Environment.NewLine;
            script_lines += "[ArduinoMainScript Start]" + System.Environment.NewLine;
            script_lines += this.ArduinoMainScript + System.Environment.NewLine;
            script_lines += "[ArduinoMainScript End]" + System.Environment.NewLine;

            for (int y = 0; y < this.LadderYSize; y++)
            {
                for (int x = 0; x < this.LadderXSize; x++)
                {
                    script_lines += "[LadderCell Start]" + System.Environment.NewLine;
                    script_lines += "Cell_ID:" + LadderCellList[y, x].Cell_ID + System.Environment.NewLine;
                    script_lines += "CELL_X:" + LadderCellList[y, x].CELL_X + System.Environment.NewLine;
                    script_lines += "CELL_Y:" + LadderCellList[y, x].CELL_Y + System.Environment.NewLine;
                    script_lines += "ArduinoMappingMode:" + LadderCellList[y, x].ArduinoMappingMode + System.Environment.NewLine;
                    script_lines += "ArduinoPin:" + LadderCellList[y, x].ArduinoPin + System.Environment.NewLine;
                    script_lines += "CmdName:" + LadderCellList[y, x].CmdName + System.Environment.NewLine;
                    script_lines += "CmdText:" + LadderCellList[y, x].CmdText + System.Environment.NewLine;
                    script_lines += "CmdType:" + LadderCellList[y, x].CmdType + System.Environment.NewLine;
                    script_lines += "HasDownLine:" + LadderCellList[y, x].HasDownLine + System.Environment.NewLine;
                    script_lines += "HasUpLine:" + LadderCellList[y, x].HasUpLine + System.Environment.NewLine;
                    script_lines += "IsFirstCell:" + LadderCellList[y, x].IsFirstCell + System.Environment.NewLine;
                    script_lines += "IsInputCell:" + LadderCellList[y, x].IsInputCell + System.Environment.NewLine;
                    script_lines += "IsLineCell:" + LadderCellList[y, x].IsLineCell + System.Environment.NewLine;
                    script_lines += "IsOutputCell:" + LadderCellList[y, x].IsOutputCell + System.Environment.NewLine;
                    script_lines += "TimerValue:" + LadderCellList[y, x].TimerValue + System.Environment.NewLine;
                    script_lines += "CounterValue:" + LadderCellList[y, x].CounterValue + System.Environment.NewLine;
                    script_lines += "[ArduinoCellScript Start]" + System.Environment.NewLine;
                    script_lines += LadderCellList[y, x].ArduinoCellScript + System.Environment.NewLine;
                    script_lines += "[ArduinoCellScript End]" + System.Environment.NewLine;
                    script_lines += "[LadderCell End]" + System.Environment.NewLine;
                }
            }


            return script_lines;
        }


        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                //_drag_flag = true;
                e.Effect = DragDropEffects.Move;
            }
            else
                e.Effect = DragDropEffects.None;

            this.Invalidate();
        }



        private void OnDragOver(object sender, DragEventArgs e)
        {
            this.Invalidate();
        }


        private void OnDragLeave(object sender, EventArgs e)
        {
            //
        }
        
        
        private void OnScroll(object sender, ScrollEventArgs e)
        {
            this.Invalidate();
        }


        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            //
        }

        private void OnMouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            this.Invalidate();
        }



        //#######################################################



        private void OnPaint(object sender, PaintEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new PaintEventHandler(this.OnPaint), new object[] { sender, e });
                return;
            }

            using (BufferedGraphics bufferedgraphic = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle))
            {
                bufferedgraphic.Graphics.Clear(System.Drawing.Color.DimGray);
                bufferedgraphic.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                bufferedgraphic.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                bufferedgraphic.Graphics.TranslateTransform(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);


                bufferedgraphic.Render(e.Graphics);
            }
        }
 

    }



    public class GlovalCellVariable
    {
        public static string CurSelectedCell = string.Empty;
        public static int CurSelectedCell_X = 0;
        public static int CurSelectedCell_Y = 0; 
    }



    public class LadderCellItem : PictureBox
    {
        public delegate void LogInfoHandler(string log);
        public LogInfoHandler LogInfoEvent = null;


        public int CELL_X = 0;
        public int CELL_Y = 0;
        public string Cell_ID = string.Empty;

        public Rectangle CellRectangle;
        public GraphicsPath CellPath;
        public Region CellRegion;

        public bool IsFirstCell = false;
        public bool IsInputCell = false;
        public bool IsOutputCell = false;
        public bool IsLineCell = false;

        public bool HasUpLine = false;
        public bool HasDownLine = false;

        public string CmdType = string.Empty;
        public string CmdName = string.Empty;
        public string CmdText = string.Empty;

        public string ArduinoMappingMode = string.Empty;
        public string ArduinoPin = string.Empty;
        public string ArduinoCellScript = string.Empty;

        public string TimerValue = string.Empty;
        public string CounterValue = string.Empty;


        public bool IsCompiled = false;


        bool _drag_flag = false;

        Font _default_normal_font14 = new System.Drawing.Font("Microsoft Sans Serif", 14.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Font _default_normal_font12 = new System.Drawing.Font("Microsoft Sans Serif", 12.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        Font _default_normal_font10 = new System.Drawing.Font("Microsoft Sans Serif", 10.0F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        private bool _mousePressedFlag = false;
        private int _MousePX = 0;
        private int _MousePY = 0;

        GraphicsPath _shadow_path = null;
        GraphicsPath _shape_path = null;


        private Rectangle _removeRect;
        private GraphicsPath _removePath;
        private Region _removeRegion;


        public Brush BgColor = new SolidBrush(Color.FromArgb(200, 200, 200));
        public Brush ShadowColor = new SolidBrush(Color.FromArgb(170, 170, 170));

        public CommandEditorClass ParentEditorClass = null;


        public LadderCellItem()
        {
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);


            ((Control)this).AllowDrop = true;

            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.OnDragEnter);
            this.DragOver += new System.Windows.Forms.DragEventHandler(this.OnDragOver);
            this.DragLeave += new System.EventHandler(this.OnDragLeave);

            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.OnMouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnMouseUp);

            this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
        }


        public void UpdatePaintInfo()
        {
            _removeRect = new Rectangle(this.Width - 20, 4, 16, 16);
            _removePath = new GraphicsPath();
            _removePath.AddRectangle(_removeRect);
            _removePath.CloseFigure();
            _removeRegion = new Region(_removePath);

            int shape_width = this.Width;
            _shadow_path = PathCreate(6, 6, shape_width - 10, this.Height - 10, 6);
            _shape_path = PathCreate(5, 5, shape_width - 10, this.Height - 10, 6);
        }


        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {

                string recv_cmd = e.Data.GetData(DataFormats.StringFormat).ToString();

                bool copy_mode = false;
                bool internal_drag_mode = false;

                if (recv_cmd.StartsWith("#COPY#"))
                {
                    copy_mode = true;
                    internal_drag_mode = true;
                    recv_cmd = recv_cmd.Substring(6);
                }
                else if (recv_cmd.StartsWith("#MOVE#"))
                {
                    internal_drag_mode = true;
                    recv_cmd = recv_cmd.Substring(6);
                }


                if (this.Cell_ID == recv_cmd)
                {
                    this.Invalidate();
                    return;
                }



                if (internal_drag_mode && copy_mode)
                {
                    //
                }
                else if (internal_drag_mode)
                {
                    //
                }
                else
                {
                    if (recv_cmd == "InputA" || recv_cmd == "InputB" || recv_cmd == "MInputA" || recv_cmd == "MInputB"
                        || recv_cmd == "TimerInputA" || recv_cmd == "TimerInputB" || recv_cmd == "CounterInputA" || recv_cmd == "CounterInputB")
                    {
                        if (this.IsFirstCell || this.IsInputCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                                _drag_flag = true;
                        }
                    }
                    else if (recv_cmd == "Output" || recv_cmd == "MOutput" || recv_cmd == "TimerOutput" || recv_cmd == "CounterOutput"
                        || recv_cmd == "SET" || recv_cmd == "RESET" || recv_cmd == "END")
                    {
                        if (this.IsOutputCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                                _drag_flag = true;
                        }
                    }
                    else if (recv_cmd == "HLine")
                    {
                        if (this.IsFirstCell || this.IsInputCell || this.IsLineCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                //not connect if up VLine
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine" && this.ParentEditorClass.LadderCellList[cell_y, x].HasUpLine)
                                    do_flag = false;
                            }

                            if (do_flag)
                                _drag_flag = true;
                        }
                    }
                    else if (recv_cmd == "VLine")
                    {
                        if (this.IsLineCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //nothing on right side of VLine
                            for (int x = cell_x + 1; x < this.ParentEditorClass.LadderXSize; x++)
                            {
                                if (!string.IsNullOrEmpty(this.ParentEditorClass.LadderCellList[cell_y, x].CmdType))
                                    do_flag = false;
                            }

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                                _drag_flag = true;
                        }
                    }
                }

                //_drag_flag = true;
                e.Effect = DragDropEffects.Move;
            }
            else
                e.Effect = DragDropEffects.None;

            this.Invalidate();
        }



        private void OnDragDrop(object sender, DragEventArgs e)
        {
            _drag_flag = false;

            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string recv_cmd = e.Data.GetData(DataFormats.StringFormat).ToString();

                bool copy_mode = false;
                bool internal_drag_mode = false;

                if (recv_cmd.StartsWith("#COPY#"))
                {
                    copy_mode = true;
                    internal_drag_mode = true;
                    recv_cmd = recv_cmd.Substring(6);
                }
                else if (recv_cmd.StartsWith("#MOVE#"))
                {
                    internal_drag_mode = true;
                    recv_cmd = recv_cmd.Substring(6);
                }


                if (this.Cell_ID == recv_cmd)
                {
                    this.Invalidate();
                    return;
                }


                if (internal_drag_mode && copy_mode)
                {
                    //
                }
                else if (internal_drag_mode)
                {
                    //
                }
                else
                {

                    if (recv_cmd == "InputA" || recv_cmd == "InputB" || recv_cmd == "MInputA" || recv_cmd == "MInputB"
                        || recv_cmd == "TimerInputA" || recv_cmd == "TimerInputB" || recv_cmd == "CounterInputA" || recv_cmd == "CounterInputB")
                    {
                        if (this.IsFirstCell || this.IsInputCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                            {
                                this.CmdType = recv_cmd;

                                if (recv_cmd == "InputA")
                                {
                                    this.CmdName = "P00000";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "Input(A)";
                                }
                                else if (recv_cmd == "InputB")
                                {
                                    this.CmdName = "P00000";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "Input(B)";
                                }
                                else if (recv_cmd == "MInputA")
                                {
                                    this.CmdName = "M00000";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "MemoryRelay(A)";
                                }
                                else if (recv_cmd == "MInputB")
                                {
                                    this.CmdName = "M00000";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "MemoryRelay(B)";
                                }
                                else if (recv_cmd == "TimerInputA")
                                {
                                    this.CmdName = "T00000";
                                    //this.TimerValue = "10";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "Timer(A)";
                                }
                                else if (recv_cmd == "TimerInputB")
                                {
                                    this.CmdName = "T00000";
                                    //this.TimerValue = "10";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "Timer(B)";
                                }
                                else if (recv_cmd == "CounterInputA")
                                {
                                    this.CmdName = "C00000";
                                    //this.CounterValue = "10";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "Counter(A)";
                                }
                                else if (recv_cmd == "CounterInputB")
                                {
                                    this.CmdName = "C00000";
                                    //this.CounterValue = "10";

                                    if (this.ParentEditorClass.CmdNameTextList.ContainsKey(this.CmdName))
                                        this.CmdText = this.ParentEditorClass.CmdNameTextList[this.CmdName];
                                    else
                                        this.CmdText = "Counter(B)";
                                }


                                //Cell
                                int pre_x = cell_x - 1;

                                for (int x = pre_x; x >= 0; x = x - 1)
                                {
                                    if (string.IsNullOrEmpty(this.ParentEditorClass.LadderCellList[cell_y, x].CmdType))
                                    {
                                        this.ParentEditorClass.LadderCellList[cell_y, x].CmdType = "HLine";
                                    }
                                }

                                GlovalCellVariable.CurSelectedCell = this.Cell_ID;
                                LogInfoEvent("#Invalidate");

                                GlovalCellVariable.CurSelectedCell_X = this.CELL_X;
                                GlovalCellVariable.CurSelectedCell_Y = this.CELL_Y;
                                LogInfoEvent("#SelectedCell:" + this.CELL_X.ToString() + "," + this.CELL_Y.ToString());

                                //update
                                //if (recv_cmd == "InputA" || recv_cmd == "InputB")
                                this.ParentEditorClass.UpdateCmdNameList(this);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (recv_cmd == "Output" || recv_cmd == "MOutput" || recv_cmd == "TimerOutput" || recv_cmd == "CounterOutput"
                        || recv_cmd == "SET" || recv_cmd == "RESET" || recv_cmd == "END")
                    {
                        if (this.IsOutputCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                            {
                                this.CmdType = recv_cmd;

                                if (recv_cmd == "Output")
                                {
                                    this.CmdName = "P00040";
                                    this.CmdText = "Output";
                                }
                                else if (recv_cmd == "MOutput")
                                {
                                    this.CmdName = "M00000";
                                    this.CmdText = "MemoryRelay";
                                }
                                else if (recv_cmd == "TimerOutput")
                                {
                                    this.CmdName = "T00000";
                                    this.CmdText = "Timer";
                                    this.TimerValue = "10";
                                }
                                else if (recv_cmd == "CounterOutput")
                                {
                                    this.CmdName = "C00000";
                                    this.CmdText = "Counter";
                                    this.CounterValue = "10";
                                }
                                else if (recv_cmd == "SET")
                                {
                                    this.CmdName = "M00000";
                                    this.CmdText = "SET";
                                }
                                else if (recv_cmd == "RESET")
                                {
                                    this.CmdName = "M00000";
                                    this.CmdText = "RESET";
                                }
                                else if (recv_cmd == "END")
                                {
                                    this.CmdName = "END";
                                    this.CmdText = "END";
                                }


                                //Cell
                                int pre_x = cell_x - 1;

                                for (int x = pre_x; x >= 0; x = x - 1)
                                {
                                    if (string.IsNullOrEmpty(this.ParentEditorClass.LadderCellList[cell_y, x].CmdType))
                                    {
                                        this.ParentEditorClass.LadderCellList[cell_y, x].CmdType = "HLine";
                                    }
                                }


                                GlovalCellVariable.CurSelectedCell = this.Cell_ID;
                                LogInfoEvent("#Invalidate");

                                GlovalCellVariable.CurSelectedCell_X = this.CELL_X;
                                GlovalCellVariable.CurSelectedCell_Y = this.CELL_Y;
                                LogInfoEvent("#SelectedCell:" + this.CELL_X.ToString() + "," + this.CELL_Y.ToString());

                                //update
                                //if (recv_cmd == "Output")
                                this.ParentEditorClass.UpdateCmdNameList(this);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (recv_cmd == "HLine")
                    {
                        if (this.IsFirstCell || this.IsInputCell || this.IsLineCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                            {
                                this.CmdType = recv_cmd;
                                this.CmdName = recv_cmd;

                                //Cell
                                int pre_x = cell_x - 1;

                                for (int x = pre_x; x >= 0; x = x - 1)
                                {
                                    if (string.IsNullOrEmpty(this.ParentEditorClass.LadderCellList[cell_y, x].CmdType))
                                    {
                                        this.ParentEditorClass.LadderCellList[cell_y, x].CmdType = "HLine";
                                    }
                                }

                                GlovalCellVariable.CurSelectedCell = this.Cell_ID;
                                LogInfoEvent("#Invalidate");

                                GlovalCellVariable.CurSelectedCell_X = this.CELL_X;
                                GlovalCellVariable.CurSelectedCell_Y = this.CELL_Y;
                                LogInfoEvent("#SelectedCell:" + this.CELL_X.ToString() + "," + this.CELL_Y.ToString());                                
                            }
                        }
                    }
                    else if (recv_cmd == "VLine")
                    {
                        if (this.IsLineCell)
                        {
                            int cell_x = this.CELL_X;
                            int cell_y = this.CELL_Y;

                            bool do_flag = true;


                            //nothing of right side of VLine
                            for (int x = cell_x + 1; x < this.ParentEditorClass.LadderXSize; x++)
                            {
                                if (!string.IsNullOrEmpty(this.ParentEditorClass.LadderCellList[cell_y, x].CmdType))
                                    do_flag = false;
                            }

                            //no VLine in front
                            for (int x = cell_x - 1; x >= 0; x--)
                            {
                                if (this.ParentEditorClass.LadderCellList[cell_y, x].CmdType == "VLine")
                                    do_flag = false;
                            }


                            if (do_flag)
                            {
                                int up_y = cell_y - 1;

                                for (int y = up_y; y >= 0; y = y - 1)
                                {
                                    if (this.ParentEditorClass.LadderCellList[y, cell_x].CmdType == "HLine")
                                    {
                                        this.ParentEditorClass.LadderCellList[y, cell_x].CmdType = "VLine";
                                        this.ParentEditorClass.LadderCellList[y, cell_x].HasDownLine = true;

                                        this.ParentEditorClass.LadderCellList[cell_y, cell_x].HasUpLine = true;


                                        for (int dumy_y = y + 1; dumy_y < cell_y; dumy_y++)
                                        {
                                            this.ParentEditorClass.LadderCellList[dumy_y, cell_x].CmdType = "VLine";
                                            this.ParentEditorClass.LadderCellList[dumy_y, cell_x].HasDownLine = true;
                                            this.ParentEditorClass.LadderCellList[dumy_y, cell_x].HasUpLine = true;
                                        }

                                        break;
                                    }
                                }

                                if (this.ParentEditorClass.LadderCellList[cell_y, cell_x].HasUpLine == false)
                                {
                                    //should not add vertical line
                                    //Do nothing
                                }
                                else
                                {
                                    this.CmdType = recv_cmd;
                                    this.CmdName = recv_cmd;
                                }

                                GlovalCellVariable.CurSelectedCell = this.Cell_ID;
                                LogInfoEvent("#Invalidate");

                                GlovalCellVariable.CurSelectedCell_X = this.CELL_X;
                                GlovalCellVariable.CurSelectedCell_Y = this.CELL_Y;
                                LogInfoEvent("#SelectedCell:" + this.CELL_X.ToString() + "," + this.CELL_Y.ToString());                                
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                        return;
                    
                }
            }


            this.Invalidate();
        }


        private void OnDragOver(object sender, DragEventArgs e)
        {
            //
        }


        private void OnDragLeave(object sender, EventArgs e)
        {
            _drag_flag = false;
            this.Invalidate();
        }


        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mousePressedFlag = true;

                _MousePX = e.X;
                _MousePY = e.Y;

                if (Control.ModifierKeys == Keys.Shift)
                    this.DoDragDrop("#COPY#" + this.Cell_ID, DragDropEffects.Copy | DragDropEffects.All);
                else
                    this.DoDragDrop("#MOVE#" + this.Cell_ID, DragDropEffects.Copy | DragDropEffects.All);

                if (!string.IsNullOrEmpty(this.CmdType))
                {
                    GlovalCellVariable.CurSelectedCell = this.Cell_ID;
                    LogInfoEvent("#Invalidate");

                    GlovalCellVariable.CurSelectedCell_X = this.CELL_X;
                    GlovalCellVariable.CurSelectedCell_Y = this.CELL_Y;
                    LogInfoEvent("#SelectedCell:" + this.CELL_X.ToString() + "," + this.CELL_Y.ToString());
                }
                else
                {
                    GlovalCellVariable.CurSelectedCell = string.Empty;
                    LogInfoEvent("#Invalidate");

                    GlovalCellVariable.CurSelectedCell_X = this.CELL_X;
                    GlovalCellVariable.CurSelectedCell_Y = this.CELL_Y;
                    LogInfoEvent("#SelectedCell:" + this.CELL_X.ToString() + "," + this.CELL_Y.ToString());
                }


                if (!string.IsNullOrEmpty(this.CmdType) && _removeRegion != null && _removeRegion.IsVisible(_MousePX, _MousePY))
                {
                    DeleteControlFromEditor();
                }
            }            
        }


        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            if (_mousePressedFlag)
            {
                //
            }

            _mousePressedFlag = false;
        }


        public void DeleteControlFromEditor()
        {
            this.DeleteControlFromEditor(false);
        }


        public void DeleteControlFromEditor(bool silent_remove_flag)
        {
            string dlg_title = "Remove confirm";
            string dlg_msg = "Do you want to remove \"" + this.CmdName + "\"?";

            if (silent_remove_flag == false)
            {
                if (MessageBox.Show(dlg_msg, dlg_title, MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
            }

            this.CmdType = string.Empty;
            this.CmdName = string.Empty;
            this.CmdText = string.Empty;

            this.HasDownLine = false;
            this.HasUpLine = false;

            this.Invalidate();
        }



        public enum RectangleCorners
        {
            None = 0, TopLeft = 1, TopRight = 2, BottomLeft = 4, BottomRight = 8,
            All = TopLeft | TopRight | BottomLeft | BottomRight
        }


        public static GraphicsPath PathCreate(int x, int y, int width, int height,
                                          int radius, RectangleCorners corners)
        {
            int xw = x + width;
            int yh = y + height;
            int xwr = xw - radius;
            int yhr = yh - radius;
            int xr = x + radius;
            int yr = y + radius;
            int r2 = radius * 2;
            int xwr2 = xw - r2;
            int yhr2 = yh - r2;

            GraphicsPath p = new GraphicsPath();
            p.StartFigure();

            //Top Left Corner
            if ((RectangleCorners.TopLeft & corners) == RectangleCorners.TopLeft)
            {
                p.AddArc(x, y, r2, r2, 180, 90);
            }
            else
            {
                p.AddLine(x, yr, x, y);
                p.AddLine(x, y, xr, y);
            }

            //Top Edge
            p.AddLine(xr, y, xwr, y);

            //Top Right Corner
            if ((RectangleCorners.TopRight & corners) == RectangleCorners.TopRight)
            {
                p.AddArc(xwr2, y, r2, r2, 270, 90);
            }
            else
            {
                p.AddLine(xwr, y, xw, y);
                p.AddLine(xw, y, xw, yr);
            }

            //Right Edge
            p.AddLine(xw, yr, xw, yhr);

            //Bottom Right Corner
            if ((RectangleCorners.BottomRight & corners) == RectangleCorners.BottomRight)
            {
                p.AddArc(xwr2, yhr2, r2, r2, 0, 90);
            }
            else
            {
                p.AddLine(xw, yhr, xw, yh);
                p.AddLine(xw, yh, xwr, yh);
            }

            //Bottom Edge
            p.AddLine(xwr, yh, xr, yh);

            //Bottom Left Corner
            if ((RectangleCorners.BottomLeft & corners) == RectangleCorners.BottomLeft)
            {
                p.AddArc(x, yhr2, r2, r2, 90, 90);
            }
            else
            {
                p.AddLine(xr, yh, x, yh);
                p.AddLine(x, yh, x, yhr);
            }

            //Left Edge
            p.AddLine(x, yhr, x, yr);

            p.CloseFigure();
            return p;
        }

        public static GraphicsPath PathCreate(Rectangle rect, int radius, RectangleCorners c)
        { return PathCreate(rect.X, rect.Y, rect.Width, rect.Height, radius, c); }

        public static GraphicsPath PathCreate(int x, int y, int width, int height, int radius)
        { return PathCreate(x, y, width, height, radius, RectangleCorners.All); }

        public static GraphicsPath PathCreate(Rectangle rect, int radius)
        { return PathCreate(rect.X, rect.Y, rect.Width, rect.Height, radius); }

        public static GraphicsPath PathCreate(int x, int y, int width, int height)
        { return PathCreate(x, y, width, height, 5); }

        public static GraphicsPath PathCreate(Rectangle rect)
        { return PathCreate(rect.X, rect.Y, rect.Width, rect.Height); }



        private void OnPaint(object sender, PaintEventArgs e)
        {

            if (this.InvokeRequired)
            {
                this.Invoke(new PaintEventHandler(this.OnPaint), new object[] { sender, e });
                return;
            }


            using (BufferedGraphics bufferedgraphic = BufferedGraphicsManager.Current.Allocate(e.Graphics, this.ClientRectangle))
            {
                bufferedgraphic.Graphics.Clear(System.Drawing.Color.DimGray);
                bufferedgraphic.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                bufferedgraphic.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;


                int cell_x = this.CELL_X;
                int cell_y = this.CELL_Y;


                //nornal state background
                if (this.IsFirstCell || this.IsInputCell || this.IsOutputCell)
                {
                    if (!string.IsNullOrEmpty(this.CmdType) && this.CmdType != "HLine")
                    {
                        bufferedgraphic.Graphics.FillPath(this.ShadowColor, _shadow_path);
                        bufferedgraphic.Graphics.FillPath(this.BgColor, _shape_path);
                    }
                }



                if (_drag_flag)
                {
                    bufferedgraphic.Graphics.DrawRectangle(Pens.White, 1, 1, this.Width - 2, this.Height - 2);
                }
                else
                {
                    if (GlovalCellVariable.CurSelectedCell == this.Cell_ID)
                        bufferedgraphic.Graphics.DrawRectangle(Pens.Yellow, 1, 1, this.Width - 2, this.Height - 2);
                }


                if (this.CmdType == "HLine" || this.CmdType == "VLine" || this.CmdType == "END")
                {
                    //Do nothing
                }
                else
                {
                    //Title and name
                    SizeF name_size = bufferedgraphic.Graphics.MeasureString(this.CmdName, _default_normal_font14);
                    bufferedgraphic.Graphics.DrawString(this.CmdName, _default_normal_font14, Brushes.White, this.Width / 2 - (name_size.Width / 2), this.Height / 2 - (name_size.Height + 10));

                    SizeF text_size = bufferedgraphic.Graphics.MeasureString(this.CmdText, _default_normal_font12);
                    bufferedgraphic.Graphics.DrawString(this.CmdText, _default_normal_font12, Brushes.White, this.Width / 2 - (text_size.Width / 2), this.Height / 2 + (text_size.Height - 6));
                }





                if (this.CmdType == "InputA" || this.CmdType == "MInputA" || this.CmdType == "TimerInputA" || this.CmdType == "CounterInputA")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 10, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 10, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("|", _default_normal_font14, Brushes.White, this.Width / 2 - 13, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString("|", _default_normal_font14, Brushes.White, this.Width / 2 + 5, this.Height / 2 - 14);

                }
                else if (this.CmdType == "InputB" || this.CmdType == "MInputB" || this.CmdType == "TimerInputB" || this.CmdType == "CounterInputB")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 10, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 10, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("|", _default_normal_font14, Brushes.White, this.Width / 2 - 13, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString("|", _default_normal_font14, Brushes.White, this.Width / 2 + 5, this.Height / 2 - 14);

                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 - 4, this.Height / 2 + 7, this.Width / 2 + 6, this.Height / 2 - 7);
                }
                else if (this.CmdType == "Output" || this.CmdType == "MOutput")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 12, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 13, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("(", _default_normal_font14, Brushes.White, this.Width / 2 - 15, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString(")", _default_normal_font14, Brushes.White, this.Width / 2 + 7, this.Height / 2 - 14);

                }
                else if (this.CmdType == "SET")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 12, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 13, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("(", _default_normal_font14, Brushes.White, this.Width / 2 - 15, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString(")", _default_normal_font14, Brushes.White, this.Width / 2 + 7, this.Height / 2 - 14);

                    SizeF val_size = bufferedgraphic.Graphics.MeasureString("S", _default_normal_font14);
                    bufferedgraphic.Graphics.DrawString("S", _default_normal_font14, Brushes.White, this.Width / 2 - (val_size.Width / 2 - 2), this.Height / 2 - (val_size.Height - 11));
                }
                else if (this.CmdType == "RESET")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 12, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 13, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("(", _default_normal_font14, Brushes.White, this.Width / 2 - 15, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString(")", _default_normal_font14, Brushes.White, this.Width / 2 + 7, this.Height / 2 - 14);

                    SizeF val_size = bufferedgraphic.Graphics.MeasureString("R", _default_normal_font14);
                    bufferedgraphic.Graphics.DrawString("R", _default_normal_font14, Brushes.White, this.Width / 2 - (val_size.Width / 2 - 2), this.Height / 2 - (val_size.Height - 11));
                }
                else if (this.CmdType == "TimerOutput")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 27, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 28, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("[", _default_normal_font14, Brushes.White, this.Width / 2 - 30, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString("]", _default_normal_font14, Brushes.White, this.Width / 2 + 22, this.Height / 2 - 14);

                    SizeF val_size = bufferedgraphic.Graphics.MeasureString(this.TimerValue, _default_normal_font14);
                    bufferedgraphic.Graphics.DrawString(this.TimerValue, _default_normal_font14, Brushes.White, this.Width / 2 - (val_size.Width / 2), this.Height / 2 - (val_size.Height - 11));
                }
                else if (this.CmdType == "CounterOutput")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 27, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 28, this.Height / 2, this.Width, this.Height / 2);

                    bufferedgraphic.Graphics.DrawString("[", _default_normal_font14, Brushes.White, this.Width / 2 - 30, this.Height / 2 - 14);
                    bufferedgraphic.Graphics.DrawString("]", _default_normal_font14, Brushes.White, this.Width / 2 + 22, this.Height / 2 - 14);

                    SizeF val_size = bufferedgraphic.Graphics.MeasureString(this.CounterValue, _default_normal_font14);
                    bufferedgraphic.Graphics.DrawString(this.CounterValue, _default_normal_font14, Brushes.White, this.Width / 2 - (val_size.Width / 2), this.Height / 2 - (val_size.Height - 11));
                }
                else if (this.CmdType == "END")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2 - 27, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2 + 28, this.Height / 2, this.Width, this.Height / 2);

                    SizeF val_size = bufferedgraphic.Graphics.MeasureString(this.CmdType, _default_normal_font14);
                    bufferedgraphic.Graphics.DrawString(this.CmdType, _default_normal_font14, Brushes.White, this.Width / 2 - (val_size.Width / 2), this.Height / 2 - (val_size.Height - 11));
                }
                else if (this.CmdType == "HLine")
                {
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width, this.Height / 2);
                }
                else if (this.CmdType == "VLine")
                {
                    if (!string.IsNullOrEmpty(this.ParentEditorClass.LadderCellList[cell_y, cell_x - 1].CmdType))
                        bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width / 2, this.Height / 2);

                    if (this.HasUpLine)
                        bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2, this.Height / 2, this.Width / 2, 0);

                    if (this.HasDownLine)
                        bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2, this.Height / 2, this.Width / 2, this.Height);

                    if (this.HasDownLine && this.HasUpLine == false)
                        bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width / 2, this.Height / 2, this.Width, this.Height / 2);
                }
                else
                {
                    //bufferedgraphic.Graphics.DrawLine(Pens.White, 0, this.Height / 2, this.Width, this.Height / 2);
                    bufferedgraphic.Graphics.DrawLine(Pens.Gray, 0, this.Height / 2, this.Width, this.Height / 2);
                }


                if (this.IsFirstCell)
                {
                    //right inner vertical line 
                    bufferedgraphic.Graphics.DrawLine(Pens.White, 0, 0, 0, this.Height);
                }
                else if (this.IsOutputCell)
                {
                    //right inner vertical line 
                    //one pixel inner not to overlap
                    bufferedgraphic.Graphics.DrawLine(Pens.White, this.Width - 1, 0, this.Width - 1, this.Height);
                }



                //remove icon
                if (this.IsLineCell)
                {
                    if (GlovalCellVariable.CurSelectedCell == this.Cell_ID && _removeRect != null)
                        bufferedgraphic.Graphics.DrawImage(Properties.Resources.Remove, _removeRect);
                }
                else
                {
                    if (GlovalCellVariable.CurSelectedCell == this.Cell_ID && _removeRect != null)
                        bufferedgraphic.Graphics.DrawImage(Properties.Resources.Remove, _removeRect);
                }

                bufferedgraphic.Render(e.Graphics);
            }
        }
    }



}
