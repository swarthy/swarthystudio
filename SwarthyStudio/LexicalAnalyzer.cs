using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwarthyStudio
{
    static class LexicalAnalyzer
    {
        public static List<Token> Lexems { get; private set; }
        public static List<string> StringConstants = new List<string>();
        static List<Transition> Transitions = new List<Transition>();
        static States currentState = States.Start;

        static string buffer = "";        
        

        public static void Process(string text)
        {
            Refresh();
            int line = 0;
            int pos = -1;
            char prev = '.';
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                pos++;
                if ((c == '\t' || c == '\r'))
                {
                    if (c == '\r') pos--;
                    continue;
                }
                if (c == '\n')
                {                    
                    postEvent(Events.newLine, c, pos, line);
                    line++;
                    pos = -1;
                    continue;
                }
                if (prev == '/' && c == '/')
                {
                    Lexems.RemoveAt(Lexems.Count - 1);
                    while (text[i] != '\n')
                        i++;
                    i--;
                    prev = '.';
                    continue;
                }
                
                    if (char.IsLetter(c) || char.IsDigit(c))
                        postEvent(Events.Symbol, c, pos, line);                    
                    else
                        if (H.isOperation(c))
                            postEvent(Events.Operation, c, pos, line);
                        else
                            switch (c)
                            {
                                case '(':
                                    postEvent(Events.OpenBracket, c, pos, line);
                                    break;
                                case ')':
                                    postEvent(Events.CloseBracket, c, pos, line);
                                    break;
                                case '{':
                                    postEvent(Events.OpenCurlyBracket, c, pos, line);
                                    break;
                                case '}':
                                    postEvent(Events.CloseCurlyBracket, c, pos, line);
                                    break;
                                case '=':
                                    postEvent(Events.Assign, c, pos, line);
                                    break;
                                case ';':
                                    postEvent(Events.Delimiter, c, pos, line);
                                    break;                                
                                case '>':
                                    postEvent(Events.More, c, pos, line);
                                    break;
                                case '<':
                                    postEvent(Events.Less, c, pos, line);
                                    break;
                                case '~':
                                    postEvent(Events.Equal, c, pos, line);
                                    break;
                                case '#':
                                    postEvent(Events.NotEqual, c, pos, line);
                                    break;
                                case '"':
                                    postEvent(Events.Quote, c, pos, line);
                                    break;
                                case ',':
                                    postEvent(Events.Comma, c, pos, line);
                                    break;
                                case ' ':
                                    postEvent(Events.Space, c, pos, line);
                                    break;
                                default:
                                    Err("Неизвестный символ", pos, line, ErrorType.LexicalError);
                                    break;
                            }
                    prev = c;
            }            
        }
        static void Refresh()
        {
            buffer = "";
            currentState = States.Start;
            Lexems.Clear();
            StringConstants.Clear();
        }        
        public static void Initialize()
        {
            FunctionManager.Initialize();
            Lexems = new List<Token>();

            Transitions.Add(new Transition(States.Start, Events.Symbol, States.Identified, (c, pos, line) => { buffer = c.ToString(); }));
            Transitions.Add(new Transition(States.Start, Events.Delimiter, States.Start, (c, pos, line) => { AddToken(c, TokenType.Delimitier, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.OpenBracket, States.Start, (c, pos, line) => { AddToken(c, TokenType.OpenBracket, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.CloseBracket, States.Start, (c, pos, line) => { AddToken(c, TokenType.CloseBracket, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.OpenCurlyBracket, States.Start, (c, pos, line) => { AddToken(c, TokenType.OpenCurlyBracket, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.CloseCurlyBracket, States.Start, (c, pos, line) => { AddToken(c, TokenType.CloseCurlyBracket, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.Operation, States.Start, (c, pos, line) => { AddToken(c, TokenType.Operation, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.Assign, States.Start, (c, pos, line) => { AddToken(c, TokenType.Assign, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.More, States.Start, (c, pos, line) => { AddToken(c, TokenType.Compare, TokenSubType.More, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.Less, States.Start, (c, pos, line) => { AddToken(c, TokenType.Compare, TokenSubType.Less, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.Equal, States.Start, (c, pos, line) => { AddToken(c, TokenType.Compare, TokenSubType.Equal, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.NotEqual, States.Start, (c, pos, line) => { AddToken(c, TokenType.Compare, TokenSubType.NotEqual, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.Quote, States.StringConst, (c, pos, line) => { AddToken(c, TokenType.Quote, TokenSubType.Equal, pos, line); }));
            Transitions.Add(new Transition(States.Start, Events.newLine, States.Start, (c, pos, line) => { }));
            Transitions.Add(new Transition(States.Start, Events.Space, States.Start, (c, pos, line) => { }));
            Transitions.Add(new Transition(States.Start, Events.Comma, States.Start, (c, pos, line) => { AddToken(c, TokenType.Comma, TokenSubType.Equal, pos, line); }));

            Transitions.Add(new Transition(States.Identified, Events.Symbol, States.Identified, (c, pos, line) => { buffer += c; }));
            Transitions.Add(new Transition(States.Identified, Events.Delimiter, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Delimitier, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.Operation, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Operation, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.CloseBracket, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.CloseBracket, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.OpenBracket, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.OpenBracket, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.OpenCurlyBracket, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier,TokenSubType.None,pos,line); AddToken(c, TokenType.OpenCurlyBracket,pos,line); }));
            Transitions.Add(new Transition(States.Identified, Events.CloseCurlyBracket, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier,TokenSubType.None,pos,line); AddToken(c, TokenType.CloseCurlyBracket,pos,line); }));
            Transitions.Add(new Transition(States.Identified, Events.Assign, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Assign, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.More, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Compare, TokenSubType.More, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.Less, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Compare, TokenSubType.Less, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.Equal, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Compare, TokenSubType.Equal, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.NotEqual, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Compare, TokenSubType.NotEqual, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.Comma, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Comma, TokenSubType.None, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.Quote, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.Quote, TokenSubType.None, pos, line); }));            
            Transitions.Add(new Transition(States.Identified, Events.EOS, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); AddToken(c, TokenType.EOS, pos, line); }));
            Transitions.Add(new Transition(States.Identified, Events.newLine, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); }));            
            Transitions.Add(new Transition(States.Identified, Events.Space, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.Identifier, TokenSubType.None, pos, line); }));

            #region события для строковой константы (удалить)
            //Transitions.Add(new Transition(States.StringConst, Events.Symbol, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Delimiter, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Operation, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.CloseBracket, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.OpenBracket, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.OpenCurlyBracket, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.CloseCurlyBracket, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Assign, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.More, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Less, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Equal, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.EOS, States.StringConst, (c, pos, line) => { buffer+=c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Space, States.StringConst, (c, pos, line) => { buffer += c; }));
            //Transitions.Add(new Transition(States.StringConst, Events.Comma, States.StringConst, (c, pos, line) => { buffer += c; }));
            #endregion

            Transitions.Add(new Transition(States.StringConst, Events.newLine, States.Start, (c, pos, line) => { Err("Неверное определение для строковой константы", pos, line, ErrorType.LexicalError); }));
            Transitions.Add(new Transition(States.StringConst, Events.Quote, States.Start, (c, pos, line) => { AddToken(buffer, TokenType.StringConstant, TokenSubType.None, pos, line); AddToken(c, TokenType.Quote, TokenSubType.None, pos, line); }));
            Transitions.Add(new Transition(States.StringConst, Events.Any, States.StringConst, (c, pos, line) => { buffer += c; }));

        }
        private static void AddToken(string value, TokenType type, TokenSubType subType=TokenSubType.None, int pos=0, int line=0)
        {            
            if (type == TokenType.Operation)
                Lexems.Add(new Token(value, type, H.operationType(value[0]),pos,line,1));
            else
                if (type==TokenType.Identifier)
                    Lexems.Add(H.parseIdentifier(value,pos,line));
                else                    
                    Lexems.Add(new Token(value, type, subType,pos,line,1));
            if (type == TokenType.StringConstant)
                StringConstants.Add(buffer);
            if (type == TokenType.Identifier || type == TokenType.StringConstant)
                buffer = "";
        }
        private static void AddToken(char c, TokenType type, int pos = 0, int line = 0)
        {
            AddToken(c.ToString(), type, TokenSubType.None, pos, line);
        }
        private static void AddToken(char c, TokenType type, TokenSubType subType, int pos = 0, int line = 0)
        {
            AddToken(c.ToString(), type, subType, pos, line);
        }
        private static void Err(string Message, ErrorType type)
        {
            Err(Message, -1, -1, type);
        }
        private static void Err(string Message, int pos, int line, ErrorType type)
        {            
            throw new ErrorException(Message, pos, line, type);
        }
        static void postEvent(Events Event, char c, int pos, int line)
        {
            bool found = false;
            foreach (Transition tr in Transitions)
                if ((tr.CurrentState == currentState || tr.alternativeCurrentStates.Contains(currentState)) && (tr.Event == Event || tr.Event == Events.Any))
                {
                    currentState = tr.NextState;
                    tr.Action(c, pos, line);
                    if (tr.NextState == States.Delimiter)
                        SyntaxAnalyzer.Process();
                    found = true;
                    break;
                }
            if (!found)
                throw new Exception("Для состояния " + Enum.GetName(typeof(States), currentState) + " нет эвента " + Enum.GetName(typeof(Events), Event));
        }
        #region Transaction
        public delegate void action(char ch, int pos, int line);
        public class Transition
        {
            public States CurrentState;
            public Events Event;
            public States NextState;
            public action Action;
            public States[] alternativeCurrentStates;

            public Transition(States currentState, Events Event, States nextState, action act, params States[] alternativeCurrentStates)
            {
                CurrentState = currentState;
                this.Event = Event;
                NextState = nextState;
                this.Action = act;
                this.alternativeCurrentStates = alternativeCurrentStates;
            }
        }
        #endregion    
    }
    #region ErrorException
    class ErrorException : Exception
    {
        public int Position { get; private set; }
        public int Line { get; private set; }
        public int Length { get; private set; }
        public ErrorType Type { get; private set; }
        public ErrorException()
            : base()
        {
        }
        public ErrorException(string message, ErrorType type)
            : base(message)
        {
            Type = type;
            Position = -1;
            Line = -1;
            Length = 1;
        }
        public ErrorException(string message, int pos, ErrorType type)
            : base(message)
        {
            Type = type;
            Position = pos;
            Line = -1;
            Length = 1;
        }
        public ErrorException(string message, int pos, int line, ErrorType type)
            : base(message)
        {
            Type = type;
            Position = pos;
            Line = line;
            Length = 1;
        }
        public ErrorException(string message, Token token, ErrorType type)
            : base(message)
        {
            Type = type;
            Position = token.Position;
            Line = token.Line;
            Length = token.Length;
        }
        public override string ToString()
        {
            return String.Format("{0}: {1}{2}", Enum.GetName(typeof(ErrorType), Type), Message, (Position == -1 ? "" : String.Format(" в строке {0} на {1} позиции", Line + 1, Position + 1)));
        }
    }
    enum ErrorType
    {
        LexicalError,
        SyntaxError,
        SemanticError,
        InternalError        
    }
    class Error
    {
        public int Position { get; private set; }
        public int Line { get; private set; }
        public ErrorType Type { get; private set; }
        public string Message { get; set; }
        public Error() { }
        public Error(string Message, ErrorType Type)
        {
            this.Message = Message;
            this.Type = Type;
        }
        public Error(string Message, ErrorType Type, int Pos, int Line)
        {
            this.Message = Message;
            Position = Pos;
            this.Line = Line;
            this.Type = Type;
        }
        public override string ToString()
        {
            return String.Format("{0}: {1}{2}", Enum.GetName(typeof(ErrorType), Type), Message, (Position == -1 ? "" : String.Format(" в строке {0} на {1} позиции", Line + 1, Position + 1)));
        }
    }
    #endregion
    #region Токены
    class Token
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }
        public TokenSubType SubType { get; set; }
        public int Position { get; set; }
        public int Line { get; set; }
        public int Length { get; set; }
        public Token() { }
        public Token(string value, TokenType type, int pos=0, int line=0, int length = 1)
        {
            Value = value;
            Type = type;
            SubType = TokenSubType.None;
            Position = pos;
            Line = line;
            Length = length;
        }
        public Token(string value, TokenType type, TokenSubType subtype, int pos = 0, int line=0, int length = 1)
        {
            Value = value;
            Type = type;
            SubType = subtype;
            Position = pos;
            Line = line;
            Length = length;
        }
        public int GetNumValue
        {
            get
            {
                if (Type != TokenType.Number)
                    throw new ErrorException("Попытка получения числового значения токена, не являющегося числом",this,ErrorType.SemanticError);
                return SubType == TokenSubType.HexNumber ? H.parseHex(Value) : H.parseRome(Value);
            }
        }
        public override string ToString()
        {
            return String.Format("[Value: \"{0}\", Type: {1}, SubType: {2}, Line: {3}, Position: {4}]", Value, Enum.GetName(typeof(TokenType), Type), Enum.GetName(typeof(TokenSubType), SubType), Line, Position);
        }
    }
    enum TokenSubType
    {
        Add, Mul, Write, Read, None, Less, More, Equal, NotEqual, RomeNumber, HexNumber
    }
    enum TokenType
    {
        Identifier = 0,
        Number,
        StringConstant,
        Assign,
        Operation,
        OpenBracket,
        CloseBracket,
        OpenCurlyBracket,
        CloseCurlyBracket,
        Delimitier,
        Comma,
        Quote,
        If,
        Else,
        While,
        For,
        Function,
        Compare,
        EOS        
    }
    #endregion
    #region События
    public enum Events
    {        
        Symbol,
        Quote,
        Comma,
        Space,
        Delimiter, Operation, Assign, OpenBracket, CloseBracket, OpenCurlyBracket, CloseCurlyBracket, EOS, Less, More, Equal, NotEqual, newLine, Any
    }
    #endregion
    #region Состояния
    public enum States
    {
        Start, Identified, Delimiter, StringConst//, Number10, Number16, Operation, Assign, Delimiter, OpenBracket, CloseBracket, OpenCurlyBracket, CloseCurlyBracket
    }
    #endregion
}