using System;
using System.Collections.Generic;
using System.Globalization;

namespace SimpleLanguage
{
    public static class ThreeAddressCodeFoldConstants
    {
        public static (bool wasChanged, IReadOnlyList<Instruction> instruction) FoldConstants(IReadOnlyCollection<Instruction> instructions)
        {
            var wasChanged = false;
            var result = new List<Instruction>();
            foreach (var instruction in instructions)
            {
                if (instruction.Argument2 != "")
                {
                    if (int.TryParse(instruction.Argument1, out var intArg1) && int.TryParse(instruction.Argument2, out var intArg2))
                    {
                        var constant = CalculateConstant(instruction.Operation, intArg1, intArg2);
                        result.Add(new Instruction(instruction.Label, "assign", constant, "", instruction.Result));
                        wasChanged = true;
                        continue;
                    }
                    else if (bool.TryParse(instruction.Argument1, out var boolArg1) && bool.TryParse(instruction.Argument2, out var boolArg2))
                    {
                        var constant = CalculateConstant(instruction.Operation, boolArg1, boolArg2);
                        result.Add(new Instruction(instruction.Label, "assign", constant, "", instruction.Result));
                        wasChanged = true;
                        continue;
                    }
                }
                result.Add(instruction);
            }

            return (wasChanged, result);
        }

        private static string CalculateConstant(string operation, bool boolArg1, bool boolArg2) => operation switch
        {
            "OR" => (boolArg1 || boolArg2).ToString(),
            "AND" => (boolArg1 && boolArg2).ToString(),
            "EQUAL" => (boolArg1 == boolArg2).ToString(),
            "NOTEQUAL" => (boolArg1 != boolArg2).ToString(),
            _ => throw new InvalidOperationException(),
        };

        private static string CalculateConstant(string operation, int intArg1, int intArg2) => operation switch
        {
            "EQUAL" => (intArg1 == intArg2).ToString(),
            "NOTEQUAL" => (intArg1 != intArg2).ToString(),
            "LESS" => (intArg1 < intArg2).ToString(),
            "GREATER" => (intArg1 > intArg2).ToString(),
            "EQGREATER" => (intArg1 >= intArg2).ToString(),
            "EQLESS" => (intArg1 <= intArg2).ToString(),
            "PLUS" => (intArg1 + intArg2).ToString(CultureInfo.InvariantCulture),
            "MINUS" => (intArg1 - intArg2).ToString(CultureInfo.InvariantCulture),
            "MULT" => (intArg1 * intArg2).ToString(CultureInfo.InvariantCulture),
            "DIV" => (intArg1 / intArg2).ToString(CultureInfo.InvariantCulture),
            _ => throw new InvalidOperationException(),
        };
    }
}
