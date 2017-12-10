using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security;
using System.Threading.Tasks;

namespace Excel
{
    class Reader
    {

        public static ICell parse(string token, int i, int j)
        {
            ICell c;
            if (int.TryParse(token, out int num))
            {
                c = new Integer(num);
                c.setX(i);
                c.setY(j);
            }
            else if (token == "[]")
            {
                c = new Empty();
                c.setX(i);
                c.setY(j);
            }
            else if (token[0] == '=')
            {
                c = new Formula(token);
                c.setX(i);
                c.setY(j);
            }
            else
            {
                c = new Invval(Error.INVVAL);
                c.setX(i);
                c.setY(j);
            }
            return c;
        }


        public static List<List<ICell>> storeTable(string inputFile)
        {
            List<List<ICell>> cur = new List<List<ICell>>();
            char[] delimiters = new char[] { ' ', '\t', '\n', '\r' };
            StreamReader sr = new StreamReader(inputFile);
            List<ICell> line = new List<ICell>();
            ICell c;
           
            int i = 0;
            while (sr.Peek() >= 0)
            {
                int j = 0;
                string[] tokens= sr.ReadLine().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    c = parse(token,i,j);
                    line.Add(c);
                    j++;
                }
                cur.Add(line);
                line = new List<ICell>();
                i++;
            }
            return cur;
        }

        public static void storeLists(string[] l)
        {
            for (int i = 2; i < l.Length; i++)
            {
                List<List<ICell>> input = storeTable(l[i]);
                Table.lists.Add(l[i], input);
            }
        }
    }


    class Writer
    {
        public static void outputEvalTable(string output)
        {
            
            StreamWriter wr = new StreamWriter(output);

            for (int i = 0; i < Table.input.Count; i++)
            {
                for (int j = 0; j < Table.input[i].Count; j++)
                {
                    if (Table.input[i][j].getSymbol() == CellType.EMPTY) { wr.Write("[]"); }
                    else if (Table.input[i][j].getSymbol() == CellType.INTEGER)
                    {
                        
                        Integer num = (Integer)Table.input[i][j];
                       // Console.WriteLine(true);
                        wr.Write(num.getValue());
                    }
                    else if (Table.input[i][j].getSymbol() == CellType.INVVAL)
                    {
                        Invval inv = (Invval)Table.input[i][j];
                        if (inv.getError() == Error.INVVAL) { wr.Write("#INVVAL"); }
                        if (inv.getError() == Error.ERROR) { wr.Write("#ERROR"); }
                        if (inv.getError() == Error.DIV0) { wr.Write("#DIV0"); }
                        if (inv.getError() == Error.FORMULA) { wr.Write("#FORMULA"); }
                        if (inv.getError() == Error.MISSOP) { wr.Write("#MISSOP"); }
                        if (inv.getError() == Error.CYCLE) { wr.Write("#CYCLE"); }
                    }
                    if (j != Table.input[i].Count - 1) { wr.Write(' '); }
                }
                if (i != Table.input.Count - 1) { wr.Write('\n'); }
                wr.Flush();
            }
            wr.Flush();
        }
    }
}
