using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excel
{
    class Program
    {
        static void Main(string[] args)
        {
            catchFalseInput(args);
            Table.buildTable(args);
            Writer.outputEvalTable(args[1]);
        }
    }
}
