using System.Collections.Generic;
using SimpleLanguage.Visitors;
using SimpleParser;

namespace SimpleLanguage
{
    public static class ASTOptimizer
    {
        private static IReadOnlyList<ChangeVisitor> ASTOptimizations { get; } = new List<ChangeVisitor>
        {
            new OptExprVarEqualToItself(),
            new OptExprMultDivByOne(),
            new OptExprMultZero(),
            new OptExprSumZero(),
            new OptExprWithOperationsBetweenConsts(),
            new OptStatIfTrue(),
            new OptStatIfFalse(),
            new OptExprEqualBoolNum(),
            new OptWhileFalseVisitor(),
            new OptExprSimilarNotEqual(),
            new OptAssignEquality(),
            new IfNullElseNull(),
            new OptExprTransformUnaryToValue(),
            new OptExprFoldUnary(),
            new OptExprAlgebraic(),
            new OptExprSubEqualVar()
        };

        public static void Optimize(Parser parser, IReadOnlyList<ChangeVisitor> Optimizations = null)
        {
            Optimizations ??= ASTOptimizations;
            var optInd = 0;
            do
            {
                parser.root.Visit(Optimizations[optInd]);
                if (Optimizations[optInd].Changed)
                {
                    optInd = 0;
                }
                else
                {
                    ++optInd;
                }
            } while (optInd < Optimizations.Count);
        }
    }
}
