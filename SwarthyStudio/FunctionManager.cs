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
            if (functions.Contains(func))
                throw new ErrorException("Переопределение функции",ErrorType.SemanticError);
            functions.Add(func);
        }
        public static void Initialize()
        {
            Add(new sFunction("read", FunctionReturnType.Number, TokenType.StringConstant));
            Add(new sFunction("write", FunctionReturnType.Void, TokenType.StringConstant));
            Add(new sFunction("write", FunctionReturnType.Void, TokenType.Identifier));
            Add(new sFunction("write", FunctionReturnType.Void, TokenType.Number));
        }
        public static FunctionReturnType GetFuncTypeByName(string name)
        {
            List<sFunction> fns = GetAllByName(name);
            if (fns.Count==0)
                throw new Exception(string.Format("Функция \"{0}\" не существует",name));
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
    }
    class sFunction
    {        
        string name;        
        FunctionReturnType retType;
        TokenType[] Params;
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
                return Params.Length;
            }
        }
        public TokenType[] Parametres
        {
            get
            {
                return Params;
            }
        }
        public sFunction(string Name, FunctionReturnType returnType, params TokenType[] parametres)
        {
            retType = returnType;
            name = Name;
            Params = parametres;            
        }
        public bool isMyParams(params TokenType[] paramentres)
        {
            return Parametres == paramentres;
        }
        internal string Execute()
        {
            return "NULL";
        }
    }
    public enum FunctionReturnType
    {
        Void, Number
    }
}
