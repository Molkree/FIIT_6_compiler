using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleLanguage;
using SimpleLanguage.DataFlowAnalysis;
using SimpleLanguage.Visitors;
using SimpleParser;
using SimpleScanner;

namespace SimpleIDE
{
    using Optimization = Func<IReadOnlyList<Instruction>, (bool wasChanged, IReadOnlyList<Instruction> instructions)>;

    internal class Controller
    {
        internal static Parser GetParser(string sourceCode)
        {
            SymbolTable.vars.Clear();
            var scanner = new Scanner();
            scanner.SetSource(sourceCode, 0);
            var parser = new Parser(scanner);

            return parser;
        }

        private static readonly List<ChangeVisitor> ASTOptimizations = new List<ChangeVisitor>{
            new OptExprAlgebraic(),
            new OptExprEqualBoolNum(),
            new OptExprFoldUnary(),
            new OptExprMultDivByOne(),
            new OptExprMultZero(),
            new OptExprSimilarNotEqual(),
            new OptExprSubEqualVar(),
            new OptExprSumZero(),
            new OptExprTransformUnaryToValue(),
            new OptExprVarEqualToItself(),
            new OptExprWithOperationsBetweenConsts(),
            new IfNullElseNull(),
            new OptAssignEquality(),
            new OptStatIfFalse(),
            new OptStatIfTrue(),
            new OptWhileFalseVisitor()
        };

        internal static string GetASTWithOpt(Parser parser, List<int> lstCheck)
        {
            var b = parser.Parse();
            if (!b)
            {
                return "Error building AST";
            }

            var fillParents = new FillParentsVisitor();
            parser.root.Visit(fillParents);
            var listOpt = new List<ChangeVisitor>();
            if (lstCheck.Count > 0)
            {
                foreach (var n in lstCheck)
                {
                    listOpt.Add(ASTOptimizations[n]);
                }
                ASTOptimizer.Optimize(parser, listOpt);
            }

            var pp = new PrettyPrintVisitor();
            parser.root.Visit(pp);
            return pp.Text;
        }

        private static readonly List<Optimization> BasicBlockOptimizations = new List<Optimization>()
        {
            ThreeAddressCodeDefUse.DeleteDeadCode,
            DeleteDeadCodeWithDeadVars.DeleteDeadCode,
            ThreeAddressCodeRemoveAlgebraicIdentities.RemoveAlgebraicIdentities,
            ThreeAddressCodeCommonExprElimination.CommonExprElimination,
            ThreeAddressCodeCopyPropagation.PropagateCopies,
            ThreeAddressCodeConstantPropagation.PropagateConstants,
            ThreeAddressCodeFoldConstants.FoldConstants
        };

        private static readonly List<Optimization> AllCodeOptimizations = new List<Optimization>
        {
            ThreeAddressCodeGotoToGoto.ReplaceGotoToGoto,
            ThreeAddressCodeRemoveGotoThroughGoto.RemoveGotoThroughGoto,
            ThreeAddressCodeRemoveNoop.RemoveEmptyNodes
        };

        internal static (string str, IReadOnlyList<Instruction> instructions) GetTACWithOpt(
            Parser parser, List<int> lstCheck)
        {
            ThreeAddressCodeTmp.ResetTmpLabel();
            ThreeAddressCodeTmp.ResetTmpName();
            var threeAddrCodeVisitor = new ThreeAddrGenVisitor();
            parser.root.Visit(threeAddrCodeVisitor);
            var threeAddressCode = threeAddrCodeVisitor.Instructions;


            if (lstCheck.Count > 0)
            {
                List<Optimization> bBlOpt = new List<Optimization>(),
                    allCodeOpt = new List<Optimization>(), allAllOpt = new List<Optimization>();
                var numPos = BasicBlockOptimizations.Count;
                var numPosFalse = BasicBlockOptimizations.Count + AllCodeOptimizations.Count;

                foreach (var n in lstCheck.TakeWhile(x => x < numPos))
                {
                    bBlOpt.Add(BasicBlockOptimizations[n]);
                    allAllOpt.Add(BasicBlockOptimizations[n]);
                }

                foreach (var n in lstCheck.SkipWhile(x => x < numPos).TakeWhile(x => x < numPosFalse))
                {
                    allCodeOpt.Add(AllCodeOptimizations[n - numPos]);
                    allAllOpt.Add(AllCodeOptimizations[n - numPos]);
                }

                var UCE = lstCheck[^1] == numPosFalse;

                var result = ThreeAddressCodeOptimizer.Optimize(threeAddressCode,
                        bBlOpt, allCodeOpt, UCE).ToList();

                var strR = new StringBuilder();
                foreach (var x in result)
                {
                    _ = strR.AppendLine(x.ToString());
                }
                return (strR.ToString(), threeAddressCode);
            }

            var str = new StringBuilder();
            foreach (var x in threeAddressCode)
            {
                _ = str.AppendLine(x.ToString());
            }

            return (str.ToString(), threeAddressCode);
        }

