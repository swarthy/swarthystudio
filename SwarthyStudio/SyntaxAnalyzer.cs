﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class SyntaxAnalyzer
    {        
        private static List<Token> tokens;
        static int pos;
        public static List<int> variables = new List<int>();
        static bool[,] TransitionTable = new bool[14,14];
        public static Visibility currentVisibility = null;
        static public void Initialize()
        {
            pos = -1;
            InitTable();
        }
        static void InitTable()
        {
            TransitionTable[(int)TokenType.Identifier, (int)TokenType.Operation] = true;
            TransitionTable[(int)TokenType.Number, (int)TokenType.Operation] = true;
            TransitionTable[(int)TokenType.Identifier, (int)TokenType.CloseBracket] = true;
            TransitionTable[(int)TokenType.Number, (int)TokenType.CloseBracket] = true;
            TransitionTable[(int)TokenType.Operation, (int)TokenType.Number] = true;
            TransitionTable[(int)TokenType.Operation, (int)TokenType.Identifier] = true;
            TransitionTable[(int)TokenType.Operation, (int)TokenType.OpenBracket] = true;
            TransitionTable[(int)TokenType.OpenBracket, (int)TokenType.OpenBracket] = true;
            TransitionTable[(int)TokenType.OpenBracket, (int)TokenType.Number] = true;
            TransitionTable[(int)TokenType.OpenBracket, (int)TokenType.Identifier] = true;
            TransitionTable[(int)TokenType.CloseBracket, (int)TokenType.CloseBracket] = true;
            TransitionTable[(int)TokenType.CloseBracket, (int)TokenType.Operation] = true;
        }
        public static void Process()
        {
            tokens = LexicalAnalyzer.Lexems;
            pos = 0;
            currentVisibility = null;
            variables.Clear();
            Stmt();      
        }
        static void Stmt()
        {
            if (tokens[pos++].Type != TokenType.OpenCurlyBracket)
                throw new ErrorException("Фрагмент кода должен начинаться с \'{\'",tokens[pos-1], ErrorType.SyntaxError);            
            new Visibility();
            do
            {
                switch (tokens[pos++].Type)
                {
                    case TokenType.Identifier:
                        currentVisibility.FindOrCreate(tokens[pos - 1].Value, 0);
                        //tokens[pos-1].Value
                        //проверить объявленность переменной
                        Assignment();                        
                        break;
                    case TokenType.If:
                        If();
                        break;
                    case TokenType.CloseCurlyBracket:
                        currentVisibility = currentVisibility.parentVisibility;
                        return;
                        break;
                    case TokenType.Number:
                        throw new ErrorException("Ожидается присвоение константе: " + tokens[pos - 1].Value, tokens[pos - 1], ErrorType.SyntaxError);
                        return;
                        break;
                    default:
                        throw new ErrorException("Неожиданный символStmt: " + tokens[pos - 1].Value, tokens[pos - 1], ErrorType.SyntaxError);
                }
            } while (true);
        }
        static void Assignment()
        {
            if (tokens[pos++].Type != TokenType.Assign)
                throw new ErrorException("Ожидался знак \'=\'", tokens[pos - 1], ErrorType.SyntaxError);
            Expression();
        }
        static void Expression()
        {
            Token prev = tokens[pos++], current=tokens[pos++];
            if (prev.Type!=TokenType.OpenBracket && prev.Type!=TokenType.Identifier && prev.Type!=TokenType.Number)
                throw new ErrorException("Неожиданный символExp1: " + prev.Value, prev, ErrorType.SyntaxError);             
            while (TransitionTable[(int)prev.Type, (int)current.Type])
            {
                //проверка на объявленность
                DeclarationCheck(current);                   
                
                prev = current;                 
                current = tokens[pos++];
            }
            if (current.Type != TokenType.Delimitier)
                throw new ErrorException("Неожиданный символExp2: " + current.Value, current, ErrorType.SyntaxError);            
            if (prev.Type!=TokenType.Identifier &&prev.Type!=TokenType.Number &&prev.Type!=TokenType.CloseBracket)
                throw new ErrorException("Неожиданный символExp3: " + prev.Value, prev, ErrorType.SyntaxError);            
        }
        static void If()
        {
            if (tokens[pos++].Type != TokenType.OpenBracket)
                throw new ErrorException("После if предполагается \'(\'", tokens[pos - 1], ErrorType.SyntaxError);            
            LogicStmt();
            if (tokens[pos++].Type != TokenType.CloseBracket)
                throw new ErrorException("После логического выражения предполагается \')\'", tokens[pos - 1], ErrorType.SyntaxError);
            Stmt();
            if (tokens[pos].Type == TokenType.Else)
            {
                pos++;
                Stmt();
            }
        }
        static void LogicStmt()
        {
            if (tokens[pos++].Type != TokenType.Identifier && tokens[pos-1].Type != TokenType.Number)
                throw new ErrorException("Первый аргумент логического выражения должен быть идентификатором или константой", tokens[pos - 1], ErrorType.SyntaxError);
            if (tokens[pos-1].Type == TokenType.Identifier && currentVisibility.Find(tokens[pos - 1].Value) == null)
                throw new ErrorException("Использование ранее необъявленной переменной", tokens[pos - 1], ErrorType.SyntaxError); 

            if (tokens[pos++].Type != TokenType.Compare)
                throw new ErrorException("Ожидалась операция сравнения", tokens[pos - 1], ErrorType.SyntaxError);

            if (tokens[pos++].Type != TokenType.Identifier && tokens[pos-1].Type != TokenType.Number)
                throw new ErrorException("Второй аргумент логического выражения должен быть идентификатором или константой", tokens[pos - 1], ErrorType.SyntaxError);
            if (tokens[pos-1].Type == TokenType.Identifier && currentVisibility.Find(tokens[pos - 1].Value) == null)
                throw new ErrorException("Использование ранее необъявленной переменной", tokens[pos - 1], ErrorType.SyntaxError); 
        }
        static void DeclarationCheck(Token t)
        {
            if (t.Type == TokenType.Identifier && currentVisibility.Find(t.Value) == null)
                throw new ErrorException("Использование ранее необъявленной переменной", t, ErrorType.SyntaxError); 
        }
    }
    class Visibility
    {
        public Visibility parentVisibility = null;
        public List<Variable> variables = new List<Variable>();
        public Visibility()
        {
            parentVisibility = SyntaxAnalyzer.currentVisibility;
            SyntaxAnalyzer.currentVisibility = this;
        }
        public Variable Find(string s)
        {
            foreach (Variable v in variables)
                if (v.name == s)
                    return v;
            if (parentVisibility == null)
                return null;
            return parentVisibility.Find(s);
        }
        public Variable FindOrCreate(string s, int val)
        {
            Variable v = Find(s);
            if (v != null)
            {
                v.Value = val;
                return v;
            }
            else
            {
                SyntaxAnalyzer.variables.Add(val);
                Variable newV = new Variable(SyntaxAnalyzer.variables.Count - 1, s);
                variables.Add(newV);
                return newV;
            }
        }        
    }
    class Variable
    {
        public int pos;
        public string name;
        public Variable()
        {
        }
        public Variable(int pos, string name)
        {
            this.name = name;
            this.pos = pos;
        }
        public int Value
        {
            get
            {
                return SyntaxAnalyzer.variables[pos];
            }
            set
            {
                SyntaxAnalyzer.variables[pos] = value;
            }
        }
    }
}
