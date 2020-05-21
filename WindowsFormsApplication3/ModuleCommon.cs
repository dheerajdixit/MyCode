using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telerik.WinControls;

namespace NSA
{
    class ModuleCommon
    {   
        public static void ChangeThemeName(Control control, string themeName)
        {
            IComponentTreeHandler radControl = control as IComponentTreeHandler;
            if (radControl != null)
            {
                radControl.ThemeName = themeName;
            }
            foreach (Control child in control.Controls)
            {
                ChangeThemeName(child, themeName);
            }
        }
    }
}
