using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using JetBrains.Annotations;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    public class SignatureControl : Control
    {
        private Bitmap _bmp;
        private Graphics _graphics;
        private readonly Pen _pen = new Pen(Color.White);

        private readonly Brush _backBrush = new SolidBrush(Color.FromArgb(0, 255, 0));
        //Brush _fore_brush = new SolidBrush(Color.Purple);

        // list of line segments

        // the current line segment
        private readonly ArrayList _points = new ArrayList();
        private Point _lastPoint = new Point(0, 0);

        // if drawing signature or not
        private bool _collectPoints;
        private readonly int _pId;
        private readonly int _qId;
        private readonly int _tId;

        public SignatureControl(int pId, int qId, int tId)
        {
            // Set the pen's width.
            _pId = pId;
            _qId = qId;
            _tId = tId;
            _pen.Width = 5.0F;
            _pen.DashStyle = DashStyle.Solid;
        }

        [PublicAPI]
        public ArrayList Lines { get; } = new ArrayList();

        protected override void OnPaint(PaintEventArgs e)
        {
            // blit the memory bitmap to the screen
            // we draw on the memory bitmap on mousemove so there
            // is nothing else to draw at this time (the memory 
            // bitmap already contains all of the lines)
            CreateGdiObjects();
            e.Graphics.DrawImage(_bmp, new Rectangle(0, 0, Width, Height), new Rectangle(0, 0, _bmp.Width, _bmp.Height), GraphicsUnit.Pixel);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // don't pass to base since we paint everything, avoid flashing
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // process if currently drawing signature
            if (_collectPoints) return;
            // start collecting points
            _collectPoints = true;

            // use current mouse click as the first point
            _lastPoint.X = e.X;
            _lastPoint.Y = e.Y;

            // this is the first point in the list
            _points.Clear();
            _points.Add(_lastPoint);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            // process if drawing signature
            if (!_collectPoints) return;
            // stop collecting points
            _collectPoints = false;

            // add current line to list of segments
            var points = new Point[_points.Count];
            for (var i = 0; i < _points.Count; i++)
            {
                var pt = (Point)_points[i];
                points[i].X = pt.X;
                points[i].Y = pt.Y;
            }

            Lines.Add(points);

            // start over with a new line
            _points.Clear();
        }

        public string SaveSignatureToDisk()
        {
            var signatureId = Guid.NewGuid().ToString();
            
            var signaturePath = DirectoryUtil.GetLocalIbrandTerminalDataDir(_pId, _qId, _tId,
                DirectoryUtil.ProjectDataDirectoriesEnum.signatures, DirectoryUtil.ProjectDataDirectoriesModEnum.none) + signatureId + ".jpg";

            _bmp.Save(signaturePath, ImageFormat.Jpeg);

            return signaturePath;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // process if drawing signature
            if (!_collectPoints) return;
            // add point to current line segment
            _points.Add(new Point(e.X, e.Y));

            // draw the new segment on the memory bitmap
            _graphics.DrawLine(_pen, _lastPoint.X, _lastPoint.Y, e.X, e.Y);

            // update the current position
            _lastPoint.X = e.X;
            _lastPoint.Y = e.Y;

            // display the updated bitmap
            Invalidate();
        }

        [PublicAPI]
        public void Clear()
        {
            Lines.Clear();
            InitMemoryBitmap();
            Invalidate();
        }

        private void CreateGdiObjects()
        {
            // only create if don't have one or the size changed
            if (_bmp == null /*|| _bmp.Width != this.Width || _bmp.Height != this.Height*/)
            {
                // memory bitmap to draw on
                InitMemoryBitmap();
            }
        }

        private void InitMemoryBitmap()
        {
            // load the background image
            _bmp = new Bitmap(Width, Height);


            // Get graphics object now to make drawing during mousemove faster
            _graphics = Graphics.FromImage(_bmp);

            _graphics.FillRectangle(_backBrush, 0, 0, Width, Height);

            DrawSignature(_graphics);
        }


        private void DrawSignature(Graphics g)
        {

            // return if don't have a signature
            if (Lines == null || Lines.Count == 0)
                return;

            // draw each line segment
            foreach (Point[] line in Lines)
            {
                if (line != null && line.Length > 0)
                {
                    g.DrawLines(_pen, line);
                }
            }
        }

        public bool IsAnswered()
        {
            return Lines.Count > 0;
        }

        public void Reset()
        {
            Clear();
        }
    }
}
