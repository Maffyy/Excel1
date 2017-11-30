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
        {qwf
            catchFalseInput(args);wfqf
            Table.buildTable(args);
            Writer.outputEvalTable(args[1]);
        }
    }
}
