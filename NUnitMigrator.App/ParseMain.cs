using NUnitMigrator.App.Logic;
using PowerArgs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NUnitMigrator.App
{
    [ArgExceptionBehavior(ArgExceptionPolicy.StandardExceptionHandling)]
    class ParseMain
    {
        [HelpHook, ArgShortcut("-?"), ArgDescription("Shows this help")]
        public bool Help { get; set; }

        [ArgShortcut("-c"), ArgDescription("Comment unsupported nodes")]
        public bool CommentUnsupported { get; set; }
        public void Main()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Solution file|*.sln;";
            if (DialogResult.OK == dialog.ShowDialog())
            {
                string path = dialog.FileName;
                string project = null;
                dialog = new OpenFileDialog();
                dialog.Filter = "Project file|*.csproj;";
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    project = Path.GetFileNameWithoutExtension(dialog.SafeFileName);
                }
                MainLogic.Migrate(path, project, new Core.RewriterLogic.Data.RewriterOptions { CommentUnsupported = CommentUnsupported });
            }
            Console.ReadLine();
        }
    }
}
