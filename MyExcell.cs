using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

namespace OOP_Lab1
{
    public class MyExcell
    {
        public Dictionary<string, Cell> Table { get; } = new Dictionary<string, Cell>();

        public MyExcell()
        {

        }
        public void AddCell(string expression, string name, int ColId, int RowId)
        {
            bool add = false;
            Cell temp = new Cell('|', expression, ColId, RowId);

            if (!Table.TryAdd(name, temp))
            {
                temp = Table[name];
            }
            else
            {
                add = true;
            }

            LinkManager.FindLincs(name, expression, Table);

            if (HasReferenceError(temp, name))
            {
                if (add) Table.Remove(name);
                throw new StackOverflowException("Помилка. Комірка рекурсивно посилається на себе.");
            }

            dynamic value;

            try
            {
                value = Calculator.Evaluate(expression);
            }
            catch(Exception ex)
            {
                if (add) Table.Remove(name);
                foreach(var i in Table.Values)
                {
                    i.GetLinksToCell().Remove(temp);
                }
                throw ex;
            }



            Cell obj = new Cell(value, expression, ColId, RowId);
            obj.Name = name;

            Table[name] = obj;

            LabCalculatorVisitor.tableIdentifier[name] = value;

        }
        public void Clear()
        {
            Table.Clear();
        }

        public bool HasReferenceError(Cell item, string FirstName)
        {
            string CellName = item.Name;
            var links = item.GetLinksToCell();
            if (!links.Any())
            {
                return false;
            }
            else if (links.IndexOf(Table[FirstName]) == -1)
            {
                return true;
            }
            else if (links.IndexOf(item) != -1)
            {
                return true;
            }
            foreach (Cell i in item.GetLinksToCell())
            {
                HasReferenceError(i, FirstName);
            }

            return false;
        }
    }
}
