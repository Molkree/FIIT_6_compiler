using System.Collections.Generic;
using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class FillParentsVisitor : AutoVisitor
    {
        private readonly Stack<Node> st = new Stack<Node>();

        public override void PreVisit(Node node)
        {
            node.Parent = st.Count != 0 ? st.Peek() : null;
            st.Push(node);
        }

        public override void PostVisit(Node node) => st.Pop();
    }
}
