using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace HelloApps
{
    public partial class SketchViewForm : Form
    {
        public SketchViewForm()
        {
            InitializeComponent();
        }
        

        public void SetText(string lines)
        {
            textBox1.Text = lines;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
