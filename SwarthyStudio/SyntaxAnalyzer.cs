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
        static public void Initialize()
        {
            
        }
        
        public static void Process()
        {
            tokens = LexicalAnalyzer.Lexems.ToList();//DEBUG: копируем список, в release версии - передать по ссылке
            currentVisibility = null;            
            Variables.Clear();
            TetradManager.list.Clear();
            Statement();
        }
        static void Statement()
        {
            bool oneAction = Peek.Type != TokenType.OpenCurlyBracket, endWhile=false;
            if (!oneAction)
                Eat(TokenType.OpenCurlyBracket);          
            new Visibility();
            while(Peek.Type!=TokenType.CloseCurlyBracket && !endWhile)
            {
                switch (Peek.Type)
                {
                    case TokenType.Identifier:
                        currentVisibility.FindOrCreate(Peek.Value, 0);
                        Assign();                        
                        break;
                    case TokenType.If:
                        If();
                        break;
                    case TokenType.Number:
                        throw new ErrorException("Невозможно присвоить значение константе: " + Peek.Value, Peek, ErrorType.SyntaxError);
                        return;
                        break;
                    default:
                        throw new ErrorException("Неожиданный символStmt: " + Peek.Value, Peek, ErrorType.SyntaxError);
                }
                if (oneAction)
                    endWhile = true;
            }            
            if (!oneAction)
                Eat(TokenType.CloseCurlyBracket);// }            
            currentVisibility = currentVisibility.parentVisibility;//возвращаемся в родительскую область видимости
        }

        static void If()
        {            
            /*             (more, a, b)
             * if (a>b)    (if, 0, startElseMark)
             * {
             * }           (goTo, endMark, 0)
             * else
             * {           (startElseMark, 0, 0) - куда переходить в случае невыполнения условия
             * }
             *             (endMark, 0, 0) - куда переходить из goTo в случае выполнения условия             
             */
            
            Eat(TokenType.If);
            Eat(TokenType.OpenBracket);
            Operand logic = new Operand(LogicalExpression());
            Tetrad iftetrad = new Tetrad(OperationType.IF, logic, null), goTo = new Tetrad(OperationType.GOTO, null, null);
            TetradManager.Add(iftetrad);
            
            Eat(TokenType.CloseBracket);

            Statement();

            if (Peek.Type == TokenType.Else)
                TetradManager.Add(goTo);            

            iftetrad.Operand2 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));
                        
            if (Peek.Type == TokenType.Else)
            {
                Eat(TokenType.Else);
                Statement();
                goTo.Operand1 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));
            }
        }

        static Tetrad LogicalExpression()
        {
            Operand op1, op2;
            Token operand1 = DeclarationCheck(Eat(TokenType.Identifier, TokenType.Number));
            op1 = operand1.Type == TokenType.Identifier ? new Operand(operand1.Value) : new Operand(operand1.SubType == TokenSubType.HexNumber ? H.parseHex(operand1.Value) : H.parseRome(operand1.Value));
            Token operation = Eat(TokenType.Compare);            
            Token operand2 = DeclarationCheck(Eat(TokenType.Identifier, TokenType.Number));
            op2 = operand2.Type == TokenType.Identifier ? new Operand(operand2.Value) : new Operand(operand2.SubType == TokenSubType.HexNumber ? H.parseHex(operand2.Value) : H.parseRome(operand2.Value));

            Tetrad logic;
            switch (operation.SubType)
            {
                case TokenSubType.Less:
                    logic = new Tetrad(OperationType.LESS, op1, op2);
                    break;
                case TokenSubType.More:
                    logic = new Tetrad(OperationType.MORE, op1, op2);
                    break;
                case TokenSubType.Equal:
                    logic = new Tetrad(OperationType.EQUAL, op1, op2);
                    break;
                default:
                    logic = null;
                    break;
            }
            return TetradManager.Add(logic);
        }
        
        static void Assign()
        {
            Operand op1 = new Operand(Eat(TokenType.Identifier).Value);
            Eat(TokenType.Assign);            
            Operand op2 = Sum();
            Eat(TokenType.Delimitier);
            TetradManager.Add(new Tetrad(OperationType.ASSIGN, op1.Clone(), op2.Clone()));
        }        

        static Operand Sum()
        {
            Operand op1 = Mul();
            while(Peek.SubType == TokenSubType.Add)
            {                
                OperationType operation = Peek.Value=="+"?OperationType.ADD:OperationType.SUB;
                Eat(TokenType.Operation);
                Operand op2 = Mul();
                Tetrad t = new Tetrad(operation, op1.Clone(), op2.Clone());
                TetradManager.Add(t);
                op1.Set(t);
            }
            return op1;
        }
        static Operand Mul()
        {
            Operand op1 = Atom();
            while(Peek.SubType == TokenSubType.Mul)
            {
                OperationType operation = Peek.Value == "*" ? OperationType.MUL : OperationType.DIV;
                Eat(TokenType.Operation);
                Operand op2 = Atom();
                Tetrad t = new Tetrad(operation, op1.Clone(), op2.Clone());
                TetradManager.Add(t);
                op1.Set(t);
            }
            return op1;
        }
        static Operand Atom()
        {
            Token d = Get;
            Operand op = new Operand();
            if (d.Type != TokenType.Identifier && d.Type != TokenType.Number)
            {
                if (d.Type == TokenType.OpenBracket)
                {
                    op = Sum();
                    Eat(TokenType.CloseBracket);
                }
                else
                    throw new ErrorException("Неожиданный символ D: " + d.Value, d, ErrorType.SyntaxError);
            }
            else
            {                
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
