using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitMigrator.Core.RewriterLogic
{
    public class UnsupportedNodeInfo
    {
        public string Info;

        public Location Location;

        public string NodeName;

        public override string ToString()
        {
            return $"{Info} at [{Location.GetLineSpan().StartLinePosition.Line}:{Location.GetLineSpan().StartLinePosition.Character}] for {NodeName}";
        }
    }
}
