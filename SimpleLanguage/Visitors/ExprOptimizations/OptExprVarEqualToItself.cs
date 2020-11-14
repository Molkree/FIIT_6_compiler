using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptExprVarEqualToItself : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            // Equality to itself   a == a, a <= a, a >= a
            if (node is BinOpNode binop && binop.Left is IdNode Left && binop.Right is IdNode Right &&
                Left.Name == Right.Name &&
                (binop.Op == OpType.EQUAL || binop.Op == OpType.EQLESS || binop.Op == OpType.EQGREATER))
            {
                ReplaceExpr(binop, new BoolValNode(true));
            }
        }
    }
}
