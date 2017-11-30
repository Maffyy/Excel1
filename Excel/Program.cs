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

        public static void storeTable(string input)
        {
            StreamReader sr = new StreamReader(input);
            char c = (char)sr.Peek();
            StringBuilder token = new StringBuilder();
            List<string> line = new List<string>();

            while (sr.Peek() > -1)
            {
                c = (char)sr.Read();

                if (c.Equals(' ') || c.Equals('\t') || c.Equals('\n'))
                {
                    if (!token.Equals(string.Empty))
                    {
                        line.Add(token.ToString());
                    }
                    token.Clear();

                    if (c.Equals('\n'))
                    {
                        Table.input.Add(line);
                        line = new List<string>();
                    }
                }
                else
                {
                    token.Append(c);
                }


            }
            line.Add(token.ToString());
            Table.input.Add(line);
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
