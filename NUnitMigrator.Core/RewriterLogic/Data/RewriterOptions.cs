using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Core.RewriterLogic.Data
{
    public class RewriterOptions
    {
        public bool CommentUnsupported { get; set; }

        public RewriterOptions()
        {
            CommentUnsupported = false;
        }
    }
}
