using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Extentions;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia.XmlTransformation.Layer;

#pragma warning disable 618

namespace NomadLogic.Multimedia.XmlTransformation.image
{
    [PublicAPI]
    public class Transformation : IDisposable, IResetToOriginalValues
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly List<ICondition> _conditions = new List<ICondition>();

        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private int _finalWidthOriginal = -1;
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private int _finalHeightOriginal = -1;

        private IRespondent _respondent;

        /// <summary>
        /// Créer une instance totalement disjointe de l'instance donnée en paramètre, mais équivalente en tout point.
        /// <br/>En clair, on fait une copie profonde.
        /// </summary>
        /// <param name="original">L'instance à copier.</param>
        public Transformation(Transformation original)
        {
            FinalHeight = original.FinalHeight;
            FinalWidth = original.FinalWidth;
            _finalHeightOriginal = original._finalHeightOriginal;
            _finalWidthOriginal = original._finalWidthOriginal;
            OutputFormat = original.OutputFormat;
            ImageType = original.ImageType;

            foreach (var cond in original._conditions)
            {
                _conditions.Add(cond);
            }

            foreach (var layer in original.Layers)
            {
                Layers.Add(layer.GetCopy());
            }
        }

        public Transformation(XmlTextReader reader)
        {
            var finalWidth = reader.GetAttribute("final_width");
            var finalHeight = reader.GetAttribute("final_height");

            if (finalWidth != null && finalHeight != null)
            {
                var w = int.Parse(finalWidth);
                var h = int.Parse(finalHeight);
                SetFinalSize(w, h);
                _finalWidthOriginal = w;
                _finalHeightOriginal = h;
            }

            var format = reader.GetAttribute("output_format");

            if (format != null)
                OutputFormat = format;

            var outputNameQuestionNo = reader.GetAttribute("output_name_question_no");

            if (outputNameQuestionNo != null)
                OutputNameQuestionNo = outputNameQuestionNo;

            var imageType = reader.GetAttribute("image_type");

            if (imageType != null)
                ImageType = imageType;

            var preview = reader.GetAttribute("preview");

            if (preview != null)
                IsPreview = bool.Parse(preview);

            var transformationFinished = false;

            while (!transformationFinished)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("conditions"))
                        {
                            var conditionsFinished = false;
                            while (!conditionsFinished)
                            {
                                reader.Read();
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (reader.Name.Equals("condition"))
                                        {
                                            ICondition condition;
                                            if (reader.GetAttribute("event_id") == null)
                                            {
                                                var questionNo = reader.GetAttribute("question_no");

                                                if (questionNo == null)
                                                {
                                                    throw new NullReferenceException("Question number cannot be omitted in transformation condition description.");
                                                }

                                                var choiceNo = reader.GetAttribute("choice_no") ?? "1";

                                                condition = new QuestionCondition(
                                                    int.Parse(questionNo),
                                                    int.Parse(choiceNo));
                                            }
                                            else
                                            {
                                                var eventId = reader.GetAttribute("event_id");
                                                condition = new EventCondition(int.Parse(eventId));
                                            }
                                            _conditions.Add(condition);
                                        }
                                        break;
                                    case XmlNodeType.EndElement: //Display the end of the element.
                                        if (reader.Name.Equals("conditions"))
                                        {
                                            conditionsFinished = true;
                                        }
                                        break;
                                }
                            }
                        }
                        else if (reader.Name.Equals("layers"))
                        {
                            var layersFinished = false;
                            while (!layersFinished)
                            {
                                reader.Read();
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (reader.Name.Equals("layer"))
                                        {
                                            AbstractLayer layer;

                                            var x = 0;
                                            var y = 0;
                                            var rotation = reader.GetAttribute("rotation");

                                            if (reader.GetAttribute("x") != null)
                                                x = int.Parse(reader.GetAttribute("x"));

                                            if (reader.GetAttribute("y") != null)
                                                y = int.Parse(reader.GetAttribute("y"));

                                            var stringScale = reader.GetAttribute("scale");
                                            int[] scale = null;

                                            if (stringScale != null)
                                            {
                                                var tmp = new int[2];
                                                var tmpString = stringScale.Split(',');
                                                tmp[0] = int.Parse(tmpString[0]);
                                                tmp[1] = int.Parse(tmpString[1]);

                                                scale = tmp;
                                            }

                                            if (reader.GetAttribute("type") == "picture")
                                            {
                                                layer = new ImageLayer(x, y, scale, reader);
                                            }
                                            else if (reader.GetAttribute("type") == "respondent_picture_filter")
                                            {
                                                log.Debug("before reading respondent_picture_filter");
                                                layer = new PhotoLayerFiltre(x, y, scale, reader);
                                            }
                                            else if (reader.GetAttribute("type") == "respondent_picture")
                                            {
                                                layer = new PhotoLayer(x, y, scale, reader);
                                            }
                                            else if (reader.GetAttribute("type") == "respondent_signature")
                                            {
                                                layer = new SignatureLayer(x, y, scale, reader);
                                            }
                                            else if (reader.GetAttribute("type") == "text")
                                            {
                                                layer = new TextLayer(x, y, reader);
                                            }
                                            else if (reader.GetAttribute("type") == "empty")
                                            {
                                                layer = new EmptyLayer(x, y, reader);
                                            }
                                            else
                                            {
                                                // layer pas supporté
                                                layer = null;
                                            }

                                            if (rotation != null && layer != null)
                                            {
                                                layer.Rotation = int.Parse(rotation);
                                            }

                                            if (layer != null)
                                            {
                                                Layers.Add(layer);
                                            }
                                        }
                                        break;
                                    case XmlNodeType.EndElement: //Display the end of the element.
                                        if (reader.Name.Equals("layers"))
                                        {
                                            layersFinished = true;
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("transformation"))
                        {
                            transformationFinished = true;
                        }
                        break;
                }
            }
        }

        public IRespondent Respondent
        {
            get => _respondent;
            set { _respondent = value; Dispose(); }
        }

        public void AddCondition(ICondition condition)
        {
            _conditions.Add(condition);
        }

        public List<AbstractLayer> Layers { get; set; } = new List<AbstractLayer>();

        /// Indique que la transformation est utilisée pour une prévisualisation.
        public bool IsPreview { get; set; }

        public void SetFinalSize(int width, int height)
        {
            FinalWidth = width;
            FinalHeight = height;
        }

        public int FinalWidth { get; set; } = -1;

        public int FinalHeight { get; set; } = -1;

        public void AddLayer(AbstractLayer layer)
        {
            Layers.Add(layer);
        }

        public string OutputFormat { get; set; } = "jpg";

        public string OutputNameQuestionNo
        {
            get;
            set;
        }

        public string ImageType { get; set; } = "rgb";

        public List<PhotoLayer> GetRespondentPhotoLayers()
        {
            var nbrLayer = Layers.Count;
            var photoLayers = new List<PhotoLayer>();

            for (var i = 1; i < nbrLayer; i++)
            {
                var layer = Layers[i];
                if (layer is PhotoLayer item)
                {
                    photoLayers.Add(item);
                }
            }
            return photoLayers;
        }

        public Bitmap TransformImage()
        {
            if (_respondent == null)
            {
                throw new System.Exception("No respondent Set.");
            }

            var nbrLayer = Layers.Count;

            if (nbrLayer == 0)
            {
                throw new System.Exception("No layer in transformation.");
            }

            var layer = Layers[0];

            Bitmap image;

            switch (layer)
            {
                case FileLayer fileLayer:
                    var fl = fileLayer;
                    fl.GetBitmapFromFile(_respondent);
                    var tmp = fl.GetCurrentBitmap();
                    image = new Bitmap(tmp, tmp.Width, tmp.Height);
                    image.SetResolution(300, 300);
                    break;
                case EmptyLayer emptyLayer:
                    image = new Bitmap(emptyLayer.GetImageWidth(_respondent), emptyLayer.GetImageHeight(_respondent));
                    image.SetResolution(300, 300);

                    if (ImageType.Equals("rgb"))
                    {
                        var g2D = Graphics.FromImage(image);
                        g2D.DrawRectangle(new Pen(Color.White), 0, 0, image.Width, image.Height);
                        g2D.Flush();
                        g2D.Dispose();
                    }

                    break;
                default:
                    throw new System.Exception("Position 1 has to be FileLayer or EmptyLayer");
            }

            var error = false;
            log.Trace("transforming; nb layers : " + nbrLayer);
            try
            {
                for (var i = 1; i < nbrLayer; i++)
                {
                    layer = Layers[i];
                    log.Trace("transforming layer " + i + "; " + layer.GetType().FullName);
                    layer.Draw(image, _respondent);
                    log.Trace("layer " + i + " transformed");
                }
            }
            catch (System.Exception ex)
            {
                log.Fatal(ex);
                error = true;
                throw;
            }
            finally
            {

                if (!error)
                {

                    if (FinalWidth != -1 && FinalHeight != -1)
                    {
                        var tmp = image.Resize(FinalWidth, FinalHeight);
                        image.Dispose();
                        image = tmp;
                    }
                }

            }
            log.Debug("return image");
            return image;
        }

        public bool IsConditionsSatisfied(IRespondent respondent)
        {
            var nbrCondition = _conditions.Count;

            for (var i = 0; i < nbrCondition; i++)
            {
                var condition = _conditions[i];
                if (!condition.Evaluate(respondent))
                    return false;
            }
            return true;
        }

        public void Dispose()
        {
            foreach (var al in Layers)
            {
                al.Dispose();
            }
        }

        ~Transformation()
        {
            Dispose();
        }

        public void ResetValues()
        {
            FinalWidth = _finalWidthOriginal;
            FinalHeight = _finalHeightOriginal;

            foreach (var al in Layers)
            {
                al.ResetValues();
            }
        }

        public void ScaleTransformation(float previewFactor)
        {
            FinalHeight = (int)(FinalHeight * previewFactor);
            FinalWidth = (int)(FinalWidth * previewFactor);

            foreach (var layer in Layers)
            {
                layer.ResizeParameters(previewFactor);
            }
        }
    }
}
