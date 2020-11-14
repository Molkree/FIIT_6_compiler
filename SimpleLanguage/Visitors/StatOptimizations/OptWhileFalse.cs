using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptWhileFalseVisitor : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            if (node is not WhileNode n)
            {
                return;
            }

            if (n.Expr is BoolValNode bn && !bn.Val)
            {
                ReplaceStat(n, new EmptyNode());
            }
        }
    }
}
