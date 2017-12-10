using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
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
        public static List<List<ICell>> input;
        public static Dictionary<string,List<List<ICell>>> lists = new Dictionary<string,List<List<ICell>>>();
        public static Stack<ICell> cellStack;
        public static ICell cycle;
        public static bool cycleTest = false;

        public static void markCycle(ICell op1, ICell op2)
        {
            Stack<ICell> temp = cellStack;
            if (cellStack.Contains(op1))
            {
                cycleTest = true;
                mark(op1,temp);
            }
            if (cellStack.Contains(op2))
            {
                cycleTest = true;
                mark(op2, temp);
            }
        }
        public static void mark(ICell cycle,Stack<ICell> st)
         {
             while (st.Peek() != cycle)
             {
                 //Console.WriteLine(true);
                 ICell c = st.Pop();
                 int x = c.getX();
                 int y = c.getY();
                 input[x][y] = new Invval(Error.CYCLE);
                 input[x][y].setX(x);
                 input[x][y].setY(y);
             }
            if (st.Peek() == cycle)
            {
                ICell c = cycle;
                int x = c.getX();
                int y = c.getY();

                input[x][y] = new Invval(Error.CYCLE);
                input[x][y].setX(x);
                input[x][y].setY(y);
            }
         }
 
        public static bool cycleB = false;
        public static ICell c;
        public static int x, y;
        public static bool missOpError(char op)
        {
            if (op == '0')
            {
                c = new Invval(Error.MISSOP);
                c.setX(x);
                c.setY(y);
                input[x][y] = c;
                return true;
            }
            return false;
        }
        public static bool formulaError(ICell op1, ICell op2)
        {
            if (op1 == null || op2 == null)
            {
                input[x][y] = new Invval(Error.FORMULA);
                input[x][y].setX(x);
                input[x][y].setY(y);
                return true ;
            }
            return false;
        }
        public static bool cellError(ICell op1, ICell op2)
        {
            if (op1.getSymbol() == CellType.INVVAL || op2.getSymbol() == CellType.INVVAL)
            {
                    input[x][y] = new Invval(Error.ERROR);
                    input[x][y].setX(x);
                    input[x][y].setY(y);
                    return true;
            }
            return false;
        }

        public static bool refErrorCycle(ICell op1,ICell op2)
        {
            if (op1.getSymbol() == CellType.INVVAL && ((Invval)op1).getError() == Error.CYCLE)
            {
                if (!(op2.getSymbol() == CellType.INVVAL && ((Invval)op2).getError() == Error.CYCLE))
                {
                    input[x][y] = new Invval(Error.ERROR);
                    input[x][y].setX(x);
                    input[x][y].setY(y);
                    return true;
                }
            }
            if (op2.getSymbol() == CellType.INVVAL && ((Invval)op2).getError() == Error.CYCLE)
            {
                if (!(op1.getSymbol() == CellType.INVVAL && ((Invval)op1).getError() == Error.CYCLE))
                {
                    input[x][y] = new Invval(Error.ERROR);
                    input[x][y].setX(x);
                    input[x][y].setY(y);
                    return true;
                }
            }
            return false;
        }
        public static int evaluateOperand(ICell op)
        {
            if (op.getSymbol() == CellType.EMPTY) { return 0; }
            else if(op.getSymbol() == CellType.INTEGER)
            {
                Integer num = (Integer)op;
                return num.getValue();
            }
            else if (op.getSymbol() == CellType.FORMULA)
            {
                solveFormula(op);
            }
            return 0;
        }
        public static void solveFormula(ICell c)
        {
            Formula f = (Formula)c;
            string formula = f.getFormula().Substring(1);
            x = c.getX();
            y = c.getY();
            cellStack.Push(c);          //uloz si vstupni bunku na zasobnik kvuli detekci cyklu
            string operand1 = null;
            string operand2 = null;
            char op = '0';
            StringBuilder temp = new StringBuilder();
          

            /*
             Nacitam formuli
             */
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
            /* Pokud nemam operator, vyhodim chybu MissOp */
            if (missOpError(op)) { return; }            // nejdriv zkontroluj operatory
            
            /*Nacteni operandu*/
           ICell op1 = Coordinates.getOperand(operand1);
           ICell op2 = Coordinates.getOperand(operand2);

            if (formulaError(op1,op2)) { return; }  // zkontroluj, jestli jsou dobre zapsany operandy
            if (cellError(op1,op2)) { return; } // pokud jsou, zkontroluj, jestli existuji v tabulkach



            markCycle(op1, op2);

            if (refErrorCycle(op1,op2)) { return; }
           
           

            int opV1 = evaluateOperand(op1);
            int opV2 = evaluateOperand(op2);
            if (input[x][y].getSymbol() == CellType.INVVAL) { return; }
            if (cycleTest)
            {
                if (cycle == c)
                {
                    cycleB = false;
                }
                return;
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
            return;
        }
        
        public static void buildTable()
        {
            for (int i = 0; i < input.Count; i++)
            {
                for (int j = 0; j < input[i].Count; j++)
                {
                    if(input[i][j].getSymbol() == CellType.FORMULA) {
                       cellStack = new Stack<ICell>();
                        solveFormula(input[i][j]);
                    }
                }
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
            StreamReader sr;
            try
            {
                sr = new StreamReader(input[0]);
            }
            catch (IOException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
            catch (SecurityException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
            catch (UnauthorizedAccessException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
            catch (ArgumentException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }

            StreamWriter sw;
            try
            {
                sw = new StreamWriter(input[1]);
            }
            catch (IOException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
            catch (SecurityException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
            catch (UnauthorizedAccessException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
            catch (ArgumentException)
            { Console.WriteLine("File Error"); Environment.Exit(0); }
        }
            static void Main(string[] args)
            {
            Table.input = new List<List<ICell>>();
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
