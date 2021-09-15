using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Core.Rewriter
{
    public class UnsupportedNodeInfo
    {
        public string Info;

        public Location Location;

        public string NodeName;
    }
}
