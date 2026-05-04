using System;
using System.Windows.Forms;

namespace NomadLogic.Common.Ui
{
    // ReSharper disable once InconsistentNaming
    public class VS2008StatusStrip : StatusStrip
    {
        private readonly ToolStripStatusLabel _messageLabelLeft;
        private readonly ToolStripStatusLabel _messageLabelRight;
        
        public VS2008StatusStrip()
        {
            Renderer = new VS2008MenuRenderer();

            _messageLabelLeft = new ToolStripStatusLabel
            {
                Name = "message_label_left",
                Size = new System.Drawing.Size(209, 17)
            };

            Button = new ToolStripButton("Next")
            {
                Name = "button_next",
                Visible = false
            };

            ProgressBar = new ToolStripProgressBar {Visible = false};

            var spacer = new ToolStripStatusLabel
            {
                Name = "spacer",
                Spring = true
            };

            _messageLabelRight = new ToolStripStatusLabel
            {
                Name = "message_label_left",
                Size = new System.Drawing.Size(209, 17)
            };

            Items.AddRange(new ToolStripItem[]{
                Button,_messageLabelLeft, ProgressBar, spacer, _messageLabelRight});
        }

        public string RightMessage
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { SetRightMessage(value); }));
                }
                else
                {
                    SetRightMessage(value);
                }
            }
        }

        private void SetRightMessage(string value)
        {
            _messageLabelRight.Text = value;
            _messageLabelRight.Invalidate();
        }

        public string LeftMessage
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { SetLeftMessage(value); }));
                }
                else
                {
                    SetLeftMessage(value);
                }
            }
        }

        private void SetLeftMessage(string value)
        {
            _messageLabelLeft.Text = value;
            _messageLabelLeft.Invalidate();
        }

        public string ImportantMessage
        {
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() => { SetLeftMessage(value); }));
                }
                else
                {
                    SetLeftMessage(value);
                }
            }
        }

        public ToolStripProgressBar ProgressBar { get; }

        public void RefreshStrip() 
        {
            Button?.Invalidate();
            _messageLabelLeft.Invalidate();
            _messageLabelRight.Invalidate();
        }

        public ToolStripButton Button { private set; get; }
    }
}
