﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class SyntaxAnalyzer
    {        
        private static List<Token> tokens;
        public static Dictionary<string, int> Variables = new Dictionary<string, int>();
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
            Variables.Clear();
            TetradManager.list.Clear();
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
            currentVisibility.FreeAllVariables();//освобождаем память из под всех переменных, объявленных в этой области видимости
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
            Operand op1 = new Operand(current.Add(Eat(TokenType.Identifier)).Value);
            current.Add(Eat(TokenType.Assign));            
            Operand op2 = Sum(current);
            Eat(TokenType.Delimitier);
            TetradManager.Add(new Tetrad(OperationType.ASSIGN, op1.Clone(), op2.Clone()));
        }        

        static Operand Sum(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Sum);
            parentTree.Add(current);
            Operand op1 = Mul(current);
            while(Peek.SubType == TokenSubType.Add)
            {                
                OperationType operation = Peek.Value=="+"?OperationType.ADD:OperationType.SUB;
                current.Add(Eat(TokenType.Operation));
                Operand op2 = Mul(current);
                Tetrad t = new Tetrad(operation, op1.Clone(), op2.Clone());
                TetradManager.Add(t);
                op1.Set(t);
            }
            return op1;
        }
        static Operand Mul(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Mul);
            parentTree.Add(current);
            Operand op1 = Atom(current);
            while(Peek.SubType == TokenSubType.Mul)
            {
                OperationType operation = Peek.Value == "*" ? OperationType.MUL : OperationType.DIV;
                current.Add(Eat(TokenType.Operation));
                Operand op2 = Atom(current);
                Tetrad t = new Tetrad(operation, op1.Clone(), op2.Clone());
                TetradManager.Add(t);
                op1.Set(t);
            }
            return op1;
        }
        static Operand Atom(SyntaxTree parentTree)
        {
            SyntaxTree current = new SyntaxTree(SyntaxTreeType.Atom);
            parentTree.Add(current);
            Token d = Get;
            Operand op = new Operand();
            if (d.Type != TokenType.Identifier && d.Type != TokenType.Number)
            {
                if (d.Type == TokenType.OpenBracket)
                {
                    op = Sum(current);
                    Eat(TokenType.CloseBracket);
                }
                else
                    throw new ErrorException("Неожиданный символ D: " + d.Value, d, ErrorType.SyntaxError);
            }
            else
            {
                current.Add(d);
                if (d.Type == TokenType.Identifier)
                    op = new Operand(d.Value);
                else
                    op = new Operand(d.SubType==TokenSubType.HexNumber?H.parseHex(d.Value):H.parseRome(d.Value));
            }
            return op;
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
        public List<Visibility> subVisibilities = new List<Visibility>();
        public Visibility()
        {
            if (GLOBAL == null)
                GLOBAL = this;
            parentVisibility = SyntaxAnalyzer.currentVisibility;
            if (parentVisibility!=null)
                parentVisibility.subVisibilities.Add(this);
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
                SyntaxAnalyzer.Variables[s] = val;
                Variable newV = new Variable(s);
                variables.Add(newV);
                return newV;
            }
        }
        public void FreeAllVariables()
        {
            foreach (Variable v in variables)
                SyntaxAnalyzer.Variables.Remove(v.name);
        }
    }
    class Variable
    {        
        public string name;
        public Variable()
        {
        }
        public Variable(string name)
        {
            this.name = name;            
        }
        public int Value
        {
            get
            {
                if (SyntaxAnalyzer.Variables.Keys.Contains(name))
                    return SyntaxAnalyzer.Variables[name];
                else
                    throw new ErrorException("Обращение к ранее не объявленной переменной \""+name+"\"", ErrorType.SemanticError);
            }
            set
            {
                SyntaxAnalyzer.Variables[name] = value;
            }
        }
    }
}
