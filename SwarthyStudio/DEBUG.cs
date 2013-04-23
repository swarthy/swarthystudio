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
        public DEBUG()
        {
            InitializeComponent();
            lexems = tbLexems;
            tetrads = tbTetradList;            
        }

        private void DEBUG_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }        
    }
}
