using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Diagnostics;

using System.Threading;

namespace HelloApps
{
    public partial class SPLConsole : Form
    {
        string _arduino_version_path = "arduino-1.6.6";

        string _script_file_path = string.Empty;
        string _user_file_path = string.Empty;

        string _stderr_str = string.Empty;
        string _stdout_str = string.Empty;

        string _sketch_part1 = string.Empty;
        string _sketch_part2 = string.Empty;
        public string _sketch_script = string.Empty;

        int _maxBuffer = 40960;
        int _max_Log_Count = 1000;

        bool _open_serial_monitoring_mode = false;
        bool _required_serial_monitoring = false;

        List<string> _user_defined_include_list = new List<string>();

        bool _form_close_flag = false;

        Queue<string> _log_info_queue = new Queue<string>();

        string _script_text = string.Empty;

        SerialPort _serialPort = new SerialPort();

        public List<string> _Serial_Port_List = null;

        public string _User_ComPort = string.Empty;
        public string _User_Baudrate = string.Empty;
        //public SPL.Common.BoardItem _Board_Item = null;

        public string _currentInstalledVersion = string.Empty;


        CircularQueue _CQ = new CircularQueue();


        string _global_debug_line = string.Empty;


        public SPLConsole()
        {
            InitializeComponent();

            string amy_docu_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _script_file_path = amy_docu_path + "/ArduinoPLC/UserData/Temp/";
        }


        public SPLConsole(List<string> Serial_Port_List)
        {
            _Serial_Port_List = Serial_Port_List;

            InitializeComponent();

            string amy_docu_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            _script_file_path = amy_docu_path + "/ArduinoPLC/UserData/Temp/";   
        }



        public void Terminate()
        {
            try
            {
                if (_serialPort != null)
                {
                    try
                    {
                        CommClose();
                    }
                    catch { }

                    try
                    {
                        if (_serialPort != null)
                        {
                            try
                            {
                                _serialPort.Dispose();
                            }
                            catch { }


                            try
                            {
                                _serialPort = null;
                            }
                            catch { }
                        }

                    }
                    catch { }
                }
            }
            catch { }
        }


        private void SPLConsole_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;

            if (string.IsNullOrEmpty(_User_Baudrate))
                _User_Baudrate = "115200";

            comboBox1.Items.Clear();
            comboBox1.Items.Add("Send Text");
            comboBox1.Items.Add("Include Carriage Return");
            comboBox1.Items.Add("Send Bytes");


            if (string.IsNullOrEmpty(_User_ComPort))
                _User_ComPort = HelloApps.Helper.SPLDuinoHelper.GetSPLDuinoComPortName();


            if (!string.IsNullOrEmpty(_User_ComPort))
            {
                this.Text = "Console - Connect to " + _User_ComPort;
            }
            else
            {
                this.Text = "Console - None of Arduino Board";
            }




            timer_loginfo.Enabled = true;


            if (_open_serial_monitoring_mode == false)
            {
                timer_exec.Enabled = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(_User_ComPort))
                    timer_monitering.Enabled = true;
            }

