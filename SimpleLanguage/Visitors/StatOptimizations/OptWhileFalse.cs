using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptWhileFalseVisitor : ChangeVisitor
    {
        public override void PostVisit(Node nd)
        {
            if (!(nd is WhileNode n))
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
