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
        public static List<List<ICell>> input;
        public static Dictionary<string,List<List<ICell>>> lists = new Dictionary<string,List<List<ICell>>>();
        public static Stack<ICell> cellStack;
        public static ICell cycle;
   

        public static void markCycle(ICell cycle)
         {
             while (cellStack.Peek() != cycle)
             {
                 //Console.WriteLine(true);
                 ICell c = cellStack.Pop();
                 int x = c.getX();
                 int y = c.getY();
                 input[x][y] = new Invval(Error.CYCLE);
                 input[x][y].setX(x);
                 input[x][y].setY(y);
             }
            if (cellStack.Peek() == cycle)
            {
                //Console.WriteLine(true);
               // Console.WriteLine(((Formula)cycle).getFormula());
                ICell c = cycle;
                int x = c.getX();
                int y = c.getY();

                input[x][y] = new Invval(Error.CYCLE);
                input[x][y].setX(x);
                input[x][y].setY(y);
            }
         }
 
        public static bool cycleB = false;
        public static void solveFormula(ICell c)
        {
            Formula f = (Formula)c;
            string formula = f.getFormula().Substring(1);
            int x = c.getX();
            int y = c.getY();
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
             if (op == '0')
             {
                    c = new Invval(Error.MISSOP);
                    c.setX(x);
                    c.setY(y);
                    input[x][y] = c;
                    return;
             }
            /*Nacteni operandu*/
           ICell op1 = Coordinates.getOperand(operand1);
           ICell op2 = Coordinates.getOperand(operand2);
          
            if (cellStack.Contains(op1))
            {
                cycle = op1;
                Console.WriteLine(((Formula)cycle).getFormula());
                cycleB = true;
                markCycle(op1);
               // c = new Invval(Error.CYCLE);

                return;
            }
            if (cellStack.Contains(op2))
            {
                cycle = op2;
               // c = new Invval(Error.CYCLE);
                cycleB = true;
                markCycle(op2);

                return;
            }
            if (c.getSymbol() == CellType.INVVAL) { return; }
            
            int opV1 = 0;
            int opV2 = 0;
            
            if (op1 == null || op2 == null)
            {
                input[x][y] = new Invval(Error.FORMULA);
                input[x][y].setX(x);
                input[x][y].setY(y);
                return;
            }
            if (op1.getSymbol() == CellType.INVVAL || op2.getSymbol() == CellType.INVVAL)
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
            if (op1.getSymbol() == CellType.FORMULA)
            {
              //  cellStack.Push(c);
                solveFormula(op1);
                //    if (c.getSymbol() == CellType.INVVAL) { return; }
                

            }
            if (op2.getSymbol() == CellType.FORMULA)
            {
               // cellStack.Push(c);
                solveFormula(op2);
                

                //    if (c.getSymbol() == CellType.INVVAL) { return; }
            }

            if (cycleB)
            {
                if (cycle == c)
                {
                    Console.WriteLine(((Formula)cycle).getFormula());
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
          //  Console.WriteLine(input.Count);
            for (int i = 0; i < input.Count; i++)
            {
              //  Console.WriteLine(input[i].Count);
                for (int j = 0; j < input[i].Count; j++)
                {
               //     Console.WriteLine("b");
                   
                    if(input[i][j].getSymbol() == CellType.FORMULA) {
                       cellStack = new Stack<ICell>();
                        solveFormula(input[i][j]); }
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

            if (!File.Exists(input[0]) || !File.Exists(input[1]))
            {
                Console.WriteLine("File Error");
                Environment.Exit(0);
            }
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
               // Console.WriteLine("ahoj");
                Table.buildTable();
                Writer.outputEvalTable(args[1]);
            }
    }
}
