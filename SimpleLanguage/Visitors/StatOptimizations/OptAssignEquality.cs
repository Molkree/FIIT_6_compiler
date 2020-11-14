using ProgramTree;

namespace SimpleLanguage.Visitors
{
    public class OptAssignEquality : ChangeVisitor
    {
        public override void PostVisit(Node node)
        {
            if (node is AssignNode assignNode && assignNode.Expr is IdNode idn && assignNode.Id.Name == idn.Name)
            {
                ReplaceStat(assignNode, new EmptyNode());
            }
        }
    }
}
