using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Data;
using System.Xml;
using Microsoft.VisualBasic;

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
                SaveDataGriedView(saveFileDialog1.FileName);
            }
        }

        private void SaveDataGriedView(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("Cells");
            xmlDoc.AppendChild(rootNode);
            foreach (var i in data.Table.Values)
            {
                XmlNode userNode = xmlDoc.CreateElement("Cell");
                XmlAttribute Name = xmlDoc.CreateAttribute("NameOfCell");
                XmlAttribute Expression = xmlDoc.CreateAttribute("Expression");
                XmlAttribute Value = xmlDoc.CreateAttribute("Value");
                XmlAttribute RowId = xmlDoc.CreateAttribute("RowId");
                XmlAttribute ColId = xmlDoc.CreateAttribute("ColId");
                Name.Value = dgv.Columns[i.Col].Name + dgv.Rows[i.Row].HeaderCell.Value;
                Expression.Value = i.Expression;
                Value.Value = i.Value.ToString();
                RowId.Value = i.Row.ToString();
                ColId.Value = i.Col.ToString();
                userNode.Attributes.Append(Name);
                userNode.Attributes.Append(Expression);
                userNode.Attributes.Append(Value);
                userNode.Attributes.Append(ColId);
                userNode.Attributes.Append(RowId);
                rootNode.AppendChild(userNode);
            }

            xmlDoc.Save(path);
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
                ReadCell(openFileDialog1.FileName);

                foreach (var i in data.Table)
                {
                    LinkManager.FindLincs(i.Key, i.Value.Expression, data.Table);
                }

            }
        }
        private void ReadCell(string path)      // Метод зв'язаний з зайгрузкою таблиці з файлу. 
        {
            
            XmlReader reader = XmlReader.Create(path);
            while(reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                  if(reader.HasAttributes)
                    {
                        dynamic value = null;
                        string expression=null;
                        int RowId = -1;
                        int ColId = -1;
                        string name = null;
                        while(reader.MoveToNextAttribute())
                        {
                            if (reader.Name == "NameOfCell") name = reader.Value;
                            else if (reader.Name == "Expression") expression = reader.Value;
                            else if (reader.Name == "Value")
                            {
                                if (reader.Value == "DivBy0")
                                {
                                    value = "DivBy0";
                                }
                                else if (reader.Value == "False" || reader.Value == "True")
                                {
                                    value = bool.Parse(reader.Value);
                                }
                                else if (reader.Value == "null")
                                {
                                    value = null;
                                }
                                else
                                {
                                    value = int.Parse(reader.Value);
                                }
                            }
                            else if(reader.Name == "ColId")ColId = int.Parse(reader.Value);
                            else if(reader.Name == "RowId")RowId = int.Parse(reader.Value);
                        }
                        data.Table[name] = new Cell(value, expression, ColId, RowId);
                        data.Table[name].Name = name;
                        if (value != null) LabCalculatorVisitor.tableIdentifier[name] = value;
                        dgv[ColId, RowId].Value = value;
                    }
                }


            }
            reader.Close();
        }


        private void ClearTableClick(object sender, EventArgs e) // Очистити таблицю
        {
            data.Clear();
            LabCalculatorVisitor.tableIdentifier.Clear();
            dgv.Rows.Clear();

            dgv.Rows.Add(50);
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
            catch(ArgumentException ex)
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
                        dgv[ColId, RowId].Value = data.Table[Name].Value;
                    }
                    else
                    {
                        dgv[ColId, RowId].Value = Expression;
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
                    if(s.Value.Expression != null) dgv[s.Value.Col, s.Value.Row].Value = s.Value.Expression;
                }
                mode = false;
                ModeBox.Text = "Вираз";
            }
            
            else
            {
                foreach (var s in data.Table)
                {
                    if (s.Value.Expression != null) dgv[s.Value.Col, s.Value.Row].Value = s.Value.Value;
                }
                mode = true;
                ModeBox.Text = "значення";
            }
        }
        private void AddRowClick(object sender, EventArgs e)
        {
            dgv.Rows.Add();
            dgv.Rows[dgv.Rows.Count - 2].HeaderCell.Value = (dgv.Rows.Count - 1).ToString();
            dgv.Rows[dgv.Rows.Count - 1].HeaderCell.Value = (dgv.Rows.Count).ToString();
        }
        private void DelRowClick(object sender, EventArgs e)
        {
            int RowId = dgv.Rows.Count-1;
            if(dgv.Rows.Count == 1)
            {
                DialogResult dr = MessageBox.Show("Видалити останній рядок неможливо.",
                     "Важливо!", MessageBoxButtons.OK);
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
                    data.Table[name].Value = null;
                    data.Table[name].Expression = null;
                }
            }
            dgv.Rows.RemoveAt(RowId);
        }
        private void SendError(Cell item, string LinkToError)
        {
            string msg = "Error ";
            foreach (Cell i in item.GetLinksToCell())
            {
                string name = dgv.Columns[i.Col].HeaderText + (i.Row + 1);
                LabCalculatorVisitor.tableIdentifier.Remove(name);

                data.Table[name].Value = msg;

                dgv[i.Col, i.Row].Value = msg;
                SendError(i, LinkToError);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult dr = MessageBox.Show("Усі дані, що не були збережені, буде видалено. Вийти?",
                     "Важливо!", MessageBoxButtons.YesNo);
           
            if (dr == DialogResult.Yes)
            {
                e.Cancel = false;
                return;
            }
            else if(dr == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
    }
}
