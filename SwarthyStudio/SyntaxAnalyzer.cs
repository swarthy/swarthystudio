﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class SyntaxAnalyzer
    {        
        private static List<Token> tokens;
        public static List<Variable> Variables = new List<Variable>();
        public static Visibility currentVisibility = null;
        static List<Tetrad> breaks = new List<Tetrad>();
        static int cyclesCount = 0;
        static public void Initialize()
        {
            
        }
        static int dynPos = 0;
        public static void Process()
        {
            tokens = LexicalAnalyzer.Lexems.ToList();//DEBUG: копируем список, в release версии - передать по ссылке
            currentVisibility = null;            
            Variables.Clear();
            TetradManager.list.Clear();
            Statement();
        }
        static Visibility Statement(Visibility exist = null)
        {
            bool oneAction = Peek.Type != TokenType.OpenCurlyBracket, endWhile=false;
            if (!oneAction)
                Eat(TokenType.OpenCurlyBracket);
            if (exist != null)
                currentVisibility = exist;
            else
                new Visibility();
            bool haveActions = false;
            while(Peek.Type!=TokenType.CloseCurlyBracket && !endWhile)
            {
                haveActions = true;
                switch (Peek.Type)
                {
                    case TokenType.Identifier:                        
                        Assign();                        
                        break;
                    case TokenType.Function:
                        TetradManager.Add(new Tetrad(OperationType.FUNCTIONCALL, FunctionCall(Eat(TokenType.Function), FunctionReturnType.Void), null));
                        if (Peek.Type == TokenType.Delimitier)
                            Eat(TokenType.Delimitier);
                        break;
                    case TokenType.If:
                        If();
                        break;
                    case TokenType.While:
                        While();
                        break;
                    case TokenType.For:
                        For();
                        break;
                    case TokenType.Do:
                        Do();
                        break;
                    case TokenType.Break:
                        if (cyclesCount == 0)
                            throw new ErrorException("Неожиданный break", Peek, ErrorType.SemanticError);
                        Eat(TokenType.Break);
                        Tetrad brk = new Tetrad(OperationType.GOTO, null, null);
                        breaks.Add(brk);
                        TetradManager.Add(brk);
                        if (Peek.Type == TokenType.Delimitier)
                            Eat(TokenType.Delimitier);
                        break;
                    case TokenType.Number:
                        throw new ErrorException("Невозможно присвоить значение константе: " + Peek.Value, Peek, ErrorType.SyntaxError);                        
                        break;
                    default:
                        throw new ErrorException("Неожиданный символ: " + Peek.Value, Peek, ErrorType.SyntaxError);
                }
                if (oneAction)
                    endWhile = true;
            }
            if (!haveActions)
                throw new ErrorException("Блок Statment должен содержать хотя бы 1 действие",Peek, ErrorType.SyntaxError);
            if (!oneAction)
                Eat(TokenType.CloseCurlyBracket);// }   
            Visibility old = currentVisibility;
            currentVisibility = currentVisibility.parentVisibility;//возвращаемся в родительскую область видимости
            return old;
        }
        #region Циклические конструкции/Ветвление
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
            Tetrad iftetrad = LogicalExpression();            
            Tetrad logic = new Tetrad(iftetrad.Operation, new Operand(iftetrad), null),            
                   goTo = new Tetrad(OperationType.GOTO, null, null);
            iftetrad.Operation = OperationType.IF;
            TetradManager.Add(iftetrad);
            TetradManager.Add(logic);
            
            Eat(TokenType.CloseBracket);
            
            Statement();

            if (Peek.Type == TokenType.Else)
                TetradManager.Add(goTo);

            logic.Operand2 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));            
                        
            if (Peek.Type == TokenType.Else)
            {
                Eat(TokenType.Else);
                Statement();
                goTo.Operand1 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));
            }
        }

        static void While()
        {
            Eat(TokenType.While);
            Eat(TokenType.OpenBracket);
            Tetrad iftetrad = LogicalExpression();
            Tetrad logic = new Tetrad(iftetrad.Operation, new Operand(iftetrad), null);
            iftetrad.Operation = OperationType.IF;

            TetradManager.Add(iftetrad);
            TetradManager.Add(logic);
            
            Eat(TokenType.CloseBracket);

            cyclesCount++;
            Statement();
            cyclesCount--;            

            TetradManager.Add(new Tetrad(OperationType.GOTO, new Operand(iftetrad), null));
            logic.Operand2 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));
            breaks.ForEach(t => t.Operand1 = logic.Operand2);
            breaks.Clear();
        }

        static void Do()
        {
            Eat(TokenType.Do);
            Operand beginCycle = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));
            cyclesCount++;
            Statement();
            cyclesCount--;

            Eat(TokenType.While);

            Eat(TokenType.OpenBracket);
            Tetrad iftetrad = LogicalExpression();
            Tetrad logic = new Tetrad(iftetrad.Operation, new Operand(iftetrad), null);
            iftetrad.Operation = OperationType.IF;
            Eat(TokenType.CloseBracket);
            TetradManager.Add(iftetrad);
            TetradManager.Add(logic);
            TetradManager.Add(new Tetrad(OperationType.GOTO, beginCycle, null));
            logic.Operand2 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));
            breaks.ForEach(t => t.Operand1 = logic.Operand2);
            breaks.Clear();
        }

        static void For()
        {
            Eat(TokenType.For);
            Eat(TokenType.OpenBracket);  //for(statement;logic;statement)
            Visibility forVis = Statement();
            //Operand logic = new Operand(LogicalExpression(forVis));
            Tetrad iftetrad = LogicalExpression(forVis);
            Eat(TokenType.Delimitier);

            
            Tetrad logic = new Tetrad(iftetrad.Operation, new Operand(iftetrad), null);
            iftetrad.Operation = OperationType.IF;


            Tetrad goTo = new Tetrad(OperationType.GOTO, new Operand(iftetrad), null);
            TetradManager.Add(iftetrad);
            TetradManager.Add(logic);
            int oldSize = TetradManager.list.Count;
            Statement(forVis); // i++
            int incSize = TetradManager.list.Count - oldSize;
            var stCut = TetradManager.list.GetRange(oldSize, incSize);
            TetradManager.list.RemoveRange(oldSize, incSize);

            Eat(TokenType.CloseBracket);

            cyclesCount++;
            Statement(forVis);
            cyclesCount--;

            TetradManager.list.AddRange(stCut);
            TetradManager.Add(goTo);//после тела цикла - проверяем условие
            logic.Operand2 = new Operand(TetradManager.Add(new Tetrad(OperationType.MARK, null, null)));//точка выхода
            breaks.ForEach(t => t.Operand1 = logic.Operand2);
            breaks.Clear();
        }

        static Tetrad LogicalExpression(Visibility exists = null)
        {
            if (exists!=null)
                currentVisibility = exists;
            Operand op1, op2;
            Token operand1 = Eat(TokenType.Identifier, TokenType.Number); // a       
            op1 = operand1.Type == TokenType.Identifier ? new Operand(DeclarationCheck(operand1)) : new Operand(operand1.GetNumValue);

            Token operation = Eat(TokenType.Compare);            // = 

            Token operand2 = Eat(TokenType.Identifier, TokenType.Number); // b
            op2 = operand2.Type == TokenType.Identifier ? new Operand(DeclarationCheck(operand2)) : new Operand(operand2.GetNumValue);

            Tetrad logic;
            switch (operation.SubType)
            {
                case TokenSubType.Less:
                    logic = new Tetrad(OperationType.LESS, op1, op2);
                    break;
                case TokenSubType.More:
                    logic = new Tetrad(OperationType.GREATER, op1, op2);
                    break;
                case TokenSubType.Equal:
                    logic = new Tetrad(OperationType.EQUAL, op1, op2);
                    break;
                case TokenSubType.NotEqual:
                    logic = new Tetrad(OperationType.NOTEQUAL, op1, op2);
                    break;
                default:
                    logic = null;
                    break;
            }
            if (exists != null)
                currentVisibility = currentVisibility.parentVisibility;
            return logic;
        }
        #endregion

        static Operand FunctionCall(Token fToken, params FunctionReturnType[] validReturnTypes)
        {
            sFunction func = null;
            List<sFunction> funcs = FunctionManager.GetAllByName(fToken.Value);
            Eat(TokenType.OpenBracket);
            List<Parameter> parametres = FunctionParams();
            foreach (sFunction f in funcs)
            {
                if (f.isMyParams(parametres, validReturnTypes))
                {
                    func = f;
                    break;
                }
            }
            if (func==null)
                throw new ErrorException(string.Format("Функция {0} не поддерживает такой набор параметров, для случая, когда необходимо вернуть {1}",funcs[0].Name,validReturnTypes.Select(vt=>vt.ToString()).Aggregate((i,j)=>i+", "+j)), ErrorType.SemanticError);
            //if (!validReturnTypes.Contains(func.ReturnType))
                //throw new ErrorException(string.Format("Функция {0} возвращает значение {1}, а ожидается {2}",func.Name,func.ReturnType,validReturnTypes.Select(vt=>vt.ToString()).Aggregate((i,j)=>i+", "+j)), ErrorType.SemanticError);
            Operand op = new Operand(func.Execute(parametres));
            Eat(TokenType.CloseBracket);
            return op;
        }

        static List<Parameter> FunctionParams()
        {
            List<Parameter> parametres = new List<Parameter>();
            while (Peek.Type != TokenType.CloseBracket)
            {
                if (Peek.Type == TokenType.Quote)// "
                    Eat(TokenType.Quote);
                Token p = Eat(TokenType.StringConstant, TokenType.Identifier, TokenType.Number);                
                switch (p.Type)
                {
                    case TokenType.StringConstant:
                        parametres.Add(new Parameter(LexicalAnalyzer.StringConstants.IndexOf(p.Value)));
                        break;
                    case TokenType.Identifier:
                        DeclarationCheck(p);
                        parametres.Add(new Parameter(currentVisibility.Find(p.Value)));
                        break;
                    case TokenType.Number:
                        parametres.Add(new Parameter(p.Value, p.GetNumValue));
                        break;
                }
                //parametres.Add(p);
                if (Peek.Type == TokenType.Quote)// "
                    Eat(TokenType.Quote);
                if (Peek.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    if (Peek.Type == TokenType.CloseBracket)
                        throw new ErrorException("Неверное определение параметров", Peek, ErrorType.SyntaxError);
                }
            }
            return parametres;            
        }

        static void Assign()
        {            
            Operand op1 = new Operand(currentVisibility.FindOrCreate(Eat(TokenType.Identifier).Value));
            Eat(TokenType.Assign);
            Operand memSize = new Operand(0);
            TetradManager.Add(new Tetrad(OperationType.ALLOCMEM, memSize, null));
            int prevTetradCount = TetradManager.list.Count;
            dynPos = 0;
            Operand op2 = Sum();            
            if (Peek.Type==TokenType.Delimitier)
                Eat(TokenType.Delimitier);
            memSize.Constant = TetradManager.list.Count - prevTetradCount;
            TetradManager.Add(new Tetrad(OperationType.ASSIGN, op1.Clone(), op2.Clone()));
            if (memSize.Constant>0)
                TetradManager.Add(new Tetrad(OperationType.FREEMEM, null, null));
        }        

        static Operand Sum()
        {
            if (Peek.Value == "-")
            {
                Eat(TokenType.Operation);
                Operand op = Sum();
                Tetrad t = new Tetrad(OperationType.MUL, op.Clone(), new Operand(-1), dynPos++);
                TetradManager.Add(t);
                op.Set(t);
                return op;
            }
            else
            {
                Operand op1 = Mul();
                while (Peek.SubType == TokenSubType.Add)
                {
                    OperationType operation = Peek.Value == "+" ? OperationType.ADD : OperationType.SUB;
                    Eat(TokenType.Operation);
                    Operand op2 = Mul();
                    Tetrad t = new Tetrad(operation, op1.Clone(), op2.Clone(), dynPos++);
                    TetradManager.Add(t);
                    op1.Set(t);
                }
                return op1;
            }
        }

        static Operand Mul()
        {
            Operand op1 = Atom();
            while(Peek.SubType == TokenSubType.Mul)
            {
                OperationType operation = Peek.Value == "*" ? OperationType.MUL : OperationType.DIV;
                Eat(TokenType.Operation);
                Operand op2 = Atom();
                Tetrad t = new Tetrad(operation, op1.Clone(), op2.Clone(), dynPos++);
                TetradManager.Add(t);
                op1.Set(t);
            }
            return op1;
        }

        static Operand Atom()
        {
            Token d = Get;
            Operand op = new Operand();

            switch (d.Type)
            {
                case TokenType.Identifier:                    
                    op = new Operand(DeclarationCheck(d));
                    break;
                case TokenType.Number:
                    op = new Operand(d.GetNumValue);
                    break;
                case TokenType.Function:
                    op = FunctionCall(d, FunctionReturnType.sInt);
                    break;
                case TokenType.OpenBracket:
                    op = Sum();
                    Eat(TokenType.CloseBracket);
                    break;
                default:
                    throw new ErrorException("Неожиданный символ: " + d.Value, d, ErrorType.SyntaxError);
                    break;
            }
            return op;
        }
        
        static Variable DeclarationCheck(Token t)
        {
            Variable v = currentVisibility.Find(t.Value);
            if (t.Type == TokenType.Identifier && v == null)
                throw new ErrorException("Использование ранее необъявленной переменной", t, ErrorType.SyntaxError);
            else
                return v;
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
                if (v.Name == s)
                    return v;
            if (parentVisibility == null)
                return null;
            return parentVisibility.Find(s);
        }
        public Variable FindOrCreate(string s)
        {
            Variable v = Find(s);
            if (v != null)                            
                return v;            
            else
            {                
                Variable newV = new Variable(s);
                SyntaxAnalyzer.Variables.Add(newV);
                variables.Add(newV);                
                return newV;
            }
        }
    }

    class Variable
    {        
        string name;
        public string Name
        {
            get
            {
                return name;
            }
        }
        public Variable()
        {
        }
        public Variable(string name)
        {
            this.name = name;            
        }
        public int IndexInList
        {
            get
            {
                return SyntaxAnalyzer.Variables.IndexOf(this);
            }
        }
    }
}
