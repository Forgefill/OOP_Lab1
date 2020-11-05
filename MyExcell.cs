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

        public MyExcell(){}
        public void AddCell(string expression, string name, int ColId, int RowId)
        {
            bool add = false;
            Cell temp = new Cell(null, expression, ColId, RowId);
            
            if (!Table.TryAdd(name, temp))
            {
                temp = Table[name];
            }
            else
            {
                add = true;
            }

            try
            {
                LinkManager.FindLincs(name, expression, Table);
            }
            catch(ArgumentException e)
            {
                Table.Remove(name);
                throw e;
            }

            if (HasReferenceError(temp, name))
            {
                if (add)
                {
                    Table.Remove(name);
                }
                DelLink(temp);
                throw new StackOverflowException("Помилка. Комірка рекурсивно посилається на себе.");
            }


            dynamic value;

            try
            {
                value = Calculator.Evaluate(expression);
            }
            catch(Exception ex)
            {
                DelLink(temp);
                Table.Remove(name);
                throw ex;
            }
            


            Cell obj = new Cell(value, expression, ColId, RowId);
            obj.Name = name;


            if (Table.TryAdd(name, obj))
            {

            }
            else
            {
                Table[name].Value = obj.Value;
                Table[name].Expression = obj.Expression;
            }

            LinkManager.FindLincs(name, expression, Table);
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
            else if (links.IndexOf(Table[FirstName]) != -1)
            {
                links.Remove(Table[FirstName]);
                return true;
            }
            else if (links.IndexOf(item) != -1)
            {
                links.Remove(item);
                return true;
            }
            foreach (Cell i in item.GetLinksToCell())
            {
                if(HasReferenceError(i, FirstName)) return true;
            }

            return false;
        }

        private void DelLink(Cell item)
        {
            foreach(var i in Table)
            {
                if(i.Value.GetLinksToCell().IndexOf(item) != -1)
                {
                    i.Value.GetLinksToCell().Remove(item);
                }
            }
        }
    }
}
