using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Reflection;

using System.IO.Ports;
using System.Management;
using System.Diagnostics;

using System.Collections.Specialized;

using HelloApps.Common;


namespace HelloApps
{
    public partial class MainForm : Form
    {

        string _currentInstalledVersion = "v1.00.0";    
        string _currentTitle = string.Empty;
        string _InstalledVersionNum = "1.00";

        string _arduino_version_path = "arduino-1.6.6";

        string _spl_exec_path = string.Empty;


        string _copyrightText = "Copyright © HelloApps, 2019, All Rights Reserved. ";


        string[] _args;     

        string _curUserDirectory = string.Empty;
        string _curSampleDirectory = string.Empty;
        string _curTutorialDirectory = string.Empty;
        
        string _currentFile = string.Empty;
         
        string _lastSelectDirectory = string.Empty;
        string _previousTargetType = string.Empty;


        List<TreeView> _treeViewList = new List<TreeView>();
        Queue<string> _timer_util_target_list = new Queue<string>();
        Queue<int> _timer_util_target_time = new Queue<int>();

         
        Hashtable _allCmdList = new Hashtable();   
        Hashtable _allOptionList = new Hashtable();   
        Hashtable _allGroupList = new Hashtable();   
        Hashtable _allExprList = new Hashtable();   

        string _currentCmdName = string.Empty;    
        Font _defaltFont = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

        string _lastIndentString = string.Empty;


        private ImageList _gpl_ImageList;
        private ListView _curListView = null;

        private List<ListView> _listViewList = new List<ListView>();


        private string _last_printed_variable_name = string.Empty;


        List<string> _Serial_Port_List = new List<string>();
        string _spl_duino_comport = string.Empty;



        string _timer_form_load_mode = "FormLoad";  

        SPLConsole _spl_console = new SPLConsole();


        public enum EditorOpenMode
        {
            Text = 0, Graphic = 1
        }


