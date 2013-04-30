using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace SwarthyStudio
{
    public static class CodeGenerator
    {
        static List<string> Code = new List<string>();
        static int labelCount = 0;
        public static string GetCode()
        {
            labelCount = 0;
            Code.Clear();
            Libs();
            DataQSection();
            DataSection();
            //Code();
            MainCode();
            return Code.Aggregate((i, j) => i + "\r\n" + j);
        }
        static void Libs()
        {
            //__UNICODE__ equ 1
            //Add("__UNICODE__ equ 1");
            CommentLine();
            Comment("Подключение необходимых библиотек");
            Add("include include\\masm32rt.inc");
            Add("include include\\swarthy.inc");
            Add("includelib lib\\swarthy.lib");
            CommentLine();
        }
        static void DataQSection()
        {
            Add(".data?");
            Add(string.Format("variables\tSDWORD\t{0} dup (?)", SyntaxAnalyzer.Variables.Count));
            Add("tempBuffer\tSDWORD\t?");
            CommentLine();
        }
        static void DataSection()
        {
            Add(".data");
            int i=0;
            Add(string.Format("ConsoleTitle\tdb\t\"{0}\", 0", "тестCaption"));
            Add("nl\tdb\t 13, 10, 0");
            foreach (string cnst in LexicalAnalyzer.StringConstants)            
                Add(string.Format("strConst{0}\tdb\t\"{1}\", 0", i++, cnst));            
            CommentLine();
        }
        static void MainCode()
        {
            Add(".code");
            label("start:");

            CommentLine();
            Add("invoke CharToOem, addr ConsoleTitle, addr ConsoleTitle");            
            Add("invoke SetConsoleTitle, addr ConsoleTitle");
            cl();//rus
            for (int i = 0; i < LexicalAnalyzer.StringConstants.Count; i++)
                Add(string.Format("invoke CharToOem, addr strConst{0}, addr strConst{0}", i));
            cl();
            Add("cls");
            Add("call main");
            Add("inkey");
            Add("exit");
            CommentLine();

            Add("main proc");

            TetradManager.BeginSolve();

            Add("ret");
            Add("main endp");
            cl();
            label("end start");
            
        }
        public static void Add(string s)
        {
            Code.Add(string.Format("\t\t{0}", s));
        }
        public static void cl()
        {
            Add("");
        }
        public static void Addl(string s)
        {
            Code.Add(string.Format("{1}\t{0}", s));
        }
        public static void label(string l)
        {
            Code.Add(l);
        }
        public static void CommentLine()
        {            
            Comment("--------------------------------------------");            
        }
        public static void Comment(string comment)
        {
            Code.Add("\t;" + comment);
        }
        static string StudioDir
        {
            get
            {
                return Path.GetDirectoryName(Application.ExecutablePath);
            }

        }
    }
}
