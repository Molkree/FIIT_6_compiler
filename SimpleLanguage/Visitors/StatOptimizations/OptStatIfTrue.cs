using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptStatIfTrue : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            // if (true) st1; else st2
            if (node is IfElseNode ifNode && ifNode.Expr is BoolValNode boolNode && boolNode.Val)
            {
                ReplaceStat(ifNode, ifNode.TrueStat);
            }
        }
    }
}
