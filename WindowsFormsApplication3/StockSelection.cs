using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using CommonFeatures;


using Model;

namespace NSA
{
    public partial class StockSelection : Telerik.WinControls.UI.RadForm
    {

        List<StockInventory> data = new List<StockInventory>();
        public StockSelection()
        {
            InitializeComponent();
        }
        static Settings s;
        private void StockSelection_Load(object sender, EventArgs e)
        {
            s = CommonFeatures.Common.GetSettings();
            data = Common.GetStocks();
            SetDataSource();
            radDropDownList1.SelectedIndex = s.RunMode;
            //radDropDownList1.ReadOnly = true;
            //radDropDownList1.Items.re
            ModuleCommon.ChangeThemeName(this, "TelerikMetroBlue");
        }

        private void RadCheckedListBox1_ItemCheckedChanged(object sender, Telerik.WinControls.UI.ListViewItemEventArgs e)
        {
            if (e.Item.CheckState == Telerik.WinControls.Enumerations.ToggleState.Off)
                data.Where(a => a.StockName == e.Item.Value.ToString()).First().Use = true;
            else if (e.Item.CheckState == Telerik.WinControls.Enumerations.ToggleState.On)
                data.Where(a => a.StockName == e.Item.Value.ToString()).First().Use = false;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            string json = JsonConvert.SerializeObject(data.ToArray());
            File.WriteAllText(Common.stockFileName, json);
            json = JsonConvert.SerializeObject(s);
            File.WriteAllText(Common.settingFileName, json);
        }

        private void RadSelectAll_CheckStateChanged(object sender, EventArgs e)
        {
            if (radSelectAll.Checked)
            {
                radSelectAll.Text = "Remove All";
                UseAll(true);

            }
            else if (!radSelectAll.Checked)
            {
                radSelectAll.Text = "Select All";
                UseAll(false);
            }
        }

        private void UseAll(bool decision)
        {
            data.ForEach((i) => { i.Use = decision; });
            SetDataSource();

        }

        private void SetDataSource()
        {
            radCheckedListBox1.DataSource = null;
            this.radCheckedListBox1.DisplayMember = "StockName";
            this.radCheckedListBox1.ValueMember = "StockName";
            this.radCheckedListBox1.CheckedMember = "IsSelected";
            radCheckedListBox1.DataSource = data;
            radCheckedListBox1.Refresh();
        }

        private void RadDropDownList1_SelectedIndexChanged(object sender, Telerik.WinControls.UI.Data.PositionChangedEventArgs e)
        {
            s.RunMode = radDropDownList1.SelectedIndex;
        }
    }


}
