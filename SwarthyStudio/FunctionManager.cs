using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class FunctionManager
    {
        public static List<sFunction> functions = new List<sFunction>();
        public static void Add(sFunction func)
        {            
            functions.Add(func);
        }
        public static void Initialize()
        {
            Add(new sFunction("read", FunctionReturnType.Number, prms => { return string.Format("FUNC(atol, input(\"{0}\"))", prms[0].StrValue); }, ParameterType.StringConstant));
            Add(new sFunction("read", FunctionReturnType.Void, prms => { return string.Format("inkey \"{0}\"", prms[0].StrValue); }, ParameterType.StringConstant));
            Add(new sFunction("write", FunctionReturnType.Void, prms => { return string.Format("print \"{0}\"", prms[0].StrValue); }, ParameterType.StringConstant));
            Add(new sFunction("write", FunctionReturnType.Void, prms => { return string.Format("print str$(Variables[{0}*4])", prms[0].intValue); }, ParameterType.Variable));
            Add(new sFunction("write", FunctionReturnType.Void, prms => { return string.Format("print \"{0}\"", prms[0].intValue); }, ParameterType.NumericConstant));            
        }
        public static FunctionReturnType GetFuncTypeByName(string name)
        {
            List<sFunction> fns = GetAllByName(name);
            if (fns.Count==0)
                throw new ErrorException(string.Format("Функция \"{0}\" не существует",name),ErrorType.SemanticError);
            return fns[0].ReturnType;
        }
        public static bool isFunction(string identifier)
        {
            return functions.Exists(f => f.Name == identifier.ToLower());
        }
        public static List<sFunction> GetAllByName(string funcName)
        {
            return functions.FindAll(f => f.Name==funcName);
        }
        public static sFunction GetByNameAndParams(string funcName, params ParameterType[] parameters)
        {
            return functions.Find(f => f.Name == funcName && f.ValidParametres.Intersect(parameters) == parameters);
        }
    }
    
    class sFunction
    {
        public delegate string Action(params Parameter[] parametres);
        string name;        
        FunctionReturnType retType;
        ParameterType[] ValidParams;
        Action action;
        public string Code;
        public string Name
        {
            get
            {
                return name;
            }
        }
        public FunctionReturnType ReturnType
        {
            get
            {
                return retType;
            }
        }
        public int ParamsCount
        {
            get
            {
                return ValidParams.Length;
            }
        }
        public ParameterType[] ValidParametres
        {
            get
            {
                return ValidParams;
            }
        }
        public sFunction(string Name, FunctionReturnType returnType, Action act, params ParameterType[] parametres)
        {
            retType = returnType;
            name = Name;
            ValidParams = parametres;
            action = act;
        }
        public bool isMyParams(List<Parameter> parametres, params FunctionReturnType[] validReturnTypes)
        {
            if (parametres.Count != ValidParametres.Length)
                return false;
            for (int i = 0; i < ValidParametres.Length; i++)
                if (parametres[i].Type != ValidParametres[i])
                    return false;
            if (!validReturnTypes.Contains(ReturnType))
                return false;
            return true;
        }
        internal string Execute(List<Parameter> parametres)
        {            
            return action(parametres.ToArray());
        }
    }
    public enum FunctionReturnType
    {
        Void, Number
    }
    class Parameter
    {
        public ParameterType Type;
        string constant;
        int indexInConstantList;
        int numValue;
        Variable variable;
        public Parameter(string value, int numVal)
        {
            constant = value;
            numValue = numVal;
            Type = ParameterType.NumericConstant;
        }
        public Parameter(int indexInConstantList)
        {
            this.indexInConstantList = indexInConstantList;
            Type = ParameterType.StringConstant;
        }
        public Parameter(Variable v)
        {
            variable = v;
            Type = ParameterType.Variable;
        }        
        public string StrValue
        {
            get
            {
                switch (Type)
                {             
                    case ParameterType.NumericConstant:
                        return constant;
                        break;
                    case ParameterType.StringConstant:
                        return LexicalAnalyzer.StringConstants[indexInConstantList];
                        break;
                    case ParameterType.Variable:
                        return variable.Name;
                        break;
                    default:
                        return "<ZERO PARAMETER>";
                        break;
                }
            }
        }
        public int intValue
        {
            get
            {
                switch (Type)
                {
                    case ParameterType.NumericConstant:
                        return numValue;
                        break;
                    case ParameterType.StringConstant:
                        return 0;
                        break;
                    case ParameterType.Variable:
                        return variable.IndexInList;
                        break;
                    default:
                        return -1;
                        break;
                }
            }
        }
    }
    public enum ParameterType
    {
        StringConstant,
        NumericConstant,
        Variable        
    }
}
