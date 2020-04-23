using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ORM_Resourses
{
    public partial class fRConsume : Form
    {
        private DataGridViewComboBoxColumn cbResorcesId;
        private DataGridViewComboBoxColumn cbBuldingsId;
        private bool newRowAdded = false;

        public fRConsume()
        {
            InitializeComponent();
            menuStrip1.CausesValidation = false;
            cbResorcesId = new DataGridViewComboBoxColumn()
            {
                Name = "rId",
                HeaderText = "Ресурс",
                DisplayMember = "name",
                ValueMember = "id"
            };
            cbBuldingsId = new DataGridViewComboBoxColumn()
            {
                Name = "bId",
                HeaderText = "Здание",
                DisplayMember = "name",
                ValueMember = "id"
            };
            InitializeDGVResources();
            InitializeDGVRConsume();
        }

        private void загрузитьЗановоToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dgvResources.CancelEdit();
            dgvRConsume.CancelEdit();
            dgvRConsume.Columns.Clear();
            dgvResources.Columns.Clear();
            InitializeDGVResources();
            InitializeDGVRConsume();
        }

        private void InitializeDGVRConsume()
        {
            using (var ctx = new OpenDataContext())
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(int));
                dt.Columns.Add("name", typeof(string));
                //dt.Columns.Add("ready", typeof(bool));
                foreach (var build in ctx.buildings)
                {
                    dt.Rows.Add(build.building_id, build.building_name);
                }
                BindingSource bs = new BindingSource(dt, "");
                cbBuldingsId.DataSource = bs;

                dgvRConsume.Columns.Add(cbResorcesId);
                dgvRConsume.Columns.Add(cbBuldingsId);
                dgvRConsume.Columns.Add("consumeSpeed", "Скорость потребления");
                foreach (var brs in ctx.buildings_resources_consume)
                {
                    int i = dgvRConsume.Rows.Add();
                    dgvRConsume.Rows[i].Cells[0].Value = brs.resources_id;
                    dgvRConsume.Rows[i].Cells[1].Value = brs.building_id;
                    dgvRConsume.Rows[i].Cells[2].Value = brs.consume_speed;
                }
            }
        }

        private void InitializeDGVResources()
        {
            using (var ctx = new OpenDataContext())
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(int));
                dt.Columns.Add("name", typeof(string));
                foreach (var res in ctx.resources)
                {
                    dt.Rows.Add(res.resources_id, res.resources_name);
                }
                BindingSource bs = new BindingSource(dt, "");
                cbResorcesId.DataSource = bs;
                dgvResources.DataSource = dt;
                dgvResources.Columns["id"].Visible = false;
                dgvResources.Columns["name"].HeaderText = "Ресурс";
            }
        }

        private bool IsResourcesExists(string name)
        {
            using (var ctx = new OpenDataContext())
            {
                return (ctx.resources.FirstOrDefault(r => (r.resources_name == name)) != null);
            }
        }

        private int InsertToDB(DataGridView dgv, DataGridViewRow row)
        {
            using (var ctx = new OpenDataContext())
            {
                if (dgv == dgvResources)
                {
                    resource res = new resource { resources_name = row.Cells["name"].Value.ToString() };
                    ctx.resources.Add(res);
                    ctx.SaveChanges();
                    row.Cells["id"].Value = res.resources_id;
                }
                else
                {
                    buildings_resources_consume brs = new buildings_resources_consume
                    {
                        resources_id = (int)row.Cells["rId"].Value,
                        building_id = (int)row.Cells["bId"].Value,
                        consume_speed = int.Parse(row.Cells["consumeSpeed"].Value.ToString())
                    };
                    ctx.buildings_resources_consume.Add(brs);
                    ctx.SaveChanges();
                }
            }
            return -1;
        }

        private void UpdateDB(DataGridView dgv, DataGridViewRow row)
        {
            using (var ctx = new OpenDataContext())
            {
                if (dgv == dgvResources)
                {
                    resource res = ctx.resources.Find(Convert.ToInt32(row.Cells["id"].Value));
                    res.resources_name = row.Cells["name"].Value.ToString();
                    ctx.SaveChanges();
                }
            }
        }

        private void CellValidating(DataGridView dgv, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex == dgv.Rows.Count - 1) return;
            if (dgv == dgvRConsume && e.ColumnIndex == 2)
            {
                int t;
                if (e.FormattedValue.ToString().Trim() != "")
                {
                    if (!int.TryParse(e.FormattedValue.ToString(), out t) || t < 0)
                    {
                        e.Cancel = true;
                        SystemSounds.Beep.Play();
                    }
                }//Row это плохо!
            }
            if (string.IsNullOrEmpty(e.FormattedValue.ToString()))
                dgv.CancelEdit();
        }

        private void RowValidating(DataGridView dgv, DataGridViewCellCancelEventArgs e)
        {
            if (dgv.Rows[e.RowIndex].IsNewRow)
            {
                newRowAdded = true;
                return;
            }
            //foreach (DataGridViewCell cell in dgv.Rows[e.RowIndex].Cells)
            //{
            //    if (cell.Value == null || cell.Value.ToString().Trim() == "")
            //    {
            //        //dgv.Rows[e.RowIndex].ErrorText += "Пустая ячейка! ";
            //        cell.ErrorText = "Пустая ячейка! ";
            //    }
            //    else
            //    {
            //        //dgv.Rows[e.RowIndex].ErrorText = dgv.Rows[e.RowIndex].ErrorText.Replace("Пустая ячейка! ", "");
            //        cell.ErrorText = cell.ErrorText.Replace("Пустая ячейка! ", "");
            //    }
            //}
        }

        private void CellEndEdit(DataGridView dgv, DataGridViewCellEventArgs e)
        {
            if (dgv.Rows[e.ColumnIndex].IsNewRow) return;
            bool canCommit = true;
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                var cell = dgv[i, e.RowIndex];
                if (cell.Value == null || string.IsNullOrWhiteSpace(cell.ToString()))
                {
                    canCommit = false;
                    cell.ErrorText = cell.ErrorText.Replace("Пустая ячейка! ", "");
                    cell.ErrorText = "Пустая ячейка! ";
                }
                else
                {
                    cell.ErrorText = cell.ErrorText.Replace("Пустая ячейка! ", "");
                }
            }
            if (dgv == dgvResources)
            {
                if (IsResourcesExists(dgv.Rows[e.RowIndex].Cells["name"].Value.ToString()))
                {
                    //dgv.Rows[e.RowIndex].ErrorText = dgv.Rows[e.RowIndex].ErrorText.Replace("Ресурс уже существует! ", "");
                    //dgv.Rows[e.RowIndex].ErrorText = "Ресурс уже существует! ";
                    MessageBox.Show("Ресурс уже существует! Введённая строчка будет удалена!");
                    dgv.Rows.RemoveAt(e.RowIndex);
                    return;
                }
            }
            if (!canCommit) return;

            //if (dgv == dgvResources)
            //    if (newRowAdded)
            //    {

            //        InsertToDB(dgv, dgv.Rows[e.RowIndex]);
            //        newRowAdded = false;
            //    }
            //    else
            //        UpdateDB(dgv, dgv.Rows[e.RowIndex]);
            //if (dgv == dgvRConsume)
            //    if (dgv.Rows[e.RowIndex].Cells["id"].Value == null)
            //        InsertToDB(dgv, dgv.Rows[e.RowIndex]);
            //    else
            //        UpdateDB(dgv, dgv.Rows[e.RowIndex]);

            //foreach (var cell in dgv)
            //{

            //    if (cell.Value == null || cell.Value.ToString().Trim() == "")
            //    {
            //        cell.ErrorText = cell.ErrorText.Replace("Пустая ячейка! ", "");
            //        cell.ErrorText = "Пустая ячейка! ";
            //    }
            //    else
            //    {
            //        cell.ErrorText = cell.ErrorText.Replace("Пустая ячейка! ", "");
            //    }
            //}
        }

        private void dgvResources_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            CellEndEdit(dgvResources, e);

        }

        private void dgvResources_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            CellValidating(dgvResources, e);
        }


        private void dgvResources_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            RowValidating(dgvResources, e);
        }

        private void dgvRConsume_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            CellEndEdit(dgvRConsume, e);
        }

        private void dgvRConsume_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            CellValidating(dgvRConsume, e);
        }

        private void dgvRConsume_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            RowValidating(dgvRConsume, e);
        }

        private void отменитьИзмененияToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            switch (tabControl.SelectedIndex)
            {
                case 0:
                    dgvResources.CancelEdit();
                    break;
                case 1:
                    dgvRConsume.CancelEdit();
                    break;
            }
        }

    }
}

