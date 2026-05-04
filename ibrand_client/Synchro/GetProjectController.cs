using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NomadLogic.iBrandDesktop.Exception;
using NomadLogic.iBrandDesktop.Synchro.Report;
using NomadLogic.iBrandDesktop.Ui.Synchro;

namespace NomadLogic.iBrandDesktop.Synchro
{
    public class GetProjectController
    {
        public event EventHandler OnCompleted;

        private GetProject _getProject;
        private GetProjectDlg _projectDlg;

        private readonly Form _mainForm;
        private Task _runningTask;
        private CancellationTokenSource _cts;

        public GetProjectController(Form mainForm)
        {
            _mainForm = mainForm;
        }
        
        public void OnGetProject(object sender, EventArgs e)
        {
            _getProject = new GetProject();
            _projectDlg = new GetProjectDlg {Owner = _mainForm};
            _projectDlg.OnStartClick += OnStart;
            _projectDlg.OnCancelClick += OnCancel;

            _getProject.ProgressReport = new Progress<DownloadProjectReport>(ReportHandler);

            _projectDlg.Show();
        }

        private void ReportHandler(DownloadProjectReport obj)
        {
            if (obj.SetMaxStep != 0)
            {
                _projectDlg?.SetProgressBarMinMax(0, obj.SetMaxStep);
            }

            if (obj.AddStep > 0)
            {
                _projectDlg?.AddProgressStep(obj.AddStep);
            }

            if (obj.Message != null)
            {
                _projectDlg?.AddUiMessage(obj.Message, 0);
            }
        }

        private void OnCancel(object sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private void OnStart(string username, string password)
        {
            if (_cts != null || _runningTask != null) return;
            _cts = new CancellationTokenSource();
            _runningTask = Task.Run(() => Execute(username, password, _cts.Token), _cts.Token);
            _runningTask.ContinueWith(t =>
            {
                _cts = null;
                _runningTask = null;
            });
        }

        private async void Execute(string username, string password, CancellationToken ct)
        {
            _getProject.UserName = username;
            _getProject.Password = password;

            try
            {
                _projectDlg.DisableButtons();
                _projectDlg.ClearUiMessages();
                
                await _getProject.Go(ct);
                await _getProject.GetProjectFiles();

            }
            catch (SynchroException ex)
            {
                _projectDlg.AddUiMessage(ex.Message, 0);
                _projectDlg.EnableButtons();
            }
            catch (System.Exception ex)
            {
                var message = ex.Message;

                _projectDlg.AddUiMessage(Resources.iBrand.SynchroError + message, 1);
                _projectDlg.EnableButtons();
            }
            finally
            {
                _projectDlg.SetCancelButtonToOk();
                OnCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
