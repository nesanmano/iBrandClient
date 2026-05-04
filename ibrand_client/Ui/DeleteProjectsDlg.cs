using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NomadLogic.iBrandDesktop.Ui
{
    public partial class DeleteProjectsDlg : Form
    {
        public event EventHandler OnCancelClick;
        public event EventHandler OnDeleteClick;
        
        
        public DeleteProjectsDlg()
        {
            InitializeComponent();
            InitMenus();
            AddColumnsHeader();

            ProjectListView.CheckBoxes = true;
            ProjectListView.FullRowSelect = true;
        }

        private void InitMenus()
        {
            var deleteMenuItem = new MenuItem {Text = Resources.iBrand.Delete};
            deleteMenuItem.Click += OnDelete;
            mainMenu.MenuItems.Add(deleteMenuItem);


            var cancelMenuItem = new MenuItem {Text = Resources.iBrand.Cancel};
            cancelMenuItem.Click += OnCancel;
            mainMenu.MenuItems.Add(cancelMenuItem);
        }

        private void OnCancel(object sender, EventArgs e)
        {
            OnCancelClick?.Invoke(this, EventArgs.Empty);
        }

        private void OnDelete(object sender, EventArgs e)
        {
            OnDeleteClick?.Invoke(this, EventArgs.Empty);
        }

        public bool HasProjectSelected()
        {
            var nbrProject = ProjectListView.Items.Count;

            for (var i = 0; i < nbrProject; i++)
            {
                var li = ProjectListView.Items[i];
                if (li.Checked)
                {
                    return true;
                }
            }

            return false;
        }

        public ListView ProjectListView { get; private set; }

        private void AddColumnsHeader()
        {
            ProjectListView.View = View.Details;

            ProjectListView.Columns.Add("", 21, HorizontalAlignment.Left);

            ProjectListView.Columns.Add(Resources.iBrand.Project, ProjectListView.Width - 23, HorizontalAlignment.Left);
        }

        public void AddProjects(List<Project> projects)
        {
            var nbrProject = projects.Count;

            for (var i = 0; i < nbrProject; i++)
            {
                var project = projects[i];

                var li = new ListViewItem {Tag = project};

                li.SubItems.Add(project.Name);

                ProjectListView.Items.Add(li);
            }
        }
    }
}