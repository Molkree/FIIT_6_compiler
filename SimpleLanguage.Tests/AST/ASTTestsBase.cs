using System;
using SimpleLanguage.Visitors;
using SimpleParser;
using SimpleScanner;

namespace SimpleLanguage.Tests.AST
{
    public class ASTTestsBase
    {
        protected static Parser BuildAST(string sourceCode)
        {
            SymbolTable.vars.Clear();
            var scanner = new Scanner();
            scanner.SetSource(sourceCode, 0);
            var parser = new Parser(scanner);
            _ = parser.Parse();
            var fillParents = new FillParentsVisitor();
            parser.root.Visit(fillParents);
            return parser;
        }

        protected static string[] ApplyAstOpt(Parser AST, ChangeVisitor opt)
        {
            AST.root.Visit(opt);
            var pp = new PrettyPrintVisitor();
            AST.root.Visit(pp);
            return pp.Text.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        protected static string[] TestASTOptimization(string sourceCode, ChangeVisitor optimization) =>
            ApplyAstOpt(BuildAST(sourceCode), optimization);
    }
}
