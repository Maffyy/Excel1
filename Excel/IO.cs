using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Excel
{
    class Reader
    {

        public static ICell parse(string token)
        {
            ICell c;
            if (int.TryParse(token, out int num))
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
                Table.lists.Add(l[i], input);
            }
        }
    }


    class Writer
    {
        public static void outputEvalTable(string output)
        {
            StreamWriter wr = new StreamWriter(output);
            foreach (List<ICell> line in Table.input)
            {
                for (int i = 0; i < line.Count; i++)
                {
                    if (line[i].getSymbol() == CellType.EMPTY) { wr.Write("[]"); }
                    else if (line[i].getSymbol() == CellType.INTEGER)
                    {
                        Integer num = (Integer)line[i];
                        wr.Write(num.getValue());
                    }
                    else if (line[i].getSymbol() == CellType.INVVAL)
                    {
                        Invval inv = (Invval)line[i];
                        if (inv.getError() == Error.INVVAL) { wr.Write("#INVVAL"); }
                        if (inv.getError() == Error.ERROR) { wr.Write("#ERROR"); }
                        if (inv.getError() == Error.DIV0) { wr.Write("#DIV0"); }
                        if (inv.getError() == Error.FORMULA) { wr.Write("#FORMULA"); }
                        if (inv.getError() == Error.MISSOP) { wr.Write("#MISSOP"); }
                    }
                    if (i != line.Count - 1) { wr.Write(' '); }
                }
                wr.Write("\n");
                wr.Flush();
            }
            wr.Flush();
        }
    }
}
