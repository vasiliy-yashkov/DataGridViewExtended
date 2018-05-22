﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;

namespace DataGridViewExtended
{
    public class DataGridViewExtended : DataGridView
    {
        List<FilterStatus> Filter = new List<FilterStatus>();
        List<FilterStatus> oldFilter = new List<FilterStatus>();
        TextBox textBoxCtrl = new TextBox();
        DateTimePicker DateTimeCtrl = new DateTimePicker();
        CheckedListBox CheckCtrl = new CheckedListBox();
        Button ApplyButtonCtrl = new Button();
        Button ClearFilterCtrl = new Button();
        ToolStripDropDown popup = new ToolStripDropDown();

        string StrFilter = "";
        string ButtonCtrlText = "Применить";
        string ClearFilterCtrlText = "Очистить фильтры";
        string CheckCtrlAllText = "<Выбрать всё>";
        string SpaceText = "<Пустые>";

        // Текущий индекс ячейки
        private int columnIndex { get; set; }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            var header = new DataGridFilterHeader();
            header.FilterButtonClicked += new EventHandler<ColumnFilterClickedEventArg>(header_FilterButtonClicked);
            e.Column.HeaderCell = header;
            base.OnColumnAdded(e);
        }

        // Скролл после сортировки
        public override void Sort(DataGridViewColumn dataGridViewColumn, System.ComponentModel.ListSortDirection direction)
        {
            int scrl = this.HorizontalScrollBar.Value;
            int scrlOffset = this.HorizontalScrollingOffset;
            base.Sort(dataGridViewColumn, direction);
            this.HorizontalScrollBar.Value = scrl;
            this.HorizontalScrollingOffset = scrlOffset;
        }

        // Событие кнопки фильтрации
        void header_FilterButtonClicked(object sender, ColumnFilterClickedEventArg e)
        {
            int widthTool = GetWhithColumn(e.ColumnIndex) + 50;
            if (widthTool < 130) widthTool = 130;

            columnIndex = e.ColumnIndex;

            textBoxCtrl.Clear();
            CheckCtrl.Items.Clear();

            //textBoxCtrl.Text = textBoxCtrlText;
            textBoxCtrl.Size = new System.Drawing.Size(widthTool, 30);
            textBoxCtrl.TextChanged -= textBoxCtrl_TextChanged;
            textBoxCtrl.TextChanged += textBoxCtrl_TextChanged;

            DateTimeCtrl.Size = new System.Drawing.Size(widthTool, 30);
            DateTimeCtrl.Format = DateTimePickerFormat.Custom;
            DateTimeCtrl.CustomFormat = "dd.MM.yyyy";
            DateTimeCtrl.TextChanged -= DateTimeCtrl_TextChanged;
            DateTimeCtrl.TextChanged += DateTimeCtrl_TextChanged;

            CheckCtrl.ItemCheck -= CheckCtrl_ItemCheck;
            CheckCtrl.ItemCheck += CheckCtrl_ItemCheck;
            CheckCtrl.CheckOnClick = true;

            GetChkFilter();

            CheckCtrl.MaximumSize = new System.Drawing.Size(widthTool, GetHeightTable() - 120);
            CheckCtrl.Size = new System.Drawing.Size(widthTool, (CheckCtrl.Items.Count + 1) * 18);

            ApplyButtonCtrl.Text = ButtonCtrlText;
            ApplyButtonCtrl.Size = new System.Drawing.Size(widthTool, 30);
            ApplyButtonCtrl.Click -= ApplyButtonCtrl_Click;
            ApplyButtonCtrl.Click += ApplyButtonCtrl_Click;

            ClearFilterCtrl.Text = ClearFilterCtrlText;
            ClearFilterCtrl.Size = new System.Drawing.Size(widthTool, 30);
            ClearFilterCtrl.Click -= ClearFilterCtrl_Click;
            ClearFilterCtrl.Click += ClearFilterCtrl_Click;

            popup.Items.Clear();
            popup.AutoSize = true;
            popup.Margin = Padding.Empty;
            popup.Padding = Padding.Empty;

            ToolStripControlHost host1 = new ToolStripControlHost(textBoxCtrl);
            host1.Margin = Padding.Empty;
            host1.Padding = Padding.Empty;
            host1.AutoSize = false;
            host1.Size = textBoxCtrl.Size;

            ToolStripControlHost host2 = new ToolStripControlHost(CheckCtrl);
            host2.Margin = Padding.Empty;
            host2.Padding = Padding.Empty;
            host2.AutoSize = false;
            host2.Size = CheckCtrl.Size;

            ToolStripControlHost host3 = new ToolStripControlHost(ApplyButtonCtrl);
            host3.Margin = Padding.Empty;
            host3.Padding = Padding.Empty;
            host3.AutoSize = false;
            host3.Size = ApplyButtonCtrl.Size;

            ToolStripControlHost host4 = new ToolStripControlHost(ClearFilterCtrl);
            host4.Margin = Padding.Empty;
            host4.Padding = Padding.Empty;
            host4.AutoSize = false;
            host4.Size = ClearFilterCtrl.Size;

            ToolStripControlHost host5 = new ToolStripControlHost(DateTimeCtrl);
            host5.Margin = Padding.Empty;
            host5.Padding = Padding.Empty;
            host5.AutoSize = false;
            host5.Size = DateTimeCtrl.Size;

            switch (this.Columns[columnIndex].ValueType.ToString())
            {
                case "System.DateTime":
                    popup.Items.Add(host5);
                    break;
                default:
                    popup.Items.Add(host1);
                    break;
            }
            popup.Items.Add(host2);
            popup.Items.Add(host3);
            popup.Items.Add(host4);

            popup.Show(this, e.ButtonRectangle.X, e.ButtonRectangle.Bottom);
            host2.Focus();
        }

