using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SwarthyStudio
{
    public partial class DEBUG : Form
    {
        public TextBox lexems;
        public TextBox tetrads;
        public TreeNodeCollection FieldOfView;
        public DEBUG()
        {
            InitializeComponent();
            lexems = tbLexems;
            tetrads = tbTetradList;
            //FieldOfView = tvFieldOfView.Nodes;
        }

        private void DEBUG_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        internal void ShowTree(SyntaxTree tree)
        {
            tvSyntaxTree.Nodes.Clear();
            TreeNode root = newNode(tree);            
            tvSyntaxTree.Nodes.Add(root);
            foreach (SyntaxTree t in tree.SubTrees)
            {
                addNodeToTree(t,root);
            }
        }
        TreeNode newNode(SyntaxTree tree)
        {
            TreeNode node = new TreeNode();
            node.Text = tree.ToString();
            node.ForeColor = Color.Black;
            node.BackColor = Color.White;
            //ParentNode.ImageIndex = 0;
            //ParentNode.SelectedImageIndex = 0;
            return node;
        }
        void addNodeToTree(SyntaxTree tree, TreeNode parrentNode)
        {
            foreach (SyntaxTree t in tree.SubTrees)
            {
                TreeNode newN = newNode(t);
                parrentNode.Nodes.Add(newN);
                addNodeToTree(t, newN);
            }
        }
    }
}
