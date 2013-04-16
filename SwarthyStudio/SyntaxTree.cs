using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    internal class SyntaxTree
    {
        public List<SyntaxTree> SubTrees = new List<SyntaxTree>();
        internal SyntaxTreeType Type { get; private set; }        
        internal Token LeafValue { get; private set; }        
        public SyntaxTree()
        {
            LeafValue = null;
            Type = SyntaxTreeType.Statement;            
        }
        public SyntaxTree(Token t)
        {
            LeafValue = t;
            Type = SyntaxTreeType.Leaf;
        }
        public SyntaxTree(SyntaxTreeType type)
        {
            Type = type;
        }
        public void Add(SyntaxTree tree)
        {
            SubTrees.Add(tree);
        }
        public void Add(Token t)
        {
            SyntaxTree tree = new SyntaxTree(t);
            SubTrees.Add(tree);
        }
        public int Count
        {
            get
            {
                return SubTrees.Count;
            }
        }
        public Token Value
        {
            get
            {
                if (SubTrees.Count == 1)
                    return SubTrees.First().Value;
                else
                    return LeafValue;
            }
        }
        public override string ToString()
        {
            return (Type == SyntaxTreeType.Leaf ? "<" + LeafValue.ToString() + ">":"<" + Enum.GetName(typeof(SyntaxTreeType), Type) + ">");
        }
    }
    public enum SyntaxTreeType
    {
        Sum, Mul, Atom, Assign, LogicalExpression, Statement, If, Leaf
    }
}
