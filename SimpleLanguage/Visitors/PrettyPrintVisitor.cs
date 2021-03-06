﻿using System;
using System.Globalization;
using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class PrettyPrintVisitor : Visitor
    {
        public string Text { get; set; } = "";
        private int Indent;

        private string IndentStr() => new string(' ', Indent);
        private void IndentPlus() => Indent += 4;
        private void IndentMinus() => Indent -= 4;
        public override void VisitIdNode(IdNode id) => Text += id.Name;
        public override void VisitIntNumNode(IntNumNode num) => Text += num.Num.ToString(CultureInfo.CurrentCulture);

        private static string GetOp(OpType t) => t switch
        {
            OpType.OR => "or",
            OpType.AND => "and",
            OpType.EQUAL => "==",
            OpType.NOTEQUAL => "!=",
            OpType.GREATER => ">",
            OpType.LESS => "<",
            OpType.EQGREATER => ">=",
            OpType.EQLESS => "<=",
            OpType.PLUS => "+",
            OpType.MINUS or OpType.UNMINUS => "-",
            OpType.MULT => "*",
            OpType.DIV => "/",
            OpType.NOT => "!",
            _ => throw new ArgumentException("Unknown operation"),
        };

        public override void VisitBinOpNode(BinOpNode binop)
        {
            Text += "(";
            binop.Left.Visit(this);
            Text += " " + GetOp(binop.Op) + " ";
            binop.Right.Visit(this);
            Text += ")";
        }

        public override void VisitUnOpNode(UnOpNode unop)
        {
            Text += "(" + GetOp(unop.Op);
            unop.Expr.Visit(this);
            Text += ")";
        }

        public override void VisitAssignNode(AssignNode a)
        {
            Text += IndentStr();
            a.Id.Visit(this);
            Text += " = ";
            a.Expr.Visit(this);
            Text += ";";
        }

        public override void VisitBlockNode(BlockNode bl)
        {
            Text += "{" + Environment.NewLine;
            IndentPlus();
            bl.List.Visit(this);
            IndentMinus();
            Text += Environment.NewLine + IndentStr() + "}";
        }

        public override void VisitStListNode(StListNode bl)
        {
            var Count = bl.StatChildren.Count;
            if (Count > 0)
            {
                bl.StatChildren[0].Visit(this);
            }
            for (var i = 1; i < Count; i++)
            {
                Text += Environment.NewLine;
                bl.StatChildren[i].Visit(this);
            }
        }

        public override void VisitVarListNode(VarListNode varList)
        {
            Text += IndentStr() + "var " + varList.Vars[0].Name;
            for (var i = 1; i < varList.Vars.Count; i++)
            {
                Text += ", " + varList.Vars[i].Name;
            }
            Text += ";";
        }

        public override void VisitForNode(ForNode f)
        {
            Text += IndentStr() + "for ";
            f.Id.Visit(this);
            Text += " = ";
            f.From.Visit(this);
            Text += ", ";
            f.To.Visit(this);
            if (f.Stat is BlockNode)
            {
                Text += " ";
                f.Stat.Visit(this);
            }
            else
            {
                IndentPlus();
                Text += Environment.NewLine;
                f.Stat.Visit(this);
                IndentMinus();
            }
        }

        public override void VisitWhileNode(WhileNode w)
        {
            Text += IndentStr() + "while ";
            w.Expr.Visit(this);

            if (w.Stat is BlockNode)
            {
                Text += " ";
                w.Stat.Visit(this);
            }
            else
            {
                IndentPlus();
                Text += Environment.NewLine;
                w.Stat.Visit(this);
                IndentMinus();
            }
        }

        public override void VisitLabelstatementNode(LabelStatementNode l)
        {
            Text += IndentStr();
            l.Label.Visit(this);
            Text += ": ";
            l.Stat.Visit(this);
        }

        public override void VisitIfElseNode(IfElseNode i)
        {
            Text += IndentStr() + "if ";
            i.Expr.Visit(this);

            if (i.TrueStat is BlockNode)
            {
                Text += " ";
                i.TrueStat.Visit(this);
            }
            else
            {
                IndentPlus();
                Text += Environment.NewLine;
                i.TrueStat.Visit(this);
                IndentMinus();
            }
            if (i.FalseStat == null)
            {
                return;
            }
            Text += Environment.NewLine + IndentStr() + "else";
            if (i.FalseStat is BlockNode)
            {
                Text += " ";
                i.FalseStat.Visit(this);
            }
            else
            {
                IndentPlus();
                Text += Environment.NewLine;
                i.FalseStat.Visit(this);
                IndentMinus();
            }
        }

        public override void VisitGotoNode(GotoNode g)
        {
            Text += IndentStr() + "goto ";
            g.Label.Visit(this);
            Text += ";";
        }

        public override void VisitPrintNode(PrintNode p)
        {
            Text += IndentStr() + "print(";
            p.ExprList.Visit(this);
            Text += ");";
        }

        public override void VisitExprListNode(ExprListNode e)
        {
            e.ExprChildren[0].Visit(this);
            for (var i = 1; i < e.ExprChildren.Count; ++i)
            {
                Text += ", ";
                e.ExprChildren[i].Visit(this);
            }

        }

        public override void VisitInputNode(InputNode i)
        {
            Text += IndentStr() + "input(";
            i.Ident.Visit(this);
            Text += ");";
        }

        public override void VisitBoolValNode(BoolValNode b) => Text += b.Val.ToString().ToLower(CultureInfo.CurrentCulture);
    }
}
