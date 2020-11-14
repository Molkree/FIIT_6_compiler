using System.Collections.Generic;
using System.Linq;

namespace SimpleLanguage.DataFlowAnalysis
{
    /// <summary>
    /// Оптимизация по доступным выражениям
    /// </summary>
    public static class AvailableExpressionsOptimization
    {
        /// <summary>
        /// Применение оптимизации
        /// </summary>
        /// <param name="controlFlowGraph">Граф потока управления</param>
        /// <param name="inOutData">In Out - данные о доступных выражениях</param>
        public static void Execute(ControlFlowGraph controlFlowGraph, InOutData<List<OneExpression>> inOutData)
        {
            if (!controlFlowGraph.IsReducibleGraph())
            {
                return;
            }
            GraphTraversing(controlFlowGraph, inOutData);
        }

        private static int targetInstructionIndex; // для алгоритма
        private static BasicBlock targetBlock;     // для алгоритма
        private static readonly List<(BasicBlock block, int numberInstruction, Instruction instruction)> listOfPointsInBlock
                = new List<(BasicBlock, int, Instruction)>();

        private static void GraphTraversing(ControlFlowGraph controlFlowGraph, InOutData<List<OneExpression>> inOutData)
        {
            foreach (var block in controlFlowGraph.GetCurrentBasicBlocks())
            {
                var instructions = block.GetInstructions();
                foreach (var expression in inOutData[block].In)
                {
                    if (ContainsExpressionInInstructions(instructions.ToList(), expression))
                    {
                        targetBlock = block;
                        if (OpenBlock(controlFlowGraph, block, expression))
                        {
                            ChangeInstructionsInGraph();
                        }
                    }
                    listOfPointsInBlock.Clear();
                }
            }
        }

        private static bool OpenBlock(ControlFlowGraph controlFlowGraph, BasicBlock block, OneExpression expression)
        {
            var stack = new Stack<(BasicBlock block, List<BasicBlock> way)>();
            foreach (var bblock in controlFlowGraph.GetParentsBasicBlocks(controlFlowGraph.VertexOf(block)))
            {
                var way = new List<BasicBlock>() { bblock.block };
                stack.Push((bblock.block, way));
            }
            while (stack.Count != 0)
            {
                var element = stack.Pop();
                if (!IsContainedInListOfInstr(element.block.GetInstructions(), expression, element.block))
                {
                    foreach (var parent in controlFlowGraph.GetParentsBasicBlocks(controlFlowGraph.VertexOf(element.block)))
                    {
                        if (parent.block == targetBlock)
                        {
                            return false;
                        }
                        if (!element.way.Contains(parent.block))
                        {
                            stack.Push((parent.block, new List<BasicBlock>(element.way) { parent.block }));
                        }
                    }
                }
            }
            return true;
        }

        private static void ChangeInstructionsInGraph()
        {
            var tmpVariable = ThreeAddressCodeTmp.GenTmpName();
            foreach (var (block, numberInstruction, instruction) in listOfPointsInBlock)
            {
                block.RemoveInstructionByIndex(numberInstruction);
                block.InsertInstruction(numberInstruction,
                    new Instruction("", "assign", tmpVariable, "", instruction.Result));
                block.InsertInstruction(numberInstruction,
                    new Instruction(instruction.Label, instruction.Operation, instruction.Argument1,
                    instruction.Argument2, tmpVariable));
            }
            var targetTemp = targetBlock.GetInstructions()[targetInstructionIndex];
            targetBlock.RemoveInstructionByIndex(targetInstructionIndex);
            targetBlock.InsertInstruction(targetInstructionIndex,
                new Instruction(targetTemp.Label, "assign", tmpVariable, "", targetTemp.Result));
        }

        private static bool InstructionContainsExpression(Instruction instruction, OneExpression expression)
           =>
            instruction.Operation == expression.Operation && (instruction.Operation == "MINUS" || instruction.Operation == "DIV")
            && instruction.Argument1 == expression.Argument1 && instruction.Argument2 == expression.Argument2
            ||
            instruction.Operation == expression.Operation && (instruction.Operation != "MINUS" || instruction.Operation != "DIV")
            && (instruction.Argument1 == expression.Argument1 && instruction.Argument2 == expression.Argument2
            || instruction.Argument1 == expression.Argument2 && instruction.Argument2 == expression.Argument1);

        private static bool ContainsExpressionInInstructions(List<Instruction> instructions, OneExpression expression)
        {
            for (var i = 0; i < instructions.Count; i++)
            {
                if (InstructionContainsExpression(instructions[i], expression))
                {
                    targetInstructionIndex = i;
                    return true;
                }
            }
            return false;
        }

        private static bool IsContainedInListOfInstr(IReadOnlyList<Instruction> instructions,
            OneExpression expression,
            BasicBlock block)
        {
            for (var i = instructions.Count - 1; i >= 0; i--)
            {
                if (InstructionContainsExpression(instructions[i], expression))
                {
                    if (!listOfPointsInBlock.Select(element => element.block).Contains(block))
                    {
                        listOfPointsInBlock.Add((block, i, instructions[i]));
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
