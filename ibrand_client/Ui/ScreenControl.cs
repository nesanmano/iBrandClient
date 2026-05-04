using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.Dialog;
using NomadLogic.iBrandDesktop.Exception;
using NomadLogic.iBrandDesktop.Util;

namespace NomadLogic.iBrandDesktop.Ui
{
    //public delegate void AddMenuItem(ToolStripMenuItem menu_item);
    //public delegate void RemoveMenuItem(ToolStripMenuItem menu_item);

    public partial class ScreenControl : UserControl
    {
        //public event AddMenuItem OnAddMenuItem;
        //public event RemoveMenuItem OnRemoveMenuItem;

        public event EventHandler OnNext;
        public event EventHandler OnBack;

        private readonly List<IScreenElement> _screenElements = new List<IScreenElement>();

        private bool _scrollBarVisible;
        
        private Question.Question _firstQuestion;

        //private ToolStripMenuItem clear_signature_menu = null;

        private readonly int _buttonsPanelHeight = 30;

        public ScreenControl(int no)
        {
            InitializeComponent();

            No = no;

            Visible = false;

            AutoScroll = true;
        }

        public bool HasFocusableQuestion()
        {
            return _screenElements.OfType<Question.Question>().Any(question => question.Focusable);
        }

        public bool Validate(ScreensContainer questionsContainer)
        {
            foreach (var screenElement in _screenElements)
            {
                if (!(screenElement is Question.Question)) continue;

                var question = (Question.Question)screenElement;
                if (!question.ValidateAnswers())
                {
                    MsgBox.Show(questionsContainer, Resources.iBrand.MandatoryQuestion, Resources.iBrand.AppName);
                    return false;
                }

                try
                {
                    if (!question.ValidateAnswersPattern())
                    {
                        MsgBox.Show(questionsContainer, Resources.iBrand.WrongPattern1 + " " + question.No + " " + Resources.iBrand.WrongPattern2 + question.AnswersPattern, Resources.iBrand.AppName);
                        return false;
                    }
                }
                catch (UnsuportedPatternException)
                {
                    //TBD
                }

                try
                {
                    if (question.ValidateDatasourceField()) continue;
                    MsgBox.Show("Yo YO !!!");
                    return false;
                }
                catch (System.Exception)
                {
                    //TBD
                }
            }

            return true;
        }

        private void OnNextButton(object sender, EventArgs e)
        {
            OnNext?.Invoke(this, EventArgs.Empty);
        }

        public int No { get; }

        private void OnBackButton(object sender, EventArgs e)
        {
            OnBack?.Invoke(this, EventArgs.Empty);
        }

        public void AddScreenElement(IScreenElement screenElement)
        {
            _screenElements.Add(screenElement);
        }

        public void RemoveElements()
        {
            Controls.Clear();
        }

        public void AddScreenElements(int availableHeight, int nbrQuestions)
        {
            Size = new Size(WorkingArea.Size.Width, availableHeight);

            var availableWidth = Size.Width;

            if (IsScrollbarNeeded(availableHeight, availableWidth))//La scroll bar va apparaitre.
            {
                availableWidth = availableWidth - SystemInformation.VerticalScrollBarWidth - 2; //-2 = temp bug fix...;
                _scrollBarVisible = true;
            }
            else
            {
                _scrollBarVisible = false;
            }

            var y = Constants.QUESTION_MARGIN; //y initial de la position sur le question panel

            var firstQuestion = true;

            Question.Question lastQuestion = null;

            foreach (var screenElement in _screenElements)
            {
                if (screenElement == null) continue;
                if (screenElement is Question.Question element)
                {
                    var question = element;

                    lastQuestion = question;

                    if (question.Visible)
                    {
                        if (question.HasChoiceText())
                            HasQuestionText = true;
                        
                        if (firstQuestion)//First question of the control
                        {
                            _firstQuestion = question;
                            firstQuestion = false;
                        }

                        y += question.AddToUserControl(this, availableWidth, y, CreateGraphics());
                    }

                    question.Reset();

                }
                else
                {
                    y += screenElement.AddToUserControl(this, availableWidth, y, CreateGraphics());
                }
            }

            var buttonsPanel = new Panel
            {
                Size = new Size(availableWidth, _buttonsPanelHeight),
                Location = new Point(0, y)
            };

            Controls.Add(buttonsPanel);

            if (No > 1)
            {
                var backButton = new Button
                {
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(187, 2),
                    Name = "back_button",
                    Size = new Size(91, 23),
                    Text = @"Back"
                };
                backButton.Click += OnBackButton;
                backButton.UseVisualStyleBackColor = true;

                buttonsPanel.Controls.Add(backButton);
            }

            var nextButton = new ColorButton {Size = new Size(91, 23)};

            var x = (availableWidth / 2) - (nextButton.Width / 2);
            nextButton.Location = new Point(x, 1);
            nextButton.Name = "next_button";
            nextButton.Click += OnNextButton;

            buttonsPanel.Controls.Add(nextButton);

            if (lastQuestion != null && lastQuestion.No == nbrQuestions)
                nextButton.Text = @"Submit";
            else
                nextButton.Text = @"Next";

        }
        
        public bool HasQuestionText { get; private set; }

        public new bool Visible
        {
            set
            {
                base.Visible = value;
                _firstQuestion?.SetFocus();
            }
        }

        private bool IsScrollbarNeeded(int availableHeight, int availableWidth)
        {
            //y initial de la position sur le question panel
            var totalHeigth = Constants.QUESTION_MARGIN + 
                _screenElements.Where(screenElement => screenElement != null).
                Sum(screenElement => screenElement.GetHeight(availableWidth, CreateGraphics()));

            //Verifier si la scroll bar est necessaire

            totalHeigth += _buttonsPanelHeight;

            return (totalHeigth >= availableHeight);
        }

        public void RedrawIfScrollBarNeeded(int availableHeight, int nbrQuestions)
        {
            var availableWidth = Size.Width;
            var scrollbarNeeded = IsScrollbarNeeded(availableHeight, availableWidth);
            if (scrollbarNeeded != _scrollBarVisible)
            {
                Redraw(availableHeight, nbrQuestions);
            }
        }

        public void Redraw(int availableHeight, int nbrQuestions)
        {
            Controls.Clear();
            AddScreenElements(availableHeight, nbrQuestions);
        }

        public List<IScreenElement> GetScreenElements()
        {
            return _screenElements;
        }
    }
}
