using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Extention
{
    public class OptionsGrid : DialogPage
    {
        private bool _commentUnsupported = false;

        [Category("NUnitMigrator")]
        [DisplayName("Force run")]
        [Description("Comment unsupported nodes")]
        public bool CommentUnsupported
        {
            get { return _commentUnsupported; }
            set { _commentUnsupported = value; }
        }
    }
}
