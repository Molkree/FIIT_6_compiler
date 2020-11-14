using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptExprMultZero : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            if (node is BinOpNode binOpNode && binOpNode.Op == OpType.MULT &&
                (binOpNode.Left is IntNumNode intNumLeft && intNumLeft.Num == 0 ||
                binOpNode.Right is IntNumNode intNumRight && intNumRight.Num == 0))
            {
                {
                    ReplaceExpr(binOpNode, new IntNumNode(0));
                }
            }
        }
    }
}
