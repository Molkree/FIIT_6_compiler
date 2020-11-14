﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleLanguage
{
    public class InOutData<T> : Dictionary<BasicBlock, (T In, T Out)>
        where T : IEnumerable
    {
        public override string ToString()
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine("++++");
            foreach (var kv in this)
            {
                _ = sb.AppendLine(kv.Key + ":\n" + kv.Value);
            }
            _ = sb.AppendLine("++++");
            return sb.ToString();
        }

        public InOutData() { }

        public InOutData(Dictionary<BasicBlock, (T, T)> dictionary)
        {
            foreach (var b in dictionary)
            {
                this[b.Key] = b.Value;
            }
        }
    }

    public enum Direction { Forward, Backward }

    public abstract class GenericIterativeAlgorithm<T> where T : IEnumerable
    {
        /// <summary>
        /// Оператор сбора
        /// </summary>
        public abstract Func<T, T, T> CollectingOperator { get; }

        /// <summary>
        /// Сравнение двух последовательностей (условие продолжения цикла)
        /// </summary>
        public abstract Func<T, T, bool> Compare { get; }

        /// <summary>
        /// Начальное значение для всех блоков, кроме первого
        /// (при движении с конца - кроме последнего)
        /// </summary>
        public abstract T Init { get; protected set; }

        /// <summary>
        /// Начальное значение для первого блока
        /// (при движении с конца - для последнего)
        /// </summary>
        public virtual T InitFirst { get => Init; protected set { } }

        /// <summary>
        /// Передаточная функция
        /// </summary>
        public abstract Func<BasicBlock, T, T> TransferFunction { get; protected set; }

        /// <summary>
        /// Направление
        /// </summary>
        public virtual Direction Direction => Direction.Forward;

        /// <summary>
        /// Количество итераций
        /// </summary>
        public int Iterations { get; private set; }

        /// <summary>
        /// Выполнить алгоритм
        /// </summary>
        /// <param name="graph"> Граф потока управления </param>
        /// <param name="useRenumbering"> Использовать перенумерацию вершин графа после DFS </param>
        /// <returns></returns>
        public virtual InOutData<T> Execute(ControlFlowGraph graph, bool useRenumbering = true)
        {
            GetInitData(graph, useRenumbering, out var blocks, out var data,
                out var getPreviousBlocks, out var getDataValue, out var combine);

            var outChanged = true;
            Iterations = 0;
            while (outChanged)
            {
                outChanged = false;
                foreach (var block in blocks)
                {
                    var inset = getPreviousBlocks(block).Aggregate(Init, (x, y) => CollectingOperator(x, getDataValue(y)));
                    var outset = TransferFunction(block, inset);

                    if (!Compare(outset, getDataValue(block)))
                    {
                        outChanged = true;
                    }
                    data[block] = combine(inset, outset);
                }
                ++Iterations;
            }
            return data;
        }

        internal void GetInitData(
            ControlFlowGraph graph,
            bool useRenumbering,
            out IEnumerable<BasicBlock> blocks,
            out InOutData<T> data,
            out Func<BasicBlock, IEnumerable<BasicBlock>> getPreviousBlocks,
            out Func<BasicBlock, T> getDataValue,
            out Func<T, T, (T, T)> combine)
        {
            var start = Direction == Direction.Backward
                ? graph.GetCurrentBasicBlocks()[graph.GetCurrentBasicBlocks().Count - 1]
                : graph.GetCurrentBasicBlocks()[0];
            blocks = graph.GetCurrentBasicBlocks().Except(new[] { start });

            if (useRenumbering)
            {
                blocks = Direction == Direction.Backward
                  ? blocks.OrderByDescending(z => graph.DepthFirstNumeration[graph.VertexOf(z)])
                  : blocks.OrderBy(z => graph.DepthFirstNumeration[graph.VertexOf(z)]);
            }

            var dataTemp = new InOutData<T>
            {
                [start] = (InitFirst, InitFirst)
            };
            foreach (var block in blocks)
            {
                dataTemp[block] = (Init, Init);
            }
            data = dataTemp;

            switch (Direction)
            {
                case Direction.Forward:
                    getPreviousBlocks = x => graph.GetParentsBasicBlocks(graph.VertexOf(x)).Select(z => z.block);
                    getDataValue = x => dataTemp[x].Out;
                    combine = (x, y) => (x, y);
                    break;
                case Direction.Backward:
                    getPreviousBlocks = x => graph.GetChildrenBasicBlocks(graph.VertexOf(x)).Select(z => z.block);
                    getDataValue = x => dataTemp[x].In;
                    combine = (x, y) => (y, x);
                    break;
                default:
                    throw new NotImplementedException("Undefined direction type");
            }
        }
    }
}
