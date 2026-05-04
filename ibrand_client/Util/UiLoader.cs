using System;
using System.IO;
using System.Xml;
using NLog;
using NomadLogic.Common;
using NomadLogic.iBrandDesktop.Ui;

namespace NomadLogic.iBrandDesktop.Util
{
    public class UiLoader
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private readonly ScreenController _screensController;
        private readonly Questionnaire _questionnaire;
        private readonly Project _project;

        private ScreenControl _activeScreenControl;
        private ScreenView _activeScreenView;

        private string _lg;
        public string _text;

        public UiLoader(ScreenController screensController, Questionnaire questionnaire, Project project)
        {
            _screensController = screensController;
            _questionnaire = questionnaire;
            _project = project;
        }

        public void Load()
        {
            StreamReader stream = null;

            try
            {
                var fileName = _project.ProjectDir + _questionnaire.UiFile;

                if (!File.Exists(fileName))
                {
                    // On créé un UI de base.
                    _activeScreenControl = new ScreenControl(1);
                    for (var i = 1; i <= _questionnaire.GetNbrQuestion(); i++)
                    {
                        _activeScreenControl.AddScreenElement(_questionnaire.GetQuestion(i));
                    }
                    _screensController.AddScreenControl(_activeScreenControl);
                    _activeScreenControl = null;
                }
                else
                {
                    stream = new StreamReader(fileName);

                    var settings = new XmlReaderSettings
                    {
                        ConformanceLevel = ConformanceLevel.Fragment,
                        IgnoreWhitespace = true,
                        IgnoreComments = true
                    };

                    var xmlReader = XmlReader.Create(stream, settings);

                    ParseXml(xmlReader);
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during parsing of UI file.");
                throw;
            }
            finally
            {
                stream?.Close();
            }
        }

        private void ParseXml(XmlReader xmlReader)
        {

            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        StartElement(xmlReader);
                        break;
                    case XmlNodeType.EndElement:
                        EndElement(xmlReader);
                        break;
                    case XmlNodeType.CDATA:
                        _text = xmlReader.Value;
                        break;
                    case XmlNodeType.Text:
                        _text = xmlReader.Value;
                        break;
                }
            }
        }

        private void StartElement(XmlReader xmlReader)
        {
            try
            {
                if (xmlReader.Name.Equals("screen"))
                {
                    var no = xmlReader.GetAttribute("no");
                    _activeScreenControl = new ScreenControl(Convert.ToInt32(no));
                }
                else if (xmlReader.Name.Equals("view"))
                {
                    _activeScreenView = new ScreenView();
                    _activeScreenControl.AddScreenElement(_activeScreenView);
                }
                else if (xmlReader.Name.Equals("question"))
                {
                    var no = xmlReader.GetAttribute("no");
                    _activeScreenControl.AddScreenElement(_questionnaire.GetQuestion(Convert.ToInt32(no)));
                }
                else if (xmlReader.Name.Equals("text"))
                {
                    var lg = xmlReader.GetAttribute("lg");
                    _lg = lg != null && lg.Equals("fr") ? Constants.FR : Constants.EN;
                }
                else if (xmlReader.Name.Equals("image"))
                {
                    var lg = xmlReader.GetAttribute("lg");

                    if (lg != null && lg.Equals("fr"))
                        _lg = Constants.FR;
                    else
                        _lg = Constants.EN;

                    var src = xmlReader.GetAttribute("src");

                    _activeScreenView.SetImage(_lg, _project.ProjectDir + _questionnaire.Id + Path.DirectorySeparatorChar + src);
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during UI parsing start element with element ({0}) for file named ({1}) in questionnaire {2}",
                    xmlReader.Name, _questionnaire.UiFile, _questionnaire.Id);
                throw;
            }
        }

        public void EndElement(XmlReader xmlReader)
        {
            try
            {
                if (xmlReader.Name.Equals("view"))
                {
                    _activeScreenView = null;
                }
                else if (xmlReader.Name.Equals("screen"))
                {
                    _screensController.AddScreenControl(_activeScreenControl);
                    _activeScreenControl = null;
                }
                else if (xmlReader.Name.Equals("text"))
                {
                    _activeScreenView.SetText(_lg, _text);

                    _text = null;
                    _lg = null;
                }
            }
            catch (System.Exception e)
            {
                log.Error(e, "Error during UI parsing end element with element ({0}) for file named ({1}) in questionnaire {2}",
                    xmlReader.Name, _questionnaire.UiFile, _questionnaire.Id);
                throw;
            }
        }
    }
}
