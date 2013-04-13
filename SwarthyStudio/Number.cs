using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    public class Number
    {
        public int value;
        public string strVal;
        public Number(string str)
        {
            str = str.ToUpper();
            strVal = str;
            int mul = 1;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                value += Helper.HexDigits.IndexOf(str[i]) * mul;
                mul *= 16;
            }
        }
        public static int Parse(string str)
        {
            str = str.ToUpper();
            int temp = 0, mul = 1;
            for (int i = str.Length - 1; i >= 0; i--)
            {
                temp += Helper.HexDigits.IndexOf(str[i]) * mul;
                mul *= 16;
            }
            return temp;
        }
    }        
}
