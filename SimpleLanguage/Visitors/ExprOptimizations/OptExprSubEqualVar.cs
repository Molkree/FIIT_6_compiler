using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptExprSubEqualVar : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            // a - a => 0
            if (node is BinOpNode binop && binop.Op == OpType.MINUS
                && binop.Left is IdNode id1 && binop.Right is IdNode id2 && id1.Name == id2.Name)
            {
                if (id1.Name == id2.Name)
                {
                    ReplaceExpr(binop, new IntNumNode(0));
                }
            }
        }
    }
}
