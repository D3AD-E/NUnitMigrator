using Microsoft.CodeAnalysis.CSharp.Syntax;
using static NUnitMigrator.Core.Rewriter.RewriterStates;

namespace NUnitMigrator.Core.Rewriter
{
    public class ExceptionSyntaxData : IClearable
    {
        public bool Supported { get; set; }

        public string TypeName { get; set; }
        public MatchType Match { get; set; }
        public ArgumentListSyntax MatchArguments { get; set; }
        public string MatchTarget { get; set; }
        public ArgumentListSyntax MatchTargetArguments { get; set; }

        public ExceptionSyntaxData()
        {
            Supported = true;
        }
        public void Clear()
        {
            Supported = true;
            MatchTarget = null;
            Match = MatchType.None;
            MatchArguments = null;
            MatchTargetArguments = null;
        }

        public enum MatchType
        {
            None,
            Matches,
            EqualTo,
            Contains,
            StartsWith,
            EndsWith
        }
    }
}