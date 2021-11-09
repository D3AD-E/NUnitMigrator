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
            if (DialogResult.OK == dialog.ShowDialog())
            {
                string path = dialog.FileName;
                MainLogic.Migrate(path, new Core.RewriterLogic.Data.RewriterOptions { CommentUnsupported = CommentUnsupported });
            }
            Console.ReadLine();
        }
    }
}
