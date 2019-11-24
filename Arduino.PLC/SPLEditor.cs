using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Xml.Serialization;
using System.Xml;

using Microsoft.Win32;
using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.IO.Compression;

namespace HelloApps
{

    public class CustomTabControl : TabControl
    {
        public bool UpdatedFlag = false;

        public HelloApps.GUI.CommandEditorClass CurCmdEditorClass
        {
            get
            {
                if (this.SelectedIndex >= 0)
                {
                    TabPage cur_page = this.TabPages[this.SelectedIndex];

                    if (cur_page.Controls.Count == 1)
                    {
                        if (cur_page.Controls[0].GetType().Name.EndsWith("PictureBox"))
                        {
                            PictureBox pb = (PictureBox)cur_page.Controls[0];
                            if (pb.Controls.Count == 1)
                            {
                                if (pb.Controls[0].GetType().Equals(typeof(HelloApps.GUI.CommandEditorClass)))
                                {
                                    return (HelloApps.GUI.CommandEditorClass)pb.Controls[0];
                                }
                                else
                                    return null;
                            }
                            else
                                return null;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
        }


        public WebBrowser CurWebBrowser
        {
            get
            {
                if (this.SelectedIndex >= 0)
                {
                    TabPage cur_page = this.TabPages[this.SelectedIndex];

                    if (cur_page.Controls.Count == 1)
                    {
                        if (cur_page.Controls[0].GetType().Name.EndsWith("PictureBox"))
                        {
                            PictureBox pb = (PictureBox)cur_page.Controls[0];
                            if (pb.Controls.Count == 1)
                            {
                                if (pb.Controls[0].GetType().Name.EndsWith("WebBrowser"))
                                {
                                    return (WebBrowser)pb.Controls[0];
                                }
                                else
                                    return null;
                            }
                            else
                                return null;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
        }



        public PictureBox ChildPictureBox
        {
            get
            {
                if (this.SelectedIndex >= 0)
                {
                    TabPage cur_page = this.TabPages[this.SelectedIndex];

                    if (cur_page.Controls.Count == 1)
                    {
                        if (cur_page.Controls[0].GetType().Name.EndsWith("PictureBox"))
                        {
                            PictureBox pb = (PictureBox)cur_page.Controls[0];
                            return pb;
                        }
                        else
                            return null;
                    }
                    else
                        return null;
                }
                else
                    return null;
            }
        }


        public void RemoveCurrentTab()
        {
            if (this.SelectedIndex >= 0)
            {
                this.TabPages.RemoveAt(this.SelectedIndex);
            }
        }

        public string GetTabTitle(string filepath)
        {
            if (!string.IsNullOrEmpty(filepath))
            {
                string filename = Path.GetFileName(filepath);

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
    }


    internal class WebBrowserInfo
    {
        public WebBrowser wb = null;
        public string url = string.Empty;

        public WebBrowserInfo(WebBrowser p_wb, string p_url)
        {
            wb = p_wb;
            url = p_url;
        }
    }

    internal class AddWebBrowserItem
    {
        public string FileName = string.Empty;
        public string Title = string.Empty;

        public AddWebBrowserItem(string fileName, string title)
        {
            FileName = fileName;
            Title = title;
        }
    }

}
