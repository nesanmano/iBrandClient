using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia.XmlTransformation.Filters;

namespace NomadLogic.Multimedia.XmlTransformation.Layer
{
    class PhotoLayerFiltre : PhotoLayer
    {
        private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();
        private List<ImageFilter> _listFiltre;

        protected string _maskSrc;
        protected string _maskDir;
        protected Bitmap _filterMask;

        public PhotoLayerFiltre(int x, int y, int[] scale)
            : base(x, y, scale)
        {
            _listFiltre = new List<ImageFilter>();
        }

        public PhotoLayerFiltre(int x, int y, int[] scale, XmlTextReader reader)
            : this(x, y, scale)
        {
            var questionNo = reader.GetAttribute("question_no");
            if (questionNo != null)
                QuestionNo = int.Parse(questionNo);

            var greenScreen = reader.GetAttribute("green_screen");
            if (greenScreen == "true")
                GreenScreen = true;

            var faceDetection = reader.GetAttribute("face_detection");
            if (faceDetection == "true")
                FaceDetection = true;

            if (!reader.IsEmptyElement)
            {
                var layerFinished = false;

                while (!layerFinished)
                {
                    reader.Read();
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name.Equals("green_screen_config"))
                            {
                                _greenScreenFilterParams = new Dictionary<string, float>();
                                var paramsFinished = false;

                                while (!paramsFinished)
                                {
                                    reader.Read();
                                    switch (reader.NodeType)
                                    {
                                        case XmlNodeType.Element:
                                            if (reader.Name.Equals("green_screen_parameter"))
                                            {
                                                var paramName = reader.GetAttribute("name");
                                                var value = reader.GetAttribute("value");
                                                if (value != null)
                                                {
                                                    var paramValue = float.Parse(value);
                                                    if (paramName != null)
                                                        _greenScreenFilterParams.Add(paramName, paramValue);
                                                }
                                            }
                                            break;

                                        case XmlNodeType.EndElement:
                                            if (reader.Name.Equals("green_screen_config"))
                                            {
                                                paramsFinished = true;
                                            }
                                            break;
                                    }
                                }
                            }
                            else if (reader.Name.Equals("image_filter"))
                            {
                                var filterClass = reader.GetAttribute("filter");
                                log.Debug("before reading filter " + filterClass);
                                if (filterClass != null)
                                {
                                    // ReSharper disable once AssignNullToNotNullAttribute
                                    var filtre = (ImageFilter)Activator.CreateInstance(Type.GetType(filterClass));
                                    filtre.ParseXml(reader);
                                    log.Debug("adding filter to list");
                                    _listFiltre.Add(filtre);
                                }
                            }
                            else if (reader.Name.Equals("filter_mask"))
                            {
                                var src = reader.GetAttribute("src");
                                var dir = reader.GetAttribute("dir");

                                if (src != null && !src.Equals(""))
                                    _maskSrc = src;

                                if (dir != null && !dir.Equals(""))
                                    _maskDir = dir;

                            }// if image_filtre

                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("layer"))
                            {
                                layerFinished = true;
                            }
                            break;
                    }
                }
            }
        }

        public PhotoLayerFiltre(PhotoLayer layer)
            : base(layer.X, layer.Y, layer.Scale)
        {
            QuestionNo = layer.QuestionNo;
            GreenScreen = layer.GreenScreen;
            GreenScreenFilterParams = layer.GreenScreenFilterParams;
            FaceDetection = layer.FaceDetection;
            _rectangleFace = layer.RectangleFace;
            _listFiltre = new List<ImageFilter>();
        }

        public List<ImageFilter> ListFiltres
        {
            get { return _listFiltre; }
            set { _listFiltre = value; SettingsHaveChanged(); }
        }

        public void AddFilter(ImageFilter filtre)
        {
            _listFiltre.Add(filtre);
            SettingsHaveChanged();
        }

        private string GetMask(IRespondent respondent)
        {
            if (_maskSrc != null)
            {
                var fileName = "";

                if (_maskDir != null)
                    fileName += _maskDir + Path.DirectorySeparatorChar;

                fileName += _maskSrc;

                var contextDir = respondent.TerminalDirectory;

                return contextDir + fileName;
            }

            return null;
        }

        /// <summary>
        /// Applique des filters additionnels pour l'image. À surcharger dans des sous-classes.
        /// </summary>
        /// <param name="respondent">Le répondant.</param>
        protected override void AdditionalFilters(IRespondent respondent)
        {
            if (_listFiltre.Count > 0)
            {
                var maskFile = GetMask(respondent);

                if (maskFile != null && File.Exists(maskFile))
                {
                    _filterMask = new Bitmap(maskFile);
                    _filterMask.SetResolution(300, 300);
                }

                var imageTmp = new Bitmap(_finalImage.Width, _finalImage.Height, Graphics.FromImage(_finalImage));
                imageTmp.SetResolution(300, 300);
                foreach (var imageFiltre in _listFiltre)
                {
                    if (_filterMask != null)
                    {
                        imageFiltre.Mask = _filterMask;
                        if (FaceDetection)
                        {
                            //TODO: faire offset pour mask
                        }
                    }
                    imageTmp = imageFiltre.Filter(_finalImage);
                    imageTmp.SetResolution(300, 300);
                    var g2D = Graphics.FromImage(_finalImage);
                    g2D.DrawImageUnscaled(imageTmp, 0, 0);
                    g2D.Flush();
                    g2D.Dispose();
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_filterMask != null)
            {
                _filterMask.Dispose();
                _filterMask = null;
            }
        }
    }
}
