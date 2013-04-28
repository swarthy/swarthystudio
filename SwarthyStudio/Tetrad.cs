using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static public class TetradManager
    {
        public static string AssemblerCode = "";
        #region НачалоКода
        //public static string 
        #endregion
        internal static List<Tetrad> list = new List<Tetrad>();        
        static Tetrad currentTetrad;
        static public void Initialize()
        {            
            if (list.Count != 0)
                list.Clear();            
        }
        static internal Tetrad Add(Tetrad t)
        {
            list.Add(t);
            return t;
        }
        
        static public void BeginSolve()
        {
            AssemblerCode = "";
            currentTetrad = list.First();
            bool needIncrease;
            while (currentTetrad != null)
            {
                needIncrease = true;
                switch (currentTetrad.Operation)
                {
                    case OperationType.ADD:
                        currentTetrad.Result = currentTetrad.Operand1.Value + currentTetrad.Operand2.Value;                        
                        break;
                    case OperationType.SUB:
                        currentTetrad.Result = currentTetrad.Operand1.Value - currentTetrad.Operand2.Value;                        
                        break;
                    case OperationType.MUL:
                        currentTetrad.Result = currentTetrad.Operand1.Value * currentTetrad.Operand2.Value;
                        break;
                    case OperationType.DIV:
                        currentTetrad.Result = currentTetrad.Operand1.Value / currentTetrad.Operand2.Value;
                        break;
                    case OperationType.GREATER:
                        currentTetrad.Result = currentTetrad.Operand1.Value > currentTetrad.Operand2.Value ? 1 : 0;
                        break;
                    case OperationType.LESS:
                        currentTetrad.Result = currentTetrad.Operand1.Value < currentTetrad.Operand2.Value ? 1 : 0;
                        break;
                    case OperationType.EQUAL:
                        currentTetrad.Result = currentTetrad.Operand1.Value == currentTetrad.Operand2.Value ? 1 : 0;
                        break;
                    case OperationType.IF:
                        if (currentTetrad.Operand1.Value == 1)//operand1 == true
                        {

                        }
                        else
                        {
                            if (currentTetrad.Operand2.Tetrad == null)
                                throw new ErrorException("Переход из if в тетраду которая null", ErrorType.InternalError);                            
                            goTo(currentTetrad.Operand2.Tetrad);
                            needIncrease = false;
                            break;
                        }
                        break;
                    case OperationType.ASSIGN:
                        currentTetrad.Operand1.Variable.Value = currentTetrad.Operand2.Value;
                        break;
                    case OperationType.MARK://просто тетрада-метка, ничего не делает. нужна в if/if-else для упрощения передачи управления
                        break;
                    case OperationType.GOTO:
                        goTo(currentTetrad.Operand1.Tetrad);
                        break;
                }
                if (needIncrease)
                    currentTetrad = NextTetrad;
            }
        }
        static void goTo(Tetrad Next)
        {            
            currentTetrad = Next;
        }
        static Tetrad NextTetrad
        {
            get
            {
                return list[list.IndexOf(currentTetrad) + 1];
            }
        }
    }
    internal class Tetrad
    {
        public Operand Operand1, Operand2;
        public OperationType Operation;
        bool isLink = false;        
        public bool IsLink
        {
            get
            {
                return isLink;
            }
        }        
        internal int Result = 0;
        public int Value
        {
            get
            {                
                return Result;
            }
        }
        public Tetrad() { }
        public Tetrad(OperationType operation, Operand operand1, Operand operand2)
        {
            Operation = operation;
            Operand1 = operand1;
            Operand2 = operand2;
        }
        public override string ToString()
        {
            return string.Format("[{3}]\t({0}, {1}, {2})", Enum.GetName(typeof(OperationType), Operation),Operand1, Operand2, TetradManager.list.IndexOf(this));
        }
    }
    internal class Operand
    {
        OperandType type = OperandType.Constant;
        Tetrad tetrad;
        Variable var;
        sFunction func;
        int constant=0;
        public OperandType Type
        { get { return type; } }
        public Variable Variable
        {
            get { return var; }
        }
        public Tetrad Tetrad
        {
            get
            {
                return tetrad;                    
            }
        }
        public int Constant
        {
            get
            {
                return constant;
            }
        }
        public Operand()
        {
            type = OperandType.Constant;
        }
        public Operand(Variable var)
        {
            this.var = var;
            type = OperandType.Variable;
        }
        public Operand(sFunction func)
        {
            this.func = func;
            type = OperandType.Function;
        }
        public Operand(string existVariableName)
        {
            this.var = new Variable(existVariableName);
            type = OperandType.Variable;
        }
        public Operand(int constant)
        {
            this.constant = constant;
            type = OperandType.Constant;
        }
        public Operand(Tetrad tetrad)
        {
            this.tetrad = tetrad;
            type = OperandType.Tetrad;
        }
        public int Value
        {
            get
            {
                switch (type)
                {
                    case OperandType.Constant:
                        return constant;
                        break;
                    case OperandType.Variable:
                        return var.Value;
                        break;
                    case OperandType.Tetrad:
                        return tetrad.Value;
                        break;
                    default:
                        return -1;
                        break;
                }
            }
        }
        public void Set(Tetrad tetrad)
        {
            this.tetrad = tetrad;
            type = OperandType.Tetrad;
        }
        public void Set(Variable var)
        {
            this.var = var;
            type = OperandType.Variable;
        }
        public void Set(string existVariableName)
        {
            this.var = new Variable(existVariableName);
            type = OperandType.Variable;
        }
        public void Set(int constant)
        {
            this.constant = constant;
            type = OperandType.Constant;
        }
        public void Set(sFunction func)
        {
            this.func = func;
            type = OperandType.Function;
        }
        public override string ToString()
        {
            switch (Type)
            {
                case OperandType.Constant:
                    return constant.ToString();
                    break;
                case OperandType.Variable:
                    return string.Format("<{0}>", var.name);
                    break;
                case OperandType.Tetrad:
                    return string.Format("^{0}",TetradManager.list.IndexOf(tetrad));
                    break;
                case OperandType.Function:
                    return func.Execute();
                    break;
                default:
                    return "[unknown operand type]";
            }
        }
        public Operand Clone()
        {            
            switch (Type)
            {
                case OperandType.Constant:
                    return new Operand(constant);
                    break;
                case OperandType.Tetrad:
                    return new Operand(tetrad);
                    break;
                case OperandType.Variable:
                    return new Operand(var);
                    break;
                case OperandType.Function:
                    return new Operand(func);
                    break;
                default:
                    return null;
            }            
        }
    }    
    internal enum OperandType
    {
        Constant, Variable, Tetrad, Function
    }
    internal enum OperationType
    {
        ADD, SUB, MUL, DIV, ASSIGN, IF, WRITE, READ, GREATER, LESS, EQUAL, MARK, GOTO
    }    
}