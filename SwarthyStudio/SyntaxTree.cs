using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    class SyntaxTree
    {
        public List<SyntaxTree> SubTrees = new List<SyntaxTree>();
        public SyntaxTreeType Type { get; private set; }
        public Token LeafValue { get; private set; }        
        public SyntaxTree()
        {
            LeafValue = null;
            Type = SyntaxTreeType.Tree;            
        }
        public SyntaxTree(Token t)
        {
            LeafValue = t;
            Type = SyntaxTreeType.Leaf;
        }
        public void Add(SyntaxTree tree)
        {
            SubTrees.Add(tree);
        }
    }
    enum SyntaxTreeType
    {
        Tree, Leaf
    }
}
