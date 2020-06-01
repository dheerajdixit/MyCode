using _15MCE;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NSA
{
    public partial class Selector : Form
    {
        public Selector()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TestStrategy t = new TestStrategy();
            t.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TScan t = new TScan();
            t.Show();
        }
    }
}
