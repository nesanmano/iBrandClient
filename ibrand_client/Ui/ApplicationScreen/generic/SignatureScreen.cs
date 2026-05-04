using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Util;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;


namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic
{
    /**
     * Cette class permet d'afficher un écran pour la saisi de signature,
     * Elle se charge de sauvegarder la signatur en tant que png a la fin
     */
    class SignatureScreen : AbstractScreen
    {

        private readonly Rectangle _signatureRectangle;
        private bool _mousePressed;
        private Bitmap _bmp;

        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private readonly IList<Point> _coordinates;
        private PictureBox _signaturePictureBox;
        private readonly Point _signatureBoxLocation;

        private IList<SectionSkipScreenData> _zones;

        private Bitmap _checkmark;

        private bool _haveSigned;
        
        //properties 
        public Rectangle PreviousArea { get; set; }

        public Rectangle NextArea { get; set; }

        public Rectangle CancelArea { get; set; }

        public Rectangle Reset { get; set; }

        public int NbrPageToGoBack { get; set; }

        public bool CleanOnBack { get; set; }


        /// La liste des zones Checkbox. Optionel.
        [PublicAPI, CanBeNull]
        public IList<ScreenCheckOption> CheckBoxZones { get; set; }
        /// <summary>
        /// Constrcuteur principal.
        /// Veuillez setter le signatureBoxLocation à new Rectangle(0,0,992,178).
        /// Vue qu'on utilise un nouveau pictureBox pour la signature, on commence a (0,0)
        /// </summary>
        public SignatureScreen(string backgroundImageSrcEn, string backgroundImageSrcFr, ScreenConfig sc,int signatureRectangleWidth,int signatureRectangleHeight,Point signatureBoxLocation)
        : base(backgroundImageSrcEn, backgroundImageSrcFr, sc)
        {
             _signatureRectangle = new Rectangle(0,0,signatureRectangleWidth,signatureRectangleHeight);
             _signatureBoxLocation = signatureBoxLocation;

             _coordinates = new List<Point>();
        }


        /// <summary>
        /// Assigne ou va chercher un tableau de définition de zones.
        /// Cette valeur ne peut jamais être vide ou nulle, et dans cette situation on lui assignera une valeur 
        /// par défaut (Tout l'écran, va à la prochaine page) la prochaine fois qu'on demandera sa valeur.
        /// </summary>
        [PublicAPI]
        public IList<SectionSkipScreenData> ZoneList
        {
            set => _zones = value;
            get => _zones ?? (_zones = new List<SectionSkipScreenData>
                       {
                           new SectionSkipScreenData
                           {
                               Zone = new Rectangle(new Point(0, 0), new Size(PictureBox.Image.Width, PictureBox.Image.Height)),
                               PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                           }
                       }
                   );
        }

        protected override void OnLoad()
        {
            CreateSignaturePictureBox(_signatureRectangle.Width,_signatureRectangle.Height);
            PrepareSignatureBox(_signatureRectangle.Width,_signatureRectangle.Height);

            PictureBox.Controls.Add(_signaturePictureBox);
            _signaturePictureBox.MouseDown += MousePressedDownEvent;
            _signaturePictureBox.MouseMove += MouseMoveEvent;
            _signaturePictureBox.MouseUp += MouseUpEvent;
            _signaturePictureBox.Paint += _signaturePictureBoxPaint;

            _checkmark = GetNoneLanguageBitmap("check.png");
        }

        protected void _signaturePictureBoxPaint(object sender, PaintEventArgs e)
        {
            AddSignatureBox(e, _signatureRectangle);
        }

        protected override void OnPaintEvent(Graphics g)
        {
            base.OnPaintEvent(g);
            PaintCheckBoxList(g, CheckBoxZones, _checkmark);
        }

        private void AddSignatureBox(PaintEventArgs e, Rectangle signatureBoxLocation)
        {
            e.Graphics.DrawRectangle(
                new Pen(Color.Red, 5),
                signatureBoxLocation);
        }

        /// <inheritdoc />
        protected override bool OnMouseDownHandler(int x, int y)
        {
            if (!base.OnMouseDownHandler(x, y)) return false;

            if (InArea(x, y, PreviousArea))
            {
                if (CleanOnBack)
                {
                    PrepareSignatureBox(_signatureRectangle.Width,_signatureRectangle.Height);
                    _signaturePictureBox.Invalidate();
                }

                Back(NbrPageToGoBack);
                return false;
            }

            if (InArea(x, y, Reset))
            {
                PrepareSignatureBox(_signatureRectangle.Width,_signatureRectangle.Height);
                _signaturePictureBox.Invalidate();
                ResetTimer();
                return false;
            }

            if (InArea(x, y, NextArea))
            {
                if (ZoneList != null)
                {
                    foreach (var data in ZoneList.Where(data => InArea(x, y, data.Zone)))
                    {
                        if (!data.SkipValidation && !AllMandatoryChecks(CheckBoxZones))
                        {
                            var mBox = new MessageDialog();
                            mBox.ShowMessage(Resources.iBrand.NotAllMandatoryCheckEnabled);
                            return false;
                        }
                        if (!_haveSigned)
                        {
                            var mBox = new MessageDialog();
                            mBox.ShowMessage(" You must sign!");
                            return false;
                        }
                    }
                }

                var image = _signaturePictureBox.Image;
                using (var ms = new MemoryStream())
                {
                    image.Save(ms, ImageFormat.Bmp);

                    var img = new Bitmap(ms);
                    var signatureFileName = DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".png";
                    var signature = GetTerminalDirectoryDataFile(signatureFileName,
                                        DirectoryUtil.ProjectDataDirectoriesEnum.signatures,
                                        DirectoryUtil.ProjectDataDirectoriesModEnum.none);

                    if (!img.Save(ImageFormat.Png, 1, signature))
                    {
                        log.Error("Probleme lors de la sauvegarde de la signature");
                        return false;
                    }

                    if (_screenConfig.QuestionSignatureNo != null)
                    {
                        SaveAnswerToQuestion(new Answer
                        {
                            QuestionType = QuestionTypeEnum.Signature,
                            QuestionNo = _screenConfig.QuestionSignatureNo.Value, 
                            Response = signature
                        });
                    }
                    Next();
                    return false;
                }
            }

            if (InArea(x, y, CancelArea))
            {
                PrepareSignatureBox(_signatureRectangle.Width,_signatureRectangle.Height);
                _signaturePictureBox.Invalidate();
                FirstScreen();
                return false;
            }

            return EvaluateCheckBoxsMouseDown(x, y, CheckBoxZones);
        }


        /*
         * Cette event sert a detecter un mouse down dans la zone de signature
         */
        private void MousePressedDownEvent(object sender, MouseEventArgs e)
        {
           // Console.WriteLine("X position: " + e.X + "   Y Position: " + e.Y);
            if (!InArea(e.X, e.Y, _signatureRectangle)) return;
            // Ici on recupere la coordonnee pour initier la signature
            _mousePressed = true;
            _coordinates.Clear();
            _coordinates.Add(new Point(e.X, e.Y));
            SuspendTimer();
        }

        /// <summary>
        /// Cette methode est utilise pour eventuellement recuperer les coordonees de la souris.
        /// Ces coordonnees seront utilises pour dessiner la signature.
        /// </summary>
        /// <param name="sender">Envoyeur</param>
        /// <param name="e">Evenement</param>
        private void MouseMoveEvent(object sender, MouseEventArgs e)
        {
            if (!_mousePressed) return;

            if (InArea(e.X, e.Y, _signatureRectangle))
            {
                if (_coordinates == null) return;
                _coordinates.Add(new Point(e.X, e.Y));

                if (_coordinates.Count >= 2)
                {
                    using (var g = Graphics.FromImage(_signaturePictureBox.Image))
                    {
                        g.DrawLine(new Pen(Color.Black, 2), _coordinates.ElementAt(0), _coordinates.ElementAt(1));
                        g.SmoothingMode = SmoothingMode.AntiAlias;

                        var tmp = _coordinates.ElementAt(1);
                        _coordinates.Clear();
                        _coordinates.Add(tmp);
                    }
                }

                _signaturePictureBox.Invalidate();
            }
            else
            {   // On gere la cas ou la souris depasse la zone defini pour la signature
                EndWriteToSignature();
                PictureBox.Invalidate();
            }
        }

        /// <summary>
        /// Cette methode determiner la fin d'une signature.
        /// Ces coordonnees seront utilises pour dessiner la signature.
        /// </summary>
        private void MouseUpEvent(object sender, MouseEventArgs e)
        {
            EndWriteToSignature();
            _haveSigned = true;
        }

        private void EndWriteToSignature()
        {
            _mousePressed = false;
            ResetTimer();
        }

        /// <summary>
        /// Cette methode permet de creer un pictureBox pour la Signature 
        /// </summary>
        private void CreateSignaturePictureBox(int width,int height)
        {
            _signaturePictureBox = new PictureBox
            {
                Location = _signatureBoxLocation,
                Name = "pictureBoxSignature",
                Size = new Size(width,height),
                BackColor = Color.Transparent
            };

        }

        /// <summary>
        /// Cette methode permet de remettre a neuf le pictureBox pour la signature
        /// </summary>
        private void PrepareSignatureBox(int width,int height)
        {
            _signaturePictureBox.Image?.Dispose();
            _bmp = new Bitmap(_signatureRectangle.Width, _signatureRectangle.Height);
            using (var gfx = Graphics.FromImage(_bmp))
            using (var brush = new SolidBrush(Color.White))
            {
                gfx.FillRectangle(brush, 0, 0, width, height);
            }
            
            _signaturePictureBox.Image = _bmp;
        }

        public override void Recycle()
        {
            base.Recycle();
            _haveSigned = false;

            PrepareSignatureBox(_signatureRectangle.Width,_signatureRectangle.Height);
            _signaturePictureBox.Invalidate();

            RecycleCheckBoxs(CheckBoxZones);
        }

    }



}
