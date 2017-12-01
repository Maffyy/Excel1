﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Excel
{
    public interface ICell { }
    public interface ICell<T>
    {
        T value { get; set; }
    }
    struct CellValue : ICell
    {
        public int value { get; set; }
    }
    struct CellString : ICell
    {

    }

    public class Cell
    {
        public string content { get; set; }
        public int value { get; set; }
        public Cell(int v)
        {
            value = v;
        }
        public Cell(string c)
        {
            content = c;
        }
    }

    class Reader
    {

        public static void storeTable(string input)
        {
            char[] delimiters = new char[] { ' ', '\t', '\n', '\r' };
            StreamReader sr = new StreamReader(input);
            List<Cell> line = new List<Cell>();
            Cell c;
            while(sr.Peek() >= 0)
            {
                string[] tokens = sr.ReadLine().Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
                foreach (string token in tokens)
                {
                    if (int.TryParse(token, out int num))
                    {
                        c = new Cell(num);
                    }
                    else
                    {
                        c = new Cell(token);
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
