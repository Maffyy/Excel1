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
    
    class Table
    {
        public static List<List<ICell>> input = new List<List<ICell>>();
        public static Dictionary<string,List<List<ICell>>> lists = new Dictionary<string,List<List<ICell>>>();
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
            int x = c.getX();
            int y = c.getY();
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
            ICell op1 = Coordinates.getOperand(operand1);
            ICell op2 = Coordinates.getOperand(operand2);
            int opV1 = 0;
            int opV2 = 0;
            if (op1.getSymbol() == CellType.INVVAL || op2.getSymbol() == CellType.INVVAL )
            {
                input[x][y] = new Invval(Error.ERROR);
                input[x][y].setX(x);
                input[x][y].setY(y);
                return;
            }
            if (op1.getSymbol() == CellType.EMPTY) { opV1 = 0; }
            if (op2.getSymbol() == CellType.EMPTY) { opV2 = 0; }
            if (op1.getSymbol() == CellType.INTEGER)
            {
                Integer num = (Integer)op1;
                opV1 = num.getValue();
            }
            if (op2.getSymbol() == CellType.INTEGER)
            {
                Integer num = (Integer)op2;
                opV2 = num.getValue();
            }
            if (opV2 == 0 && op == '/')
            {
                input[x][y] = new Invval(Error.DIV0);
                input[x][y].setX(x);
                input[x][y].setY(y);
                return;
            }
            int result = 0;
            switch (op)
            {
                case '+':
                    result = opV1 + opV2;
                    break;
                case '-':
                    result = opV1 - opV2;
                    break;
                case '/':
                    result = opV1 / opV2;
                    break;
                case '*':
                    result = opV1 * opV2;
                    break;
                default:
                    break;
            }
            input[x][y] = new Integer(result);
            input[x][y].setX(x);
            input[x][y].setY(y);

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

    /// <summary>
    /// find coordinates of cell from operand
    /// </summary>
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
        public static bool findElem(int x, int y, List<List<ICell>> l)
        {
            for (int i = 0; i < l.Count; i++)
            {
                for (int j = 0; j < l[i].Count; j++)
                {
                    if (x == i && y == j)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static ICell getOperand(string adr)
        {

            StringBuilder temp = new StringBuilder();
            string l = null;
            int col = 0;
            int row = 0;
            int i = 0;
            bool digit = false;
            
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
                    digit = true;
                }
                if (!digit) {  return null; }
                row = int.Parse(temp.ToString()) - 1;
                temp.Clear();
                if(!findElem(row,col,Table.input)) { return null; }
                return Table.input[row][col];
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
                    digit = true;
                }
                if (!digit) { return null; }
                List<List<ICell>> otherList = Table.lists[l];
                if (findElem(row,col,otherList))
                row = int.Parse(temp.ToString());
                temp.Clear();
                return otherList[row][col];
                
            }
            

        }
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
                Writer.outputEvalTable(args[1]);
            }
    }
}
