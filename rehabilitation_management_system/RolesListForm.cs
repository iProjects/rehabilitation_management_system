using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using dal;

namespace rehabilitation_management_system
{
    public partial class RolesListForm : Form
    {

        #region "Private Fields"
        repository rep;
        rehabilitation_management_dbEntities db;
        string connection;
        #endregion "Private Fields"

        #region "Constructor"
        public RolesListForm(string Conn)
        {
            InitializeComponent();
            if (string.IsNullOrEmpty(Conn))
                throw new ArgumentNullException("connection");
            connection = Conn;
            rep = new repository(connection);
            db = new rehabilitation_management_dbEntities(connection);
        }
        #endregion "Constructor"

        #region "Private Methods"
        private void btnClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }
        public void RefreshGrid()
        {
            try
            {
                bindingSourceRoles.DataSource = null;
                var roles = from rls in db.tbl_roles
                            select rls;
                bindingSourceRoles.DataSource = roles.ToList();
                groupBox1.Text = bindingSourceRoles.Count.ToString();
                foreach (DataGridViewRow row in dataGridViewRoles.Rows)
                {
                    dataGridViewRoles.Rows[dataGridViewRoles.Rows.Count - 1].Selected = true;
                    int nRowIndex = dataGridViewRoles.Rows.Count - 1;
                    bindingSourceRoles.Position = nRowIndex;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }


        private void btnDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dataGridViewRoles.SelectedRows.Count != 0)
            {
                try
                {
                    tbl_roles role = (tbl_roles)bindingSourceRoles.Current;

                    var _userolesquery = from us in db.tbl_users
                                         where us.RoleId == role.role_id
                                         select us;
                    List<tbl_users> _Users = _userolesquery.ToList();

                    var _AllowedRoleMenusquery = from us in db.tbl_AllowedRoleMenus
                                                 where us.RoleId == role.role_id
                                                 select us;
                    List<tbl_AllowedRoleMenus> _AllowedRoleMenus = _AllowedRoleMenusquery.ToList();

                    var _AllowedReportsRolesMenusquery = from us in db.tbl_AllowedReportsRolesMenus
                                                         where us.RoleId == role.role_id
                                                         select us;
                    List<tbl_AllowedReportsRolesMenus> _AllowedReportsRolesMenus = _AllowedReportsRolesMenusquery.ToList();

                    if (_Users.Count > 0)
                    {
                        MessageBox.Show("There is a User Associated with this Role.\n Delete the User First!", "Confirm Delete", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else if (_AllowedRoleMenus.Count > 0)
                    {
                        MessageBox.Show("There is a Menu Right Associated with this Role.\n Delete the Menu Right First!", "Confirm Delete", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else if (_AllowedReportsRolesMenus.Count > 0)
                    {
                        MessageBox.Show("There is a Report Menu Right Associated with this Role.\n Delete the Report Menu Right First!", "Confirm Delete", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                    else
                    {
                        if (DialogResult.Yes == MessageBox.Show("Are you sure you want to delete Role\n" + role.Description.ToUpper().ToString().Trim(), "Confirm Delete", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                        {
                            db.tbl_roles.DeleteObject(role);
                            //db.SaveChanges();
                            RefreshGrid();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex);
                }
            }
        }
        private void btnEdit_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dataGridViewRoles.SelectedRows.Count != 0)
            {
                try
                {
                    dal.tbl_roles userrole = (dal.tbl_roles)bindingSourceRoles.Current;
                    //EditRolesForm es = new EditRolesForm(userrole, connection) { Owner = this };
                    //es.Text = userrole.Description.ToUpper().Trim();
                    //es.ShowDialog();

                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex);
                }
            }
        }
        private void btnAdd_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                //AddRolesForm arf = new AddRolesForm(connection) { Owner = this };
                //arf.ShowDialog();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }


        private void dataGridViewRoles_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewRoles.SelectedRows.Count != 0)
            {
                try
                {

                    dal.tbl_roles userrole = (dal.tbl_roles)bindingSourceRoles.Current;
                    //EditRolesForm es = new EditRolesForm(userrole, connection) { Owner = this };
                    //es.Text = userrole.Description.ToUpper().Trim();
                    //es.ShowDialog();

                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex);
                }
            }
        }


        private void RolesListForm_Load(object sender, EventArgs e)
        {
            try
            {
                var roles = from rls in db.tbl_roles
                            select rls;
                bindingSourceRoles.DataSource = roles.ToList();
                dataGridViewRoles.AutoGenerateColumns = false;
                dataGridViewRoles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridViewRoles.DataSource = bindingSourceRoles;
                groupBox1.Text = bindingSourceRoles.Count.ToString();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }

        }
        #endregion "Private Methods"


    }
}
