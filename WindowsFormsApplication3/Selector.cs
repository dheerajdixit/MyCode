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
            TestStrategy t = new TestStrategy(settings);
            t.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            TScan t = new TScan(settings);
            t.Show();


        }

        Dictionary<string, string> settings = new Dictionary<string, string>();

        private void Selector_Load(object sender, EventArgs e)
        {
            settings.Clear();
            int x = 20;
            int y = 20;
            int i = 0;
            foreach (var c in System.Configuration.ConfigurationSettings.AppSettings)
            {
               
                Label l = new Label();
                l.Name = "lbl" + c;
                l.Text = c.ToString();
                l.Location = new Point(x, y);
                //  l.Margin = new Padding { Left = 20, Top = 20, Bottom = 0, Right = 0 };



                TextBox t = new TextBox();
                t.Name = "txt" + c;
                t.Text = System.Configuration.ConfigurationSettings.AppSettings[c.ToString()];
                t.Margin = new Padding { Left = 20, Top = 0, Bottom = 0, Right = 0 };
                t.Location = new Point(x + 100, y);
                t.TextChanged += T_TextChanged;
                groupBox1.Controls.Add(l);
                groupBox1.Controls.Add(t);

                x = x + 200;

                if (i % 2 == 0)
                {
                    y = y + 30;
                    x = 20;
                }
                i++;
                settings.Add(c.ToString(), System.Configuration.ConfigurationSettings.AppSettings[c.ToString()]);
            }
        }

        private void T_TextChanged(object sender, EventArgs e)
        {
            settings[((TextBox)sender).Name] = ((TextBox)sender).Text;
        }
    }
}
