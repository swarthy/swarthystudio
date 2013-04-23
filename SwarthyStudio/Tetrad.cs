using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static public class TetradManager
    {
        internal static List<Tetrad> list = new List<Tetrad>();
        internal static int positionInList = 0;
        static Tetrad currentTetrad;
        static public void Initialize()
        {
            positionInList = 0;
            if (list.Count != 0)
                list.Clear();            
        }
        static internal void Add(Tetrad t)
        {
            list.Add(t);
        }

        static Tetrad ParseTree(SyntaxTree tree)
        {
            Tetrad t = new Tetrad();
            if (tree.Type == SyntaxTreeType.Assign)
            {
                //tree.SubTrees[0].LeafValue.Value;
            }
            foreach (SyntaxTree sub in tree.SubTrees)
            {
                switch (sub.Type)
                {
                    case SyntaxTreeType.Assign:


                        break;
                    case SyntaxTreeType.Leaf:

                        break;
                }
            }
            return null;
        }

        static public void BeginSolve()
        {
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
                    case OperationType.MORE:
                        currentTetrad.Result = currentTetrad.Operand1.Value > currentTetrad.Operand2.Value ? 1 : 0;
                        break;
                    case OperationType.LESS:
                        currentTetrad.Result = currentTetrad.Operand1.Value < currentTetrad.Operand2.Value ? 1 : 0;
                        break;
                    case OperationType.EQUAL:
                        currentTetrad.Result = currentTetrad.Operand1.Value == currentTetrad.Operand2.Value ? 1 : 0;
                        break;
                    case OperationType.IF:
                        if (currentTetrad.Operand1.Value == 1)
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
                }
                if (needIncrease)
                    currentTetrad = list[++positionInList];
            }
        }
        static void goTo(Tetrad Next)
        {
            positionInList = list.IndexOf(Next);
            currentTetrad = Next;
        }
    }
    internal class Tetrad
    {
        public Operand Operand1, Operand2;
        public OperationType Operation;
        bool isLink = false;
        bool solved = false;
        public bool IsLink
        {
            get
            {
                return isLink;
            }
        }
        public bool Solved
        {
            get
            {
                return solved;
            }
        }
        internal int Result = 0;
        public int Value
        {
            get
            {
                if (!Solved)
                    throw new ErrorException("Обращение к еще не обработанной триаде", ErrorType.InternalError);
                else
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
    }
    internal class Operand
    {
        OperandType type = OperandType.Constant;
        Tetrad tetrad;
        Variable var;
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
        public Operand(int constant)
        {
            this.constant = constant;
            type = OperandType.Constant;
        }
        public Operand(Tetrad triad)
        {
            this.tetrad = triad;
            type = OperandType.Triad;
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
                    case OperandType.Triad:
                        return tetrad.Value;
                        break;
                    default:
                        return -1;
                        break;
                }
            }
        }
        public bool Ready
        {
            get
            {
                if (Type == OperandType.Triad)
                    return tetrad.Solved;
                else
                    return true;
            }
        }

    }    
    internal enum OperandType
    {
        Constant, Variable, Triad
    }
    internal enum OperationType
    {
        ADD, SUB, MUL, DIV, ASSIGN, IF, WRITE, READ, MORE, LESS, EQUAL
    }
    interface Leaf
    {
        int Value
        {
            get;
            set;
        }
    }
}