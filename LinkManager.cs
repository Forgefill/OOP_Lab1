using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
namespace OOP_Lab1
{
    public static class LinkManager
    {
        // Перевизначення всіх колонок у яких був присутній(і всіх що вище) item.
        public static void SendRedefinition(Cell item, DataGridView dgv)  
        {
            var names = item.GetLinksToCell();
            foreach (Cell i in names)
            {
                
                string ColumnName = dgv.Columns[i.Col].HeaderText;
                string RowName = dgv.Rows[i.Row].HeaderCell.Value.ToString();

                i.Evaluate();
                
                LabCalculatorVisitor.tableIdentifier[ColumnName + RowName] = i.Value;
                dgv[i.Col, i.Row].Value = i.Value;
            }
            foreach (Cell s in names)
            {
                SendRedefinition(s, dgv);
            }
        }
        // Метод для знаходження імен колонок, що знаходяться в name.Expression.
        public static void FindLincs(string name, string expression, Dictionary<string, Cell> MyTable) 
        {  //   1 + 2 > 3  = A1;
            for (int i = 0; i < expression.Length - 1; ++i)
            {
                if (expression[i] > 64 && expression[i] < 91) // A-Z
                {
                    string res = "";
                    while ((expression[i] > 64 && expression[i] < 91) || (expression[i] > 47 && expression[i] < 58)) // A-Z || 0-9 
                    {
                        if (expression[i] == expression.Last())
                        {
                            res += expression[i];
                            break;
                        }
                        res += expression[i];
                        ++i;
                    }
                    Cell obj;
                    if (MyTable.TryGetValue(res, out obj))
                    {
                        MyTable[res].AddLinkToCell(MyTable[name]);
                    }
                    else
                    {
                        throw new ArgumentException();
                    }
                }
            }
        }
        
    }
}
