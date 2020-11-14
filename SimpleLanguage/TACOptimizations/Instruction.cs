namespace SimpleLanguage
{
    public class Instruction
    {
        public string Label { get; internal set; }
        public string Operation { get; }
        public string Argument1 { get; }
        public string Argument2 { get; }
        public string Result { get; }

        public Instruction(string label, string operation, string argument1, string argument2, string result)
        {
            Label = label;
            Operation = operation;
            Argument1 = argument1;
            Argument2 = argument2;
            Result = result;
        }

        public Instruction Copy() => new Instruction(Label, Operation, Argument1, Argument2, Result);

        public override bool Equals(object obj) =>
            obj != null
            && obj is Instruction instruction
            && Label == instruction.Label
            && Operation == instruction.Operation
            && Argument1 == instruction.Argument1
            && Argument2 == instruction.Argument2
            && Result == instruction.Result;

        public override int GetHashCode() =>
            Label.GetHashCode()
            ^ Operation.GetHashCode()
            ^ Argument1.GetHashCode()
            ^ Argument2.GetHashCode()
            ^ Result.GetHashCode();

        public override string ToString()
        {
            var label = Label != "" ? Label + ": " : "";
            return Operation switch
            {
                "assign" => label + Result + " = " + Argument1,
                "ifgoto" => $"{label}if {Argument1} goto {Argument2}",
                "goto" => $"{label}goto {Argument1}",
                "input" => $"{label}input {Result}",
                "print" => $"{label}print {Argument1}",
                "NOT" or "UNMINUS" => $"{label}{Result} = {ConvertToMathNotation(Operation)}{Argument1}",
                "OR" or "AND" or "EQUAL" or "NOTEQUAL" or "LESS" or "GREATER" or "EQGREATER" or "EQLESS" or "PLUS" or "MINUS" or "MULT" or "DIV" => $"{label}{Result} = {Argument1} {ConvertToMathNotation(Operation)} {Argument2}",
                "noop" => $"{label}noop",
                _ => $"label: {Label}; op {Operation}; arg1: {Argument1}; arg2: {Argument2}; res: {Result}",
            };
        }

        private static string ConvertToMathNotation(string operation) => operation switch
        {
            "OR" => "or",
            "AND" => "and",
            "EQUAL" => "==",
            "NOTEQUAL" => "!=",
            "LESS" => "<",
            "GREATER" => ">",
            "EQGREATER" => ">=",
            "EQLESS" => "<=",
            "PLUS" => "+",
            "MINUS" or "UNMINUS" => "-",
            "MULT" => "*",
            "DIV" => "/",
            "NOT" => "!",
            _ => operation,
        };
    }
}
