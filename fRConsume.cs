using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ORM_Resourses
{
    public partial class fRConsume : Form
    {
        private DataGridViewComboBoxColumn cbResorcesId;
        private DataGridViewComboBoxColumn cbBuldingsId;

        public fRConsume()
        {
            InitializeComponent();
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

        private void InitializeDGVRConsume()
        {
            dgvRConsume.Rows.Clear();
            dgvRConsume.Columns.Clear();
            using (var ctx = new OpenDataContext())
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("id", typeof(int));
                dt.Columns.Add("name", typeof(string));
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
            dgvResources.Rows.Clear();
            dgvResources.Columns.Clear();
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
        private void CellValidating(DataGridView dgv, DataGridViewCellValidatingEventArgs e)
        {
            //dgv.CancelEdit();
            int i = e.RowIndex;
        }

        private void dgvResources_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            CellValidating(dgvResources, e);
        }
    }
}

