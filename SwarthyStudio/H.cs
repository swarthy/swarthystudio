using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Runtime.InteropServices;

namespace SwarthyStudio
{
    static class H
    {
        static public char Delimiter = ';';
        static public char Assign = '=';
        static public string Operations = "+-*/";
        static public string DecDigits = "0123456789";
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
                case "do":
                    type = TokenType.Do;
                    break;
                case "while":
                    type = TokenType.While;
                    break;
                case "break":
                    type = TokenType.Break;
                    break;
                default:
                    if (FunctionManager.isFunction(s))
                        type = TokenType.Function;
                    break;
            }
            if (type == TokenType.Identifier)
            {
                bool isHex = true, isRome = true, isDec = (s.Last()=='d' && s.Length>1);                
                foreach (char c in s)
                {
                    if (!HexDigits.Contains(c))
                        isHex = false;
                    if (!RomeDigits.Contains(c))
                        isRome = false;
                    if (isDec && s.IndexOf(c)!=s.Length-1 && !DecDigits.Contains(c))
                        isDec = false;
                    if (!isHex && !isRome && !isDec)
                        break;
                }
                if (isHex || isRome || isDec)
                    type = TokenType.Number;
                else
                    if (char.IsDigit(s[0]))
                        throw new ErrorException("Идентификатор/функция не может начинаться с цифры", pos - s.Length, line, ErrorType.LexicalError);

                subType = isHex ? TokenSubType.HexNumber : isRome ? TokenSubType.RomeNumber : isDec ? TokenSubType.DecNumber : TokenSubType.None;
            }
            return new Token(s, type, subType, pos - s.Length, line, s.Length);
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
        public static void SetAssociationWithExtension(string Extension, string KeyName, string OpenWith, string FileDescription)
        {
            RegistryKey BaseKey;
            RegistryKey OpenMethod;
            RegistryKey Shell;
            RegistryKey CurrentUser;

            BaseKey = Registry.ClassesRoot.CreateSubKey(Extension);
            BaseKey.SetValue("", KeyName);

            OpenMethod = Registry.ClassesRoot.CreateSubKey(KeyName);
            OpenMethod.SetValue("", FileDescription);
            OpenMethod.CreateSubKey("DefaultIcon").SetValue("", "\"" + OpenWith + "\",0");
            Shell = OpenMethod.CreateSubKey("Shell");
            Shell.CreateSubKey("edit").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
            Shell.CreateSubKey("open").CreateSubKey("command").SetValue("", "\"" + OpenWith + "\"" + " \"%1\"");
            BaseKey.Close();
            OpenMethod.Close();
            Shell.Close();

            CurrentUser = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\"+Extension, true);
            CurrentUser.DeleteSubKey("UserChoice", false);
            CurrentUser.Close();

            // Tell explorer the file association has been changed
            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
        [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
    }

}
