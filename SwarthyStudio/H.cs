using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class H
    {
        static public char Delimiter = ';';
        static public char Assign = '=';
        static public string Operations = "+-*/";        
        static public string HexDigits = "0123456789ABCDEF";
        static public string RomeDigits = "IVXLCDM";
        static string[] helpRomeDigits = new string[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
        static int[] helpArabDigits = new int[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
        public static bool isOperation(char sym)
        {
            return Operations.Contains(sym);
        }
        public static TokenSubType operationType(char sym)
        {
            switch (Operations.IndexOf(sym))
            {
                case 0:
                case 1:
                    return TokenSubType.Add;
                case 2:
                case 3:
                    return TokenSubType.Mul;
                default:
                    return TokenSubType.None;
            }
        }
        public static Token parseIdentifier(string s, int pos, int line)
        {
            TokenType type = TokenType.Identifier;
            TokenSubType subType = TokenSubType.None;
            switch (s.ToLower())
            {
                case "if":
                    type = TokenType.If;
                    break;
                case "else":
                    type = TokenType.Else;
                    break;
                case "for":
                    type = TokenType.For;
                    break;
                case "while":
                    type = TokenType.While;
                    break;
                case "read":
                    type = TokenType.Read;
                    break;
                case "write":
                    type = TokenType.Write;
                    break;
            }
            bool isHex = true, isRome = true;
            foreach (char c in s)
            {
                if (!HexDigits.Contains(c))
                    isHex = false;
                if (!RomeDigits.Contains(c))
                    isRome = false;
                if (!isHex && !isRome)
                    break;
            }
            if (isHex || isRome)            
                type = TokenType.Number;                
            else
                if (char.IsDigit(s[0]))
                    throw new ErrorException("Идентификатор не может начинаться с цифры", pos-s.Length, line, ErrorType.LexicalError);
            
            subType = isHex ? TokenSubType.HexNumber : isRome ? TokenSubType.RomeNumber : TokenSubType.None;            
            return new Token(s, type, subType,pos-s.Length,line,s.Length);                 
        }        
        static public int parseRome(string romeString)
        {
            int i=0, n=0;
            while (romeString.Length > 0)
            {
                while (romeString.Length > 0 && helpRomeDigits[i] == ((helpRomeDigits[i].Length<=romeString.Length)?romeString.Substring(0, helpRomeDigits[i].Length):""))
                {
                    romeString = romeString.Remove(0, helpRomeDigits[i].Length);
                    n += helpArabDigits[i];
                }
                i++;
            }
            return n;
        }
        static public int parseHex(string hexString)
        {
            hexString = hexString.ToUpper();
            int temp = 0, mul = 1;
            for (int i = hexString.Length - 1; i >= 0; i--)
            {
                temp += H.HexDigits.IndexOf(hexString[i]) * mul;
                mul *= 16;
            }
            return temp;
        }
    }

}