        // Выбор всех
        void CheckCtrl_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index == 0)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    for (int i = 1; i < CheckCtrl.Items.Count; i++)
                        CheckCtrl.SetItemChecked(i, true);
                }
                else
                {
                    for (int i = 1; i < CheckCtrl.Items.Count; i++)
                        CheckCtrl.SetItemChecked(i, false);
                }
            }
        }

        // Очистить фильтры
        void ClearFilterCtrl_Click(object sender, EventArgs e)
        {
            Filter.Clear();
            StrFilter = "";
            ApllyFilter();
            popup.Close();
        }

        // Событие при изменении текста в TextBox
        void textBoxCtrl_TextChanged(object sender, EventArgs e)
        {
            //if (textBoxCtrl.Text != textBoxCtrlText)       
            (this.DataSource as BindingSource).Filter = string.Format("convert([" + this.Columns[columnIndex].DataPropertyName + "], 'System.String') LIKE '%{0}%'", textBoxCtrl.Text);
        }
        void DateTimeCtrl_TextChanged(object sender, EventArgs e)
        {
            (this.DataSource as BindingSource).Filter = string.Format("convert([" + this.Columns[columnIndex].DataPropertyName + "], 'System.String') LIKE '%{0}%'", DateTimeCtrl.Text);
        }

        // Событие кнопки применить
        void ApplyButtonCtrl_Click(object sender, EventArgs e)
        {
            StrFilter = "";
            SaveChkFilter();
            ApllyFilter();
            popup.Close();
        }

        // Получаем данные из выбранной колонки 
        private List<string> GetDataColumns(int e, out List<string> formattedValueCellList)
        {
            List<string> ValueCellList = new List<string>();
            string Value;

            formattedValueCellList = new List<string>();
            string formattedValue;

            // Посик данных в столбце, исключая повторения
            foreach (DataGridViewRow row in this.Rows)
            {
                if (row.Cells[e].Value != null)
                {
                    Value = row.Cells[e].Value.ToString();
                    if (Value == "") Value = SpaceText;

                    formattedValue = row.Cells[e].FormattedValue.ToString();
                    if (formattedValue == "") formattedValue = SpaceText;

                    if (!ValueCellList.Contains(Value))
                        ValueCellList.Add(Value);

                    if (!formattedValueCellList.Contains(formattedValue))
                        formattedValueCellList.Add(formattedValue);
                }
            }
            return ValueCellList;
        }

        // Получаем высоту таблицы
        private int GetHeightTable()
        {
            return this.Height;
        }

        // Получаем ширину выбранной колонки
        private int GetWhithColumn(int e)
        {
            return this.Columns[e].Width;
        }

        // Запомнить чекбоксы фильтра
        private void SaveChkFilter()
        {
            string col = this.Columns[columnIndex].DataPropertyName;
            string itemChk;
            string oldValue = "";
            bool statChk;

            Filter.RemoveAll(x => x.columnName == col);

            for (int i = 1; i < CheckCtrl.Items.Count; i++)
            {
                itemChk = CheckCtrl.Items[i].ToString();
                statChk = CheckCtrl.GetItemChecked(i);   
                foreach (FilterStatus status in oldFilter)
                {
                    if (status.formatedValue == itemChk)
                    {
                        oldValue = status.valueString;
                        break;
                    }
                }            
                Filter.Add(new FilterStatus() { columnName = col, valueString = oldValue, formatedValue = itemChk, check = statChk });
            }
        }

        // Загрузить чекбоксы
        private void GetChkFilter()
        {
            List<FilterStatus> CheckList = new List<FilterStatus>();
            List<FilterStatus> CheckListSort = new List<FilterStatus>();

            // Посик сохранённых данных
            foreach (FilterStatus val in Filter)
            {
                if (this.Columns[columnIndex].Name == val.columnName)
                {
                    if (val.valueString == "") val.valueString = SpaceText;
                    CheckList.Add(new FilterStatus() { columnName = "", valueString = val.valueString, check = val.check, formatedValue = val.formatedValue });
                }
            }

            // Поиск данных в таблице
            List<string> formattedList;
            List<string> list = GetDataColumns(columnIndex, out formattedList);
            int i = 0;
            foreach (string ValueCell in list)
            {
                int index = CheckList.FindIndex(item => item.valueString == ValueCell);
                if (index == -1)
                {
                    CheckList.Add(new FilterStatus { valueString = ValueCell, formatedValue = formattedList[i], check = true });
                }
                i++;
            }

            CheckCtrl.Items.Add(CheckCtrlAllText, CheckState.Indeterminate);
            // Сортировка
            switch (this.Columns[columnIndex].ValueType.ToString())
            {
                case "System.Int32":
                    CheckListSort = CheckList.OrderBy(x => Int32.Parse(x.valueString)).ToList();
                    foreach (FilterStatus val in CheckListSort)
                    {
                        if (val.check == true)
                            CheckCtrl.Items.Add(val.valueString, CheckState.Checked);
                        else
                            CheckCtrl.Items.Add(val.valueString, CheckState.Unchecked);
                    }
                    break;
                case "System.DateTime":
                    CheckListSort = CheckList.OrderBy(x => DateTime.Parse(x.valueString)).ToList();
                    foreach (FilterStatus val in CheckListSort)
                    {
                        if (val.check == true)
                            CheckCtrl.Items.Add(DateTime.Parse(val.valueString).ToString("dd.MM.yyyy"), CheckState.Checked);
                        else
                            CheckCtrl.Items.Add(DateTime.Parse(val.valueString).ToString("dd.MM.yyyy"), CheckState.Unchecked);
                    }
                    break;
                default:
                    CheckListSort = CheckList.OrderBy(x => x.valueString).ToList();
                    foreach (FilterStatus val in CheckListSort)
                    {
                        if (val.check == true)
                            CheckCtrl.Items.Add(val.formatedValue/*valueString*/, CheckState.Checked);
                        else
                            CheckCtrl.Items.Add(val.formatedValue, CheckState.Unchecked);
                    }
                    break;
            }
            oldFilter = CheckListSort;
        }


        // Применить фильтр
        private void ApllyFilter()
        {
            foreach (FilterStatus val in Filter)
            {
                if (val.valueString == SpaceText) val.valueString = "";
                if (val.check == false)
                {
                    // Исключение если bool              
                    string valueFilter = "'" + val.valueString + "' ";
                    if (valueFilter == "True")
                    {
                        valueFilter = "1";
                    }
                    if (valueFilter == "False")
                    {
                        valueFilter = "0";
                    }


                    if (StrFilter.Length == 0)
                    {
                        StrFilter = StrFilter + ("[" + val.columnName + "] <> " + valueFilter);
                    }
                    else
                    {
                        StrFilter = StrFilter + (" AND [" + val.columnName + "] <> " + valueFilter);
                    }
                }
            }
            BindingSource a = this.DataSource as BindingSource;
            a.Filter = StrFilter;
        }
    }
}
