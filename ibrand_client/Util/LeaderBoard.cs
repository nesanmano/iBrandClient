using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.iBrandDesktop.Services;

namespace NomadLogic.iBrandDesktop.Util
{
    public partial class LeaderBoard : Form
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        delegate void ChangeCounterValueDelegate(string value, bool reset);
        delegate void ShowLeadersDelegate(List<Participant> leaders);
        delegate void ShowParticipantDelegate(Participant participant);
        delegate void CloseFormDelegate();

        private WebBrowser _webBrowser;
        
        public LeaderBoard()
        {
            InitializeComponent();

            if (Screen.AllScreens.Length > 1)
            {
                Bounds = Screen.AllScreens[1].Bounds;
            }

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.OnRespondentInserted += OnRespondentInserted;

        }

        public void LoadHtml()
        {
            _webBrowser = new WebBrowser();
            _webBrowser.Dock = DockStyle.Fill;
            _webBrowser.Location = new Point(0, 0);
            _webBrowser.Name = "webBrowser";
            _webBrowser.Size = Size;
            _webBrowser.TabIndex = 0;

            var dir = Constants.APP_DIR + "leader_board";
            _webBrowser.Url = new Uri(string.Format("file:///{0}/test.html", dir));
            _webBrowser.DocumentCompleted += DocumentCompletedEventHandler;
            Controls.Add(_webBrowser);           

            ShowDialog();
        }

        public void DocumentCompletedEventHandler(object sender, WebBrowserDocumentCompletedEventArgs e) 
        {
            UpdateLeadersList();
        }

        private void OnRespondentInserted(object sender, EventArgs e)
        {
            //Presenter les resultats du derniers repondant
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondent = db.GetRespondent(db.GetLastInsertedRespondentId());

            ShowParticipant(CreateParticipant(db.GetAnswers(Convert.ToInt32(respondent.Id))));

            //Mettre a jour la liste des meneurs
            UpdateLeadersList();

        }

        private void UpdateLeadersList()
        {
            var db = ServicesContainer.GetService<IbrandDatabase>();
            var respondentsId = db.GetRespondentsId();
            var participants = new List<Participant>();
            try
            {

                foreach (var respId in respondentsId)
                {
                    var answers = db.GetAnswers(respId);
                    
                    participants.Add(CreateParticipant(answers));
                }

                participants.Sort(new ParticipantComparer());

                ShowLeaders(participants);
            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        private Participant CreateParticipant(List<Answer> answers)
        {
            var a = answers[2];
            var score = int.Parse(a.Response);
            a = answers[1];
            var time = double.Parse(a.Response);

            var name = "Test";
            if (answers.Count == 4)
            {
                a = answers[3];
                name = a.Response;
            }
            return new Participant(score, time, name);
        }

        private void ShowLeaders(List<Participant> leaders)
        {
            try
            {
                if (_webBrowser.InvokeRequired)
                {
                    Invoke(new ShowLeadersDelegate(ShowLeaders), leaders);
                    return;
                }

                for (var i = 0; i <= 10; i++)
                {
                    Participant p = null;
                    
                    if (leaders.Count > i)
                        p = leaders[i];

                    var nameId = "name_" + (i + 1);
                    var scoreId = "score_" + (i + 1);
                    var timeId = "time_" + (i + 1);

                    var elem = _webBrowser.Document.GetElementById(nameId);
                    
                    if (elem != null)
                    {
                        if (p != null)
                            elem.InnerText = p.NickName;
                        else
                            elem.InnerText = "-";
                    }

                    elem = _webBrowser.Document.GetElementById(scoreId);

                    if (elem != null)
                    {
                        if (p != null)
                            elem.InnerText = p.Score.ToString();
                        else
                            elem.InnerText = "-";

                    }

                    elem = _webBrowser.Document.GetElementById(timeId);

                    if (elem != null)
                    {
                        if (p != null)
                            elem.InnerText = string.Format("{0:0.00}", p.Time);  
                        else
                            elem.InnerText = "-";
                    }
                }

                _webBrowser.Document.InvokeScript("setGetReadyVisibleFct");

            }
            catch (System.Exception ex)
            {
                var test = ex.Message;
            }
        }

        private void ShowParticipant(Participant p)
        {
            try
            {
                if (_webBrowser.InvokeRequired)
                {
                    Invoke(new ShowParticipantDelegate(ShowParticipant), p);
                    return;
                }

                var elem = _webBrowser.Document.GetElementById("last_participant_name");

                if (elem != null)
                {
                    elem.InnerText = p.NickName;
                }

                elem = _webBrowser.Document.GetElementById("last_participant_score");

                if (elem != null)
                {
                    elem.InnerText = p.Score.ToString();
                }

                elem = _webBrowser.Document.GetElementById("last_participant_time");

                if (elem != null)
                {
                    elem.InnerText = string.Format("{0:0.00}", p.Time);  
                }

            }
            catch (System.Exception ex)
            {
                log.Error(ex);
            }
        }

        public void CloseForm()
        {
            if (InvokeRequired)
            {
                Invoke(new CloseFormDelegate(CloseForm));
                return;
            }

            Close();
        }

        public void ChangeCounterValue(string value, bool reset)
        {
            try
            {
                if (_webBrowser.InvokeRequired)
                {
                    Invoke(new ChangeCounterValueDelegate(ChangeCounterValue), value, reset);
                    return;
                }

                if (value.Equals("0"))
                    _webBrowser.Document.InvokeScript("setTakeYourShotVisibleFct");

                var elem = _webBrowser.Document.GetElementById("count_down");
                
                if (elem != null)
                    elem.InnerText = value;

                if (reset)
                    _webBrowser.Document.InvokeScript("setGetReadyVisibleFct");
            }
            catch (System.Exception ex)
            {
               var test = ex.Message;
            }
        }

    }
}
