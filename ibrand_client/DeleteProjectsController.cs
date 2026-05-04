using System;
using System.Windows.Forms;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui;

namespace NomadLogic.iBrandDesktop
{
    public class DeleteProjectsController
    {
        public event EventHandler OnCompleted;
                
        DeleteProjectsDlg _deleteProjectsDlg;

        readonly Form _mainForm;

        public DeleteProjectsController(Form mainForm)
        {
            _mainForm = mainForm;
        }

        public void OnDeleteProjects(object sender, EventArgs e)
        {
            _deleteProjectsDlg = new DeleteProjectsDlg();
            _deleteProjectsDlg.Owner = _mainForm;
            _deleteProjectsDlg.OnCancelClick += OnCancel;
            _deleteProjectsDlg.OnDeleteClick += OnDelete;

            var db = ServicesContainer.GetService<IbrandDatabase>();
            var projects = db.GetProjects();

            _deleteProjectsDlg.AddProjects(projects);

            _deleteProjectsDlg.ShowDialog();
        }

        private void OnDelete(object sender, EventArgs e)
        {
            if (!_deleteProjectsDlg.HasProjectSelected())
            {
                MsgBox.Show(_deleteProjectsDlg, Resources.iBrand.NoProjectSelected, Resources.iBrand.AppName);
                return;
            }

            var result = MsgBox.Show(_deleteProjectsDlg, Resources.iBrand.DeleteProjects, Resources.iBrand.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
            if (result != DialogResult.Yes) return;

            var db = ServicesContainer.GetService<IbrandDatabase>();
                
            var nbrProject = _deleteProjectsDlg.ProjectListView.Items.Count;

            for (var i = 0; i < nbrProject; i++)
            {
                var li = _deleteProjectsDlg.ProjectListView.Items[i];
                if (!li.Checked) continue;

                var project = (Project)li.Tag;
                if (db.HasRespondentsToSync(project.Id))
                {
                    var res = MsgBox.Show(_deleteProjectsDlg, "Project(s) have respondent not yet synchronized. Proceed anyway?", Resources.iBrand.AppName, MessageBoxButtons.OKCancel);
                    if (res == DialogResult.Cancel) return;
                }

                FileUtil.DeleteFolderContent(project.ProjectDir);
                db.DeleteProject(project);
            }

            OnCompleted?.Invoke(this, EventArgs.Empty);

            _deleteProjectsDlg.Close();
        }

        private void OnCancel(object sender, EventArgs e)
        {
            _deleteProjectsDlg.Close();
        }

    }
}
