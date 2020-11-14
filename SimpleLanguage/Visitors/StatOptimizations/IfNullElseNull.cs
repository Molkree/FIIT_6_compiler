using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class IfNullElseNull : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            if (node is IfElseNode ifn &&
                (ifn.FalseStat is EmptyNode || ifn.FalseStat == null) &&
                (ifn.TrueStat is EmptyNode || ifn.TrueStat == null))
            {
                ReplaceStat(ifn, new EmptyNode());
            }
        }
    }
}
