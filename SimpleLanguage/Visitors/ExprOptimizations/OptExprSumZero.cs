﻿using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptExprSumZero : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            if (node is BinOpNode binOpNode && binOpNode.Op == OpType.PLUS)
            {
                if (binOpNode.Left is IntNumNode intNodeLeft && intNodeLeft.Num == 0)
                {
                    ReplaceExpr(binOpNode, binOpNode.Right);
                }
                else if (binOpNode.Right is IntNumNode intNodeRight && intNodeRight.Num == 0)
                {
                    ReplaceExpr(binOpNode, binOpNode.Left);
                }
            }
        }
    }
}
