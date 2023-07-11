using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms; 
using dal;

namespace rehabilitation_management_system
{
    public partial class RightsListForm : Form
    {

        #region "Private Fields"
        repository rep;
        rehabilitation_management_dbEntities db;
        string connection;
        IQueryable<tbl_AllowedRoleMenus> _AllowedRoleMenus;
        #endregion "Private Fields"

        #region "Constructor"
        public RightsListForm(string Conn)
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
        private void btnDelete_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (dataGridViewRights.SelectedRows.Count != 0)
            {
                try
                {
                    tbl_AllowedRoleMenus _right = (tbl_AllowedRoleMenus)bindingSourceRights.Current;
                    if (DialogResult.Yes == MessageBox.Show("Are you sure you want to delete Right", "Confirm Delete", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                    {
                        db.tbl_AllowedRoleMenus.DeleteObject(_right);
                        //db.SaveChanges();
                        RefreshGrid();
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
            if (dataGridViewRights.SelectedRows.Count != 0)
            {
                try
                {
                    tbl_AllowedRoleMenus _right = (tbl_AllowedRoleMenus)bindingSourceRights.Current;
                    //EditRightForm eus = new EditRightForm(_right, connection) { Owner = this };
                    //eus.Text = "Edit Right";
                    //eus.ShowDialog();
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
                //AddRightsForm usf = new AddRightsForm(connection) { Owner = this };
                //usf.ShowDialog();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }

        }
        private void dataGridViewRights_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridViewRights.SelectedRows.Count != 0)
            {
                try
                {
                    tbl_AllowedRoleMenus _right = (tbl_AllowedRoleMenus)bindingSourceRights.Current;
                    //EditRightForm eus = new EditRightForm(_right, connection) { Owner = this };
                    //eus.Text = "Edit Right";
                    //eus.ShowDialog();
                }
                catch (Exception ex)
                {
                    Utils.ShowError(ex);
                }
            }

        }
        public void RefreshGrid()
        {
            try
            {
                bindingSourceRights.DataSource = null;

                var rightsquery = from rts in db.tbl_AllowedRoleMenus
                                  select rts;
                bindingSourceRights.DataSource = rightsquery.ToList();
                dataGridViewRights.DataSource = bindingSourceRights;
                groupBox1.Text = bindingSourceRights.Count.ToString();

                foreach (DataGridViewRow row in dataGridViewRights.Rows)
                {
                    //dataGridViewRights.Rows[dataGridViewRights.Rows.Count - 1].Selected = true;
                    //int nRowIndex = dataGridViewRights.Rows.Count - 1;
                    //bindingSourceRights.Position = nRowIndex;
                }
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }


        private void RightsListForm_Load(object sender, EventArgs e)
        {
            try
            {

                var _Rolesquery = from rl in db.tbl_roles
                                  select rl;
                List<tbl_roles> _Roles = _Rolesquery.ToList();
                DataGridViewComboBoxColumn colCboxRole = new DataGridViewComboBoxColumn();
                colCboxRole.HeaderText = "Role";
                colCboxRole.Name = "cbRole";
                colCboxRole.DataSource = _Roles;
                // The display member is the name column in the column datasource  
                colCboxRole.DisplayMember = "Description";
                // The DataPropertyName refers to the foreign key column on the datagridview datasource
                colCboxRole.DataPropertyName = "RoleId";
                // The value member is the primary key of the parent table  
                colCboxRole.ValueMember = "Id";
                colCboxRole.MaxDropDownItems = 10;
                colCboxRole.Width = 120;
                colCboxRole.DisplayIndex = 1;
                colCboxRole.MinimumWidth = 5;
                colCboxRole.FlatStyle = FlatStyle.Flat;
                colCboxRole.DefaultCellStyle.NullValue = "--- Select ---";
                colCboxRole.ReadOnly = true;
                //colCboxRole.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (!this.dataGridViewRights.Columns.Contains("cbRole"))
                {
                    dataGridViewRights.Columns.Add(colCboxRole);
                }


                var _menuItemsquery = from rl in db.tbl_MenuItems
                                      select rl;
                List<tbl_MenuItems> _MenuItems = _menuItemsquery.ToList();
                DataGridViewComboBoxColumn colCboxMenu = new DataGridViewComboBoxColumn();
                colCboxMenu.HeaderText = "Menu";
                colCboxMenu.Name = "cbMenu";
                colCboxMenu.DataSource = _MenuItems;
                // The display member is the name column in the column datasource  
                colCboxMenu.DisplayMember = "Description";
                // The DataPropertyName refers to the foreign key column on the datagridview datasource
                colCboxMenu.DataPropertyName = "MenuItemId";
                // The value member is the primary key of the parent table  
                colCboxMenu.ValueMember = "Id";
                colCboxMenu.MaxDropDownItems = 10;
                colCboxMenu.Width = 150;
                colCboxMenu.DisplayIndex = 2;
                colCboxMenu.MinimumWidth = 5;
                colCboxMenu.FlatStyle = FlatStyle.Flat;
                colCboxMenu.DefaultCellStyle.NullValue = "--- Select ---";
                colCboxMenu.ReadOnly = true;
                //colCboxRole.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                if (!this.dataGridViewRights.Columns.Contains("cbMenu"))
                {
                    dataGridViewRights.Columns.Add(colCboxMenu);
                }

                dataGridViewRights.AutoGenerateColumns = false;
                dataGridViewRights.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

                var rightsquery = from rts in db.tbl_AllowedRoleMenus
                                  select rts;
                bindingSourceRights.DataSource = rightsquery.ToList();
                dataGridViewRights.DataSource = bindingSourceRights;
                groupBox1.Text = bindingSourceRights.Count.ToString();

                _AllowedRoleMenus = rightsquery;

                AutoCompleteStringCollection acsctrm = new AutoCompleteStringCollection();
                acsctrm.AddRange(this.AutoComplete_Roles());
                txtRole.AutoCompleteCustomSource = acsctrm;
                txtRole.AutoCompleteMode =
                    AutoCompleteMode.SuggestAppend;
                txtRole.AutoCompleteSource =
                     AutoCompleteSource.CustomSource;

                AutoCompleteStringCollection acscdsc = new AutoCompleteStringCollection();
                acscdsc.AddRange(this.AutoComplete_Menu());
                txtMenu.AutoCompleteCustomSource = acscdsc;
                txtMenu.AutoCompleteMode =
                    AutoCompleteMode.SuggestAppend;
                txtMenu.AutoCompleteSource =
                     AutoCompleteSource.CustomSource;
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }
        private string[] AutoComplete_Roles()
        {
            try
            {
                var rolesquery = from rl in db.tbl_roles
                                 where rl.IsDeleted == false
                                 select rl.Description;
                return rolesquery.ToArray();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
                return null;
            }
        }
        private string[] AutoComplete_Menu()
        {
            try
            {
                var menuquery = from mn in db.tbl_MenuItems
                                select mn.Description;
                return menuquery.ToArray();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
                return null;
            }
        }

        private void dataGridViewRights_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                throw e.Exception;
            }
            catch (Exception ex)
            {
                Log.WriteToErrorLogFile(ex);
            }
        }
        private void txtRole_Validated(object sender, EventArgs e)
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }
        private void txtMenu_Validated(object sender, EventArgs e)
        {
            try
            {
                ApplyFilter();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }
        // apply the filter
        private void ApplyFilter()
        {
            try
            {
                var filter = CreateFilter(_AllowedRoleMenus);
                // set the filter
                this.bindingSourceRights.DataSource = filter;
                groupBox1.Text = bindingSourceRights.Count.ToString();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }
        private IQueryable<tbl_AllowedRoleMenus> CreateFilter(IQueryable<tbl_AllowedRoleMenus> _allowedMenu)
        {
            //none
            if (string.IsNullOrEmpty(txtRole.Text) && string.IsNullOrEmpty(txtMenu.Text))
            {
                return _allowedMenu;
            }
            //all
            if (!string.IsNullOrEmpty(txtRole.Text) && !string.IsNullOrEmpty(txtMenu.Text))
            {
                string _Role = txtRole.Text;
                string _Menu = txtMenu.Text;
                _allowedMenu = from st in db.tbl_AllowedRoleMenus
                               join rl in db.tbl_roles on st.RoleId equals rl.role_id
                               where rl.Description.StartsWith(_Role)
                               join mn in db.tbl_MenuItems on st.MenuItemId equals mn.menu_item_Id
                               where mn.Description.StartsWith(_Menu)
                               select st;
                return _allowedMenu;
            }
            //role
            if (!string.IsNullOrEmpty(txtRole.Text) && string.IsNullOrEmpty(txtMenu.Text))
            {
                _allowedMenu = null;
                string _Role = txtRole.Text;
                _allowedMenu = from st in db.tbl_AllowedRoleMenus
                               join rl in db.tbl_roles on st.RoleId equals rl.role_id
                               where rl.Description.StartsWith(_Role)
                               select st;
                return _allowedMenu;
            }
            //menu
            if (string.IsNullOrEmpty(txtRole.Text) && !string.IsNullOrEmpty(txtMenu.Text))
            {
                _allowedMenu = null;
                string _Menu = txtMenu.Text;
                _allowedMenu = from st in db.tbl_AllowedRoleMenus
                               join mn in db.tbl_MenuItems on st.MenuItemId equals mn.menu_item_Id
                               where mn.Description.StartsWith(_Menu)
                               select st;
                return _allowedMenu;
            }
            return _allowedMenu;

        }

        private void btnReports_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                //ReportsRightsListForm rrf = new ReportsRightsListForm(connection);
                //rrf.Show();
            }
            catch (Exception ex)
            {
                Utils.ShowError(ex);
            }
        }
        #endregion "Private Methods"




    }
}