using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace HelloApps.GUI
{
    public enum CommandTypes
    {
        None = 0, Read = 1, Write = 2, Logic = 3, Procedure = 4, Call = 5, 
        Flush = 6, TargetExpr = 7, Tone = 8, Expr1 = 9, Expr2 = 10, 
        Expr3 = 11, Print = 12, EmptyLine = 13,
        Map = 14, Delay = 15, Millis = 16,
        SerialWrite = 17, SetColor = 18, SetAllColor = 19, Expr0 = 20, DriveWrite = 21,
        DigitalTempRead = 22, AnalogTempRead = 23, ImportScript = 24, Expr3_XYV = 25, SerialBegin = 26, MotorDrive = 27,
        NSBeginWithPin = 28, NSClearWithPin = 29, NSSetColorWithPin = 30, NSShowWithPin = 31, Expr5_XYRGB = 32,
        LoRa_SendMessage = 33, LoRa_SendMessageHex = 34, LoRa_GetMessage = 35, LoRa_GetMessageHex = 36, LoRa_Reset = 37, LoRa_PrintTTV = 38, LoRa_StringToHex = 39
    }


    public class GUICommandItem
    {
        public BlockListType CommandMode = BlockListType.None;
        public string CommandName = string.Empty;
        public CommandTypes CommandType = CommandTypes.Expr1;

        public string DisplayName = string.Empty;
        public string ToolTip = string.Empty;

        public string OptionPosition = "0  0  0";
        public string OptionScale = "1  1  1";
        public string OptionOrientation = "0  0  0";
        public string OptionColor = string.Empty;
        public string OptionStartPoint = "0  0  0";
        public string OptionEndPoint = "0  0  1";
        public string OptionSlices = "36";
        public string OptionVerticalSlices = "36";

        public string[] Values = new string[10];

        public bool Checked = true;
        public bool VisibleYN = true;
        public bool Shrinked = false;


        public Brush BgColor = new SolidBrush(Color.FromArgb(200, 200, 200));
        public Brush ShadowColor = new SolidBrush(Color.FromArgb(170, 170, 170));

        public Brush UnChecked_BgColor = new SolidBrush(Color.FromArgb(128, 128, 128));
        public Brush UnChecked_ShadowColor = new SolidBrush(Color.FromArgb(98, 98, 98));


        public GUICommandItem()
        {
        }

        public GUICommandItem(BlockListType cmd_mode, string cmd_name)
        {
            CommandMode = cmd_mode;
            CommandName = cmd_name;
        }

        public GUICommandItem(BlockListType cmd_mode, string cmd_name, string display_name, CommandTypes command_type, string[] values, string tooltip)
        {
            CommandMode = cmd_mode;
            CommandName = cmd_name;
            DisplayName = display_name;
            this.CommandType = command_type;
            
            if (values != null)
                Values = values;

            ToolTip = tooltip;
        }

        public GUICommandItem Clone(Control sender)
        {
            GUICommandItem new_instance = new GUICommandItem(this.CommandMode, this.CommandName, this.DisplayName, this.CommandType, null, this.ToolTip);


            //Values
            if (this.Values != null)
            {
                new_instance.Values = new string[this.Values.Length];

                for (int i = 0; i < this.Values.Length; i++)
                {
                    new_instance.Values[i] = this.Values[i];
                }
            }

            new_instance.Checked = this.Checked;

            new_instance.BgColor = this.BgColor;
            new_instance.ShadowColor = this.ShadowColor;

            new_instance.UnChecked_BgColor = this.UnChecked_BgColor;
            new_instance.UnChecked_ShadowColor = this.UnChecked_ShadowColor;

            return new_instance;
        }
    }


    public class GUICommands
    {
        private static object _copyed_object = null;

        public static object GetCopyedObject()
        {
            return _copyed_object;
        }


        public static string GetNextForLoopIndexName(Control sender)
        {
            HelloApps.GUI.CommandEditorClass top_commandEditorClass = HelloApps.GUI.GUIUtils.GetEditorInstance(sender);
            return top_commandEditorClass.GetNextForLoopIndexName();
        }


        public static string GetStringWithUniquePostfix(string name, Control sender)
        {
            return GetStringWithUniquePostfix(name, sender, 1);
        }

        public static string GetStringWithUniquePostfix(string name, Control sender, int start_ind)
        {
            HelloApps.GUI.CommandEditorClass top_commandEditorClass = HelloApps.GUI.GUIUtils.GetEditorInstance(sender);
            Dictionary<string, int> stringToIntMapping = top_commandEditorClass._stringToIntMapping;


            int count = 0;

            if (stringToIntMapping.ContainsKey(name))
            {
                count = stringToIntMapping[name];
                count = count + 1;
                stringToIntMapping[name] = count;
            }
            else
            {
                count = start_ind;
                stringToIntMapping.Add(name, count);
            }

            return name + count.ToString();
        }


        public static GUICommandItem GetCmdModeName(string recv_cmd, string[] default_values, Control sender)
        {           
            GUICommandItem new_item = new GUICommandItem();

            //bool is_korean = false;
            
            if (default_values != null)
                new_item.Values = default_values;

            string default_target_name0 = HelloApps.GUI.GUIUtils.GetSafeArrayValue(default_values, 0);
            string default_target_name1 = HelloApps.GUI.GUIUtils.GetSafeArrayValue(default_values, 1);
            string default_target_name2 = HelloApps.GUI.GUIUtils.GetSafeArrayValue(default_values, 2);
            string default_target_name3 = HelloApps.GUI.GUIUtils.GetSafeArrayValue(default_values, 3);
            string default_target_name4 = HelloApps.GUI.GUIUtils.GetSafeArrayValue(default_values, 4);
            string default_target_name5 = HelloApps.GUI.GUIUtils.GetSafeArrayValue(default_values, 5);

            if (recv_cmd == "Paste")
            {
                new_item.CommandMode = BlockListType.Paste;
                new_item.CommandName = recv_cmd;
            }
            else if (recv_cmd == "EmptyLine")
            {
                new_item.CommandMode = BlockListType.Item;
                new_item.CommandName = recv_cmd;
                new_item.CommandType = CommandTypes.EmptyLine;
            }
            else
            {
                new_item.CommandMode = BlockListType.Item;
                new_item.CommandName = "Expression";
                new_item.DisplayName = new_item.CommandName;
                new_item.CommandType = CommandTypes.Expr1;


                if (!string.IsNullOrEmpty(recv_cmd))
                    HelloApps.GUI.GUIUtils.SetSafeArrayValue(new_item.Values, 0, recv_cmd);


                new_item.BgColor = new SolidBrush(Color.FromArgb(0, 128, 0));
                new_item.ShadowColor = new SolidBrush(Color.FromArgb(0, 98, 0));
            }



            new_item.CommandMode = BlockListType.Item;
            new_item.CommandName = "Expression";
            new_item.DisplayName = new_item.CommandName;
            new_item.CommandType = CommandTypes.Expr1;


            if (!string.IsNullOrEmpty(recv_cmd))
                HelloApps.GUI.GUIUtils.SetSafeArrayValue(new_item.Values, 0, recv_cmd);


            new_item.BgColor = new SolidBrush(Color.FromArgb(0, 128, 0));
            new_item.ShadowColor = new SolidBrush(Color.FromArgb(0, 98, 0));

            return new_item;
        }


    }
}
