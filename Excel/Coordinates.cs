using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Excel
{
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
            if (adr == null) { return null; }
            if(!adr.Any(char.IsDigit)) { return null; }
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
                if (!digit) { return null; }
                row = int.Parse(temp.ToString()) - 1;
                temp.Clear();
                if (!findElem(row, col, Table.input)) { return null; }
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
                if (findElem(row, col, otherList))
                    row = int.Parse(temp.ToString());
                temp.Clear();
                return otherList[row][col];

            }


        }
    }
}
