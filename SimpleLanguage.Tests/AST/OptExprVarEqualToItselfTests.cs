﻿using NUnit.Framework;
using SimpleLanguage.Visitors;

namespace SimpleLanguage.Tests.AST
{
    internal class OptExprVarEqualToItselfTests : ASTTestsBase
    {
        [TestCase(@"
var a;
a = a == a;
",
            ExpectedResult = new[]
            {
                "var a;",
                "a = true;"
            },
            TestName = "EQUAL")]

        [TestCase(@"
var a;
a = a <= a;
",
            ExpectedResult = new[]
            {
                "var a;",
                "a = true;"
            },
            TestName = "EQLESS")]

        [TestCase(@"
var a;
a = a >= a;
",
            ExpectedResult = new[]
            {
                "var a;",
                "a = true;"
            },
            TestName = "EQGREATER")]

        public string[] TestOptExprVarEqualToItself(string sourceCode) =>
            TestASTOptimization(sourceCode, new OptExprVarEqualToItself());
    }
}
