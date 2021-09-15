using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using NUnitMigrator.Tests;

namespace NUnitMigrator.Core
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
            //throw new Exception("Should not be run like this");
        }

    }
}