        internal static (string str, ControlFlowGraph cfg) BuildCFG(IReadOnlyList<Instruction> instructions)
        {
            var divResult = BasicBlockLeader.DivideLeaderToLeader(instructions);
            var cfg = new ControlFlowGraph(divResult);

            var str = new StringBuilder();

            foreach (var block in cfg.GetCurrentBasicBlocks())
            {
                _ = str.AppendLine($"{cfg.VertexOf(block)} --------");
                foreach (var inst in block.GetInstructions())
                {
                    _ = str.AppendLine(inst.ToString());
                }
                _ = str.AppendLine($"----------");

                var children = cfg.GetChildrenBasicBlocks(cfg.VertexOf(block));

                var childrenStr = string.Join(" | ", children.Select(v => v.vertex));
                _ = str.AppendLine($" children: {childrenStr}");

                var parents = cfg.GetParentsBasicBlocks(cfg.VertexOf(block));
                var parentsStr = string.Join(" | ", parents.Select(v => v.vertex));
                _ = str.AppendLine($" parents: {parentsStr}\r\n");
            }

            return (str.ToString(), cfg);
        }

        internal static string GetGraphInformation(ControlFlowGraph cfg)
        {
            var str = new StringBuilder();

            _ = str.AppendLine("Доминаторы:");
            var domTree = new DominatorTree().GetDominators(cfg);

            foreach (var pair in domTree)
            {
                foreach (var x in pair.Value)
                {
                    _ = str.AppendLine($"{cfg.VertexOf(x)} dom {cfg.VertexOf(pair.Key)}");
                }
                _ = str.AppendLine("----------------");
            }


            _ = str.AppendLine("\r\nКлассификация рёбер:");

            foreach (var pair in cfg.ClassifiedEdges)
            {
                _ = str.AppendLine($"{ pair }");
            }

            _ = str.AppendLine("\r\nОбходы графа:");

            _ = str.AppendLine($"Прямой: { string.Join(" -> ", cfg.PreOrderNumeration) }");
            _ = str.AppendLine($"Обратный: { string.Join(" -> ", cfg.PostOrderNumeration) }");

            _ = str.AppendLine($"\r\nГлубинное остовное дерево:");
            foreach (var (from, to) in cfg.DepthFirstSpanningTree)
            {
                _ = str.AppendLine($"({from} - > {to})");
            }

            var backEdges = cfg.GetBackEdges();
            if (backEdges.Count > 0)
            {
                _ = str.AppendLine("\r\nОбратные рёбра:");
                foreach (var x in backEdges)
                {
                    _ = str.AppendLine($"({cfg.VertexOf(x.Item1)}, {cfg.VertexOf(x.Item2)})");
                }
            }
            else
            {
                _ = str.AppendLine("\r\nОбратных рёбер нет");
            }


            var answ = cfg.IsReducibleGraph() ? "Граф приводим" : "Граф неприводим";
            _ = str.AppendLine($"\r\n{answ}");

            if (cfg.IsReducibleGraph())
            {
                var natLoops = NaturalLoop.GetAllNaturalLoops(cfg);
                if (natLoops.Count > 0)
                {
                    _ = str.AppendLine($"\r\nЕстественные циклы:");
                    foreach (var x in natLoops)
                    {
                        if (x.Count == 0)
                        {
                            continue;
                        }
                        for (var i = 0; i < x.Count; i++)
                        {
                            _ = str.AppendLine($"Номер блока: {i}");
                            foreach (var xfrom in x[i].GetInstructions())
                            {
                                _ = str.AppendLine(xfrom.ToString());
                            }
                        }
                        _ = str.AppendLine();
                        _ = str.AppendLine("-------------");
                    }
                }
                else
                {
                    _ = str.AppendLine($"\r\nЕстественных циклов нет");
                }
            }
            else
            {
                _ = str.AppendLine($"\r\nНевозможно определить естественные циклы, т.к. граф неприводим");
            }

            return str.ToString();
        }

        internal static (string, string) ApplyIterativeAlgorithm(ControlFlowGraph cfg, List<string> opts)
        {
            var strReturn = new StringBuilder();
            var strBefore = new StringBuilder();


            foreach (var b in cfg.GetCurrentBasicBlocks())
            {
                foreach (var inst in b.GetInstructions())
                {
                    _ = strBefore.AppendLine(inst.ToString());
                }
                _ = strBefore.AppendLine("----------");
            }

            foreach (var opt in opts)
            {
                switch (opt)
                {
                    case "Доступные выражения":
                        var inout = new AvailableExpressions().Execute(cfg);
                        AvailableExpressionsOptimization.Execute(cfg, inout);
                        break;
                    case "Активные переменные":
                        LiveVariablesOptimization.DeleteDeadCode(cfg);
                        break;
                    case "Достигающие определения":
                        ReachingDefinitionsOptimization.DeleteDeadCode(cfg);
                        break;
                    default:
                        break;
                }
            }


            foreach (var b in cfg.GetCurrentBasicBlocks())
            {
                foreach (var inst in b.GetInstructions())
                {
                    _ = strReturn.AppendLine(inst.ToString());
                }
                _ = strReturn.AppendLine("----------");
            }

            return (strBefore.ToString(), strReturn.ToString());
        }
    }
}
