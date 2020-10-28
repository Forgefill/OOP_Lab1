using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
namespace OOP_Lab1
{        
    public partial class Form1 : Form
    {
        MyExcell data;
        bool mode = true;
        
        public Form1()
        {
            InitializeComponent();
            InfoBox.Click += InfoClick;
            Save.Click += SaveButtonClick;
            Open.Click += OpenFileClick;
            ClearFile.Click += ClearTableClick;
            AddRow.Click += AddRowClick;
            dgv.CellEndEdit += dgv_CellEndEdit;
            DeleteRow.Click += DelRowClick;
            data = new MyExcell();
            char s = 'A';
            while (s != (char)91)
            {
                dgv.Columns.Add(s.ToString(), s.ToString());
                ++s;
            }

            dgv.Rows.Add(27);
            for (int i = 0; (i <= (dgv.Rows.Count - 1)); i++)
            {
                dgv.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        
        private void SaveButtonClick(object sender, EventArgs e)       //Кнопка збереження таблиці
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream FileStream = saveFileDialog1.OpenFile();
                StreamWriter Sw = new StreamWriter(FileStream);

                Sw.Write("Комірка Значення Вираз\tСномер\tРномер\n");


                foreach(var i in data.Table)
                {
                    Sw.WriteLine(i.Key + "\t" + i.Value.Value.ToString() + "\t" + i.Value.Expression + "\t" + i.Value.Col.ToString() + "\t" + i.Value.Row.ToString());
                }                   // B1 false  1 > 2  0 0
                Sw.Close();
                FileStream.Close();
            }
        }


        private void OpenFileClick(object sender, EventArgs e)     //Кнопка загрузки таблиці
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                DialogResult dr = MessageBox.Show("Поточні дані буде видалено. Відкрити файл?",
                      "Важливо!", MessageBoxButtons.YesNo);
                switch (dr)
                {
                    case DialogResult.Yes:
                        break;
                    case DialogResult.No:
                        return;
                }
                ClearTableClick("a", new EventArgs());

                Stream FileStream = openFileDialog1.OpenFile();
                StreamReader Sr = new StreamReader(FileStream);

                Sr.ReadLine();


                while (!Sr.EndOfStream)
                {
                    string cell = Sr.ReadLine();
                    try
                    {
                        ReadCell(cell);
                    }
                    catch(Exception)
                    {
                        label1.Text = "Дані в файлі, знаходяться у неправильній формі";
                        break;
                    }
                }

                Sr.Close();
                FileStream.Close();

