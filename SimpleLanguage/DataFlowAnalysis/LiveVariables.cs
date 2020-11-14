using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLanguage
{
    public class InOutSet
    {
        public HashSet<string> IN { get; set; }
        public HashSet<string> OUT { get; set; }

        public InOutSet()
        {
            IN = new HashSet<string>();
            OUT = new HashSet<string>();
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            _ = str.Append('{');
            foreach (var i in IN)
            {
                _ = str.Append($" {i}");
            }
            _ = str.Append(" } ");
            _ = str.Append('{');
            foreach (var i in OUT)
            {
                _ = str.Append($" {i}");
            }
            _ = str.Append(" } ");

            return str.ToString();
        }
    }

    internal class DefUseSet
    {
        public HashSet<string> Def { get; set; }
        public HashSet<string> Use { get; set; }

        public DefUseSet()
        {
            Def = new HashSet<string>();
            Use = new HashSet<string>();
        }
        public DefUseSet((HashSet<string> def, HashSet<string> use) a)
        {
            Def = a.def;
            Use = a.use;
        }
    }

    public class LiveVariables : GenericIterativeAlgorithm<HashSet<string>>
    {
        /// <inheritdoc/>
        public override Func<HashSet<string>, HashSet<string>, HashSet<string>> CollectingOperator =>
            (a, b) => a.Union(b).ToHashSet();

        /// <inheritdoc/>
        public override Func<HashSet<string>, HashSet<string>, bool> Compare =>
            (a, b) => a.SetEquals(b);

        /// <inheritdoc/>
        public override HashSet<string> Init { get => new HashSet<string>(); protected set { } }

        /// <inheritdoc/>
        public override Func<BasicBlock, HashSet<string>, HashSet<string>> TransferFunction { get; protected set; }

        /// <inheritdoc/>
        public override Direction Direction => Direction.Backward;

        public Dictionary<int, InOutSet> DictInOut { get; set; }

        public void ExecuteInternal(ControlFlowGraph cfg)
        {
            var blocks = cfg.GetCurrentBasicBlocks();
            var transferFunc = new LiveVariablesTransferFunc(cfg);

            foreach (var x in blocks)
            {
                DictInOut.Add(cfg.VertexOf(x), new InOutSet());
            }

            var isChanged = true;
            while (isChanged)
            {
                isChanged = false;
                for (var i = blocks.Count - 1; i >= 0; --i)
                {
                    var children = cfg.GetChildrenBasicBlocks(i);

                    DictInOut[i].OUT =
                        children
                        .Select(x => DictInOut[x.vertex].IN)
                        .Aggregate(new HashSet<string>(), (a, b) => a.Union(b).ToHashSet());

                    var pred = DictInOut[i].IN;
                    DictInOut[i].IN = transferFunc.Transfer(blocks[i], DictInOut[i].OUT);
                    isChanged = !DictInOut[i].IN.SetEquals(pred) || isChanged;
                }
            }
        }

        public override InOutData<HashSet<string>> Execute(ControlFlowGraph graph, bool useRenumbering = true)
        {
            TransferFunction = new LiveVariablesTransferFunc(graph).Transfer;
            return base.Execute(graph);
        }

        public LiveVariables() => DictInOut = new Dictionary<int, InOutSet>();

        public string ToString(ControlFlowGraph cfg)
        {
            var str = new StringBuilder();

            foreach (var x in cfg.GetCurrentBasicBlocks())
            {
                var n = cfg.VertexOf(x);
                _ = str.Append($"Block № {n} \n\n");
                foreach (var b in x.GetInstructions())
                {
                    _ = str.Append(b.ToString() + "\n");
                }
                _ = str.Append($"\n\n---IN set---\n");
                _ = str.Append('{');
                foreach (var i in DictInOut[n].IN)
                {
                    _ = str.Append($" {i}");
                }
                _ = str.Append(" }");
                _ = str.Append($"\n\n---OUT set---\n");
                _ = str.Append('{');
                foreach (var i in DictInOut[n].OUT)
                {
                    _ = str.Append($" {i}");
                }
                _ = str.Append(" }\n\n");
            }
            return str.ToString();
        }
    }

    public class LiveVariablesTransferFunc
    {
        private readonly Dictionary<BasicBlock, DefUseSet> dictDefUse;

        private static (HashSet<string> def, HashSet<string> use) FillDefUse(IReadOnlyCollection<Instruction> block)
        {
            Func<string, bool> IsId = ThreeAddressCodeDefUse.IsId;

            var def = new HashSet<string>();
            var use = new HashSet<string>();
            foreach (var instruction in block)
            {
                if (IsId(instruction.Argument1) && !def.Contains(instruction.Argument1))
                {
                    _ = use.Add(instruction.Argument1);
                }
                if (IsId(instruction.Argument2) && !def.Contains(instruction.Argument2))
                {
                    _ = use.Add(instruction.Argument2);
                }
                if (IsId(instruction.Result) && !use.Contains(instruction.Result))
                {
                    _ = def.Add(instruction.Result);
                }
            }

            return (def, use);
        }

        public LiveVariablesTransferFunc(ControlFlowGraph cfg)
        {
            var blocks = cfg.GetCurrentBasicBlocks();
            dictDefUse = new Dictionary<BasicBlock, DefUseSet>();
            foreach (var x in blocks)
            {
                dictDefUse.Add(x, new DefUseSet(FillDefUse(x.GetInstructions())));
            }
        }

        public HashSet<string> Transfer(BasicBlock basicBlock, HashSet<string> OUT) =>
            dictDefUse[basicBlock].Use.Union(OUT.Except(dictDefUse[basicBlock].Def)).ToHashSet();
    }
}
