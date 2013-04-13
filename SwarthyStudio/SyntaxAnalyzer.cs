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
        static SyntaxTree Tree;
        static public void Initialize()
        {
            
        }
        
        public static void Process()
        {
            tokens = LexicalAnalyzer.Lexems;
            currentVisibility = null;
            Tree = new SyntaxTree();
            variables.Clear();
            Statement(Tree);
        }
        static void Statement(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree();
            parentTree.Add(current);

            Eat(TokenType.OpenCurlyBracket);          
            new Visibility();
            while(Peek().Type!=TokenType.CloseCurlyBracket)
            {
                Token t = Get();
                switch (t.Type)
                {
                    case TokenType.Identifier:
                        currentVisibility.FindOrCreate(t.Value, 0);   
                        //ArithmeticExpression();               
                        break;
                    case TokenType.If:
                        //If();
                        break;
                    case TokenType.Number:
                        throw new ErrorException("Невозможно присвоить значение константе: " + t.Value, t, ErrorType.SyntaxError);
                        return;
                        break;
                    default:
                        throw new ErrorException("Неожиданный символStmt: " + t.Value, t, ErrorType.SyntaxError);
                }
            }            
            Eat(TokenType.CloseCurlyBracket);// }
            currentVisibility = currentVisibility.parentVisibility;//возвращаемся в родительскую область видимости
        }

        static void If()
        {

        }

        static void DeclarationCheck(Token t)
        {
            if (t.Type == TokenType.Identifier && currentVisibility.Find(t.Value) == null)
                throw new ErrorException("Использование ранее необъявленной переменной", t, ErrorType.SyntaxError); 
        }
        static Token Peek(int Pos = 0)
        {
            return tokens[Pos];
        }
        static Token Get()
        {
            Token t = Peek();
            tokens.RemoveAt(0);
            return t;
        }
        static void PutBack(Token t)
        {
            tokens.Insert(0, t);
        }
        static void Eat(TokenType type)
        {
            Token t = Get();
            if (t.Type != type)
                throw new ErrorException("Ожидаемый токен: " + type.ToString() + ", а найдено: " + t.Type.ToString(), t, ErrorType.SyntaxError);
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
