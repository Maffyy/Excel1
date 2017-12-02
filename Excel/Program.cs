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
        public override CellType getSymbol()
        {
            return CellType.INTEGER;
        }
        
    }
    class Formula : Cell
    {
        string formula;
        public Formula(string f)
        {
            formula = f;
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
                c = new Invval();
            }
            return c;
        }
        public static void storeTable(string input)
        {
            char[] delimiters = new char[] { ' ', '\t', '\n', '\r' };
            StreamReader sr = new StreamReader(input);
            List<ICell> line = new List<ICell>();
            while(sr.Peek() >= 0)
            {
                string[] tokens = sr.ReadLine().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    ICell c = parse(token);
                    line.Add(c);
                }
                line.Clear();
            }
        }
    }
    class Table
    {
        public static void buildTable()
        {
            
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
                Table.buildTable(args);
                Writer.outputEvalTable(args[1]);
            }
    }
}
