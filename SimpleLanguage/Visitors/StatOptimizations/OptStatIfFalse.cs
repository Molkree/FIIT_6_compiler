using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptStatIfFalse : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            // if (false) st1; else st2; => st2;
            if (node is IfElseNode ifNode &&
                ifNode.Expr is BoolValNode boolNode && boolNode.Val == false)
            {
                if (ifNode.FalseStat != null)
                {
                    ReplaceStat(ifNode, ifNode.FalseStat);
                }
                else
                {
                    ReplaceStat(ifNode, new EmptyNode());
                }
            }
        }
    }
}
