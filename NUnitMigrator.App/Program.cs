using NUnitMigrator.App.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUnitMigrator.App
{
    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            if (DialogResult.OK == dialog.ShowDialog())
            {
                string path = dialog.FileName;
                MainLogic.Migrate(path);
            }
            Console.ReadLine();
        }
    }
}
