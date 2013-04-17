﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class SyntaxAnalyzer
    {        
        private static List<Token> tokens;
        public static List<int> variables = new List<int>();
        public static Visibility currentVisibility = null;
        public static SyntaxTree Tree;
        static public void Initialize()
        {
            
        }
        
        public static void Process()
        {
            tokens = LexicalAnalyzer.Lexems.ToList();//DEBUG: копируем список, в release версии - передать по ссылке
            currentVisibility = null;
            Tree = new SyntaxTree();
            variables.Clear();
            Statement(Tree);
        }
        static void Statement(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Statement);
            parentTree.Add(current);
            Eat(TokenType.OpenCurlyBracket);          
            new Visibility();
            while(Peek.Type!=TokenType.CloseCurlyBracket)
            {
                switch (Peek.Type)
                {
                    case TokenType.Identifier:
                        currentVisibility.FindOrCreate(Peek.Value, 0);
                        Assign(current);                        
                        break;
                    case TokenType.If:
                        If(current);
                        break;
                    case TokenType.Number:
                        throw new ErrorException("Невозможно присвоить значение константе: " + Peek.Value, Peek, ErrorType.SyntaxError);
                        return;
                        break;
                    default:
                        throw new ErrorException("Неожиданный символStmt: " + Peek.Value, Peek, ErrorType.SyntaxError);
                }
            }            
            Eat(TokenType.CloseCurlyBracket);// }
            currentVisibility = currentVisibility.parentVisibility;//возвращаемся в родительскую область видимости
        }

        static void If(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.If);
            parentTree.Add(current);
            current.Add(Eat(TokenType.If));
            Eat(TokenType.OpenBracket);
            LogicalExpression(current);
            Eat(TokenType.CloseBracket);
            Statement(current);
            if (Peek.Type == TokenType.Else)
            {
                current.Add(Eat(TokenType.Else));
                Statement(current);
            }
        }

        static void LogicalExpression(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.LogicalExpression);
            parentTree.Add(current);
            current.Add(DeclarationCheck(Eat(TokenType.Identifier, TokenType.Number)));
            current.Add(Eat(TokenType.Compare));
            current.Add(DeclarationCheck(Eat(TokenType.Identifier, TokenType.Number)));
        }
        
        static void Assign(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Assign);
            parentTree.Add(current);
            current.Add(Eat(TokenType.Identifier));
            current.Add(Eat(TokenType.Assign));            
            Sum(current);
            Eat(TokenType.Delimitier);
        }        

        static void Sum(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Sum);
            parentTree.Add(current);
            Mul(current);
            while(Peek.SubType == TokenSubType.Add)
            {
                current.Add(Eat(TokenType.Operation));
                Mul(current);
            }
        }
        static void Mul(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Mul);
            parentTree.Add(current);
            Atom(current);
            while(Peek.SubType == TokenSubType.Mul)
            {
                current.Add(Eat(TokenType.Operation));
                Atom(current);
            }
        }
        static void Atom(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Atom);
            parentTree.Add(current);
            Token d = Get;
            if (d.Type != TokenType.Identifier && d.Type != TokenType.Number)
            {
                if (d.Type == TokenType.OpenBracket)
                {
                    Sum(current);
                    Eat(TokenType.CloseBracket);
                }
                else
                    throw new ErrorException("Неожиданный символ D: " + d.Value, d, ErrorType.SyntaxError);
            }
            else           
                current.Add(d);                
        }
        
        static Token DeclarationCheck(Token t)
        {
            if (t.Type == TokenType.Identifier && currentVisibility.Find(t.Value) == null)
                throw new ErrorException("Использование ранее необъявленной переменной", t, ErrorType.SyntaxError);
            else
                return t;
        }
        static Token Peek
        {
            get
            {
                return tokens.First();
            }
        }
        static Token Get
        {
            get
            {
                Token t = Peek;
                tokens.RemoveAt(0);
                return t;
            }
        }
        static void PutBack(Token t)
        {
            tokens.Insert(0, t);
        }
        static Token Eat(params TokenType[] types)
        {
            Token t = Get;            
            if (!types.Contains(t.Type))
                throw new ErrorException("Ожидаемый токен: " + String.Join(",", types.Select(p => p.ToString()).ToArray()) + ", а найдено: " + t.Type.ToString(), t, ErrorType.SyntaxError);
            return t;
        }
    }
    class Visibility
    {
        public static Visibility GLOBAL = null;
        public Visibility parentVisibility = null;
        public List<Variable> variables = new List<Variable>();
        public Visibility()
        {
            if (GLOBAL == null)
                GLOBAL = this;
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
