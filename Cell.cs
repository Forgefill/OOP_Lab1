using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace OOP_Lab1
{
    public class Cell
    {
        public Cell(dynamic value, string expr, int ColId, int RowId)
        {
            this.Value = value;
            this.RowId = RowId;
            this.ColId = ColId;
            Expression = expr;
        }
        private int RowId;
        private int ColId;
        private List<Cell> CellLinks = new List<Cell>();
        //
        public string Name { get; set; }
        public dynamic Value { get; set; }
        public string Expression { get; set; }
        public int Row
        {
            get { return RowId; }
        }
        public int Col
        {
            get { return ColId; }
        }

        //
        public void Evaluate()
        {
            Value = Calculator.Evaluate(Expression);
        }
        public void AddLinkToCell(Cell item)
        {
            CellLinks.Add(item);
        }

        public List<Cell> GetLinksToCell()
        {
            return CellLinks;
        }

        
    }
}
