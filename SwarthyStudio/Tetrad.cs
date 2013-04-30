using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static public class TetradManager
    {        
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
            foreach(Tetrad t in list)
            {                
                switch (t.Operation)
                {
                    case OperationType.ALLOCMEM:
                        if (t.Operand1.Constant>0)
                            CodeGenerator.Add(string.Format("lbl{1}: mov tempBuffer, alloc({0}*4)",t.Operand1.Constant,t.indexInList));
                        break;
                    case OperationType.FREEMEM:
                        CodeGenerator.Add(string.Format("lbl{0}: free([tempBuffer])",t.indexInList));
                        break;
                    case OperationType.GOTO:
                        CodeGenerator.Add(string.Format("lbl{1}: jmp lbl{0}",t.Operand1.Tetrad.indexInList,t.indexInList));
                        break;
                    case OperationType.ADD:                        
                        CodeGenerator.Add(string.Format("lbl{3}: mov tempBuffer[{0}*4], FUNC(IntAdd, {1}, {2})", t.positionInDynamicMemory, t.Operand1.Code, t.Operand2.Code,t.indexInList));
                        break;
                    case OperationType.SUB:
                        CodeGenerator.Add(string.Format("lbl{3}: mov tempBuffer[{0}*4], FUNC(IntSub, {1}, {2})", t.positionInDynamicMemory, t.Operand1.Code, t.Operand2.Code, t.indexInList));
                        break;
                    case OperationType.MUL:
                        CodeGenerator.Add(string.Format("lbl{3}: mov tempBuffer[{0}*4], FUNC(IntMul, {1}, {2})", t.positionInDynamicMemory, t.Operand1.Code, t.Operand2.Code, t.indexInList));
                        break;
                    case OperationType.DIV:
                        CodeGenerator.Add(string.Format("lbl{3}: mov tempBuffer[{0}*4], FUNC(IntDiv, {1}, {2})", t.positionInDynamicMemory, t.Operand1.Code, t.Operand2.Code, t.indexInList));
                        break;
                    case OperationType.IF:
                        CodeGenerator.Add(string.Format("lbl{0}: mov eax, {1}", t.indexInList, t.Operand1.Code));
                        CodeGenerator.Add(string.Format("      mov ebx, {0}", t.Operand2.Code));
                        CodeGenerator.Add(string.Format("      cmp eax, ebx"));
                        //
                        break;
                    case OperationType.GREATER:
                        CodeGenerator.Add(string.Format("      jle lbl{0}", t.Operand2.Tetrad.indexInList));
                        break;
                    case OperationType.LESS:
                        CodeGenerator.Add(string.Format("      jge lbl{0}", t.Operand2.Tetrad.indexInList));
                        break;
                    case OperationType.EQUAL:
                        CodeGenerator.Add(string.Format("      jne lbl{0}", t.Operand2.Tetrad.indexInList));
                        break;
                    case OperationType.NOTEQUAL:
                        CodeGenerator.Add(string.Format("      je lbl{0}", t.Operand2.Tetrad.indexInList));
                        break;
                    
                    case OperationType.MARK:
                        CodeGenerator.Add(string.Format("lbl{0}: ", t.indexInList));
                        break;
                    case OperationType.ASSIGN:
                        CodeGenerator.Add(string.Format("lbl{1}: mov eax, {0}", t.Operand2.Code, t.indexInList));
                        CodeGenerator.Add(string.Format("\t   mov {0}, eax", t.Operand1.Code));
                        break;
                    case OperationType.FUNCTIONCALL:
                        CodeGenerator.Add(string.Format("lbl{0}: {1}",t.indexInList,t.Operand1.FunctionCode));
                        break;                           
                }
                
            }
        }        
    }
    internal class Tetrad
    {
        public Operand Operand1, Operand2;
        public OperationType Operation;
        public int positionInDynamicMemory = -1;
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
        public Tetrad(OperationType operation, Operand operand1, Operand operand2, int posInDynMem = -1)
        {
            Operation = operation;
            Operand1 = operand1;
            Operand2 = operand2;
            positionInDynamicMemory = posInDynMem;
        }
        public override string ToString()
        {
            return string.Format("[{3}.{4}]\t({0}, {1}, {2})", Enum.GetName(typeof(OperationType), Operation),Operand1, Operand2, TetradManager.list.IndexOf(this),positionInDynamicMemory);
        }
        public int indexInList
        {
            get
            {
                return TetradManager.list.IndexOf(this);
            }
        }
    }
    internal class Operand
    {
        OperandType type = OperandType.Constant;
        Tetrad tetrad;
        Variable var;
        public string FunctionCode = "<NullCode>";
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
            internal set
            {
                constant = value;
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
        public Operand(string value)
        {            
            this.FunctionCode = value;
            type = OperandType.FunctionCode;            
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
                        return SyntaxAnalyzer.Variables.IndexOf(var);//возвращает номер переменной в списке переменных
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
        public string Code
        {
            get
            {
                switch (type)
                {
                    case OperandType.Constant:
                        return constant.ToString();
                        break;
                    case OperandType.Variable:
                        return string.Format("variables[{0}*4]",var.IndexInList);
                        break;
                    case OperandType.Tetrad:
                        return string.Format("tempBuffer[{0}*4]",tetrad.positionInDynamicMemory);
                        break;
                    case OperandType.FunctionCode:
                        return FunctionCode;
                        break;
                    default:
                        return "NONE";
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
        public void Set(string value, bool isCode = false)
        {
            if (isCode)
            {
                this.FunctionCode = value;
                type = OperandType.FunctionCode;
            }
            else
            {
                this.var = new Variable(value);
                type = OperandType.Variable;
            }
        }
        public void Set(int constant)
        {
            this.constant = constant;
            type = OperandType.Constant;
        }        
        public override string ToString()
        {
            switch (Type)
            {
                case OperandType.Constant:
                    return constant.ToString();
                    break;
                case OperandType.Variable:
                    return string.Format("<{0}>", var.Name);
                    break;
                case OperandType.Tetrad:
                    return string.Format("^{0}",TetradManager.list.IndexOf(tetrad));
                    break;
                case OperandType.FunctionCode:
                    return FunctionCode;
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
                case OperandType.FunctionCode:
                    return new Operand(FunctionCode);
                    break;
                default:
                    return null;
            }            
        }
    }    
    internal enum OperandType
    {
        Constant, Variable, Tetrad, FunctionCode
    }
    internal enum OperationType
    {
        ADD, SUB, MUL, DIV, ASSIGN, IF, FUNCTIONCALL, GREATER, LESS, EQUAL, NOTEQUAL, MARK, GOTO, ALLOCMEM, FREEMEM
    }    
}