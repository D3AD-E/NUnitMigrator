using NUnitMigrator.App.Logic;
using PowerArgs;
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
            Args.InvokeMain<ParseMain>(args);
           
        }
    }
}
