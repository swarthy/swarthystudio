using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace SwarthyStudio
{
    public partial class Form1 : Form
    {
        bool isCodeChanged = false;
        string filePath = "";
        bool catchedError = false;

        bool debug = true;
        DEBUG debugForm;
        public Form1()
        {
            InitializeComponent();
            debugForm = new DEBUG();            
        }

        private void новыйToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isCodeChanged && MessageBox.Show("Сохранить изменения" + (filePath == "" ? "" : " в файле " + filePath) + "?", "Swarthy Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                Save();
            tbCode.Text = "";
            filePath = "";
            isCodeChanged = false;
            UpdateTitle();
        }

        private void tbCode_TextChanged(object sender, EventArgs e)
        {
            isCodeChanged = true;
            if (catchedError)
                clearCode();
        }

        void Save()
        {            
            if (!isCodeChanged)
                return;
            if (filePath == "")
                SaveAsCode();
            else
                SaveCode(filePath);            
        }
        
        void SaveAsCode()
        {
            if (dlgSave.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlgSave.FileName.Length > 0)
                SaveCode(dlgSave.FileName);            
        }

        void SaveCode(string path)
        {
            try
            {
                File.WriteAllText(path, tbCode.Text);
            }
            catch (Exception errorMsg)
            {
                MessageBox.Show(errorMsg.Message);
            }
            filePath = path;
            isCodeChanged = false;
            UpdateTitle();
        }
        void LoadCode(string path)
        {
            try
            {                
                tbCode.Text = File.ReadAllText(path);
            }
            catch (Exception errorMsg)
            {
                MessageBox.Show(errorMsg.Message);
            }
            filePath = path;
            isCodeChanged = false;
            UpdateTitle();
        }

        void Open()
        {
            if (isCodeChanged && MessageBox.Show("Сохранить изменения" + (filePath == "" ? "" : " в файле " + filePath) + "?", "Swarthy Studio", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                Save();
            if (dlgOpen.ShowDialog() == System.Windows.Forms.DialogResult.OK && dlgOpen.FileName.Length > 0)
                LoadCode(dlgOpen.FileName);            
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            Open();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsCode();
        }
        void UpdateTitle()
        {
            Text = (filePath == "" ? "" : Path.GetFileName(filePath) + " - ") + "Swarthy Studio";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isCodeChanged && !debug)
                switch (MessageBox.Show("Сохранить изменения" + (filePath == "" ? "" : " в файле " + filePath) + " перед выходом?", "Swarthy Studio", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case System.Windows.Forms.DialogResult.Yes:
                        Save();
                        break;
                    case System.Windows.Forms.DialogResult.Cancel:
                        e.Cancel = true;
                        break;
                }
        }

        private void tbCode_SelectionChanged(object sender, EventArgs e)
        {
            cursorPosition.Text = "Строка: " + (tbCode.GetLineFromCharIndex(tbCode.SelectionStart) + 1).ToString() + " Столбец: " + (tbCode.SelectionStart - tbCode.GetFirstCharIndexOfCurrentLine() + 1).ToString();            
        }

        private void компиляцияToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            tbLog.Clear();
            if (debug)
            {
                debugForm.lexems.Clear();
                debugForm.tetrads.Clear();
                debugForm.Show();
                this.BringToFront();
            }
            try
            {
                LexicalAnalyzer.Process(tbCode.Text);                                
                SyntaxAnalyzer.Process();
            }
            catch (ErrorException ex)
            {
                tbLog.Text += ex.ToString()+"\r\n";
                if (ex.Position != -1)
                {
                    int oldPos = tbCode.SelectionStart;
                    tbCode.Select(tbCode.GetFirstCharIndexFromLine(ex.Line) + ex.Position, ex.Length);
                    tbCode.SelectionBackColor = Color.Gray;
                    tbCode.SelectionColor = Color.Red;
                    tbCode.SelectionStart = oldPos;
                    tbCode.SelectionLength = 0;
                }
                catchedError = true;
            }

            foreach (Token t in LexicalAnalyzer.Lexems)
                debugForm.lexems.Text += t.ToString() + "\r\n";
            debugForm.ShowTree(SyntaxAnalyzer.Tree);
            foreach (Tetrad tetrad in TetradManager.list)
                debugForm.tetrads.Text += tetrad.ToString() + "\r\n";
        }

        void clearCode()
        {
            int oldPos = tbCode.SelectionStart;
            tbCode.SelectAll();
            tbCode.SelectionBackColor = Color.White;
            tbCode.SelectionColor = Color.Black;
            tbCode.SelectionStart = oldPos;
            tbCode.SelectionLength = 0;
            catchedError = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LexicalAnalyzer.Initialize();
            SyntaxAnalyzer.Initialize();
        }

        class test
        {
            public int val;
            public test(int a)
            {
                val = a;
            }
        }

        private void tbCode_KeyDown(object sender, KeyEventArgs e)
        {            
            if (e.KeyValue == 13)
            {
                int pos = tbCode.SelectionStart;
                int line = tbCode.GetLineFromCharIndex(pos);                
                string s = "\n";
                int count = 0;
                for (int i = 0; tbCode.Lines[line].Length-1>=i && tbCode.Lines[line][i] == '\t'; i++)
                {
                    s += "\t";
                    count++;
                }
                tbCode.Text = tbCode.Text.Insert(pos, s);
                tbCode.SelectionStart = pos+count+1;
                e.Handled = true;
            }            
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            debugForm.Show();
        }
    }
}
