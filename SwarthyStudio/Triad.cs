using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    class Triad
    {
        Operand Operand1, Operand2;
        OperationType Operation;
        public bool isLink = false;

        public Triad() { }
        public Triad(OperationType operation, Operand operand1, Operand operand2)
        {
            Operation = operation;
            Operand1 = operand1;
            Operand2 = operand2;
        }
    }
    class Operand
    {
        OperandType type = OperandType.Constant;
        Triad triad;
        Variable var;
        int constant=0;
        public OperandType Type
        { get { return type; } }
        public Variable Variable
        {
            get { return var; }
        }
        public Triad Triad
        {
            get
            {
                return triad;                    
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
        public Operand(Triad triad)
        {
            this.triad = triad;
            type = OperandType.Triad;
        }
    }    
    enum OperandType
    {
        Constant, Variable, Triad
    }
    enum OperationType
    {
        ADD, SUB, MUL, DIV, ASSIGN, IF, WRITE, READ
    }
}