                foreach (var i in data.Table)
                {
                    LinkManager.FindLincs(i.Key, i.Value.Expression, data.Table);
                }

            }
        }
        
        
        private void ReadCell(string cell)      // Метод зв'язаний з зайгрузкою таблиці з файлу. 
        {
            List<string> a = new List<string>();
            string temp = "";
            for (int i = 0; i < cell.Length; ++i)
            {
                if (cell[i] != '\t')
                {
                    temp += cell[i];
                }
                else
                {
                    a.Add(temp);
                    temp = "";
                }
            }
            a.Add(cell.Last().ToString());

            bool BValue;
            dynamic Value;
            string Name = a[0];
            if(bool.TryParse(a[1], out BValue))
            {
                Value = BValue;
            }
            else
            {
                Value = double.Parse(a[1]);
            }
            string Expression = a[2];
            int ColId = int.Parse(a[3]);
            int RowId = int.Parse(a[4]);

            Cell obj = new Cell(Value, Expression, ColId, RowId);

            data.Table.Add(a[0], obj);
            LabCalculatorVisitor.tableIdentifier[Name] = Value;

            if (mode)
            {
                dgv[ColId, ColId].Value = Value;
            }
            else
            {
                dgv[ColId, RowId].Value = Expression;
            }
        }


        private void ClearTableClick(object sender, EventArgs e) // Очистити таблицю
        {
            data.Clear();
            LabCalculatorVisitor.tableIdentifier.Clear();
            dgv.Rows.Clear();

            dgv.Rows.Add(100);
            for (int i = 0; (i <= (dgv.Rows.Count - 1)); i++)
            {
                dgv.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
        }

        private void InfoClick(object sender, EventArgs e)        // Вивести інформацію про програму
        {
            DialogResult dr = MessageBox.Show("\t\tОсновні операції: " +
                "\n'!' - заперечення логічного виразу." +
                "\n'=','<','>' - операції порівняння." +
                "\n'+','-' - сума та різниця." +
                "\n'*','/' - множення та ділення" +
                "\n'++'('--') - інкремент та декремент(префіксні)" +
                "\nможливість посилатися на комірки за допомогою назв стовпчик/рядок(A1)",
                      "Довідка", MessageBoxButtons.OK);
        }   


       private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv.CurrentCell.Value == null )
            {
                return;
            }

            int RowId = dgv.CurrentCell.RowIndex;
            int ColId = dgv.CurrentCell.ColumnIndex;
            string Name = dgv.Columns[ColId].Name + dgv.Rows[RowId].HeaderCell.Value; //A1
            string Expression = dgv.CurrentCell.Value.ToString();
            Cell obj;

            try
            {
                data.AddCell(Expression, Name, ColId, RowId);
            }
            catch(ArgumentException)
            {
                label1.Text = "Вираз в комірці " + Name + " не вдалося обробити.";
                dgv.CurrentCell.Value = null;
                return;
            }
            catch (DivideByZeroException ex)
            {
                label1.Text = ex.Message + " в комірці " + Name;
                dgv[ColId, RowId].Value = null; 
                return;
            }
            catch(StackOverflowException ex)
            {
                label1.Text = ex.Message + Name;
                if(data.Table.TryGetValue(Name, out obj))
                {
                    if (mode)
                    {
                        dgv.CurrentCell.Value = obj.Value;
                    }
                    else
                    {
                        dgv.CurrentCell.Value = obj.Expression;
                    }
                }
                else
                {
                    dgv.CurrentCell.Value = null;
                    return;
                }
            }

            if (mode)
            {
                dgv[ColId, RowId].Value = data.Table[Name].Value;
            }
            else
            {
                dgv[ColId, RowId].Value = Expression;
            }

            LinkManager.SendRedefinition(data.Table[Name], dgv);

        }


        private void ModeButtonClick(object sender, EventArgs e) // Кнoпка зміни режиму;
        {
            if (mode)
            {
                foreach (var s in data.Table)
                {
                    dgv[s.Value.Col, s.Value.Row].Value = s.Value.Expression;
                    mode = false;
                }
                ModeBox.Text = "Вираз";
            }
            
            else
            {
                foreach (var s in data.Table)
                {
                    dgv[s.Value.Col, s.Value.Row].Value = s.Value.Value;
                    mode = true;
                }
                ModeBox.Text = "значення";
            }
        }
        private void AddRowClick(object sender, EventArgs e)
        {
            dgv.Rows.Add();
            dgv.Rows[dgv.Rows.Count - 2].HeaderCell.Value = (dgv.Rows.Count - 1).ToString();
            dgv.Rows[dgv.Rows.Count - 1].HeaderCell.Value = (dgv.Rows.Count).ToString();
        }
        
        
        
       private void DelRowClick(object sender, EventArgs e)             ////////////////
        {
            int RowId = dgv.Rows.Count-1;
            if(dgv.Rows.Count == 1)
            {
                label1.Text = "Ви не можете видалити останній рядок";
                return;
            }

            foreach (DataGridViewTextBoxCell i in dgv.Rows[RowId].Cells)
            {
                if (i.Value == null)
                {
                    continue;
                }
                else
                {
                    DialogResult dr = MessageBox.Show("В останньому рядку присутні дані, усі значення, що залежать  від нього буде втрачено. Видалити рядок?",
                      "Важливо!", MessageBoxButtons.YesNo);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            break;
                        case DialogResult.No:
                            return;
                    }
                    break;
                }
            }


            foreach (DataGridViewTextBoxCell i in dgv.Rows[RowId].Cells)
            {
                if(i.Value == null)
                {
                    continue;
                }
                else
                {
                    string name = dgv.Columns[i.ColumnIndex].HeaderText + (RowId + 1);
                    LabCalculatorVisitor.tableIdentifier.Remove(name);
                    SendError(data.Table[name], name);
                    data.Table.Remove(name);
                }
            }
            dgv.Rows.RemoveAt(RowId);
        }
        
        
         
        private void SendError(Cell item, string LinkToError)         ////////////////
        {
            string msg = "Відсутнє значення " + LinkToError;
            foreach (Cell i in item.GetLinksToCell())
            {
                string name = dgv.Columns[i.Col].HeaderText + (i.Row + 1);
                LabCalculatorVisitor.tableIdentifier.Remove(name);

                data.Table[name].Value = msg;

                dgv[i.Col, i.Row].Value = msg;
                SendError(i, LinkToError);
            }
        }
    }
}