        public MainForm(string[] args)
        {
            try
            {
                InitializeComponent();

                _currentTitle = _currentInstalledVersion;

                string my_docu_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                _spl_exec_path = my_docu_path + @"\ArduinoPLC\";

                tabControl2.TabPages[0].Text = "Recent Files";

                listView4.Columns[0].Text = "File Name";
                listView4.Columns[1].Text = "Date";
                listView4.Columns[2].Text = "Path";

                _args = args;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }




        public void SetSPLDuinoComPort(string com_port, string baud_rate)
        {
            string etc_path = _spl_exec_path + "etc\\";
            

            if (!Directory.Exists(etc_path))
                Directory.CreateDirectory(etc_path);

            string splduino_comport_path = etc_path + "compile_mode.txt";
            string splduino_baudrate_path = etc_path + "compile_mode2.txt";

            if (File.Exists(splduino_comport_path))
                File.Delete(splduino_comport_path);

            if (File.Exists(splduino_baudrate_path))
                File.Delete(splduino_baudrate_path);

            if (!string.IsNullOrEmpty(com_port) && !string.IsNullOrEmpty(baud_rate))
            {
                if (com_port.StartsWith("COM"))
                {
                    File.WriteAllText(splduino_comport_path, "HA" + com_port.Substring(3) + "1559");
                    File.WriteAllText(splduino_baudrate_path, baud_rate);
                }
            }
        }


        public void DeleteSPLDuinoComPortFile()
        {
            string etc_path = _spl_exec_path + "etc\\";

            if (!Directory.Exists(etc_path))
                Directory.CreateDirectory(etc_path);

            string splduino_comport_path = etc_path + "compile_mode.txt";
            string splduino_baudrate_path = etc_path + "compile_mode2.txt";

            if (File.Exists(splduino_comport_path))
                File.Delete(splduino_comport_path);

            if (File.Exists(splduino_baudrate_path))
                File.Delete(splduino_baudrate_path);
        }




        private void LogInfoFromGUI(string log)
        {
            if (log.StartsWith("#SelectedCell:"))
            {
                int cell_x = HelloApps.GUI.GlovalCellVariable.CurSelectedCell_X;
                int cell_y = HelloApps.GUI.GlovalCellVariable.CurSelectedCell_Y;

                if (customTabControl1.CurCmdEditorClass != null)
                {
                    if (customTabControl1.CurCmdEditorClass != null)
                    {
                        if (HelloApps.GUI.GlovalCellVariable.CurSelectedCell == "_PARENT_")
                        {
                            textBox1.Text = "Arduino Main Script";
                            textBox2.Text = string.Empty;
                            textBox3.Text = string.Empty;

                            bool has_setup = customTabControl1.CurCmdEditorClass.ArduinoMainScript.Contains("setup");
                            bool has_loop = customTabControl1.CurCmdEditorClass.ArduinoMainScript.Contains("loop");

                            if (string.IsNullOrEmpty(customTabControl1.CurCmdEditorClass.ArduinoMainScript) || has_setup == false || has_loop == false)
                            {
                                string default_line = Environment.NewLine;
                                default_line += "void setup()" + Environment.NewLine;
                                default_line += "{" + Environment.NewLine;
                                default_line += "\t" + Environment.NewLine;
                                default_line += "}" + Environment.NewLine;

                                default_line += Environment.NewLine;

                                default_line += "void loop()" + Environment.NewLine;
                                default_line += "{" + Environment.NewLine;
                                default_line += "\t" + Environment.NewLine;
                                default_line += "}" + Environment.NewLine;

                                customTabControl1.CurCmdEditorClass.ArduinoMainScript = default_line;
                            }

                            richTextBox1.Text = customTabControl1.CurCmdEditorClass.ArduinoMainScript;
                            radioButton2.Checked = true;
                        }
                        else
                        {
                            HelloApps.GUI.LadderCellItem ladder_cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[cell_y, cell_x];
                            ShowCellInfo(ladder_cell_item);
                        }
                    }
                }
            }
            else if (log == "#UpdateCmdNameList")
            {
                if (customTabControl1.CurCmdEditorClass != null)
                {
                    listBox1.Items.Clear();

                    foreach (string cmd_name in customTabControl1.CurCmdEditorClass.CmdNameTextList.Keys)
                    {
                        string cmd_text = customTabControl1.CurCmdEditorClass.CmdNameTextList[cmd_name];
                        string arduino_pin = customTabControl1.CurCmdEditorClass.CmdNamePinList[cmd_name];
                        string arduino_mapping = customTabControl1.CurCmdEditorClass.CmdNameArduinoMappingList[cmd_name];

                        string line = "[" + cmd_name + "] " + cmd_text;

                        if (arduino_mapping != "SCRIPT" && !string.IsNullOrEmpty(arduino_pin))
                        {
                            line = line + " - D" + arduino_pin;                            
                        }

                        if (IsUsingCmdName(cmd_name))
                        {
                            listBox1.Items.Add(line);
                        }
                    }
                }
            }
        }


        private bool IsUsingCmdName(string targetCmdName)
        {
            bool res = false;

            for (int y = 0; y < customTabControl1.CurCmdEditorClass.LadderYSize; y++)
            {
                for (int x = 0; x < customTabControl1.CurCmdEditorClass.LadderXSize; x++)
                {
                    if (customTabControl1.CurCmdEditorClass.LadderCellList[y, x].CmdName == targetCmdName)
                    {
                        return true;
                    }
                }
            }

            return res;
        }


        private void ShowCellInfo(HelloApps.GUI.LadderCellItem ladder_cell_item)
        {
            textBox1.Text = ladder_cell_item.CmdName;
            textBox2.Text = ladder_cell_item.CmdText;


            if (ladder_cell_item.CmdType == "MInputA" || ladder_cell_item.CmdType == "MInputB" || ladder_cell_item.CmdType == "MOutput")
            {
                textBox3.Visible = false;
                richTextBox1.Visible = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
            }
            else if (ladder_cell_item.CmdType == "TimerInputA" || ladder_cell_item.CmdType == "TimerInputB")
            {
                textBox3.Visible = false;
                richTextBox1.Visible = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
            }
            else if (ladder_cell_item.CmdType == "TimerOutput")
            {
                textBox3.Visible = true;
                richTextBox1.Visible = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                label4.Visible = false;
                label5.Visible = true;
                label5.Text = "Timer Time";
                label6.Visible = false;

                textBox3.Text = ladder_cell_item.TimerValue;
            }
            else if (ladder_cell_item.CmdType == "CounterInputA" || ladder_cell_item.CmdType == "CounterInputB")
            {
                textBox3.Visible = false;
                richTextBox1.Visible = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                label4.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
            }
            else if (ladder_cell_item.CmdType == "CounterOutput")
            {
                textBox3.Visible = true;
                richTextBox1.Visible = false;
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                label4.Visible = false;
                label5.Visible = true;
                label5.Text = "Timer Count";
                label6.Visible = false;

                textBox3.Text = ladder_cell_item.CounterValue;
            }
            else
            {
                textBox3.Visible = true;
                richTextBox1.Visible = true;
                radioButton1.Visible = true;
                radioButton2.Visible = true;
                label4.Visible = true;
                label5.Visible = true;
                label5.Text = "Pin";
                label6.Visible = true;


                if (ladder_cell_item.ArduinoMappingMode == "SCRIPT")
                {
                    radioButton2.Checked = true;
                }
                else
                {
                    radioButton1.Checked = true;
                }

                textBox3.Text = ladder_cell_item.ArduinoPin;
                richTextBox1.Text = ladder_cell_item.ArduinoCellScript;

                if (ladder_cell_item.ArduinoMappingMode != "SCRIPT" && string.IsNullOrEmpty(textBox3.Text))
                {
                    if (ladder_cell_item.CmdType == "InputA" || ladder_cell_item.CmdType == "InputB")
                        ladder_cell_item.ArduinoPin = "2";
                    else if (ladder_cell_item.CmdType == "Output")
                        ladder_cell_item.ArduinoPin = "13";

                    textBox3.Text = ladder_cell_item.ArduinoPin;
                }
            }


            
        }


        private void SaveCellInfo(HelloApps.GUI.LadderCellItem ladder_cell_item)
        {
            ladder_cell_item.CmdName = textBox1.Text;
            ladder_cell_item.CmdText = textBox2.Text;

            if (ladder_cell_item.CmdType == "InputA" || ladder_cell_item.CmdType == "InputB" || ladder_cell_item.CmdType == "Output")
            {
                if (radioButton2.Checked == true)
                {
                    ladder_cell_item.ArduinoMappingMode = "SCRIPT";
                }
                else
                {
                    ladder_cell_item.ArduinoMappingMode = "PIN";
                }

                ladder_cell_item.ArduinoPin = textBox3.Text;
                ladder_cell_item.ArduinoCellScript = richTextBox1.Text;
            }
            else if (ladder_cell_item.CmdType == "TimerOutput")
            {
                ladder_cell_item.TimerValue = textBox3.Text;
            }
            else if (ladder_cell_item.CmdType == "CounterOutput")
            {
                ladder_cell_item.CounterValue = textBox3.Text;
            }
        }




        private void LogInfo(string log)
        {
            this.Invoke(

                new MethodInvoker(delegate
                {
                    
                })
            );
        }





        private void timer_auto_script_Tick(object sender, EventArgs e)
        {
            timer_auto_script.Enabled = false;

            LauncheService(string.Empty, SPLCommon.SPLEngineLaunchMode.AutoExecute);
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {

                CheckForIllegalCrossThreadCalls = false;

                _spl_duino_comport = HelloApps.Helper.SPLDuinoHelper.GetSPLDuinoComPortName();

                this.Text = "HelloApps Arduino PLC Editor " + _currentTitle + ".";

                string last_board = string.Empty;
                string last_comport = string.Empty;
                string last_baudrate = string.Empty;


                if (!Directory.Exists(_spl_exec_path + "Config"))
                    Directory.CreateDirectory(_spl_exec_path + "Config");

                if (File.Exists(_spl_exec_path + "Config\\LastBoard.txt"))
                    last_board = File.ReadAllText(_spl_exec_path + "Config\\LastBoard.txt");

                if (File.Exists(_spl_exec_path + "Config\\LastComport.txt"))
                    last_comport = File.ReadAllText(_spl_exec_path + "Config\\LastComport.txt");

                if (File.Exists(_spl_exec_path + "Config\\LastBaudrate.txt"))
                    last_baudrate = File.ReadAllText(_spl_exec_path + "Config\\LastBaudrate.txt");


                if (!File.Exists(_spl_exec_path + "Config\\InitRef" + _InstalledVersionNum + ".txt"))
                {
                    File.WriteAllText(_spl_exec_path + "Config\\InitRef" + _InstalledVersionNum + ".txt", _currentInstalledVersion);

                    string ref_path = _spl_exec_path + _arduino_version_path + "\\ref\\";

                    if (Directory.Exists(ref_path))
                        Directory.Delete(ref_path, true);
                }

               

                comboBox1.Width = 110;
                comboBox2.Width = 110;
                comboBox3.Width = 110;

                comboBox2.Items.Clear();

                foreach (string port_name in HelloApps.Helper.SPLDuinoHelper.GetComPortNameList())
                {
                    comboBox2.Items.Add(port_name);
                }

                if (comboBox2.Items.Count > 0)
                    comboBox2.Text = comboBox2.Items[0].ToString();




                //Baudrate

                comboBox3.Items.Clear();
                comboBox3.Items.Add("115200");
                comboBox3.Items.Add("57600");
                comboBox3.Items.Add("38400");
                comboBox3.Items.Add("19200");
                comboBox3.Items.Add("9600");

                comboBox3.Text = "115200";


                string user_com_port = string.Empty;
                string user_baud_rate = string.Empty;

                user_com_port = HelloApps.Common.ParsingHelper.GetFirstToken(comboBox2.Text);

                user_baud_rate = comboBox3.Text;

                timer_form_load.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("[Form1_Load] " + ex.ToString());
            }
        }





        private void timer_form_load_Tick(object sender, EventArgs e)
        {
            timer_form_load.Enabled = false;

            try
            {
                if (_timer_form_load_mode == "FormLoad")
                    FormOnLoad_Internal();
                else if (_timer_form_load_mode == "FileCopy")
                {                   
                    CopyArduinoFiles();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void CopyArduinoFiles()
        {
            string cur_path = Path.GetDirectoryName(Application.ExecutablePath) + "\\";

            int new_copy_count = SyncArduinoFolder(cur_path + _arduino_version_path, _spl_exec_path + _arduino_version_path, true);

            string arduino_lib_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            arduino_lib_path = arduino_lib_path + @"\Arduino\libraries";
            new_copy_count = new_copy_count + SyncDocumentArduinoLibraryFolder(cur_path + _arduino_version_path + "\\libraries", arduino_lib_path);
            
            new_copy_count = new_copy_count + SyncArduinoFolder(cur_path + _arduino_version_path + "\\hardware\\tools\\avr\\etc\\", _spl_exec_path + _arduino_version_path + "\\hardware\\tools\\avr\\etc\\", false);
        }


        private int SyncArduinoFolder(string from_dir, string to_dir, bool tool_skip)
        {
            int new_copy_count = 0;

            if (Directory.Exists(from_dir))
            {
                if (!Directory.Exists(to_dir))
                    Directory.CreateDirectory(to_dir);


                foreach (string file_path in Directory.GetFiles(from_dir))
                {
                    string file_name = Path.GetFileName(file_path);

                    if (!File.Exists(to_dir + "\\" + file_name))
                    {
                        File.Copy(from_dir + "\\" + file_name, to_dir + "\\" + file_name);
                        new_copy_count++;
                    }
                    else
                    {
                        if (File.GetLastWriteTime(to_dir + "\\" + file_name) != File.GetLastWriteTime(from_dir + "\\" + file_name))
                        {
                            File.Delete(to_dir + "\\" + file_name);
                            File.Copy(from_dir + "\\" + file_name, to_dir + "\\" + file_name);
                            new_copy_count++;
                        }
                    }


                }


                foreach (string sub_path in Directory.GetDirectories(from_dir))
                {
                    string sub_dir_name = Path.GetFileName(sub_path);

                    if (tool_skip)
                    {
                        if (sub_dir_name != "tools" && sub_dir_name != "bootloaders" && sub_dir_name != "firmwares")
                            new_copy_count = new_copy_count + SyncArduinoFolder(sub_path, to_dir + "\\" + sub_dir_name, tool_skip);
                    }
                    else
                        new_copy_count = new_copy_count + SyncArduinoFolder(sub_path, to_dir + "\\" + sub_dir_name, tool_skip);

                }
            }

            Application.DoEvents();

            return new_copy_count;
        }



        private int SyncDocumentArduinoLibraryFolder(string from_dir, string to_dir)
        {
            int new_copy_count = 0;

            if (Directory.Exists(from_dir))
            {

                if (!Directory.Exists(to_dir))
                    Directory.CreateDirectory(to_dir);



                foreach (string file_path in Directory.GetFiles(from_dir))
                {
                    string file_name = Path.GetFileName(file_path);

                    if (!File.Exists(to_dir + "\\" + file_name))
                    {
                        File.Copy(from_dir + "\\" + file_name, to_dir + "\\" + file_name);
                        new_copy_count++;
                    }
                    else
                    {
                        if (File.GetLastWriteTime(to_dir + "\\" + file_name) != File.GetLastWriteTime(from_dir + "\\" + file_name))
                        {
                            File.Delete(to_dir + "\\" + file_name);
                            File.Copy(from_dir + "\\" + file_name, to_dir + "\\" + file_name);
                            new_copy_count++;
                        }
                    }


                }


                foreach (string sub_path in Directory.GetDirectories(from_dir))
                {
                    string sub_dir_name = Path.GetFileName(sub_path);
                    if (sub_dir_name == "src")
                        new_copy_count = new_copy_count + SyncDocumentArduinoLibraryFolder(sub_path, to_dir);
                    else
                        new_copy_count = new_copy_count + SyncDocumentArduinoLibraryFolder(sub_path, to_dir + "\\" + sub_dir_name);

                }
            }

            Application.DoEvents();

            return new_copy_count;
        }


       
        private void FormOnLoad_Internal()
        {
            label10.Text = _copyrightText;

            string debug_info = string.Empty;


            try
            {

                if (!Directory.Exists(_spl_exec_path + "BlockDiagram"))
                    Directory.CreateDirectory(_spl_exec_path + "BlockDiagram");


                if (!Directory.Exists(_spl_exec_path + "Script\\Config"))
                    Directory.CreateDirectory(_spl_exec_path + "Script\\Config");


                if (!Directory.Exists(_spl_exec_path + "Config"))
                    Directory.CreateDirectory(_spl_exec_path + "Config");





                if (File.Exists(_spl_exec_path + "SPL\\BlockDiagram\\RecentFiles.txt"))
                {
                    string recent_text = File.ReadAllText(_spl_exec_path + "SPL\\BlockDiagram\\RecentFiles.txt");

                    string[] recentList = recent_text.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                    for (int rInd = 0; rInd < recentList.Length; rInd++)
                    {
                        if (!string.IsNullOrEmpty(recentList[rInd]))
                        {
                            string[] nameAccess = recentList[rInd].Split(new Char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);

                            if (nameAccess.Length == 2)
                            {
                                AddToRecentFileList(nameAccess[0], nameAccess[1]);
                            }
                        }
                    }
                }



                _currentFile = string.Empty;



                List<string> scriptLines = new List<string>();
                


                LoadTreeMenuScript(scriptLines, string.Empty, "ScriptEditorMenu.txt", false);


                Hashtable menuNameList = new Hashtable();
                Hashtable dirNameList = new Hashtable();


                debug_info = "E01";

                ExecuteScriptList(scriptLines, menuNameList, dirNameList);

                debug_info = "F01";

                foreach (TreeView tv in _treeViewList)
                {
                    foreach (TreeNode tn in tv.Nodes)
                    {
                        tn.Expand();
                    }
                }


                debug_info = "G01";


                bool startPageErrorFlag = false;

                if (!Directory.Exists(_spl_exec_path + "Config"))
                    Directory.CreateDirectory(_spl_exec_path + "Config");

                string last_editor_type = string.Empty;

                if (File.Exists(_spl_exec_path + "Config\\LastEditorType.txt"))
                    last_editor_type = File.ReadAllText(_spl_exec_path + "Config\\LastEditorType.txt");


                if (!string.IsNullOrEmpty(last_editor_type) && last_editor_type == "Script")
                    AddNewTabPage("untitled", string.Empty, "Script");
                else
                    AddNewTabPage("untitled", string.Empty, string.Empty);                   


                debug_info = "H01";


                if (!startPageErrorFlag)
                {
                    customTabControl1.SelectedIndex = 0;
                }


                debug_info = "J01";

                _curUserDirectory = _spl_exec_path + "BlockDiagram";

                SetDirectory(_curUserDirectory, "User");


                debug_info = "K01";

                if (tabControl1.TabPages.Count > 0)
                {
                    TreeView tv = tabControl1.TabPages[0].Controls[0] as TreeView;

                    if (tv != null)
                    {
                        tv.SelectedNode = tv.Nodes[0];
                    }
                }


                debug_info = "L01";

                if (tabControl2.TabPages.Count > 0)
                {
                    tabControl2.SelectedIndex = 0;
                }


                _timer_form_load_mode = "FileCopy";
                timer_form_load.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[FormOnLoad_Internal #2] " + debug_info + " : " + ex.ToString());
            }
        }


        string _debug_data1 = "";
        string _debug_data2 = "";
        string _debug_data3 = "";


        private void Set_Debug_Data(string d1, string d2, string d3)
        {
            _debug_data1 = d1;
            _debug_data2 = d2;
            _debug_data3 = d3;
        }

        private string Get_Debug_Data()
        {
            return System.Environment.NewLine + "/" + _debug_data1 + System.Environment.NewLine + "/" + _debug_data2 + System.Environment.NewLine + "/" + _debug_data3;
        }


        private void AddToRecentFileList(string filePath, string accessTime)
        {

            try
            {
                if (File.Exists(filePath))
                {

                    string fileName = Path.GetFileName(filePath);
                    string newAccessTime = accessTime;

                    if (string.IsNullOrEmpty(newAccessTime))
                        newAccessTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    ListViewItem recentitem = new ListViewItem();

                    bool firstItemUpdateFlag = false;

                    if (string.IsNullOrEmpty(accessTime))
                    {
                        if (listView4.Items.Count > 0)
                        {
                            int itemPos = 0;
                            foreach (ListViewItem rItem in listView4.Items)
                            {
                                string subpath = rItem.SubItems[2].Text;

                                if (subpath == filePath)
                                {
                                    if (itemPos == 0)
                                    {
                                        firstItemUpdateFlag = true;

                                        rItem.SubItems[1].Text = newAccessTime;
                                    }
                                    else
                                        listView4.Items.Remove(rItem);

                                    break;
                                }

                                itemPos++;
                            }
                        }
                    }

                    recentitem.Text = fileName;
                    recentitem.SubItems.Add(newAccessTime);
                    recentitem.SubItems.Add(filePath);

                    if (!firstItemUpdateFlag)
                    {
                        if (string.IsNullOrEmpty(accessTime))
                            listView4.Items.Insert(0, recentitem);
                        else
                            listView4.Items.Add(recentitem);
                    }

                    if (listView4.Items.Count > 20)
                        listView4.Items.RemoveAt(listView4.Items.Count - 1);



                    if (string.IsNullOrEmpty(accessTime))
                    {
                        string writeText = string.Empty;

                        foreach (ListViewItem rItem in listView4.Items)
                        {
                            string accesstime = rItem.SubItems[1].Text;
                            string subpath = rItem.SubItems[2].Text;

                            writeText += subpath + '\t' + accesstime + Environment.NewLine;
                        }


                        if (File.Exists(_spl_exec_path + "SPL\\BlockDiagram\\RecentFiles.txt"))
                            File.Delete(_spl_exec_path + "SPL\\BlockDiagram\\RecentFiles.txt");

                        if (!Directory.Exists(_spl_exec_path + "SPL\\BlockDiagram\\"))
                            Directory.CreateDirectory(_spl_exec_path + "SPL\\BlockDiagram\\");

                        File.WriteAllText(_spl_exec_path + "SPL\\BlockDiagram\\RecentFiles.txt", writeText);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[AddToRecentFileList] " + ex.ToString());
            }
        }


        public string GetDirectoryName(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                string filename = filepath;

                if (filename.Length > 43)
                {
                    string prefix = filename.Substring(0, 10);
                    string postfix = filename.Substring(filename.Length - 30, 30);

                    return prefix + "..." + postfix;
                }
                return filename;
            }
            else
                return string.Empty;
        }



        private void FileListing(string dir, string type)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                if (dir == "Drive")
                {
                    string[] dList = Directory.GetLogicalDrives();

                    for (int z = 0; z < dList.Length; z++)
                    {
                        ListViewItem item = new ListViewItem();

                        item.Text = dList[z];
                        item.SubItems.Add(string.Empty);
                        item.SubItems.Add(string.Empty);
                        item.SubItems.Add("R");
                        item.ImageIndex = 3;
                    }
                }
                else if (Directory.Exists(dir))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);

                    ListViewItem item1 = new ListViewItem();

                    item1.Text = "..";
                    item1.SubItems.Add(string.Empty);
                    item1.SubItems.Add(string.Empty);
                    item1.SubItems.Add("P");
                    item1.ImageIndex = 4;

                    foreach (DirectoryInfo fi in di.GetDirectories())
                    {
                        ListViewItem item = new ListViewItem();

                        item.Text = fi.Name;
                        item.SubItems.Add(string.Empty);
                        item.SubItems.Add(fi.CreationTime.ToString());
                        item.SubItems.Add("D");
                        item.ImageIndex = 3;

                    }


                    foreach (FileInfo fi in di.GetFiles())
                    {
                        ListViewItem item = new ListViewItem();
                        item.Text = fi.Name;
                        item.SubItems.Add(fi.Extension);
                        item.SubItems.Add(fi.CreationTime.ToString());
                        item.SubItems.Add("F");

                        if (fi.Extension.ToLower() == ".txt")
                            item.ImageIndex = 5;
                        else if (fi.Extension.ToLower() == ".mrds")
                            item.ImageIndex = 1;
                        else if (fi.Extension.ToLower() == ".splx")
                            item.ImageIndex = 1;
                        else
                            item.ImageIndex = 2;

                    }
                }
            }
        }

        void SetDirectory(string dir, string type)
        {
            if (!string.IsNullOrEmpty(dir))
            {
                if (dir == "Drive")
                {
                    FileListing("Drive", type);
                }
                else if (Directory.Exists(dir))
                {
                    string labelText = dir;
                    if (labelText.Length > 3)
                        labelText = GetDirectoryName(labelText.TrimEnd('\\'));

                    FileListing(dir, type);
                }
            }
        }



        public string GetTextLinesFromResource(string filename)
        {
            Assembly assembly;
            StreamReader textStreamReader;

            string res = string.Empty;
            try
            {
                assembly = Assembly.GetExecutingAssembly();

                textStreamReader = new StreamReader(assembly.GetManifestResourceStream("HelloApps." + filename));                
                res = textStreamReader.ReadToEnd();

                textStreamReader.Close();
                textStreamReader.Dispose();
            }
            catch
            {
                res = string.Empty;
            }

            return res;
        }



        private bool AddNewTabPage(string title, string fileName, string ControlType)
        {
            bool errorFlag = false;


            listBox1.Items.Clear();


            customTabControl1.TabPages.Add(title);
            customTabControl1.TabPages[customTabControl1.TabPages.Count - 1].ImageIndex = 0;
            customTabControl1.TabPages[customTabControl1.TabPages.Count - 1].BackColor = Color.White;

            PictureBox pb = new PictureBox();
            pb.BorderStyle = BorderStyle.FixedSingle;
            pb.Dock = DockStyle.Fill;
            //pb.ContextMenuStrip = contextMenuStrip1;


            HelloApps.GUI.CommandEditorClass commandEditorClass = new GUI.CommandEditorClass();
            commandEditorClass.AllowDrop = true;
            commandEditorClass.AutoScroll = true;
            commandEditorClass.AutoScrollMargin = new System.Drawing.Size(0, 20);
            commandEditorClass.BackColor = System.Drawing.Color.DimGray;
            //commandEditorClass.ContextMenuStrip = this.contextMenuStrip1;
            commandEditorClass.Margin = new System.Windows.Forms.Padding(0);
            commandEditorClass.Padding = new System.Windows.Forms.Padding(20);
            commandEditorClass.Dock = DockStyle.Fill;

            commandEditorClass.LogInfoEvent = new HelloApps.GUI.CommandEditorClass.LogInfoHandler(this.LogInfoFromGUI);

            commandEditorClass.ImeMode = ImeMode.Alpha;

            commandEditorClass.ParentTabControl = customTabControl1;

            pb.Controls.Add(commandEditorClass);

            customTabControl1.TabPages[customTabControl1.TabPages.Count - 1].Controls.Add(pb);
            customTabControl1.SelectedIndex = customTabControl1.TabPages.Count - 1;

            commandEditorClass.FilePath = fileName;

            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName))
                {
                    _currentFile = fileName;


                    string[] lader_file_lines = File.ReadAllLines(fileName);

                    if (lader_file_lines != null)
                    {

                        bool is_arduino_main_script = false;
                        bool is_arduino_cell_script = false;
                        string arduino_main_script = string.Empty;
                        string arduino_cell_script = string.Empty;
                        string cell_id = string.Empty;
                        int cell_x = 0;
                        int cell_y = 0;

                        string cmd_type = string.Empty;

                        for (int i = 0; i < lader_file_lines.Length; i++)
                        {
                            string line = lader_file_lines[i];

                            if (line == "[ArduinoMainScript Start]")
                            {
                                is_arduino_main_script = true;
                                arduino_main_script = string.Empty;
                            }
                            else if (line == "[ArduinoMainScript End]")
                            {
                                is_arduino_main_script = false;
                                commandEditorClass.ArduinoMainScript = arduino_main_script;
                                arduino_main_script = string.Empty;
                            }
                            else if (line == "[ArduinoCellScript Start]")
                            {
                                is_arduino_cell_script = true;
                                arduino_cell_script = string.Empty;
                            }
                            else if (line == "[ArduinoCellScript End]")
                            {
                                is_arduino_cell_script = false;
                                commandEditorClass.LadderCellList[cell_y, cell_x].ArduinoCellScript = arduino_cell_script;
                                arduino_cell_script = string.Empty;
                            }
                            else if (line == "[LadderCell Start]")
                            {
                                //
                            }
                            else if (line == "[LadderCell End]")
                            {
                                //if (cmd_type == "InputA" || cmd_type == "InputB" || cmd_type == "Output")
                                {
                                    if (customTabControl1.CurCmdEditorClass != null)
                                    {
                                        customTabControl1.CurCmdEditorClass.UpdateCmdNameList(commandEditorClass.LadderCellList[cell_y, cell_x]);
                                    }
                                }
                            }
                            else if (line.StartsWith("Cell_ID:"))
                            {
                                cell_id = line.Substring("Cell_ID:".Length);
                            }
                            else if (line.StartsWith("CELL_X:"))
                            {
                                cell_x = int.Parse(line.Substring("CELL_X:".Length));
                            }
                            else if (line.StartsWith("CELL_Y:"))
                            {
                                cell_y = int.Parse(line.Substring("CELL_Y:".Length));
                            }
                            else if (line.StartsWith("ArduinoMappingMode:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].ArduinoMappingMode = line.Substring("ArduinoMappingMode:".Length);
                            }
                            else if (line.StartsWith("ArduinoPin:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].ArduinoPin = line.Substring("ArduinoPin:".Length);
                            }
                            else if (line.StartsWith("CmdName:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].CmdName = line.Substring("CmdName:".Length);
                            }
                            else if (line.StartsWith("CmdText:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].CmdText = line.Substring("CmdText:".Length);
                            }
                            else if (line.StartsWith("CmdType:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].CmdType = line.Substring("CmdType:".Length);
                                cmd_type = commandEditorClass.LadderCellList[cell_y, cell_x].CmdType;
                            }
                            else if (line.StartsWith("HasDownLine:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].HasDownLine = bool.Parse(line.Substring("HasDownLine:".Length));
                            }
                            else if (line.StartsWith("HasUpLine:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].HasUpLine = bool.Parse(line.Substring("HasUpLine:".Length));
                            }
                            else if (line.StartsWith("IsFirstCell:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].IsFirstCell = bool.Parse(line.Substring("IsFirstCell:".Length));
                            }
                            else if (line.StartsWith("IsInputCell:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].IsInputCell = bool.Parse(line.Substring("IsInputCell:".Length));
                            }
                            else if (line.StartsWith("IsLineCell:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].IsLineCell = bool.Parse(line.Substring("IsLineCell:".Length));
                            }
                            else if (line.StartsWith("IsOutputCell:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].IsOutputCell = bool.Parse(line.Substring("IsOutputCell:".Length));
                            }
                            else if (line.StartsWith("TimerValue:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].TimerValue = line.Substring("TimerValue:".Length);
                            }
                            else if (line.StartsWith("CounterValue:"))
                            {
                                commandEditorClass.LadderCellList[cell_y, cell_x].CounterValue = line.Substring("CounterValue:".Length);
                            }
                            else
                            {
                                if (is_arduino_main_script)
                                    arduino_main_script += line + System.Environment.NewLine;
                                else if (is_arduino_cell_script)
                                    arduino_cell_script += line + System.Environment.NewLine;
                            }
                        }
                    }

                }
                else
                {
                    MessageBox.Show("Can't find a file. " + fileName);
                }

            }
            else
            {
                //
            }

            return errorFlag;
        }





        private void FindTreeNodeFromList(string matchText)
        {
            for (int i = 0; i < tabControl1.TabPages.Count - 1; i++)
            {
                TreeView tvSelection = (TreeView)tabControl1.TabPages[i].Controls[0];

                TreeNode target_node = FindTreeNode(tvSelection, matchText);

                if (target_node != null)
                {
                    tabControl1.SelectedIndex = i;

                    tvSelection.Focus();
                    tvSelection.Select();
                    tvSelection.SelectedNode = null;
                    tvSelection.SelectedNode = target_node;

                    target_node.Expand();
                    break;
                }
            }
        }

        private TreeNode FindTreeNode(TreeView tvSelection, string matchText)
        {
            foreach (TreeNode node in tvSelection.Nodes)
            {
                if (node.Name.ToString() == matchText)
                {
                    return node;
                }
                else
                {
                    TreeNode nodeChild = FindChildTreeNode(node, matchText);
                    if (nodeChild != null)
                    {
                        return nodeChild;
                    }
                }
            }
            return (TreeNode)null;
        }


        private TreeNode FindChildTreeNode(TreeNode tvSelection, string matchText)
        {
            foreach (TreeNode node in tvSelection.Nodes)
            {
                if (node.Name.ToString() == matchText)
                {
                    return node;
                }
                else
                {
                    TreeNode nodeChild = FindChildTreeNode(node, matchText);
                    if (nodeChild != null) return nodeChild;
                }
            }
            return (TreeNode)null;
        }



        void LoadTreeMenuScript(List<string> scriptLines, string dirName ,string fileName, bool useCustomMenuFlag)
        {
            string allScriptText = string.Empty;

            if (useCustomMenuFlag)
                allScriptText = File.ReadAllText(dirName + fileName);
            else
            {
                allScriptText = GetTextLinesFromResource(fileName);
            }

            
            if (!string.IsNullOrEmpty(allScriptText))
            {
                int lastInd = 0;

                 
                
                string[] readlines = allScriptText.Split(new Char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                

                for (int i = 0; i < readlines.Length; i++)
                {
                    if (!readlines[i].Trim().StartsWith("//"))
                    {
                        if (readlines[i].Trim() == string.Empty)
                        {
                            // just skip
                        }
                        else if (readlines[i].Trim().StartsWith("-"))
                        {
                             
                            lastInd = scriptLines.Count - 1;

                            if (lastInd >= 0 && scriptLines.Count > 0)
                                scriptLines[lastInd] = scriptLines[lastInd] + " " + readlines[i].Substring(1, readlines[i].Length - 1).Trim();
                        }
                        else if (readlines[i].Trim().StartsWith("/"))
                        {
                             
                            lastInd = scriptLines.Count - 1;

                            if (lastInd >= 0 && scriptLines.Count > 0)
                                scriptLines[lastInd] = scriptLines[lastInd] + " " + readlines[i].Trim();
                        }
                        else if (readlines[i].Trim().StartsWith("importscript"))
                        {
                            string filename = string.Empty;

                            string[] arr;
                            arr = readlines[i].Trim().Split(new Char[] { '/' }, 1000);

                            List<string> newArr = new List<string>();

                            for (int x = 0; x < arr.Length; x++)
                            {
                                if (arr[x].Trim() != string.Empty)
                                    newArr.Add(arr[x].Trim());
                            }

                            foreach (string tag in newArr)
                            {
                                string[] tagArr;
                                tagArr = tag.Split(new Char[] { ':' }, 2);

                                if (tagArr.Length == 2 && !string.IsNullOrEmpty(tagArr[1]))
                                {
                                    tagArr[1] = HelloApps.Common.Util.RestoreSlashString(tagArr[1]);

                                    switch (tagArr[0])
                                    {
                                        case "filename":
                                            filename = tagArr[1].Trim();
                                            filename = filename.Replace('!', '\\');
                                            filename = filename.Replace('^', '\"');
                                            filename = filename.Replace('?', '^');
                                            break;
                                    }
                                }
                            }

                            if (filename != string.Empty)
                            {
                                 
                                LoadTreeMenuScript(scriptLines, dirName, filename, useCustomMenuFlag);
                                 
                            }
                        }
                        else
                        {
                            scriptLines.Add(readlines[i].Trim());
                        }
                    }
                }                
            }
        }


        TreeView GetTreeViewByName(string treeviewname)
        {
            foreach (TreeView tv in _treeViewList)
            {
                if (tv.Name == treeviewname)
                    return tv;
            }

            return null;
        }



        void ExecuteScriptList(List<string> lines, Hashtable menuNameList, Hashtable dirNameList)
        {
            try
            {
                _gpl_ImageList = new ImageList();
                _gpl_ImageList.ImageSize = new Size(32, 32);

                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon1);    //gear
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon2);    //green
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon3);    //red
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon4);    //blue
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon5);    //lego
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon6);    //flash
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon7);    //gear
                _gpl_ImageList.Images.Add(Properties.Resources.splg_icon8);    //timer
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ExecuteScriptList #1] Image List Add Error. " + ex.ToString());
            }

            string cur_tab_name = string.Empty;



            string line = string.Empty;

            try
            {

                for (int ind = 0; ind < lines.Count; ind++)
                {
                    line = HelloApps.Common.Util.ReplaceSlashString(lines[ind]);


                    string name = string.Empty;
                    string forecolor = string.Empty;
                    string tooltiptext = string.Empty;
                    string tabtext = string.Empty;
                    string typeStr = string.Empty;
                    string textStr = string.Empty;
                    string engStr = string.Empty;
                    string imageStr = string.Empty;
                    string defaultStr = string.Empty;


                    string[] arr;
                    arr = line.Split(new Char[] { '/' }, 1000);

                    List<string> newArr = new List<string>();

                    //#############################################################
                    List<string> method_list = new List<string>();
                    //#############################################################

                    for (int i = 1; i < arr.Length; i++)
                    {
                        if (arr[i].Trim() != string.Empty)
                            newArr.Add(arr[i].Trim());
                    }

                    foreach (string tag in newArr)
                    {
                        string[] tagArr;
                        tagArr = tag.Split(new Char[] { ':' }, 2);

                        if (tagArr.Length == 2 && !string.IsNullOrEmpty(tagArr[1]))
                        {
                            tagArr[1] = HelloApps.Common.Util.RestoreSlashString(tagArr[1]);

                            switch (tagArr[0])
                            {
                                case "name":
                                    name = tagArr[1].Trim();
                                    name = name.Replace('^', '\"');
                                    name = name.Replace('?', '^');
                                    name = name.Replace('!', '/');
                                    break;

                                case "text":
                                    textStr = tagArr[1].Trim();
                                    textStr = textStr.Replace('^', '\"');
                                    textStr = textStr.Replace('?', '^');
                                    textStr = textStr.Replace('!', '/');
                                    break;

                                case "eng":
                                    engStr = tagArr[1].Trim();
                                    engStr = engStr.Replace('^', '\"');
                                    engStr = engStr.Replace('?', '^');
                                    engStr = engStr.Replace('!', '/');
                                    break;

                                case "forecolor":
                                    forecolor = tagArr[1].Trim();
                                    break;

                                case "tooltiptext":
                                    tooltiptext = tagArr[1].Trim();
                                    tooltiptext = tooltiptext.Replace('^', '\"');
                                    tooltiptext = tooltiptext.Replace('?', '^');
                                    tooltiptext = tooltiptext.Replace('!', '/');
                                    break;

                                case "tabtext":
                                    tabtext = tagArr[1].Trim();
                                    break;

                                case "type":
                                    typeStr = tagArr[1].Trim();
                                    break;

                                case "image":
                                    imageStr = tagArr[1].Trim();
                                    break;

                                case "default":
                                    defaultStr = tagArr[1].Trim();
                                    defaultStr = defaultStr.Replace('^', '\"');
                                    defaultStr = defaultStr.Replace('?', '^');
                                    defaultStr = defaultStr.Replace('!', '/');
                                    break;

                                case "method":
                                    {
                                        //#############################################################
                                        method_list.Add(tagArr[1].Trim());
                                        break;
                                        //#############################################################
                                    }

                            } //end of switch
                        }  //end of if

                    }  //end of foreach



                     
                    if (name.EndsWith(":"))
                        typeStr = "option";


                    if (typeStr.ToLower() == "cmd")
                    {
                        if (name != string.Empty)
                        {
                            if (_allCmdList.ContainsKey(name) == false)
                                _allCmdList.Add(name, string.Empty);

                            if (_allCmdList.ContainsKey(name.ToLower()) == false)
                                _allCmdList.Add(name.ToLower(), string.Empty);
                        }


                        _currentCmdName = name;

                    }
                    else if (typeStr.ToLower() == "option")
                    {
                        if (name.TrimEnd(':') != string.Empty)
                        {
                            if (_allOptionList.ContainsKey(name.TrimEnd(':')) == false)
                                _allOptionList.Add(name.TrimEnd(':'), string.Empty);

                            if (_allOptionList.ContainsKey(name.TrimEnd(':').ToLower()) == false)
                                _allOptionList.Add(name.TrimEnd(':').ToLower(), string.Empty);
                        }

                        int icon_image_index = 4;

                        if (!string.IsNullOrEmpty(imageStr))
                        {
                            try
                            {
                                int image_index = int.Parse(imageStr);
                                icon_image_index = image_index;
                            }
                            catch { }
                        }

                        continue;
                    }

                    if (line.Trim().StartsWith("inserttab") && !string.IsNullOrEmpty(tabtext))
                    {
                        tabControl2.TabPages.Insert(0, engStr);


                            if (textStr == string.Empty)
                            {
                                    cur_tab_name = engStr;
                            }
                            else
                            {
                                    cur_tab_name = engStr;
                            }



                        ListView listView = new ListView();
                        listView.Name = tabtext;
                        listView.Dock = DockStyle.Fill;
                        listView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        listView.BorderStyle = BorderStyle.None;
                        listView.View = System.Windows.Forms.View.Tile;
                        listView.ShowItemToolTips = true;
                        listView.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(listView_GiveFeedback);
                        listView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(listView_ItemDrag);
                        //listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(listView_MouseDoubleClick);
                        listView.ImeMode = ImeMode.Alpha;

                        listView.LargeImageList = _gpl_ImageList;

                        listView.Columns.AddRange(new ColumnHeader[] { new ColumnHeader() });

                        _listViewList.Add(listView);
                        _curListView = listView;

                        tabControl2.TabPages[0].Controls.Add(listView);
                    }
                    else if (line.Trim().StartsWith("addtab") && !string.IsNullOrEmpty(tabtext))
                    {
                        if (textStr == string.Empty)
                        {
                                tabControl1.TabPages.Add(engStr);
                        }
                        else
                        {
                                tabControl1.TabPages.Add(engStr);
                        }


                        cur_tab_name = tabtext;


                        ListView listView = new ListView();
                        listView.Name = tabtext;
                        listView.Dock = DockStyle.Fill;
                        listView.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        listView.BorderStyle = BorderStyle.None;
                        listView.View = System.Windows.Forms.View.Tile;
                        listView.ShowItemToolTips = true;
                        listView.GiveFeedback += new System.Windows.Forms.GiveFeedbackEventHandler(listView_GiveFeedback);
                        listView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(listView_ItemDrag);
                        //listView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(listView_MouseDoubleClick);
                        listView.ImeMode = ImeMode.Alpha;

                        listView.LargeImageList = _gpl_ImageList;

                        listView.Columns.AddRange(new ColumnHeader[] { new ColumnHeader() });

                        _listViewList.Add(listView);
                        _curListView = listView;

                        tabControl1.TabPages[tabControl1.TabPages.Count - 1].Controls.Add(listView);
                    }
                    else if (line.Trim().StartsWith("addrootnode"))
                    {
                        if (_curListView != null)
                        {
                            string cur_group_name = string.Empty;

                            if (textStr == string.Empty)
                            {
                                    cur_group_name = engStr;
                            }
                            else
                            {
                                    cur_group_name = engStr;
                            }


                            _curListView.Groups.Add(new ListViewGroup(cur_group_name, HorizontalAlignment.Left));
                        }

                    }
                    else if (line.Trim().StartsWith("addchildnode") || line.Trim().StartsWith("addnode"))
                    {

                        if (_curListView != null)
                        {
                            int icon_image_index = 4;

                            if (!string.IsNullOrEmpty(imageStr))
                            {
                                try
                                {
                                    int image_index = int.Parse(imageStr);
                                    icon_image_index = image_index;
                                }
                                catch { }
                            }


                            if (!string.IsNullOrEmpty(textStr))
                            {
                                ListViewItem new_item = GetListViewItem(engStr, tooltiptext, name, icon_image_index);
                                new_item.ToolTipText = tooltiptext;
                                _curListView.Items.Add(new_item);
                            }
                            else
                            {
                                ListViewItem new_item = GetListViewItem(name, tooltiptext, name, icon_image_index);
                                new_item.ToolTipText = tooltiptext;
                                _curListView.Items.Add(new_item);
                            }

                        }

                    }
                    else if (line.Trim().StartsWith("toolmenuitem"))
                    {
                        if (name != string.Empty && textStr != string.Empty)
                            menuNameList.Add(name, textStr);
                    }
                    else if (line.Trim().StartsWith("directirytabitem"))
                    {
                        if (name != string.Empty && textStr != string.Empty)
                            dirNameList.Add(name, textStr);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("[ExecuteScriptList #2] It can't load menu script file. " + ex.ToString());
            }

        }


        private ListViewItem GetListViewItem(string c1, string c2, string c3, int image_index)
        {
            ListViewItem list = new ListViewItem(new string[] { c1, c2, c3 }, image_index);

            if (_curListView != null && _curListView.Groups.Count > 0)
                list.Group = _curListView.Groups[_curListView.Groups.Count - 1];

            list.ToolTipText = c2;

            return list;
        }



        private void listView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            ListView listView = (ListView)sender;

            if (e.Button == MouseButtons.Left)
            {
                ListViewItem dr_item = (ListViewItem)e.Item;

                if (dr_item.SubItems.Count >= 3)
                {
                    if (listView.SelectedItems.Count > 0 && customTabControl1.CurCmdEditorClass != null)
                        listView.DoDragDrop(dr_item.SubItems[2].Text, DragDropEffects.Copy | DragDropEffects.All);  
                }
            }
        }

        private void listView_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            
        }



        void ApplyCustomMenuList(Hashtable menuNameList, Hashtable dirNameList)
        {
            try
            {
                
            }
            catch
            {
                //
            }
        }



        private void LauncheService(string batchRunFileName, SPLCommon.SPLEngineLaunchMode execute_mode)
        {
            if (execute_mode == SPLCommon.SPLEngineLaunchMode.Default)
                Cursor.Current = Cursors.WaitCursor;

            try
            {
                string backUp_currentFile = _currentFile;


                if (!Directory.Exists(_spl_exec_path + "UserData\\Temp"))
                    Directory.CreateDirectory(_spl_exec_path + "UserData\\Temp");

                if (!Directory.Exists(_spl_exec_path + "BlockDiagram"))
                    Directory.CreateDirectory(_spl_exec_path + "BlockDiagram");

                foreach (string file in Directory.GetFiles(_spl_exec_path + "UserData\\Temp"))
                {
                    File.Delete(file);
                }



                #region batchRunFileName == string.Empty

                string cur_file_path = string.Empty;

                if (customTabControl1.CurCmdEditorClass != null)
                    cur_file_path = customTabControl1.CurCmdEditorClass.FilePath;


                button3_Click(null, null);

                cur_file_path = string.Empty;

                if (customTabControl1.CurCmdEditorClass != null)
                    cur_file_path = customTabControl1.CurCmdEditorClass.FilePath;


                if (string.IsNullOrEmpty(cur_file_path))
                {
                    MessageBox.Show("It can't be saved.");
                    return;
                }


                if (customTabControl1.CurCmdEditorClass != null)
                    _currentFile = customTabControl1.CurCmdEditorClass.FilePath;

                #endregion batchRunFileName == string.Empty


                if (File.Exists(_currentFile))
                {
                    AddToRecentFileList(_currentFile, string.Empty);
                }




                SPLConsole console_form = new SPLConsole(_Serial_Port_List);

                console_form._currentInstalledVersion = _currentInstalledVersion;


                string com_name = HelloApps.Common.ParsingHelper.GetFirstToken(comboBox2.Text);
                console_form._User_ComPort = com_name;

                console_form._User_Baudrate = comboBox3.Text;


                string part1_text = GetTextLinesFromResource("Sketch_Part1.txt");
                string part2_text = GetTextLinesFromResource("Sketch_Part2.txt");
                console_form.SetSketchParts(part1_text, part2_text);

                string file_ext = Path.GetExtension(_currentFile).ToLower();

                //###########################################################################################

                bool has_setup = customTabControl1.CurCmdEditorClass.ArduinoMainScript.Contains("setup");
                bool has_loop = customTabControl1.CurCmdEditorClass.ArduinoMainScript.Contains("loop");

                if (string.IsNullOrEmpty(customTabControl1.CurCmdEditorClass.ArduinoMainScript) || has_setup == false || has_loop == false)
                {
                    string default_line = Environment.NewLine;
                    default_line += "void setup()" + Environment.NewLine;
                    default_line += "{" + Environment.NewLine;
                    default_line += "\tSerial.begin(115200);" + Environment.NewLine;
                    default_line += "}" + Environment.NewLine;

                    default_line += Environment.NewLine;

                    default_line += "void loop()" + Environment.NewLine;
                    default_line += "{" + Environment.NewLine;
                    default_line += "\t" + Environment.NewLine;
                    default_line += "}" + Environment.NewLine;

                    customTabControl1.CurCmdEditorClass.ArduinoMainScript = default_line;
                }


                //###########################################################################################


                string plc_script = customTabControl1.CurCmdEditorClass.ArduinoMainScript + Environment.NewLine + Environment.NewLine;


                List<string> INPUT_VAR_LIST = new List<string>();
                List<string> OUTPUT_VAR_LIST = new List<string>();
                List<string> M_VAR_LIST = new List<string>();
                List<string> TIMER_VAR_LIST = new List<string>();
                List<string> COUNTER_VAR_LIST = new List<string>();
                Dictionary<string, string> TIMER_VALUE_LIST = new Dictionary<string, string>();
                Dictionary<string, string> COUNTER_VALUE_LIST = new Dictionary<string, string>();

                List<string> SET_VAR_LIST = new List<string>();
                List<string> RESET_VAR_LIST = new List<string>();

                List<string> INPUT_PIN_LIST = new List<string>();
                List<string> INPUT_SCRIPT_DEF_LIST = new List<string>();
                List<string> INPUT_SCRIPT_LIST = new List<string>();


                List<string> OUTPUT_PIN_LIST = new List<string>();
                List<string> OUTPUT_SCRIPT_DEF_LIST = new List<string>();
                List<string> OUTPUT_SCRIPT_LIST = new List<string>();

                List<string> INPUT_PINMODE_LIST = new List<string>();
                List<string> OUTPUT_PINMODE_LIST = new List<string>();



                Dictionary<string, string> CREATED_VAR_LIST = new Dictionary<string, string>();



                List<string> PLC_LOOP_SCRIPT_LIST = GetArduinoStringFromPLC(
                    INPUT_VAR_LIST, OUTPUT_VAR_LIST, M_VAR_LIST, TIMER_VAR_LIST, COUNTER_VAR_LIST,
                    TIMER_VALUE_LIST, COUNTER_VALUE_LIST,
                    INPUT_PIN_LIST, INPUT_SCRIPT_DEF_LIST, INPUT_SCRIPT_LIST,
                    OUTPUT_PIN_LIST, OUTPUT_SCRIPT_DEF_LIST, OUTPUT_SCRIPT_LIST,
                    SET_VAR_LIST, RESET_VAR_LIST, INPUT_PINMODE_LIST, OUTPUT_PINMODE_LIST);


                string plc_loop_lines = string.Empty;
                for (int i = 0; i < PLC_LOOP_SCRIPT_LIST.Count; i++)
                {
                    plc_loop_lines += PLC_LOOP_SCRIPT_LIST[i] + Environment.NewLine;
                }


                string input_var_lines = string.Empty;
                for (int i = 0; i < INPUT_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(INPUT_VAR_LIST[i]))
                    {
                        input_var_lines = input_var_lines + "int " + INPUT_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(INPUT_VAR_LIST[i], "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(INPUT_VAR_LIST[i] + "_set"))
                    {
                        input_var_lines = input_var_lines + "int " + INPUT_VAR_LIST[i] + "_set" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(INPUT_VAR_LIST[i] + "_set", "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(INPUT_VAR_LIST[i] + "_count"))
                    {
                        input_var_lines = input_var_lines + "int " + INPUT_VAR_LIST[i] + "_count" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(INPUT_VAR_LIST[i] + "_count", "");
                    }
                }

                string output_var_lines = string.Empty;
                for (int i = 0; i < OUTPUT_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(OUTPUT_VAR_LIST[i]))
                    {
                        output_var_lines = output_var_lines + "int " + OUTPUT_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(OUTPUT_VAR_LIST[i], "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(OUTPUT_VAR_LIST[i] + "_set"))
                    {
                        output_var_lines = output_var_lines + "int " + OUTPUT_VAR_LIST[i] + "_set" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(OUTPUT_VAR_LIST[i] + "_set", "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(OUTPUT_VAR_LIST[i] + "_count"))
                    {
                        output_var_lines = output_var_lines + "int " + OUTPUT_VAR_LIST[i] + "_count" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(OUTPUT_VAR_LIST[i] + "_count", "");
                    }
                }


                string set_var_lines = string.Empty;
                for (int i = 0; i < SET_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(SET_VAR_LIST[i]))
                    {
                        set_var_lines = set_var_lines + "int " + SET_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(SET_VAR_LIST[i], "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(SET_VAR_LIST[i] + "_set"))
                    {
                        set_var_lines = set_var_lines + "int " + SET_VAR_LIST[i] + "_set = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(SET_VAR_LIST[i] + "_set", "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(SET_VAR_LIST[i] + "_count"))
                    {
                        set_var_lines = set_var_lines + "int " + SET_VAR_LIST[i] + "_count = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(SET_VAR_LIST[i] + "_count", "");
                    }
                }


                string reset_var_lines = string.Empty;
                for (int i = 0; i < RESET_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(RESET_VAR_LIST[i]))
                    {
                        reset_var_lines = reset_var_lines + "int " + RESET_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(RESET_VAR_LIST[i], "");
                    }


                    if (!CREATED_VAR_LIST.ContainsKey(RESET_VAR_LIST[i] + "_set"))
                    {
                        reset_var_lines = reset_var_lines + "int " + RESET_VAR_LIST[i] + "_set = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(RESET_VAR_LIST[i] + "_set", "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(RESET_VAR_LIST[i] + "_count"))
                    {
                        reset_var_lines = reset_var_lines + "int " + RESET_VAR_LIST[i] + "_count = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(RESET_VAR_LIST[i] + "_count", "");
                    }
                }



                string m_var_lines = string.Empty;
                for (int i = 0; i < M_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(M_VAR_LIST[i]))
                    {
                        m_var_lines = m_var_lines + "int " + M_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(M_VAR_LIST[i], "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(M_VAR_LIST[i] + "_set"))
                    {
                        m_var_lines = m_var_lines + "int " + M_VAR_LIST[i] + "_set" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(M_VAR_LIST[i] + "_set", "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(M_VAR_LIST[i] + "_count"))
                    {
                        m_var_lines = m_var_lines + "int " + M_VAR_LIST[i] + "_count" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(M_VAR_LIST[i] + "_count", "");
                    }
                }

                string timer_var_lines = string.Empty;
                for (int i = 0; i < TIMER_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(TIMER_VAR_LIST[i]))
                    {
                        timer_var_lines = timer_var_lines + "int " + TIMER_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        timer_var_lines = timer_var_lines + "unsigned long " + TIMER_VAR_LIST[i] + "_last_chk_time = 0;" + Environment.NewLine;
                        timer_var_lines = timer_var_lines + "unsigned long " + TIMER_VAR_LIST[i] + "_target_time = " + TIMER_VALUE_LIST[TIMER_VAR_LIST[i]] + ";" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(TIMER_VAR_LIST[i], "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(TIMER_VAR_LIST[i] + "_set"))
                    {
                        timer_var_lines = timer_var_lines + "int " + TIMER_VAR_LIST[i] + "_set" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(TIMER_VAR_LIST[i] + "_set", "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(TIMER_VAR_LIST[i] + "_count"))
                    {
                        timer_var_lines = timer_var_lines + "int " + TIMER_VAR_LIST[i] + "_count" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(TIMER_VAR_LIST[i] + "_count", "");
                    }
                }

                string counter_var_lines = string.Empty;
                for (int i = 0; i < COUNTER_VAR_LIST.Count; i++)
                {
                    if (!CREATED_VAR_LIST.ContainsKey(COUNTER_VAR_LIST[i]))
                    {
                        counter_var_lines = counter_var_lines + "int " + COUNTER_VAR_LIST[i] + " = 0;" + Environment.NewLine;
                        counter_var_lines = counter_var_lines + "int " + COUNTER_VAR_LIST[i] + "_pre_status = 0;" + Environment.NewLine;
                        counter_var_lines = counter_var_lines + "int " + COUNTER_VAR_LIST[i] + "_count = 0;" + Environment.NewLine;
                        counter_var_lines = counter_var_lines + "int " + COUNTER_VAR_LIST[i] + "_target_count = " + COUNTER_VALUE_LIST[COUNTER_VAR_LIST[i]] + ";" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(COUNTER_VAR_LIST[i], "");
                    }

                    if (!CREATED_VAR_LIST.ContainsKey(COUNTER_VAR_LIST[i] + "_set"))
                    {
                        counter_var_lines = counter_var_lines + "int " + COUNTER_VAR_LIST[i] + "_set" + " = 0;" + Environment.NewLine;
                        CREATED_VAR_LIST.Add(COUNTER_VAR_LIST[i] + "_set", "");
                    }
                }


                string input_script_def_lines = string.Empty;
                for (int i = 0; i < INPUT_SCRIPT_DEF_LIST.Count; i++)
                {
                    input_script_def_lines = input_script_def_lines + INPUT_SCRIPT_DEF_LIST[i] + Environment.NewLine + Environment.NewLine;
                }

                string input_pin_lines = string.Empty;
                for (int i = 0; i < INPUT_PIN_LIST.Count; i++)
                {
                    input_pin_lines = input_pin_lines + INPUT_PIN_LIST[i] + Environment.NewLine;
                }

                string input_script_lines = string.Empty;
                for (int i = 0; i < INPUT_SCRIPT_LIST.Count; i++)
                {
                    input_script_lines = input_script_lines + INPUT_SCRIPT_LIST[i] + Environment.NewLine;
                }



                string output_script_def_lines = string.Empty;
                for (int i = 0; i < OUTPUT_SCRIPT_DEF_LIST.Count; i++)
                {
                    output_script_def_lines = output_script_def_lines + OUTPUT_SCRIPT_DEF_LIST[i] + Environment.NewLine + Environment.NewLine;
                }

                string output_pin_lines = string.Empty;
                for (int i = 0; i < OUTPUT_PIN_LIST.Count; i++)
                {
                    output_pin_lines = output_pin_lines + OUTPUT_PIN_LIST[i] + Environment.NewLine;
                }

                string output_script_lines = string.Empty;
                for (int i = 0; i < OUTPUT_SCRIPT_LIST.Count; i++)
                {
                    output_script_lines = output_script_lines + OUTPUT_SCRIPT_LIST[i] + Environment.NewLine;
                }


                plc_script += Environment.NewLine;

                plc_script = plc_script + "void plc_setup()" + Environment.NewLine;
                plc_script = plc_script + "{" + Environment.NewLine;

                for (int i = 0; i < INPUT_PINMODE_LIST.Count; i++)
                {
                    plc_script = plc_script + "\t" + "pinMode(" + INPUT_PINMODE_LIST[i] + ", INPUT);" + Environment.NewLine;
                }

                for (int i = 0; i < OUTPUT_PINMODE_LIST.Count; i++)
                {
                    plc_script = plc_script + "\t" + "pinMode(" + OUTPUT_PINMODE_LIST[i] + ", OUTPUT);" + Environment.NewLine;
                }

                plc_script = plc_script + "}" + Environment.NewLine + Environment.NewLine;


                plc_script += input_var_lines;
                plc_script += output_var_lines;
                plc_script += m_var_lines;
                plc_script += set_var_lines;
                plc_script += reset_var_lines;
                plc_script += timer_var_lines;
                plc_script += counter_var_lines;

                plc_script += Environment.NewLine;
                plc_script += input_script_def_lines;
                plc_script += output_script_def_lines;


                plc_script += Environment.NewLine;
                plc_script += "void plc_loop()" + Environment.NewLine;
                plc_script += "{" + Environment.NewLine;
                plc_script += input_pin_lines + Environment.NewLine;
                plc_script += input_script_lines + Environment.NewLine;

                plc_script += Environment.NewLine;
                plc_script += plc_loop_lines + Environment.NewLine;

                plc_script += Environment.NewLine;
                plc_script += output_pin_lines + Environment.NewLine;
                plc_script += output_script_lines + Environment.NewLine;

                plc_script += "}" + Environment.NewLine;


                string temp_plc_path = _spl_exec_path + "UserData/Temp/" + Guid.NewGuid().ToString() + ".txt";



                if (File.Exists(temp_plc_path))
                    File.Delete(temp_plc_path);

                File.WriteAllText(temp_plc_path, plc_script);

                //###########################################################################################



                //console_form.ExecuteScriptFile(_currentFile);
                console_form.ExecuteScriptFile(temp_plc_path);
                console_form.ShowDialog();


                _currentFile = backUp_currentFile;

            }
            catch (Exception ex)
            {
                MessageBox.Show("[Form LauncheService] " + ex.ToString());
            }


            Cursor.Current = Cursors.Default;
        }



        private List<string> GetArduinoStringFromPLC(List<string> INPUT_VAR_LIST, List<string> OUTPUT_VAR_LIST, 
            List<string> M_VAR_LIST, List<string> TIMER_VAR_LIST, List<string> COUNTER_VAR_LIST,
            Dictionary<string, string> TIMER_VALUE_LIST, Dictionary<string, string> COUNTER_VALUE_LIST,
            List<string> INPUT_PIN_LIST, List<string> INPUT_SCRIPT_DEF_LIST, List<string> INPUT_SCRIPT_LIST,
            List<string> OUTPUT_PIN_LIST, List<string> OUTPUT_SCRIPT_DEF_LIST, List<string> OUTPUT_SCRIPT_LIST,
            List<string> SET_VAR_LIST, List<string> RESET_VAR_LIST, List<string> INPUT_PINMODE_LIST, List<string> OUTPUT_PINMODE_LIST)
        {
            List<string> plc_script_list = new List<string>();

            if (customTabControl1.CurCmdEditorClass == null)
                return plc_script_list;


            for (int y = 0; y < customTabControl1.CurCmdEditorClass.LadderYSize; y++)
            {
                for (int x = 0; x < customTabControl1.CurCmdEditorClass.LadderXSize; x++)
                {
                    customTabControl1.CurCmdEditorClass.LadderCellList[y, x].IsCompiled = false;

                    HelloApps.GUI.LadderCellItem cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[y, x];

                    if (cell_item.CmdType == "InputA" || cell_item.CmdType == "InputB")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!INPUT_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                INPUT_VAR_LIST.Add(cell_item.CmdName);
                            }

                            if (cell_item.ArduinoMappingMode == "SCRIPT")
                            {
                                if (!string.IsNullOrEmpty(cell_item.ArduinoCellScript))
                                {
                                    string func_name = "cell_func_" + cell_item.Cell_ID;

                                    string func_lines = "int " + func_name + "()" + Environment.NewLine;
                                    func_lines += "{" + Environment.NewLine;
                                    func_lines += cell_item.ArduinoCellScript + Environment.NewLine;
                                    func_lines += "}" + Environment.NewLine;

                                    INPUT_SCRIPT_DEF_LIST.Add(func_lines);
                                    INPUT_SCRIPT_LIST.Add(cell_item.CmdName + " = " + func_name + "();");
                                }  
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(cell_item.ArduinoPin))
                                {
                                    INPUT_PIN_LIST.Add(cell_item.CmdName + " = digitalRead(" + cell_item.ArduinoPin + ");");
                                    INPUT_PINMODE_LIST.Add(cell_item.ArduinoPin);
                                }                                
                            }
                        }
                    }
                    else if (cell_item.CmdType == "Output")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!OUTPUT_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                OUTPUT_VAR_LIST.Add(cell_item.CmdName);
                            }

                            if (cell_item.ArduinoMappingMode == "SCRIPT")
                            {
                                if (!string.IsNullOrEmpty(cell_item.ArduinoCellScript))
                                {
                                    string func_name = "cell_func_" + cell_item.Cell_ID;

                                    string func_lines = "void " + func_name + "()" + Environment.NewLine;
                                    func_lines += "{" + Environment.NewLine;
                                    func_lines += cell_item.ArduinoCellScript + Environment.NewLine;
                                    func_lines += "}" + Environment.NewLine;

                                    OUTPUT_SCRIPT_DEF_LIST.Add(func_lines);
                                    OUTPUT_SCRIPT_LIST.Add(func_name + "();");
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(cell_item.ArduinoPin))
                                {
                                    OUTPUT_PIN_LIST.Add("digitalWrite(" + cell_item.ArduinoPin + ", " + cell_item.ArduinoPin + ");");
                                    OUTPUT_PINMODE_LIST.Add(cell_item.ArduinoPin);
                                }
                            }
                        }
                    }
                    else if (cell_item.CmdType == "MInputA" || cell_item.CmdType == "MInputB")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!M_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                M_VAR_LIST.Add(cell_item.CmdName);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "MOutput")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!M_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                M_VAR_LIST.Add(cell_item.CmdName);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "TimerInputA" || cell_item.CmdType == "TimerInputB")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!TIMER_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                TIMER_VAR_LIST.Add(cell_item.CmdName);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "CounterInputA" || cell_item.CmdType == "CounterInputB")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!COUNTER_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                COUNTER_VAR_LIST.Add(cell_item.CmdName);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "TimerOutput")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!TIMER_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                TIMER_VAR_LIST.Add(cell_item.CmdName);
                                TIMER_VALUE_LIST.Add(cell_item.CmdName, cell_item.TimerValue);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "CounterOutput")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!COUNTER_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                COUNTER_VAR_LIST.Add(cell_item.CmdName);
                                COUNTER_VALUE_LIST.Add(cell_item.CmdName, cell_item.CounterValue);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "SET")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!SET_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                SET_VAR_LIST.Add(cell_item.CmdName);
                            }
                        }
                    }
                    else if (cell_item.CmdType == "RESET")
                    {
                        if (!string.IsNullOrEmpty(cell_item.CmdName))
                        {
                            if (!RESET_VAR_LIST.Contains(cell_item.CmdName))
                            {
                                RESET_VAR_LIST.Add(cell_item.CmdName);
                            }
                        }
                    }

                
                }
            }



            for (int cur_cell_y = 0; cur_cell_y < customTabControl1.CurCmdEditorClass.LadderYSize; cur_cell_y++)
            {
                string line_cmd = "true";

                for (int cur_cell_x = 0; cur_cell_x < customTabControl1.CurCmdEditorClass.LadderXSize; cur_cell_x++)
                {
                    HelloApps.GUI.LadderCellItem cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[cur_cell_y, cur_cell_x];

                    if (!cell_item.IsCompiled)
                    {
                        cell_item.IsCompiled = true;

                        if (cell_item.CmdType == "InputA" || cell_item.CmdType == "InputB"
                            || cell_item.CmdType == "MInputA" || cell_item.CmdType == "MInputB"
                            || cell_item.CmdType == "TimerInputA" || cell_item.CmdType == "TimerInputB"
                            || cell_item.CmdType == "CounterInputA" || cell_item.CmdType == "CounterInputB")
                        {
                            line_cmd = line_cmd + " && " + cell_item.CmdName;
                        }
                        else if (cell_item.CmdType == "VLine")
                        {
                            List<string> sub_lines = new List<string>();
                            HelloApps.GUI.LadderCellItem dumy_cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[cur_cell_y, cur_cell_x];
                            GetSubLineForDownCell(sub_lines, dumy_cell_item);

                            if (sub_lines.Count > 0)
                            {
                                string sub_line = string.Empty;

                                for (int s = 0; s < sub_lines.Count; s++)
                                {
                                    sub_line = sub_line + " || (" + sub_lines[s] + ") ";
                                }

                                line_cmd = "((" + line_cmd + ") " + sub_line + ") ";
                            }
                        }
                        else if (cell_item.CmdType == "Output" || cell_item.CmdType == "MOutput")
                        {
                            if (line_cmd != "true")
                            {
                                line_cmd = "if (" + line_cmd + ") " + System.Environment.NewLine;
                                line_cmd = line_cmd + "{" + System.Environment.NewLine;
                                line_cmd = line_cmd + cell_item.CmdName + " = 1;" + System.Environment.NewLine;
                                line_cmd = line_cmd + "}" + System.Environment.NewLine;
                                line_cmd = line_cmd + "else" + System.Environment.NewLine;
                                line_cmd = line_cmd + "{" + System.Environment.NewLine;

                                line_cmd = line_cmd + "     if (" + cell_item.CmdName + "_set == 0)" + System.Environment.NewLine;
                                line_cmd = line_cmd + "     {" + System.Environment.NewLine;
                                line_cmd = line_cmd +           cell_item.CmdName + " = 0;" + System.Environment.NewLine;
                                line_cmd = line_cmd + "     }" + System.Environment.NewLine;

                                line_cmd = line_cmd + "}" + System.Environment.NewLine;

                                plc_script_list.Add(line_cmd);
                            }
                        }
                        else if (cell_item.CmdType == "SET")
                        {
                            if (line_cmd != "true")
                            {
                                line_cmd = "if (" + line_cmd + ") " + System.Environment.NewLine;
                                line_cmd = line_cmd + "{" + System.Environment.NewLine;
                                line_cmd = line_cmd + cell_item.CmdName + "_set = 1;" + System.Environment.NewLine;
                                line_cmd = line_cmd + "}" + System.Environment.NewLine;

                                plc_script_list.Add(line_cmd);
                            }
                        }
                        else if (cell_item.CmdType == "RESET")
                        {
                            if (line_cmd != "true")
                            {
                                line_cmd = "if (" + line_cmd + ") " + System.Environment.NewLine;
                                line_cmd = line_cmd + "{" + System.Environment.NewLine;
                                line_cmd = line_cmd + cell_item.CmdName + "_set = 0;" + System.Environment.NewLine;
                                line_cmd = line_cmd + cell_item.CmdName + "_count = 0;" + System.Environment.NewLine;
                                line_cmd = line_cmd + "}" + System.Environment.NewLine;

                                plc_script_list.Add(line_cmd);
                            }
                        }
                        else if (cell_item.CmdType == "TimerOutput")
                        {
                            if (line_cmd != "true")
                            {
                                line_cmd = "if (" + line_cmd + ") " + System.Environment.NewLine;
                                line_cmd += line_cmd + "{" + System.Environment.NewLine;

                                line_cmd += "     unsigned long cur_time = millis();" + System.Environment.NewLine;
                                line_cmd += "     unsigned long diff_time = cur_time - " + cell_item.CmdName + "_last_chk_time;" + System.Environment.NewLine;
                                line_cmd += "     if (diff_time > " + cell_item.CmdName + "_target_time)" + System.Environment.NewLine;
                                line_cmd += "     {" + System.Environment.NewLine;
                                line_cmd +=            cell_item.CmdName + " = 1;" + System.Environment.NewLine;
                                line_cmd += "     }" + System.Environment.NewLine;
                                line_cmd += "     else" + System.Environment.NewLine;
                                line_cmd += "     {" + System.Environment.NewLine;
                                line_cmd +=            cell_item.CmdName + " = 0;" + System.Environment.NewLine;
                                line_cmd += "     }" + System.Environment.NewLine;

                                line_cmd += "}" + System.Environment.NewLine;
                                line_cmd += "else" + System.Environment.NewLine;
                                line_cmd += "{" + System.Environment.NewLine;
                                line_cmd += cell_item.CmdName + " = 0;" + System.Environment.NewLine;
                                line_cmd += cell_item.CmdName + "_last_chk_time = millis();" + System.Environment.NewLine;
                                line_cmd += "}" + System.Environment.NewLine;

                                plc_script_list.Add(line_cmd);
                            }
                        }
                        else if (cell_item.CmdType == "CounterOutput")
                        {
                            if (line_cmd != "true")
                            {
                                line_cmd = "if (" + line_cmd + ") " + System.Environment.NewLine;
                                line_cmd += "{" + System.Environment.NewLine;

                                line_cmd += "   if (" + cell_item.CmdName + "_pre_status == 0)"+ System.Environment.NewLine;
                                line_cmd += "   {" + System.Environment.NewLine;
                                line_cmd +=         cell_item.CmdName + "_count++;" + System.Environment.NewLine;
                                line_cmd += "   }" + System.Environment.NewLine;

                                line_cmd += "   if (" + cell_item.CmdName + "_count >= " + cell_item.CmdName + "_target_count)" + System.Environment.NewLine;
                                line_cmd += "   {" + System.Environment.NewLine;
                                line_cmd +=         cell_item.CmdName + " = 1;" + System.Environment.NewLine;
                                line_cmd += "   }" + System.Environment.NewLine;

                                line_cmd += "     else" + System.Environment.NewLine;
                                line_cmd += "     {" + System.Environment.NewLine;
                                line_cmd +=         cell_item.CmdName + " = 0;" + System.Environment.NewLine;
                                line_cmd += "     }" + System.Environment.NewLine;

                                line_cmd +=     cell_item.CmdName + "_pre_status = 1;" + System.Environment.NewLine;

                                line_cmd += "}" + System.Environment.NewLine;
                                line_cmd += "else" + System.Environment.NewLine;
                                line_cmd += "{" + System.Environment.NewLine;

                                line_cmd +=     cell_item.CmdName + "_pre_status = 0;" + System.Environment.NewLine;
                                line_cmd += "}" + System.Environment.NewLine;

                                plc_script_list.Add(line_cmd);
                            }
                        }
                    }


                }

                
            }

            return plc_script_list;
        }


        private void GetSubLineForDownCell(List<string> sub_lines, HelloApps.GUI.LadderCellItem dumy_cell_item)
        {            
            while (dumy_cell_item.HasDownLine)
            {
                dumy_cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[dumy_cell_item.CELL_Y + 1, dumy_cell_item.CELL_X];

                dumy_cell_item.IsCompiled = true;

                HelloApps.GUI.LadderCellItem pre_cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[dumy_cell_item.CELL_Y, dumy_cell_item.CELL_X - 1];

                if (!string.IsNullOrEmpty(pre_cell_item.CmdType))
                {
                    string sub_line = "true";

                    for (int x = 0; x <= dumy_cell_item.CELL_X - 1; x++)
                    {
                        HelloApps.GUI.LadderCellItem left_cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[dumy_cell_item.CELL_Y, x];

                        if (!left_cell_item.IsCompiled)
                        {
                            left_cell_item.IsCompiled = true;

                            if (left_cell_item.CmdType == "InputA" || left_cell_item.CmdType == "InputB"
                            || left_cell_item.CmdType == "MInputA" || left_cell_item.CmdType == "MInputB"
                            || left_cell_item.CmdType == "TimerInputA" || left_cell_item.CmdType == "TimerInputB"
                            || left_cell_item.CmdType == "CounterInputA" || left_cell_item.CmdType == "CounterInputB")
                            {
                                sub_line = sub_line + " && " + left_cell_item.CmdName;
                            }
                            else if (left_cell_item.CmdType == "VLine")
                            {
                                List<string> left_lines = new List<string>();
                                HelloApps.GUI.LadderCellItem dumy_cell_item2 = customTabControl1.CurCmdEditorClass.LadderCellList[left_cell_item.CELL_Y, left_cell_item.CELL_X];
                                GetSubLineForDownCell(left_lines, dumy_cell_item2);

                                if (left_lines.Count > 0)
                                {
                                    string left_line = string.Empty;

                                    for (int s = 0; s < left_lines.Count; s++)
                                    {
                                        left_line = left_line + " || (" + left_lines[s] + ") ";
                                    }

                                    sub_line = "((" + sub_line + ") " + left_line + ") ";
                                }
                            } 
                        }                        
                    }

                    if (sub_line != "true")
                    {
                        sub_lines.Add(sub_line);
                    }
                }
            }

        }


        

              

        private string GetDefaultSaveFileName(string file)
        {
            return GetDefaultSaveFileName("", file);
        }

        private string GetDefaultSaveFileName(string format, string file)
        {
            string format_name = "Diagram";

            if (string.IsNullOrEmpty(format))
                format_name = "Diagram";
            else
                format_name = format;

            string defaultStr = string.Empty;

            string dateStr = "Y" + DateTime.Now.Year.ToString();
            dateStr += ".M" + DateTime.Now.Month.ToString().PadLeft(2, '0');
            dateStr += ".D" + DateTime.Now.Day.ToString().PadLeft(2, '0');


            string fileName = Path.GetFileNameWithoutExtension(file);

            if (format_name == "Diagram")
            {
                if (fileName != string.Empty)
                    defaultStr = format_name + "_" + fileName + "_" + dateStr;
                else
                    defaultStr = format_name + "_" + dateStr;
            }
            else
            {
                defaultStr = fileName;
            }

            return defaultStr;
        }



        void OpenLastUsedScriptFile()
        {
            string lastScriptFile = string.Empty;

            if (listView4.Items.Count > 0)
                lastScriptFile = listView4.Items[0].SubItems[2].Text;


            if (lastScriptFile != string.Empty)
            {
                if (File.Exists(lastScriptFile))
                {
                    try
                    {
                        _currentFile = lastScriptFile;

                        label_status.Text = _currentFile;

                        AddNewTabPage(customTabControl1.GetTabTitle(_currentFile), _currentFile, string.Empty);

                        AddToRecentFileList(_currentFile, string.Empty);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
                else
                {
                    //
                }
            }
        }

   
        

        private void Form1_Resize(object sender, EventArgs e)
        {           
            int formhalfHeigth = ((panel2.Height - splitter2.Height) / 3) * 2;
            
            if (tabControl2.Height < 150)
            {
                tabControl1.Height = panel2.Height - 150 - splitter2.Height;
            }
            else if (tabControl1.Height < formhalfHeigth)
            {
                tabControl1.Height = formhalfHeigth + 100;
            }
        }        


        
        string SelectOpenFile(string initialDirectory, string filter, bool replaceBackSlash)
        {
            string res = string.Empty;

            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = initialDirectory;
            openFileDialog.FileName = "";
            openFileDialog.Filter = filter;

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string tempStr = openFileDialog.FileName;

                if (replaceBackSlash)
                    tempStr = tempStr.Replace('\\', '/');

                res = "\"" + tempStr + "\"";
            }

            return res;
        }




        private void listView4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            string targetname = listView4.SelectedItems[0].SubItems[2].Text;
            string extension = Path.GetExtension(targetname);

            label_status.Text = targetname;


            if (extension.ToLower() == ".txt")
            {
                AddNewTabPage(customTabControl1.GetTabTitle(targetname), targetname, string.Empty);

                _currentFile = targetname;

            }
            else if (extension.ToLower() == ".splx")
            {
                System.Diagnostics.ProcessStartInfo ps = new System.Diagnostics.ProcessStartInfo(targetname);
                ps.WorkingDirectory = Path.GetDirectoryName(targetname);

                try
                {
                    System.Diagnostics.Process.Start(ps);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void listView4_MouseClick(object sender, MouseEventArgs e)
        {
            string targetname = listView4.SelectedItems[0].SubItems[2].Text;

            label_status.Text = targetname;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try            
            {
                while (customTabControl1.TabPages.Count > 0)
                {
                    customTabControl1.TabPages.RemoveAt(0);
                }
            }
            catch
            {
            }
        }




        private void Form1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.Control)
            {
                if (e.KeyCode == Keys.N)
                {
                    AddNewTabPage("untitled", string.Empty, string.Empty);
                }
                
                else if (e.KeyCode == Keys.S)
                    button3_Click(null, null);
                else if (e.KeyCode == Keys.O)
                    button2_Click(null, null);
            }
            else if (e.KeyCode == Keys.F5)
            {
                button7_Click(null, null);
            }
        }



        private void customTabControl1_CloseButtonClick(object sender, EventArgs e)
        {
            try
            {
                bool closeFlag = true;

               
                if (closeFlag)
                {                    
                    if (customTabControl1.ChildPictureBox != null)
                    {
                        customTabControl1.ChildPictureBox.Dispose();
                    }

                    customTabControl1.RemoveCurrentTab();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void listBox_Console_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //
        }

        

        private void customTabControl1_DragDrop(object sender, DragEventArgs e)
        {
            AddNewTabPage("untitled", string.Empty, string.Empty);
        }

        private void customTabControl1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void barEditItem3_EditValueChanged(object sender, EventArgs e)
        {
            
        }

        private void barEditItem1_EditValueChanged(object sender, EventArgs e)
        {
            
            
        }

        private void barEditItem2_EditValueChanged(object sender, EventArgs e)
        {
            
            
        }


        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(_spl_exec_path + "Config"))
                Directory.CreateDirectory(_spl_exec_path + "Config");

            if (File.Exists(_spl_exec_path + "Config\\LastScriptType.txt"))
                File.Delete(_spl_exec_path + "Config\\LastScriptType.txt");                

            if (tabControl3.SelectedIndex == 1)
            {
                File.WriteAllText(_spl_exec_path + "Config\\LastScriptType.txt", "Sketch");
            }
            else
            {
                File.WriteAllText(_spl_exec_path + "Config\\LastScriptType.txt", "SPL");
            }
            
        }

        private void timer_util_Tick(object sender, EventArgs e)
        {
            timer_util.Enabled = false;

            if (_timer_util_target_list.Count > 0)
            {
                string target_name = _timer_util_target_list.Dequeue();

                if (target_name == "button7")
                    button7.Enabled = true;
            }

            if (_timer_util_target_time.Count > 0)
            {
                int dueTime = _timer_util_target_time.Dequeue();
                timer_util.Interval = dueTime;
                timer_util.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
        }


        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                label5.Enabled = false;
                textBox3.Enabled = false;

                label6.Enabled = true;
                richTextBox1.Enabled = true;

                if (string.IsNullOrEmpty(richTextBox1.Text))
                    richTextBox1.Text = "return 0;";
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                label5.Enabled = true;
                textBox3.Enabled = true;

                label6.Enabled = false;
                richTextBox1.Enabled = false;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Do you want to quit?", "Quit", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            catch
            {
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                SPLConsole console_form = new SPLConsole(_Serial_Port_List);

                string com_name = HelloApps.Common.ParsingHelper.GetFirstToken(comboBox2.Text);

                console_form._User_ComPort = com_name;

                console_form._User_Baudrate = comboBox3.Text;


                console_form.SetSerialMoniteringMode(true);
                console_form.ShowDialog();
            }
            catch { }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (_timer_util_target_list.Count > 0)
            {
                _timer_util_target_list.Enqueue("button7");
                _timer_util_target_time.Enqueue(3000);
            }
            else
            {
                button7.Enabled = false;
                _timer_util_target_list.Enqueue("button7");
                timer_util.Interval = 3000;
                timer_util.Enabled = true;
            }

            LauncheService(string.Empty, SPLCommon.SPLEngineLaunchMode.Default);   
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            if (!Directory.Exists(_spl_exec_path + "BlockDiagram"))
                Directory.CreateDirectory(_spl_exec_path + "BlockDiagram");

            if (customTabControl1.CurCmdEditorClass != null)
            {
                #region CurCmdEditorClass


                string spl_script = string.Empty;

                if (customTabControl1.CurCmdEditorClass != null)
                    spl_script = customTabControl1.CurCmdEditorClass.GetScriptOfChilds("");


                saveFileDialog.InitialDirectory = _spl_exec_path + "BlockDiagram";

                string cur_file_path = string.Empty;

                if (customTabControl1.CurCmdEditorClass != null)
                    cur_file_path = customTabControl1.CurCmdEditorClass.FilePath;

                if (string.IsNullOrEmpty(cur_file_path))
                    saveFileDialog.FileName = GetDefaultSaveFileName("");
                else
                {
                    saveFileDialog.InitialDirectory = Path.GetDirectoryName(cur_file_path);
                    saveFileDialog.FileName = Path.GetFileName(cur_file_path);
                }

                saveFileDialog.DefaultExt = "txt";
                saveFileDialog.Filter = "Txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 0;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if (customTabControl1.CurCmdEditorClass != null)
                    {
                        string ext_name = Path.GetExtension(saveFileDialog.FileName).ToLower();
                        File.WriteAllText(saveFileDialog.FileName, spl_script, Encoding.UTF8);
                    }

                    customTabControl1.CurCmdEditorClass.FilePath = saveFileDialog.FileName;

                    customTabControl1.CurCmdEditorClass.SetSavedFlag();

                    AddToRecentFileList(saveFileDialog.FileName, string.Empty);
                }

                #endregion CurRichTextBox



            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (customTabControl1.CurCmdEditorClass != null)
            {

                string spl_script = string.Empty;

                if (customTabControl1.CurCmdEditorClass != null)
                    spl_script = customTabControl1.CurCmdEditorClass.GetScriptOfChilds("");


                #region CurCmdEditorClass

                if (!string.IsNullOrEmpty(customTabControl1.CurCmdEditorClass.FilePath))
                {
                    #region 

                    string ext_name = Path.GetExtension(customTabControl1.CurCmdEditorClass.FilePath).ToLower();

                    File.WriteAllText(customTabControl1.CurCmdEditorClass.FilePath, spl_script, Encoding.UTF8);

                    customTabControl1.CurCmdEditorClass.SetSavedFlag();

                    AddToRecentFileList(customTabControl1.CurCmdEditorClass.FilePath, string.Empty);
                    #endregion 
                }
                else
                {

                    #region 

                    saveFileDialog.InitialDirectory = _spl_exec_path + "BlockDiagram";

                    string cur_file_path = string.Empty;

                    if (customTabControl1.CurCmdEditorClass != null)
                        cur_file_path = customTabControl1.CurCmdEditorClass.FilePath;

                    if (string.IsNullOrEmpty(cur_file_path))
                        saveFileDialog.FileName = GetDefaultSaveFileName("");
                    else
                        saveFileDialog.FileName = cur_file_path;

                    saveFileDialog.DefaultExt = "txt";
                    saveFileDialog.Filter = "Txt files (*.txt)|*.txt|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 0;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        if (customTabControl1.CurCmdEditorClass != null)
                        {
                            string ext_name = Path.GetExtension(saveFileDialog.FileName).ToLower();

                            File.WriteAllText(saveFileDialog.FileName, spl_script, Encoding.UTF8);
                        }

                        customTabControl1.CurCmdEditorClass.FilePath = saveFileDialog.FileName;

                        customTabControl1.CurCmdEditorClass.SetSavedFlag();

                        AddToRecentFileList(saveFileDialog.FileName, string.Empty);
                    }

                    #endregion 

                }


                #endregion CurCmdEditorClass
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog.Multiselect = false;
                openFileDialog.InitialDirectory = _spl_exec_path + "BlockDiagram";
                openFileDialog.FileName = "";
                openFileDialog.Filter = "Txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _currentFile = openFileDialog.FileName;

                    label_status.Text = _currentFile;


                    AddNewTabPage(customTabControl1.GetTabTitle(_currentFile), openFileDialog.FileName, string.Empty);
                    AddToRecentFileList(_currentFile, string.Empty);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            AddNewTabPage("untitled", string.Empty, string.Empty);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int cell_x = HelloApps.GUI.GlovalCellVariable.CurSelectedCell_X;
            int cell_y = HelloApps.GUI.GlovalCellVariable.CurSelectedCell_Y;

            if (customTabControl1.CurCmdEditorClass != null)
            {
                if (HelloApps.GUI.GlovalCellVariable.CurSelectedCell == "_PARENT_")
                {
                    customTabControl1.CurCmdEditorClass.ArduinoMainScript = richTextBox1.Text;
                }
                else
                {
                    HelloApps.GUI.LadderCellItem ladder_cell_item = customTabControl1.CurCmdEditorClass.LadderCellList[cell_y, cell_x];

                    SaveCellInfo(ladder_cell_item);

                    customTabControl1.CurCmdEditorClass.UpdateCmdNameList(ladder_cell_item);
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string plc_type = comboBox1.Text;

            if (plc_type == "Mitsubishi" || plc_type == "Siemens")
            {
                MessageBox.Show(plc_type + " manufacturer not yet supported!");
                comboBox1.Text = "LSIS";
            }

        }

        private void comboBox2_Click(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();

            foreach (string port_name in HelloApps.Helper.SPLDuinoHelper.GetComPortNameList())
            {
                comboBox2.Items.Add(port_name);
            }
            
        }         

    }


}
