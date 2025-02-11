using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SidderApp
{
    public partial class ScrollMessageBox : Form
    {
        public ScrollMessageBox(string title, string message)
        {
            InitializeComponent();
            this.Text = title;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            richTextBox1.Text = message;
        }
    }
}
