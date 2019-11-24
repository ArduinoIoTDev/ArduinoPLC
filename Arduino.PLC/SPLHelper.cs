using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

using System.IO;
using System.IO.Ports;
using System.Management;
using System.Diagnostics;


namespace HelloApps.Helper
{
    public static class SPLDuinoHelper
    {
        public static string GetSPLDuinoComPortName()
        {
            string com_port = string.Empty;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj != null)
                    {
                        object q_obj = queryObj["Caption"];

                        if (q_obj != null)
                        {
                            string caption_str = q_obj.ToString();
                            if (!string.IsNullOrEmpty(caption_str))
                            {
                                if (caption_str.Contains("(COM"))
                                {
                                    if (caption_str.Contains("Silicon") && caption_str.Contains("UART Bridge"))
                                    {
                                        int pos = caption_str.IndexOf("(COM");
                                        string desc = caption_str.Substring(pos + 1);

                                        desc = desc.Trim();
                                        desc = desc.TrimEnd(')');

                                        return desc;
                                    }
                                }
                            }

                        }
                    }

                }
            }
            catch
            {
                
            }

            return com_port;
        }


        public static List<string> GetComPortNameList()
        {
            List<string> com_port_list = new List<string>();

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if (queryObj != null)
                    {
                        object q_obj = queryObj["Caption"];

                        if (q_obj != null)
                        {
                            string caption_str = q_obj.ToString();
                            string caption_str_lower = caption_str.ToLower();

                            if (!string.IsNullOrEmpty(caption_str))
                            {
                                if (caption_str.Contains("(COM"))
                                {
                                    int pos = caption_str.IndexOf("(COM");
                                    string port_name = caption_str.Substring(pos + 1);

                                    port_name = port_name.Trim();
                                    port_name = port_name.TrimEnd(')');

                                    if (caption_str.Contains("CH340"))
                                        port_name = port_name + " (SPL-Duino V2)";
                                    else if (caption_str.Contains("Silicon") && caption_str.Contains("UART Bridge"))
                                        port_name = port_name + " (SPL-Duino V1)";
                                    else if (caption_str_lower.Contains("bluetooth"))
                                        port_name = port_name + " (Bluetooth)";

                                    com_port_list.Add(port_name);
                                }
                            }

                        }
                    }

                }
            }
            catch
            {

            }

            return com_port_list;
        }



        public static List<string> GetComPortList()
        {
            List<string> com_port_list = new List<string>();

            ManagementScope connectionScope = new ManagementScope();
            SelectQuery serialQuery = new SelectQuery("SELECT * FROM Win32_SerialPort");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(connectionScope, serialQuery);

            try
            {
                foreach (ManagementObject item in searcher.Get())
                {
                    string desc = item["Description"].ToString();
                    string deviceId = item["DeviceID"].ToString();

                    if (desc.Contains("Silicon") && desc.Contains("UART Bridge"))
                    {
                        string com_port = deviceId;
                        com_port_list.Add(com_port);
                    }


                }
            }
            catch (ManagementException ex)
            {
                /* Do Nothing */
            }

            return com_port_list;
        }
    }


}


