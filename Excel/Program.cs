using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Excel
{
    enum CellType { EMPTY, INTEGER, FORMULA, INVVAL };
    enum Error { ERROR, DIV0, FORMULA, MISSOP, INVVAL, CYCLE };

    interface ICell
    {
        CellType getSymbol();
        int getX();
        int getY();
        void setX(int cor);
        void setY(int cor);
    }
  
    abstract class Cell : ICell
    {
        int x, y;
        abstract public CellType getSymbol();
        public int getX()
        {
            return x;
        }
        public int getY()
        {
            return y;
        }
        public void setX(int cor)
        {
            x = cor;
        }
        public void setY(int cor)
        {
            y = cor;
        }
    }

    class Integer : Cell
    {
        int value { get; set; }
        public Integer(int v)
        {
            value = v;
        }
        public int getValue()
        {
            return value;
        }
        public override CellType getSymbol()
        {
            return CellType.INTEGER;
        }
        
    }
    class Formula : Cell
    {
        string formula { get; set; }
        public Formula(string f)
        {
            formula = f;
        }
        public string getFormula()
        {
            return formula;
        }
        public override CellType getSymbol()
        {
            return CellType.FORMULA;
        }
    }
    class Empty : Cell
    {
        public override CellType getSymbol()
        {
            return CellType.EMPTY;
        }
    }
    class Invval : Cell
    {
        Error error;
        public Invval(Error er)
        {
            error = er;
        }
        public Error getError()
        {
            return error;
        }
        public override CellType getSymbol()
        {
            return CellType.INVVAL;
        }
    }
    class Reader
    {
        
        public static ICell parse(string token)
        {
            ICell c;
            if (int.TryParse(token,out int num))
            {
                c = new Integer(num);
            }
            else if (token == "[]")
            {
                c = new Empty();
            }
            else if (token[0] == '=')
            {
                c = new Formula(token);
            }
            else
            {
                c = new Invval(Error.INVVAL);
            }
            return c;
        }
        
        
        public static List<List<ICell>> storeTable(string inputFile)
        {
            List<List<ICell>> cur = new List<List<ICell>>();
            char[] delimiters = new char[] { ' ', '\t', '\n', '\r' };
            StreamReader sr = new StreamReader(inputFile);
            List<ICell> line = new List<ICell>();
            while (sr.Peek() >= 0)
            {
                string[] tokens = sr.ReadLine().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    ICell c = parse(token);
                    line.Add(c);
                }
                cur.Add(line);
                line.Clear();
            }
            return cur;
        }

        public static void storeLists(string[] l)
        {
            for (int i = 2; i < l.Length; i++)
            {
                List<List<ICell>> input = storeTable(l[i]);
                Table.lists.Add(input);
            }
        }
    }
    class Table
    {
        public static List<List<ICell>> input = new List<List<ICell>>();
        public static List<List<List<ICell>>> lists = new List<List<List<ICell>>>();
        public static Stack<ICell> cellStack = new Stack<ICell>();

        public static void missingOp(char op, string operand1, string operand2, ICell c)
        {
            if (op == '0')
            {
                int i = c.getX();
                int j = c.getY();
                c = new Invval(Error.MISSOP);
                c.setX(i);
                c.setY(j);
                input[i][j] = c;
            }
            if (operand1 == null || operand2 == null)
            {
                int i = c.getX();
                int j = c.getY();
                c = new Invval(Error.FORMULA);
                c.setX(i);
                c.setY(j);
                input[i][j] = c;
            }
        }
       
        
        public static void solveFormula(ICell c)
        {
            Formula f = (Formula)c;
            string formula = f.getFormula();
            string operand1 = null;
            string operand2 = null;
            char op = '0';
            StringBuilder temp = new StringBuilder();
            for (int i = 0; i < formula.Length; i++)
            {
                if (formula[i].Equals('+') || formula[i].Equals('/') || formula[i].Equals('*') || formula[i].Equals('-'))
                {
                    operand1 = temp.ToString();
                    temp.Clear();
                    op = formula[i];
                }
                else
                {
                    temp.Append(formula[i]);
                }
            }
            if(!string.IsNullOrEmpty(temp.ToString())) { operand2 = temp.ToString(); }
            temp.Clear();
            missingOp(op, operand1, operand2, c);   // pokud chybi operator nebo operand

           

       
        }

        public static void Evaluate(ICell c)
        {
            
            if (c.getSymbol() == CellType.EMPTY) { return; }
            else if (c.getSymbol() == CellType.INTEGER) { return; }
            else if (c.getSymbol() == CellType.FORMULA)
            {
                solveFormula(c);
            }
            else if (c.getSymbol() ==CellType.INVVAL)
            {
                return;
            }
        }
        public static void buildTable()
        {
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Count; j++)
                {
                    input[i][j].setX(i);
                    input[i][j].setY(j);
                    Evaluate(input[i][j]);
                }
            }
        }
    }
    class Coordinates
    {
        public static string getExcelIndex(int num)
        {
            int dividend = num;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public static int getNumIndex(string columnName)
        {
            columnName = columnName.ToUpperInvariant();
            int sum = 0;
            for (int i = 0; i < columnName.Length; i++)
            {
                sum *= 26;
                sum += (columnName[i] - 'A');
            }

            return sum;
        }

        public static void getAddress(string adr, out string l, out int row, out int col)
        {
            StringBuilder temp = new StringBuilder();
            l = null;
            col = 0;
            row = 0;
            int i = 0;
            if (adr == null) { return; }
            if (!adr.Contains('!'))
            {
                while (char.IsLetter(adr[i]))
                {
                    temp.Append(adr[i]);
                    ++i;
                }
                col = getNumIndex(temp.ToString());
                temp.Clear();
                while (i < adr.Length && char.IsDigit(adr[i]))
                {
                    temp.Append(adr[i]);
                    ++i;
                }
                row = int.Parse(temp.ToString()) - 1;
                temp.Clear();

            }
            else
            {
                while (!adr[i].Equals('!'))
                {
                    temp.Append(adr[i]);
                    ++i;
                }
                l = temp.ToString();
                while (char.IsLetter(adr[i]))
                {
                    temp.Append(adr[i]);
                    ++i;
                }
                col = getNumIndex(temp.ToString());
                temp.Clear();
                while (char.IsDigit(adr[i]))
                {
                    temp.Append(adr[i]);
                    ++i;
                }
                row = int.Parse(temp.ToString());
                temp.Clear();
            }

        }
    }

    class Writer
    {

    }
    class Program
    {
        public static void catchFalseInput(string[] input)
        {
            if (input.Length != 2)
            {
                Console.WriteLine("Argument Error");
                Environment.Exit(0);
            }

            if (!File.Exists(input[0]) || !File.Exists(input[1]))
            {
                Console.WriteLine("File Error");
                Environment.Exit(0);
            }
        }
            static void Main(string[] args)
            {
                catchFalseInput(args);
                Table.input = Reader.storeTable(args[0]);
                if (args.Length > 2)
                {
                Reader.storeLists(args);
                }
                Table.buildTable();
               // Writer.outputEvalTable(args[1]);
            }
    }
}
