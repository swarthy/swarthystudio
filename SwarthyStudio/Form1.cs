﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace SwarthyStudio
{
    public partial class Form1 : Form
    {
        const string filter = "Swarthy Studio Code (*.ssc)|*.ssc|Текстовые документы|*.txt|Все документы|*.*";
        bool isCodeChanged = false;
        string filePath = "";
        bool catchedError = false;
        bool showDebugForm = false, deleteASMandOBJ = true;
        bool ShowDebugForm
        {
            get
            {
                return showDebugForm;
            }
            set
            {
                if (value)
                    debugForm = new DEBUG();
                else
                    debugForm = null;
                showDebugForm = value;
            }

        }
        DEBUG debugForm;
        public Form1()
        {
            InitializeComponent();
            H.SetAssociationWithExtension(".ssc", "Swarthy Studio", Application.ExecutablePath, "Swarthy code");
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && File.Exists(args[1]))
            {
                filePath = args[1];
                LoadCode(filePath);
            }
            dlgOpen.InitialDirectory = Application.StartupPath;
            dlgOpen.Filter = filter;
            dlgSave.InitialDirectory = Application.StartupPath;
            dlgSave.Filter = filter;
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
            if (!isCodeChanged && filePath!="")
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
            if (isCodeChanged && !ShowDebugForm)
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
            compilationBar.Visible = true;
            compilationBar.Value = 0;
            Compile(filePath, true, true);
        }

        bool Compile(string filePath, bool debug = false, bool needRun = false)
        {
            bool noerror = true;
            if (debug)
                tbLog.Clear();
            if (ShowDebugForm)
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
                string name = "NewProgram";
                if (filePath != "")
                    name = Path.GetFileNameWithoutExtension(filePath);
                File.WriteAllText("code.asm", CodeGenerator.GetCode(name), Encoding.GetEncoding(1251));                
                string AssmErr = ExecuteCommand(@"ml.exe /c /coff /nologo code.asm");
                string LinkErr = ExecuteCommand(@"link.exe /SUBSYSTEM:CONSOLE /OPT:NOREF /nologo code.obj");                
                if (debug)
                {
                    if (AssmErr != "")
                    {
                        tbLog.Text += string.Format("Assembly error: {0}\r\n", AssmErr);
                        noerror = false;
                    }
                    if (LinkErr != "")
                    {
                        tbLog.Text += string.Format("Link error: {0}\r\n", LinkErr);
                        noerror = false;
                    }
                }
                if (deleteASMandOBJ)
                {
                    File.Delete("code.obj");
                    File.Delete("code.asm");
                }                
                if (filePath != "")
                {
                    string newProgPath = Path.GetDirectoryName(filePath) + "\\" + name + ".exe";
                    File.Copy("code.exe", newProgPath, true);
                    if (needRun)
                        RunProgram(newProgPath);
                }
                else
                {
                    if (needRun)
                        RunProgram(Application.StartupPath + "\\code.exe");
                }
                if(debug)
                    compilationBar.Visible = false;                
            }
            catch (ErrorException ex)
            {
                tbLog.Text += ex.ToString() + "\r\n";
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
                noerror = false;
            }
            if (ShowDebugForm)
            {
                foreach (Token t in LexicalAnalyzer.Lexems)
                    debugForm.lexems.Text += t.ToString() + "\r\n";

                foreach (Tetrad tetrad in TetradManager.list)
                    debugForm.tetrads.Text += tetrad.ToString() + "\r\n";
            }
            return noerror;
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

        static string ExecuteCommand(string command)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            processInfo.CreateNoWindow = true;
            processInfo.UseShellExecute = false;
            // *** Redirect the output ***
            processInfo.RedirectStandardError = true;
            processInfo.RedirectStandardOutput = true;

            process = Process.Start(processInfo);
            process.WaitForExit();

            // *** Read the streams ***
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            exitCode = process.ExitCode;
            process.Close();
            return exitCode == 0 ? "" : output;
        }
        static void RunProgram(string path)
        {
            int exitCode;
            ProcessStartInfo processInfo;
            Process process;

            processInfo = new ProcessStartInfo("cmd.exe", "/c " + path);
            processInfo.CreateNoWindow = false;
            processInfo.UseShellExecute = true;
            
            process = Process.Start(processInfo);
            process.WaitForExit();

            exitCode = process.ExitCode;
            process.Close();
        }

        private void выводитьОкноDebugToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            ShowDebugForm = выводитьОкноDebugToolStripMenuItem.Checked;
        }
        
        private void deleteObjAsm_CheckedChanged(object sender, EventArgs e)
        {
            deleteASMandOBJ = deleteObjAsm.Checked;
        }

        private void помощьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "help\\help.chm");
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 box = new AboutBox1();
            box.ShowDialog();
        }

    }
}