            comboBox1.SelectedIndex = 2;
        }


        public void SetSerialMoniteringMode(bool mode)
        {
            _open_serial_monitoring_mode = mode;
        }

        public void SetSketchParts(string part1, string part2)
        {
            _sketch_part1 = part1;
            _sketch_part2 = part2;
        }

        

        private void Initializevariables()
        {
            _open_serial_monitoring_mode = false;
            _required_serial_monitoring = false;
            _user_defined_include_list.Clear();
        }

        public void ExecuteScriptFile(string file_path)
        {
            _user_file_path = file_path;

            Initializevariables();

            string[] part1_arr = null;
            string[] part2_arr = null;

            if (File.Exists(file_path))
            {
                string script = File.ReadAllText(file_path);                
                
                List<string> input_lines = new List<string>();
                FileToList(input_lines, file_path, Path.GetDirectoryName(file_path));

                part1_arr = _sketch_part1.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                part2_arr = _sketch_part2.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);


                List<string> script_lines = null;
                script_lines = PreProcessingSPL(input_lines.ToArray());


                string lines = string.Empty;
                string lines_with_number = string.Empty;

                int line_num = 1;


                if (part1_arr != null)
                {
                    for (int i = 0; i < part1_arr.Length; i++)
                    {
                        lines_with_number += line_num.ToString().PadLeft(3, '0') + "  " + part1_arr[i] + Environment.NewLine;
                        lines += part1_arr[i] + Environment.NewLine;

                        line_num++;
                    }
                }

                lines_with_number += line_num.ToString().PadLeft(3, '0') + "  " + Environment.NewLine;
                lines += Environment.NewLine;

                line_num++;


                _script_text = string.Empty;

                if (script_lines != null)
                {
                    for (int i = 0; i < script_lines.Count; i++)
                    {
                        lines_with_number += line_num.ToString().PadLeft(3, '0') + "  " + script_lines[i] + Environment.NewLine;
                        lines += script_lines[i] + Environment.NewLine;

                        _script_text += script_lines[i] + Environment.NewLine;

                        line_num++;
                    }
                }


                lines_with_number += line_num.ToString().PadLeft(3, '0') + "  " + Environment.NewLine;
                lines += Environment.NewLine;

                line_num++;


                if (part2_arr != null)
                {
                    for (int i = 0; i < part2_arr.Length; i++)
                    {
                        lines_with_number += line_num.ToString().PadLeft(3, '0') + "  " + part2_arr[i] + Environment.NewLine;
                        lines += part2_arr[i] + Environment.NewLine;

                        line_num++;
                    }
                }


                _sketch_script = lines;
                textBox_sketch.Text = lines_with_number;
            }
        }

        
        public void FileToList(List<string> lines, string scriptPath, string root_path)
        {
            #region check_scriptPath

            string filename_src = scriptPath;
            string weburl_src = string.Empty;


            if (!Directory.Exists(_script_file_path))
                Directory.CreateDirectory(_script_file_path);


            if (filename_src != string.Empty)
            {
                filename_src = filename_src.Replace("\\", "/");

                if (filename_src.StartsWith("/"))
                {
                    filename_src = root_path + filename_src;
                }
            }



            #endregion check_scriptPath


            if (filename_src == string.Empty || !File.Exists(filename_src))
                return;

            try
            {
                List<string> tmpLines = new List<string>();

                string[] readlines = File.ReadAllLines(filename_src);


                for (int i = 0; i < readlines.Length; i++)
                {
                    tmpLines.Add(readlines[i].TrimEnd(' '));
                }


                foreach (string singleline in tmpLines)
                {
                    string line = singleline;
                    lines.Add(line);
                }
            }
            catch (Exception ex)
            {
                LogInfo("[FileToList] " + ex.ToString());
            }

        }




        private List<string> PreProcessingSPL(string[] lines_raw)
        {
            
            List<string> res = new List<string>();

            try
            {
                if (lines_raw == null)
                    return res;

                if (lines_raw.Length == 0)
                    return res;

                for (int i = 0; i < lines_raw.Length; i++)
                    res.Add(lines_raw[i]);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

            return res;
        }


        void LogInfo(string msg)
        {
            try
            {
                if (_form_close_flag)
                    return;

                try
                {
                    if (_log_info_queue.Count < 10)
                    {
                        _log_info_queue.Enqueue(msg);
                    }
                }
                catch { }
            }
            catch { }
        }

        void LogInfoLine(string log)
        {
            try
            {
                if (_form_close_flag)
                    return;

                if (_log_info_queue.Count < 30)
                {
                    _log_info_queue.Enqueue(log + Environment.NewLine);
                }
            }
            catch { }
        }


        private void DebugLogInfoLine(string title, string log)
        {

        }


        private void timer_loginfo_Tick(object sender, EventArgs e)
        {
            timer_loginfo.Enabled = false;

            if (_form_close_flag)
            {
                try
                {
                    _log_info_queue.Clear();
                }
                catch { }

                return;
            }

            try
            {
                for (int i = 0; i < 20; i++)
                {
                    if (_log_info_queue.Count == 0)
                        break;

                    string msg = _log_info_queue.Dequeue();

                    LogInfo_Separate_Thread(msg);

                    Application.DoEvents(); Application.DoEvents(); Application.DoEvents(); Application.DoEvents();
                }
            }
            catch { }


            if (_form_close_flag == false)
                timer_loginfo.Enabled = true;
        }


        private void LogInfo_Separate_Thread(string state)
        {
            try
            {
                if (_form_close_flag)
                    return;

                if (state == null)
                    return;

                string msg = state;

                if (string.IsNullOrEmpty(msg))
                    return;

                string[] lines = msg.Split(new char[] { '\r' });

                if (lines == null)
                    return;

                try
                {
                    try
                    {
                        if (listBox1 == null)
                            return;
                    }
                    catch { }


                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];

                        try
                        {
                            if (i == 0)
                            {
                                try
                                {
                                    if (listBox1 == null)
                                        return;

                                    if (listBox1.Items.Count == 0)
                                        listBox1.Items.Add(line);
                                    else
                                        listBox1.Items[listBox1.Items.Count - 1] = listBox1.Items[listBox1.Items.Count - 1].ToString() + line;
                                }
                                catch { }
                            }
                            else
                                try
                                {
                                    if (listBox1 == null)
                                        return;

                                    listBox1.Items.Add(line);
                                }
                                catch { }
                        }
                        catch { }
                    }
                }
                catch { }

                try
                {
                    if (listBox1 == null)
                        return;

                    if (listBox1.Items.Count > _max_Log_Count)
                    {
                        for (int i = 0; i < 50; i++)
                            listBox1.Items.RemoveAt(0);
                    }
                }
                catch { }

                try
                {
                    if (listBox1 == null)
                        return;

                    try
                    {
                        listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    }
                    catch { }

                }
                catch { }
            }
            catch { }
        }



        private void DoEventsLoop(int cnt)
        {
            for (int i = 0; i < cnt; i++)
                Application.DoEvents();
        }


        private void UploadSketch()
        {
            try
            {
                string docu_arduino_lib_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                docu_arduino_lib_path = docu_arduino_lib_path + @"\Arduino\libraries";

                List<string> pre_deployed_library = new List<string>();
                
                pre_deployed_library.Add("Arduino");
                pre_deployed_library.Add("avr/interrupt");
                pre_deployed_library.Add("avr/io");
                pre_deployed_library.Add("avr/pgmspace");
                pre_deployed_library.Add("binary");
                pre_deployed_library.Add("CDC");
                pre_deployed_library.Add("Conceptinetics");
                pre_deployed_library.Add("ctype");
                pre_deployed_library.Add("Dhcp");
                pre_deployed_library.Add("dht11");
                pre_deployed_library.Add("Dns");
                pre_deployed_library.Add("EEPROM");
                pre_deployed_library.Add("Ethernet");
                pre_deployed_library.Add("EthernetClient");
                pre_deployed_library.Add("EthernetServer");
                pre_deployed_library.Add("EthernetUdp");
                pre_deployed_library.Add("File");
                pre_deployed_library.Add("HardwareSerial");
                pre_deployed_library.Add("HID");
                pre_deployed_library.Add("inttypes");
                pre_deployed_library.Add("IPAddress");
                pre_deployed_library.Add("LCDKeypad");
                pre_deployed_library.Add("LiquidCrystal");
                pre_deployed_library.Add("LiquidCrystal_I2C");
                pre_deployed_library.Add("math");
                pre_deployed_library.Add("new");
                pre_deployed_library.Add("pins_arduino");
                pre_deployed_library.Add("Print");
                pre_deployed_library.Add("Printable");
                pre_deployed_library.Add("SD");
                pre_deployed_library.Add("Sd2Card");
                pre_deployed_library.Add("SdFile");
                pre_deployed_library.Add("SdVolume");
                pre_deployed_library.Add("Servo");
                pre_deployed_library.Add("socket");
                pre_deployed_library.Add("SPI");
                pre_deployed_library.Add("stdint");
                pre_deployed_library.Add("stdio");
                pre_deployed_library.Add("stdlib");
                pre_deployed_library.Add("Stepper");
                pre_deployed_library.Add("Stream");
                pre_deployed_library.Add("string");
                pre_deployed_library.Add("Tone");
                pre_deployed_library.Add("twi");
                pre_deployed_library.Add("USBAPI");
                pre_deployed_library.Add("USBCore");
                pre_deployed_library.Add("w5100");
                pre_deployed_library.Add("WCharacter");
                pre_deployed_library.Add("WInterrupts");
                pre_deployed_library.Add("Wire");
                pre_deployed_library.Add("wiring");
                pre_deployed_library.Add("wiring_analog");
                pre_deployed_library.Add("wiring_digital");
                pre_deployed_library.Add("wiring_pulse");
                pre_deployed_library.Add("wiring_shift");
                pre_deployed_library.Add("WMath");
                pre_deployed_library.Add("WProgram");
                pre_deployed_library.Add("WS2801");
                pre_deployed_library.Add("WString");
                pre_deployed_library.Add("hooks");
                pre_deployed_library.Add("HardwareSerial0");
                pre_deployed_library.Add("HardwareSerial1");
                pre_deployed_library.Add("HardwareSerial2");
                pre_deployed_library.Add("HardwareSerial3");

                pre_deployed_library.Add("SoftwareSerial");

                string conv_board_name = "Arduino_Uno";

                string codes = _sketch_script;

                string my_docu_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string spl_exec_path = my_docu_path + @"\ArduinoPLC\";
                string arduino_path = spl_exec_path + _arduino_version_path + "\\";
                string src_path = spl_exec_path + _arduino_version_path + "\\src\\";
                string output_path = spl_exec_path + "\\output\\";
                string ref_path = spl_exec_path + _arduino_version_path + "\\ref\\" + conv_board_name + "\\";

                string compile_cmd = "";
                string output_name = "";
                string output_file_path = "";
                string object_file_list = "";

                if (!Directory.Exists(src_path))
                    Directory.CreateDirectory(src_path);

                if (!Directory.Exists(output_path))
                    Directory.CreateDirectory(output_path);

                if (!Directory.Exists(arduino_path))
                    Directory.CreateDirectory(arduino_path);

                if (!Directory.Exists(ref_path))
                    Directory.CreateDirectory(ref_path);

                if (File.Exists(src_path + "main.cpp"))
                    File.Delete(src_path + "main.cpp");

                File.WriteAllText(src_path + "main.cpp", codes);

                int new_copy_count = 0;
                string board_info = "Arduino Uno";

                if (!string.IsNullOrEmpty(_User_ComPort))
                {
                    board_info += " - [" + _User_ComPort + "]  Upload Baudrate: " + _User_Baudrate;
                }
                else
                {
                    board_info += " - [None of Arduino Board] " + _User_Baudrate;
                }

                LogInfoLine(board_info);


                LogInfoLine("Copyng new files... Please wait...");


                string cur_path = Path.GetDirectoryName(Application.ExecutablePath) + "\\";

                new_copy_count = SyncArduinoFolder(cur_path + _arduino_version_path, spl_exec_path + _arduino_version_path, true);

                new_copy_count = new_copy_count + SyncArduinoFolder(cur_path + _arduino_version_path + "\\hardware\\tools\\avr\\etc\\", spl_exec_path + _arduino_version_path + "\\hardware\\tools\\avr\\etc\\", false);

                if (new_copy_count > 0)
                {
                    LogInfoLine(new_copy_count.ToString() + " new files are prepared");
                }



                //##################################################
                DoEventsLoop(100);
                //##################################################


                List<string> ex_library_list = new List<string>();

                List<string> compile_reference_path_list = new List<string>();

                if (!_user_defined_include_list.Contains("Arduino"))
                    _user_defined_include_list.Add("Arduino");


                string debug_lines = string.Empty;

                DebugLogInfoLine("User Defined Include List", "Count: " + _user_defined_include_list.Count.ToString());

                for (int i = 0; i < _user_defined_include_list.Count; i++ )
                {
                    if (string.IsNullOrEmpty(_user_defined_include_list[i]))
                        debug_lines = debug_lines + "[" + i.ToString() + "] NULL" + System.Environment.NewLine;
                    else
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + _user_defined_include_list[i] + System.Environment.NewLine;
                }

                DebugLogInfoLine("", debug_lines);
                debug_lines = string.Empty;

                //##################################################
                DoEventsLoop(100);
                //##################################################


                DebugLogInfoLine("[[[External Library List]]]", "Count: " + ex_library_list.Count.ToString());

                for (int i = 0; i < ex_library_list.Count; i++)
                {
                    if (string.IsNullOrEmpty(ex_library_list[i]))
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + "NULL" + System.Environment.NewLine;
                    else
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + ex_library_list[i] + System.Environment.NewLine;
                }

                DebugLogInfoLine("", debug_lines);
                debug_lines = string.Empty;


                //##################################################
                DoEventsLoop(100);
                //##################################################


                DebugLogInfoLine("[[[Compile Reference Path List]]]", "Count: " + compile_reference_path_list.Count.ToString());

                for (int i = 0; i < compile_reference_path_list.Count; i++)
                {
                    string dumy_line = compile_reference_path_list[i];

                    if (string.IsNullOrEmpty(ex_library_list[i]))
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + "NULL" + System.Environment.NewLine;
                    else
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + dumy_line + System.Environment.NewLine;
                }

                DebugLogInfoLine("", debug_lines);
                debug_lines = string.Empty;

                //##################################################
                DoEventsLoop(100);
                //##################################################

                //###################################################################################


                foreach (string usr_inc_name in _user_defined_include_list)
                {

                    _global_debug_line = string.Empty;


                    #region

                    _global_debug_line = _global_debug_line + "[User Include Name] " + usr_inc_name + System.Environment.NewLine;

                    string target_file_name = arduino_path + "hardware\\arduino\\avr\\cores\\arduino\\" + usr_inc_name + ".h";

                    string inc_name_only = Path.GetFileNameWithoutExtension(usr_inc_name);
                                       
                    if (File.Exists(target_file_name))
                    {
                        if (!ex_library_list.Contains(inc_name_only))
                        {
                            ex_library_list.Add(inc_name_only);
                            SetCompileReferencePath(spl_exec_path, target_file_name, compile_reference_path_list);
                            GetIncludeList(target_file_name, ex_library_list, compile_reference_path_list, spl_exec_path);

                            _global_debug_line = _global_debug_line + "[#01 Target File Name] " + target_file_name + System.Environment.NewLine;
                        }
                        else
                            _global_debug_line = _global_debug_line + "[#01 Exist ExLibraryList] " + inc_name_only + System.Environment.NewLine;
                    }

                    #endregion


                    DebugLogInfoLine("", _global_debug_line);
                    _global_debug_line = string.Empty;

                    //##################################################
                    DoEventsLoop(100);
                    //##################################################
                
                }


                //#######################################################

                if (!ex_library_list.Contains("CDC"))
                {
                    ex_library_list.Add("CDC");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("Print"))
                {
                    ex_library_list.Add("Print");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("WMath"))
                {
                    ex_library_list.Add("WMath");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("WString"))
                {
                    ex_library_list.Add("WString");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("new"))
                {
                    ex_library_list.Add("new");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("wiring"))
                {
                    ex_library_list.Add("wiring");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("wiring_analog"))
                {
                    ex_library_list.Add("wiring_analog");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("wiring_digital"))
                {
                    ex_library_list.Add("wiring_digital");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                if (!ex_library_list.Contains("hooks"))
                {
                    ex_library_list.Add("hooks");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }

                
                if (!ex_library_list.Contains("HardwareSerial0"))
                {
                    ex_library_list.Add("HardwareSerial0");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }
                

                if (ex_library_list.Contains("Wire"))
                {
                    if (!ex_library_list.Contains("twi"))
                    {
                        ex_library_list.Add("twi");
                        compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/libraries/Wire/utility");
                    }
                }


                if (!ex_library_list.Contains("WInterrupts"))
                {
                    ex_library_list.Add("WInterrupts");
                    compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                }


                //#######################################################

                foreach (string user_inc in _user_defined_include_list)
                {
                    if (user_inc == "Tone")
                    {
                        if (!ex_library_list.Contains("Tone"))
                        {
                            ex_library_list.Add("Tone");
                            compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                        }
                    }
                    else if (user_inc == "IPAddress")
                    {
                        if (!ex_library_list.Contains("IPAddress"))
                        {
                            ex_library_list.Add("IPAddress");
                            compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                        }
                    }
                    else if (user_inc == "PluggableUSB")
                    {
                        if (!ex_library_list.Contains("PluggableUSB"))
                        {
                            ex_library_list.Add("PluggableUSB");
                            compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                        }
                    }
                    else if (user_inc == "WInterrupts")
                    {
                        if (!ex_library_list.Contains("WInterrupts"))
                        {
                            ex_library_list.Add("WInterrupts");
                            compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                        }
                    }
                    else if (user_inc == "wiring_pulse")
                    {
                        if (!ex_library_list.Contains("wiring_pulse"))
                        {
                            ex_library_list.Add("wiring_pulse");
                            compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                        }
                    }
                    else if (user_inc == "wiring_shift")
                    {
                        if (!ex_library_list.Contains("wiring_shift"))
                        {
                            ex_library_list.Add("wiring_shift");
                            compile_reference_path_list.Add(_arduino_version_path + "/hardware/arduino/avr/cores/arduino");
                        }
                    }
                    else if (user_inc == "SD")
                    {
                        if (!ex_library_list.Contains("File"))
                        {
                            string dir_path1 = docu_arduino_lib_path + "/SD";
                            string dir_path2 = _arduino_version_path + "/libraries/SD";
                            string dir_path3 = _arduino_version_path + "/libraries/SD/src";

                            if (Directory.Exists(dir_path1))
                            {
                                ex_library_list.Add("File");
                                compile_reference_path_list.Add(dir_path1);
                            }
                            else if (Directory.Exists(dir_path2))
                            {
                                ex_library_list.Add("File");
                                compile_reference_path_list.Add(dir_path2);
                            }
                            else if (Directory.Exists(dir_path3))
                            {
                                ex_library_list.Add("File");
                                compile_reference_path_list.Add(dir_path3);
                            }
                        }

                        if (!ex_library_list.Contains("Sd2Card"))
                        {
                            string dir_path1 = docu_arduino_lib_path + "/SD/utility";
                            string dir_path2 = _arduino_version_path + "/libraries/SD/utility";
                            string dir_path3 = _arduino_version_path + "/libraries/SD/src/utility";

                            if (Directory.Exists(dir_path1))
                            {
                                ex_library_list.Add("Sd2Card");
                                compile_reference_path_list.Add(dir_path1);
                            }
                            else if (Directory.Exists(dir_path2))
                            {
                                ex_library_list.Add("Sd2Card");
                                compile_reference_path_list.Add(dir_path2);
                            }
                            else if (Directory.Exists(dir_path3))
                            {
                                ex_library_list.Add("Sd2Card");
                                compile_reference_path_list.Add(dir_path3);
                            }
                        }

                        if (!ex_library_list.Contains("SdFile"))
                        {
                            string dir_path1 = docu_arduino_lib_path + "/SD/utility";
                            string dir_path2 = _arduino_version_path + "/libraries/SD/utility";
                            string dir_path3 = _arduino_version_path + "/libraries/SD/src/utility";

                            if (Directory.Exists(dir_path1))
                            {
                                ex_library_list.Add("SdFile");
                                compile_reference_path_list.Add(dir_path1);
                            }
                            else if (Directory.Exists(dir_path2))
                            {
                                ex_library_list.Add("SdFile");
                                compile_reference_path_list.Add(dir_path2);
                            }
                            else if (Directory.Exists(dir_path3))
                            {
                                ex_library_list.Add("SdFile");
                                compile_reference_path_list.Add(dir_path3);
                            }
                        }

                        if (!ex_library_list.Contains("SdVolume"))
                        {
                            string dir_path1 = docu_arduino_lib_path + "/SD/utility";
                            string dir_path2 = _arduino_version_path + "/libraries/SD/utility";
                            string dir_path3 = _arduino_version_path + "/libraries/SD/src/utility";

                            if (Directory.Exists(dir_path1))
                            {
                                ex_library_list.Add("SdVolume");
                                compile_reference_path_list.Add(dir_path1);
                            }
                            else if (Directory.Exists(dir_path2))
                            {
                                ex_library_list.Add("SdVolume");
                                compile_reference_path_list.Add(dir_path2);
                            }
                            else if (Directory.Exists(dir_path3))
                            {
                                ex_library_list.Add("SdVolume");
                                compile_reference_path_list.Add(dir_path3);
                            }
                        }
                    }
                    else
                    {
                        if (!ex_library_list.Contains(user_inc))
                        {
                            ex_library_list.Add(user_inc);
                            compile_reference_path_list.Add(_arduino_version_path + "/libraries/src/" + user_inc);
                        }
                    }
                }


                DebugLogInfoLine("", debug_lines);
                debug_lines = string.Empty;


                //##################################################
                DoEventsLoop(100);
                //##################################################


                DebugLogInfoLine("External Library List", "Count: " + ex_library_list.Count.ToString());

                for (int i = 0; i < ex_library_list.Count; i++)
                {
                    if (string.IsNullOrEmpty(ex_library_list[i]))
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + "NULL" + System.Environment.NewLine;
                    else
                        debug_lines = debug_lines + "[" + i.ToString() + "] " +  ex_library_list[i] + System.Environment.NewLine;
                }

                DebugLogInfoLine("", debug_lines);
                debug_lines = string.Empty;


                //##################################################
                DoEventsLoop(100);
                //##################################################
                                
                List<string> unique_reference_path_list = new List<string>();


                DebugLogInfoLine("Compile Reference Path List", "Count: " + compile_reference_path_list.Count.ToString());

                for (int i = 0; i < compile_reference_path_list.Count; i++)
                {
                    string dumy_line = compile_reference_path_list[i];

                    if (!unique_reference_path_list.Contains(dumy_line) && dumy_line != "[NULL]" && dumy_line != "[NOT EXIST]")
                        unique_reference_path_list.Add(dumy_line);

                    if (string.IsNullOrEmpty(ex_library_list[i]))
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + "NULL" + System.Environment.NewLine;
                    else
                        debug_lines = debug_lines + "[" + i.ToString() + "] " + dumy_line + System.Environment.NewLine;
                }

                DebugLogInfoLine("", debug_lines);
                debug_lines = string.Empty;

                //##################################################
                DoEventsLoop(100);
                //##################################################


                if (ex_library_list.Count != compile_reference_path_list.Count)
                {
                    LogInfoLine("[Error in Header Counting Match] " + ex_library_list.Count.ToString() + " / " + compile_reference_path_list.Count.ToString());
                }


                //##################################################
                DoEventsLoop(100);
                //##################################################

                
                LogInfoLine("Compiling libraries...");


                //##################################################
                DoEventsLoop(100);
                //##################################################

                string compiler_c_elf_cmd = "\"" + cur_path + _arduino_version_path + "/hardware/tools/avr/bin/avr-g++.exe\" ";
                string compiler_c_elf_flags = " -w -Os -Wl,--gc-sections ";

                string compiler_c_cmd = "\"" + cur_path + _arduino_version_path + "/hardware/tools/avr/bin/avr-gcc.exe\" ";
                string compiler_cpp_cmd = "\"" + cur_path + _arduino_version_path + "/hardware/tools/avr/bin/avr-g++.exe\" ";

                string compiler_hex_build_cmd = "\"" + cur_path + _arduino_version_path + "/hardware/tools/avr/bin/avr-objcopy.exe\" ";
                string compiler_size_check_cmd = "\"" + cur_path + _arduino_version_path + "/hardware/tools/avr/bin/avr-size.exe\" ";
                string compiler_upload_cmd = "\"" + cur_path + _arduino_version_path + "/hardware/tools/avr/bin/avrdude.exe\" ";

                string avrdude_config_path =  " ./" + _arduino_version_path + "/hardware/tools/avr/etc/avrdude.conf ";
                

                string arduino_archietct = "ARDUINO_ARCH_AVR";  

                string compiler_cpp_flags = " -c -g -Os -w -fno-exceptions -ffunction-sections -fdata-sections -fno-threadsafe-statics -MMD -D ARDUINO=200 -D HAVE_HWSERIAL0  -D " + arduino_archietct;

                string build_mcu = "atmega328p";
                string build_f_cpu = "16000000L";
                string build_board = "AVR_UNO";

                string build_varient = "standard";

                string variant_sub_dir = " -I\"./" + _arduino_version_path + "/hardware/arduino/avr/variants/" + build_varient + "\" ";

                string custom_lib_inc_path = variant_sub_dir;


                foreach (string unique_path in unique_reference_path_list)
                {
                    if (unique_path.StartsWith("arduino-"))
                        custom_lib_inc_path = custom_lib_inc_path + " -I\"./" + unique_path + "\" ";
                    else       
                    {
                        custom_lib_inc_path = custom_lib_inc_path + " -I\"" + unique_path + "\" ";
                    }
                }

                string err_msg = string.Empty;


                //#######################################################
                //DEBUG
                //#######################################################
                DebugLogInfoLine("Compile Path", custom_lib_inc_path);
                //#######################################################

                new_copy_count = 0;

                for (int i = 0; i < ex_library_list.Count; i++)
                {
                    string file_name_without_ext = ex_library_list[i];
                    string compile_reference_path = compile_reference_path_list[i];


                    //####################
                    // DEBUG
                    //####################
                    DebugLogInfoLine("", "");
                    DebugLogInfoLine("Target " + i.ToString(), file_name_without_ext + " / " + compile_reference_path);
                    //####################


                    {

                        if (compile_reference_path == "[NULL]" || compile_reference_path == "[NOT EXIST]")
                        {
                            DebugLogInfoLine("Continue " + i.ToString(), "compile path is [NULL]");
                            continue;
                        }

                        string sub_dir = string.Empty;

                        if (file_name_without_ext == "Servo")
                        {
                            if (arduino_archietct == "ARDUINO_ARCH_AVR")
                                sub_dir = "avr";
                            else if (arduino_archietct == "ARDUINO_ARCH_SAM")
                                sub_dir = "sam";
                            else if (arduino_archietct == "ARDUINO_ARCH_SAMD")
                                sub_dir = "samd";
                            else
                                sub_dir = "avr";
                        }

                        
                        string file_name = string.Empty;

                        if (compile_reference_path.StartsWith("arduino-"))
                        {
                            //####################
                            // DEBUG
                            //####################
                            DebugLogInfoLine("GetExistCompileTargetFileName_A " + i.ToString(), "[[" + spl_exec_path + compile_reference_path + "/" + "]] [[" + file_name_without_ext + "]] [[" + sub_dir + "]]");
                            //####################

                            file_name = GetExistCompileTargetFileName(spl_exec_path + compile_reference_path + "/", file_name_without_ext, sub_dir);
                        }
                        else
                        {
                            //####################
                            // DEBUG
                            //####################
                            DebugLogInfoLine("GetExistCompileTargetFileName_B " + i.ToString(), "[[" + compile_reference_path + "/" + "]] [[" + file_name_without_ext + "]] [[" + sub_dir + "]]");
                            //####################

                            file_name = GetExistCompileTargetFileName(compile_reference_path + "/", file_name_without_ext, sub_dir);
                        }
                    
                        //####################
                        // DEBUG
                        //####################
                        DebugLogInfoLine("File Name " + i.ToString(), file_name);
                        //####################

                        if (string.IsNullOrEmpty(file_name))
                        {
                            DebugLogInfoLine("Continue " + i.ToString(), "Just h file");
                            continue;
                        }


                        string compile_target = Path.GetDirectoryName(file_name);
                        compile_target = compile_target.Replace('\\', '/');

                        string conv_spl_exec_path_path = spl_exec_path.Replace('\\', '/');

                        if (compile_reference_path.StartsWith("arduino-"))
                            file_name = file_name.Substring(spl_exec_path.Length);
                        

                        output_name = _arduino_version_path + "/ref/" + conv_board_name + "/" + file_name_without_ext + ".o";
                        output_file_path = spl_exec_path + _arduino_version_path + "/ref/" + conv_board_name + "/" + file_name_without_ext + ".o";


                        object_file_list = object_file_list + " ./" + output_name + " ";


                        if (pre_deployed_library.Contains(file_name_without_ext) && File.Exists(output_file_path))
                        {
                            DebugLogInfoLine("Continue " + i.ToString(), "Already Exists");
                            continue;
                        }


                        LogInfoLine("[Library Compile] " + file_name);


                        if (Path.GetExtension(file_name) == ".c")
                        {
                            if (compile_reference_path.StartsWith("arduino-"))
                                compile_cmd = compiler_c_cmd + compiler_cpp_flags + " -mmcu=" + build_mcu + " -DF_CPU=" + build_f_cpu + " -DARDUINO_" + build_board + custom_lib_inc_path + " -o \"./" + output_name + "\" \"./" + file_name + "\"";
                            else
                                compile_cmd = compiler_c_cmd + compiler_cpp_flags + " -mmcu=" + build_mcu + " -DF_CPU=" + build_f_cpu + " -DARDUINO_" + build_board + custom_lib_inc_path + " -o \"./" + output_name + "\" \"" + file_name + "\"";

                            DebugLogInfoLine("**Compile** " + i.ToString(), compile_cmd);

                            err_msg = Exec_AVR_Build(spl_exec_path, compile_cmd, false);
                        }
                        else
                        {
                            if (compile_reference_path.StartsWith("arduino-"))
                                compile_cmd = compiler_cpp_cmd + compiler_cpp_flags + " -mmcu=" + build_mcu + " -DF_CPU=" + build_f_cpu + " -DARDUINO_" + build_board + custom_lib_inc_path + " -o \"./" + output_name + "\" \"./" + file_name + "\"";
                            else
                                compile_cmd = compiler_cpp_cmd + compiler_cpp_flags + " -mmcu=" + build_mcu + " -DF_CPU=" + build_f_cpu + " -DARDUINO_" + build_board + custom_lib_inc_path + " -o \"./" + output_name + "\" \"" + file_name + "\"";

                            DebugLogInfoLine("**Compile** " + i.ToString(), compile_cmd);

                            err_msg = Exec_AVR_Build(spl_exec_path, compile_cmd, false);
                        }

                        err_msg = err_msg.Trim();


                        if (!string.IsNullOrEmpty(err_msg) && err_msg.Contains("error"))
                        {
                            LogInfoLine("[Library Compile] Error: " + err_msg);
                        }
                        else
                        {
                            if (!_user_defined_include_list.Contains(file_name_without_ext))
                                _user_defined_include_list.Add(file_name_without_ext);

                            new_copy_count++;
                        }



                    }


                    //##################################################
                    DoEventsLoop(100);
                    //##################################################
                }



                if (new_copy_count > 0)
                {
                    LogInfoLine(new_copy_count.ToString() + " new library files were compiled.");
                }



                //##################################################
                DoEventsLoop(100);
                //##################################################



                //#######################################################################

                //main.cpp Compile

                LogInfoLine("Compiling... Please wait...");



                //##################################################
                DoEventsLoop(100);
                //##################################################


                //main.cpp Compile
                //################################################################################

                output_name = _arduino_version_path + "/ref/" + conv_board_name + "/main.o";

                object_file_list = object_file_list + " ./" + output_name + " ";

                compile_cmd = compiler_cpp_cmd + compiler_cpp_flags + " -mmcu=" + build_mcu + " -DF_CPU=" + build_f_cpu + " -DARDUINO_" + build_board + custom_lib_inc_path + " -o \"./" + output_name + "\" \"./" + _arduino_version_path + "/src/main.cpp\"";

                err_msg = Exec_AVR_Build(spl_exec_path, compile_cmd, false);

                err_msg = err_msg.Trim();

                //################################################################################



                if (!string.IsNullOrEmpty(err_msg) && err_msg.Contains("error"))
                {
                    LogInfoLine("[Compile] Error: " + err_msg);

                    if (err_msg.Contains("error: stray"))
                    {
                        LogInfoLine("[Compile] Error: Check your comments around your codes.");
                    }

                    return;
                }
                else
                {
                    LogInfoLine("[Compile] OK");
                }


                err_msg = string.Empty;

                //##################################################
                DoEventsLoop(100);
                //##################################################

                if (File.Exists(arduino_path + "ObjectList.txt"))
                    File.Delete(arduino_path + "ObjectList.txt");

                File.WriteAllText(arduino_path + "ObjectList.txt", object_file_list);

                compile_cmd = compiler_c_elf_cmd + compiler_c_elf_flags;
                compile_cmd += " -mmcu=" + build_mcu + " -o\"./output/spl_deploy.elf\" " + object_file_list + " -lm";


                err_msg = Exec_AVR_Build(spl_exec_path, compile_cmd, false);

                err_msg = err_msg.Trim();

                if (!string.IsNullOrEmpty(err_msg))
                {
                    LogInfoLine("[Build] Error: " + err_msg);

                    return;
                }
                else
                {
                    LogInfoLine("[Build] OK");
                }



                //##################################################
                DoEventsLoop(100);
                //##################################################


                //##################################################
                DoEventsLoop(100);
                //##################################################

                string hex_option = " -R .eeprom -O ihex ./output/spl_deploy.elf  \"./output/spl_deploy.hex\"";

                err_msg = Exec_AVR_Build(spl_exec_path, compiler_hex_build_cmd + hex_option, false);                

                err_msg = err_msg.Trim();

                if (!string.IsNullOrEmpty(err_msg))
                {
                    LogInfoLine("[HEX File] Error: " + err_msg);


                    if (File.Exists(arduino_path + "HexInfo.txt"))
                        File.Delete(arduino_path + "HexInfo.txt");

                    string hex_info_line = spl_exec_path + System.Environment.NewLine;

                    hex_info_line += compiler_hex_build_cmd + System.Environment.NewLine;
                    hex_info_line += hex_option + System.Environment.NewLine;
                    hex_info_line += err_msg + System.Environment.NewLine;

                    File.WriteAllText(arduino_path + "HexInfo.txt", hex_info_line);

                    return;
                }
                else
                {
                    LogInfoLine("[HEX File] OK");
                }


                //#########################################################################


                //##################################################
                DoEventsLoop(100);
                //##################################################


                _stderr_str = string.Empty;
                _stdout_str = string.Empty;


                if (!string.IsNullOrEmpty(_User_ComPort))
                {
                    LogInfoLine("[Upload] Uploading... Please wait... (about 30 seconds)");


                    //##################################################
                    DoEventsLoop(100);
                    //##################################################


                    string option_str = " -V -P " + _User_ComPort + "  -b " + _User_Baudrate + "  -p " + build_mcu + " -c arduino ";
                    option_str = option_str + " -C " + avrdude_config_path + " -D -U flash:w:\"./output/spl_deploy.hex\"";

                    string upload_cmd = compiler_upload_cmd + option_str;

                    err_msg = Exec_AVR_Build(spl_exec_path, upload_cmd, false);

                    if (!string.IsNullOrEmpty(err_msg))
                    {
                        LogInfoLine("[Upload] OK " + err_msg);
                    }
                    else
                    {
                        LogInfoLine("[Upload] OK");
                    }


                    //##################################################
                    DoEventsLoop(100);
                    //##################################################

                    if (_required_serial_monitoring)
                        Serial_Monitoring();

                    //##################################################
                    DoEventsLoop(100);
                    //##################################################

                }
                else
                {
                    LogInfoLine("None of Arduino Board.");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        void SetCompileReferencePath(string root_path, string file_path, List<string> compile_reference_path_list)
        {

            _global_debug_line = _global_debug_line + "** SetCompileReferencePath root ** " + root_path + System.Environment.NewLine;
            _global_debug_line = _global_debug_line + "** SetCompileReferencePath file ** " + file_path + System.Environment.NewLine;


            if (string.IsNullOrEmpty(file_path))
            {
                compile_reference_path_list.Add("[NULL]");
                _global_debug_line = _global_debug_line + "** SetCompileReferencePath NULL ** " + file_path + System.Environment.NewLine;
                return;
            }

            if (!File.Exists(file_path))
            {
                compile_reference_path_list.Add("[NOT EXIST]");
                _global_debug_line = _global_debug_line + "** SetCompileReferencePath NOT EXIST ** " + file_path + System.Environment.NewLine;
                return;
            }

            string arduino_lib_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            arduino_lib_path = arduino_lib_path + @"\Arduino\libraries";


            try
            {
                if (file_path.StartsWith(arduino_lib_path))
                {
                    string ref_path = Path.GetDirectoryName(file_path);
                    ref_path = ref_path.Replace('\\', '/');
                    compile_reference_path_list.Add(ref_path);

                    _global_debug_line = _global_debug_line + "## SetCompileReferencePath Add ## " + ref_path + System.Environment.NewLine;
                }
                else
                {
                    string ref_path = Path.GetDirectoryName(file_path);
                    ref_path = ref_path.Substring(root_path.Length);

                    ref_path = ref_path.Replace('\\', '/');

                    compile_reference_path_list.Add(ref_path);

                    _global_debug_line = _global_debug_line + "## SetCompileReferencePath Add ## " + ref_path + System.Environment.NewLine;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString() + " / " + file_path);
            }
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



        private void CopyArduinoSubFiles(string cur_path, string sub_dir, string target_dir)
        {
            if (Directory.Exists(cur_path + "arduino\\" + sub_dir))
            {
                foreach (string file_path in Directory.GetFiles(cur_path + "arduino\\" + sub_dir))
                {
                    string file_name = Path.GetFileName(file_path);

                    if (file_name.ToLower().EndsWith(".h") || file_name.ToLower().EndsWith(".cpp") || file_name.ToLower().EndsWith(".c"))
                    {
                        if (!File.Exists(target_dir + file_name))
                        {
                            File.Copy(cur_path + "arduino\\" + sub_dir + "\\" + file_name, target_dir + file_name);
                        }
                        else
                        {
                            if (File.GetLastWriteTime(target_dir + file_name) != File.GetLastWriteTime(cur_path + "arduino\\" + sub_dir + "\\" + file_name))
                            {
                                File.Delete(target_dir + file_name);
                                File.Copy(cur_path + "arduino\\" + sub_dir + "\\" + file_name, target_dir + file_name);
                            }
                        }
                    }
                }

            }
        }


        private string GetExistCompileTargetHeaderName(string root_path, string file_name_without_ext)
        {
            if (File.Exists(root_path + file_name_without_ext + ".h"))
                return root_path + file_name_without_ext + ".h";
            else
            {
                string res = string.Empty;

                foreach (string sub_dir in Directory.GetDirectories(root_path))
                {
                    string sub_path = sub_dir;

                    if (!sub_path.EndsWith("/") && !sub_path.EndsWith("\\"))
                        sub_path = sub_dir + "/";

                    string sub_check = GetExistCompileTargetHeaderName(sub_path, file_name_without_ext);

                    if (!string.IsNullOrEmpty(sub_check))
                    {
                        res = sub_check;
                        break;
                    }
                }

                return res;
            }
        }


        private string GetExistCompileTargetFileName(string root_path, string file_name_without_ext, string sub_dir)
        {
            string arduino_lib_path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            arduino_lib_path = arduino_lib_path + @"\Arduino\libraries";
            string docu_lib_c_name = arduino_lib_path + "\\" + sub_dir + "\\" + file_name_without_ext + ".c";
            string docu_lib_cpp_name = arduino_lib_path + "\\" + sub_dir + "\\" + file_name_without_ext + ".cpp";


            if (File.Exists(root_path + file_name_without_ext + ".cpp"))
                return root_path + file_name_without_ext + ".cpp";
            else if (File.Exists(root_path + file_name_without_ext + ".c"))
                return root_path + file_name_without_ext + ".c";
            else if (File.Exists(root_path + "src\\" + file_name_without_ext + ".cpp"))
                return root_path + "src\\" + file_name_without_ext + ".cpp";
            else if (File.Exists(root_path + "src\\" + file_name_without_ext + ".c"))
                return root_path + "src\\" + file_name_without_ext + ".c";
            else if (File.Exists(docu_lib_c_name))
                return docu_lib_c_name;
            else if (File.Exists(docu_lib_cpp_name))
                return docu_lib_cpp_name;
            else if (file_name_without_ext == "Servo")
            {
                if (File.Exists(root_path + sub_dir + "/" + file_name_without_ext + ".cpp"))
                    return root_path + sub_dir + "/" + file_name_without_ext + ".cpp";
                else if (File.Exists(root_path + sub_dir + "/" + file_name_without_ext + ".c"))
                    return root_path + sub_dir + "/" + file_name_without_ext + ".c";
                else
                    return string.Empty;
            }
            else
            {
                string res = string.Empty;

                return res;
            }
        }


        private void GetIncludeList(string target_file_name, List<string> include_list, List<string> compile_reference_path_list, string spl_exec_path)
        {

            _global_debug_line = _global_debug_line + System.Environment.NewLine + "** GetIncludeList target ** " + target_file_name + System.Environment.NewLine;


            if (File.Exists(target_file_name))
            {
                string root_path = Path.GetDirectoryName(target_file_name) + "/";

                _global_debug_line = _global_debug_line + "** GetIncludeList root ** " + root_path + System.Environment.NewLine;

                string[] lines = File.ReadAllLines(target_file_name);

                for (int i = 0; i < lines.Length; i++)
                {
                    string inc_name_only = GetIncludeNameOnly(lines[i]);

                    if (!string.IsNullOrEmpty(inc_name_only))
                    {
                        if (!include_list.Contains(inc_name_only))
                        {
                            include_list.Add(inc_name_only);

                            string arduino_path = spl_exec_path + _arduino_version_path + "\\";
                            string child_file_name = GetValidArduinoHederPath(arduino_path, inc_name_only);

                            _global_debug_line = _global_debug_line + "#1 GetIncludeList Child Name #1 " + child_file_name + System.Environment.NewLine;

                            if (string.IsNullOrEmpty(child_file_name))
                            {
                                string child_temp_name = root_path + inc_name_only + ".h";

                                if (File.Exists(child_temp_name))
                                    child_file_name = child_temp_name;
                            }

                            _global_debug_line = _global_debug_line + "#2 GetIncludeList Inc Name #2 " + inc_name_only + System.Environment.NewLine;
                            _global_debug_line = _global_debug_line + "#3 GetIncludeList Child Name #3 " + child_file_name + System.Environment.NewLine;
                            

                            SetCompileReferencePath(spl_exec_path, child_file_name, compile_reference_path_list);

                            if (File.Exists(child_file_name))
                                GetIncludeList(child_file_name, include_list, compile_reference_path_list, spl_exec_path);
                        }
                    }
                }
            }

        }


        string GetValidArduinoHederPath(string arduino_path, string usr_inc_name)
        {
            string res = string.Empty;


            #region

            string target_file_name = arduino_path + "hardware\\arduino\\avr\\cores\\arduino\\" + usr_inc_name + ".h";

            string inc_name_only = Path.GetFileNameWithoutExtension(usr_inc_name);

            if (File.Exists(target_file_name))
            {
                res = target_file_name;
            }
            else
            {
                #region'

                target_file_name = arduino_path + "libraries\\" + usr_inc_name + "\\" + usr_inc_name + ".h";

                if (File.Exists(target_file_name))
                {
                    res = target_file_name;
                }
                else
                {
                    target_file_name = arduino_path + "libraries\\" + usr_inc_name + "\\src\\" + usr_inc_name + ".h";

                    if (File.Exists(target_file_name))
                    {
                        res = target_file_name;
                    }
                    else
                    {

                        #region

                        target_file_name = arduino_path + "hardware\\arduino\\avr\\libraries\\" + usr_inc_name + "\\" + usr_inc_name + ".h";

                        if (File.Exists(target_file_name))
                        {
                            res = target_file_name;
                        }
                        else
                        {
                            string script_path = Path.GetDirectoryName(_user_file_path);
                            target_file_name = Path.Combine(script_path, usr_inc_name + ".h");

                            if (File.Exists(target_file_name))
                            {
                                res = target_file_name;
                            }
                        }

                        #endregion

                    }


                }

                #endregion
            }


            #endregion


            return res;
        }


        private string GetIncludeNameOnly(string line)
        {
            string res = string.Empty;

            string trim_line = line.Trim('\t');
            trim_line = trim_line.Trim(' ');

            int tmp_ind = trim_line.IndexOf('>');

            if (tmp_ind > 0)
                trim_line = trim_line.Substring(0, tmp_ind);

            tmp_ind = -1;
            int found_cnt = 0;

            for (int t = 0; t < trim_line.Length; t++)
            {
                if (trim_line[t] == '\"')
                    found_cnt = found_cnt + 1;

                if (found_cnt == 2)
                    tmp_ind = t;
            }

            if (tmp_ind > 0)
                trim_line = trim_line.Substring(0, tmp_ind);


            if (trim_line.StartsWith("#include"))
            {
                string inc_name_only = trim_line.Substring(8).Trim();
                inc_name_only = inc_name_only.Trim('\t');
                inc_name_only = inc_name_only.Trim(' ');
                inc_name_only = inc_name_only.Trim('<');
                inc_name_only = inc_name_only.Trim('>');
                inc_name_only = inc_name_only.Trim('\'');
                inc_name_only = inc_name_only.Trim('\"');


                if (inc_name_only.Length > 2)
                    inc_name_only = inc_name_only.Substring(0, inc_name_only.Length - 2);

                res = inc_name_only;
            }


            return res;
        }




        private string Exec_AVR_Build(string exec_path, string cmd_line, bool bg_exec)
        {
            string res = string.Empty;

            Process avrprog = new Process();

            StreamReader avrstdout, avrstderr;
            StreamWriter avrstdin;
            ProcessStartInfo ps = new ProcessStartInfo("cmd");

            ps.UseShellExecute = false;
            ps.RedirectStandardInput = true;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = true;
            ps.CreateNoWindow = true;
            ps.WorkingDirectory = exec_path;

            avrprog.StartInfo = ps;
            avrprog.Start();
            avrstdin = avrprog.StandardInput;
            avrstdout = avrprog.StandardOutput;
            avrstderr = avrprog.StandardError;
            avrstdin.AutoFlush = true;

            avrstdin.WriteLine(cmd_line);

            avrstdin.Close();            

            System.Threading.Thread.Sleep(100);

            res = avrstderr.ReadToEnd() + System.Environment.NewLine;

            return res;
        }


        private string Exec_AVR_Build_Get_StdOut(string exec_path, string cmd_line, bool bg_exec)
        {
            string res = string.Empty;

            Process avrprog = new Process();

            StreamReader avrstdout, avrstderr;
            StreamWriter avrstdin;
            ProcessStartInfo ps = new ProcessStartInfo("cmd");

            ps.UseShellExecute = false;
            ps.RedirectStandardInput = true;
            ps.RedirectStandardOutput = true;
            ps.RedirectStandardError = true;
            ps.CreateNoWindow = true;
            ps.WorkingDirectory = exec_path;

            avrprog.StartInfo = ps;
            avrprog.Start();
            avrstdin = avrprog.StandardInput;
            avrstdout = avrprog.StandardOutput;
            avrstderr = avrprog.StandardError;
            avrstdin.AutoFlush = true;

            avrstdin.WriteLine(cmd_line);

            avrstdin.Close();

            System.Threading.Thread.Sleep(100);

            res = avrstdout.ReadToEnd() + System.Environment.NewLine;

            return res;
        }


        private void timer_exec_Tick(object sender, EventArgs e)
        {
            timer_exec.Enabled = false;
            UploadSketch();            
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (listBox1 == null)
                    return;

                if (listBox1.SelectedIndex >= 0)
                {
                    MessageBox.Show(listBox1.Items[listBox1.SelectedIndex].ToString());
                }
            }
            catch { }
        }


        private void Serial_Monitoring()
        {
            try
            {
                int try_count = 0;


                if (!string.IsNullOrEmpty(_User_ComPort))
                {
                    while (try_count < 10)
                    {
                        try
                        {
                            if (CommOpen(_User_ComPort))
                                break;
                        }
                        catch { }

                        try_count++;
                        System.Threading.Thread.Sleep(1000);
                    }
                }
            }
            catch { }
        }


        private void SPLConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            _form_close_flag = true;

            try
            {

                System.Threading.Timer timer = new System.Threading.Timer(new System.Threading.TimerCallback(FormClosing_Separate_Thread), "", 0, -1);

                System.Threading.Thread.Sleep(200);
            }
            catch { }
        }

        private void FormClosing_Separate_Thread(object state)
        {
            try
            {
                Terminate();
            }
            catch { }

            try
            {
                try
                {
                    if (_serialPort != null)
                    {
                        try
                        {
                            _serialPort.Dispose();
                        }
                        catch { }


                        try
                        {
                            _serialPort = null;
                        }
                        catch { }
                    }

                }
                catch { }

            }
            catch { }
        }




        private void timer_monitering_Tick(object sender, EventArgs e)
        {
            timer_monitering.Enabled = false;
            
            if (_form_close_flag)
                return;

            try
            {
                Serial_Monitoring();
            }
            catch { }
        }


        public void UI_InputNotify(string ui_type, byte data)
        {
            if (_form_close_flag)
                return;

            try
            {
                if (IsSerialOpen)
                {
                    SendSerialData(new byte[] { data });
                }
            }
            catch { }

        }

        public void UI_InputNotifyExt(string ui_type, byte[] data)
        {
            if (_form_close_flag)
                return;

            try
            {
                if (IsSerialOpen)
                {
                    SendSerialData(data);
                }
            }
            catch { }
        }

        public void UI_InputNotifyText(string ui_type, string data)
        {
            if (_form_close_flag)
                return;

            try
            {
                if (IsSerialOpen)
                {
                    SendSerialData(data);
                }
            }
            catch { }
        }


        private void timer_serial_receive_Tick(object sender, EventArgs e)
        {
            timer_serial_receive.Enabled = false;

            if (_form_close_flag)
                return;

            string log_info_str = string.Empty;

            try
            {
                int count = _CQ.CQ_GetLength();

                if (count == 0)
                {
                    timer_serial_receive.Enabled = true;
                    return;
                }

                byte[] recv_bytes = _CQ.CQ_GetData();


                log_info_str = Encoding.ASCII.GetString(recv_bytes);

                if (log_info_str != string.Empty)
                    LogInfo(log_info_str);
            }
            catch { }

            timer_serial_receive.Enabled = true;
        }



        private void timer_check_running_Tick(object sender, EventArgs e)
        {
            if (_form_close_flag)
                return;

            try
            {

                if (timer_serial_receive.Enabled == false)
                    timer_serial_receive.Enabled = true;

            }
            catch { }


            try
            {

                if (timer_loginfo.Enabled == false)
                    timer_loginfo.Enabled = true;

            }
            catch { }
        }


        #region Serial Communication

        bool CommOpen(string comPort)
        {
            if (string.IsNullOrEmpty(comPort))
                return false;

            bool res = false;

            try
            {
                res = CommOpen(comPort, int.Parse(_User_Baudrate));
            }
            catch { }

            return res;
        }

        bool CommOpen(string comPort, int baudRate)
        {
            try
            {
                _serialPort = new SerialPort(comPort, baudRate);
                _serialPort.Encoding = Encoding.Default;
                _serialPort.Parity = Parity.None;
                _serialPort.DataBits = 8;
                _serialPort.StopBits = StopBits.One;

                try
                {
                    CommClose();
                }
                catch { }

                try
                {
                    _serialPort.Open();
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(serialPort_DataReceived);

                    LogInfoLine("Open Serial Port - " + comPort);
                }
                catch (Exception ex)
                {
                    LogInfoLine("<< Serial Error >> " + ex.ToString());

                    return false;
                }
            }
            catch { }


            return true;
        }



        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_form_close_flag)
                return;

            try
            {

                if (e.EventType == SerialData.Chars)
                {
                    byte[] data = new byte[_maxBuffer];
                    int count = _serialPort.Read(data, 0, _maxBuffer);

                    _CQ.CQ_AddBytes(data, count);
                }
            }
            catch { }
        }


        void SendSerialData(string data)
        {
            try
            {
                byte[] send_bytes = Encoding.ASCII.GetBytes(data);
                _serialPort.Write(send_bytes, 0, send_bytes.Length);
            }
            catch { }
        }

        void SendSerialData(byte[] buffer)
        {
            try
            {
                _serialPort.Write(buffer, 0, buffer.Length);
            }
            catch { }
        }


        public void CommClose()
        {
            try
            {
                if (_serialPort != null)
                {
                    try
                    {
                        {
                            try
                            {
                                _serialPort.DiscardInBuffer();
                            }
                            catch { }

                            try
                            {
                                _serialPort.DiscardOutBuffer();
                            }
                            catch { }

                            try
                            {
                                _serialPort.Close();
                            }
                            catch { }

                            try
                            {
                                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(serialPort_DataReceived);
                            }
                            catch { }

                            
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        public bool IsSerialOpen
        {
            get { return _serialPort != null && _serialPort.IsOpen; }
        }

        #endregion Serial Communication




        private void textEdit1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button2_Click(null, null);
        }


        void SendByteValue(byte val)
        {
            if (IsSerialOpen)
            {
                byte[] buf = new byte[1];
                buf[0] = val;
                SendSerialData(buf);
            }
        }

        


        private void SPLConsole_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                button1_Click(null, null);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _form_close_flag = true;

            try
            {
                Terminate();
            }
            catch { }

            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            try
            {
                if (IsSerialOpen)
                {
                    if (comboBox1.SelectedIndex == 1)
                        SendSerialData(textEdit1.Text + '\r');
                    else if (comboBox1.SelectedIndex == 2)
                    {
                        try
                        {
                            string[] arr;
                            arr = textEdit1.Text.Split(new Char[] { ' ', ',', '\t' }, 200);

                            List<string> byte_list = new List<string>();

                            for (int i = 0; i < arr.Length; i++)
                            {
                                if (arr[i] != string.Empty)
                                {
                                    byte_list.Add(arr[i]);
                                }
                            }

                            if (byte_list.Count > 0)
                            {
                                byte[] buf = new byte[byte_list.Count];

                                for (int i = 0; i < byte_list.Count; i++)
                                    buf[i] = byte.Parse(byte_list[i]);

                                SendSerialData(buf);
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Please type byte value.");
                        }
                    }
                    else
                        SendSerialData(textEdit1.Text);


                    textEdit1.Text = "";
                    textEdit1.Focus();
                }
            }
            catch { }
        }

    }


    public class CircularQueue
    {
        #region Circular Queue

        const int CIRCLED_QUEUE_SIZE = 90840;

        byte[] _CQ_Array = new byte[CIRCLED_QUEUE_SIZE];
        int _s_array_ind = -1;
        int _e_array_ind = 0;

        public CircularQueue()
        {
        }

        public void CQ_AddByte(byte b)
        {
            if (_s_array_ind < 0)
                _s_array_ind = 0;

            _CQ_Array[_e_array_ind] = b;
            _e_array_ind++;

            if (_e_array_ind >= CIRCLED_QUEUE_SIZE)
                _e_array_ind = 0;
        }

        public void CQ_AddBytes(byte[] bytes, int count)
        {
            if (_s_array_ind < 0)
                _s_array_ind = 0;

            for (int i = 0; i < count; i++)
            {
                _CQ_Array[_e_array_ind] = bytes[i];

                _e_array_ind++;

                if (_e_array_ind >= CIRCLED_QUEUE_SIZE)
                    _e_array_ind = 0;
            }
        }

        public int CQ_GetLength()
        {
            if (_s_array_ind < 0 || _e_array_ind < 0)
                return 0;
            else if (_e_array_ind >= _s_array_ind)
                return (_e_array_ind - _s_array_ind);
            else
            {
                int len1 = CIRCLED_QUEUE_SIZE - _s_array_ind;
                int len2 = _e_array_ind;

                return len1 + len2;
            }
        }


        public byte[] CQ_GetToken()
        {
            byte[] res = null;

            int max_lan = CQ_GetLength();

            bool start_flag = false;
            bool end_flag = false;

            int start_ind = 0;
            int end_ind = 0;

            for (int i = 0; i < max_lan; i++)
            {
                int ind = _s_array_ind + i;

                if (ind >= CIRCLED_QUEUE_SIZE)
                    ind = ind - CIRCLED_QUEUE_SIZE;


                if (_CQ_Array[ind] == '#')
                {
                    start_flag = true;
                    start_ind = i;
                }
                else if (start_flag && _CQ_Array[ind] == '|')
                {
                    end_flag = true;
                    end_ind = i;
                    break;
                }
            }

            if (start_flag && end_flag)
            {
                _s_array_ind = start_ind;
                int data_len = end_ind - start_ind + 1;
                res = CQ_GetData(data_len);
            }

            return res;
        }


        public string CQ_GetString()
        {
            return Encoding.ASCII.GetString(CQ_GetData(CQ_GetLength()));
        }

        public byte[] CQ_GetData()
        {
            return CQ_GetData(CQ_GetLength());
        }


        public byte[] CQ_GetData(int length)
        {
            byte[] res = new byte[length];

            for (int i = 0; i < length; i++)
            {
                res[i] = _CQ_Array[_s_array_ind];

                _s_array_ind++;

                if (_s_array_ind >= CIRCLED_QUEUE_SIZE)
                    _s_array_ind = 0;
            }

            return res;
        }


        public void CQ_RemoveData(int length)
        {
            for (int i = 0; i < length; i++)
            {
                _s_array_ind++;

                if (_s_array_ind >= CIRCLED_QUEUE_SIZE)
                    _s_array_ind = 0;
            }
        }

        public void CQ_ClearData()
        {
            _s_array_ind = -1;
            _e_array_ind = 0;
        }

        #endregion Circular Queue
    }
}