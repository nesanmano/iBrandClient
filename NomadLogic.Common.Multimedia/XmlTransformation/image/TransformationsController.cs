using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Multimedia.XmlTransformation.Exception;
using NomadLogic.Multimedia.XmlTransformation.Layer;

namespace NomadLogic.Multimedia.XmlTransformation.image
{
    [PublicAPI]
    public class TransformationsController
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private List<Transformation> _transformations = new List<Transformation>();

        public void Initialize(string configFile)
        {
            Console.WriteLine(configFile);

            if (!File.Exists(configFile))
                throw new FileNotFoundException("Could not find file :" + configFile);

            try
            {
                var reader = new XmlTextReader(configFile);
                _transformations = ParseConfigFile(reader);
                reader.Close();
            }
            catch (System.Exception e)
            {
                log.Error(e);
            }
        }

        private List<Transformation> ParseConfigFile(XmlTextReader reader)
        {
            var parseFinished = false;

            while (!parseFinished)
            {
                reader.Read();
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element: // The node is an element.
                        if (reader.Name.Equals("transformation"))
                        {
                            var transformation = new Transformation(reader);
                            _transformations.Add(transformation);
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        if (reader.Name.Equals("picture_transformation_configuration"))
                        {
                            parseFinished = true;
                        }
                        break;
                }
            }
            return _transformations;
        }

        public SortedSet<int> GetQuestionDependency()
        {
            //List<Int32> questionDependency = new List<int>();
            var questionDependency = new SortedSet<int>();

            foreach (var transformation in _transformations)
            {
                foreach (var layer in transformation.Layers)
                {
                    if (layer.QuestionNo != -1)
                    {
                        questionDependency.Add(layer.QuestionNo);
                    }
                }
            }

            return questionDependency;
        }

        public IList<Bitmap> TransformPictures(IRespondent respondent, int newX, int newY,
                int newWidth, int newHeight)
        {
            var transformations = GetTransformations(respondent, false);
            var bmps = new List<Bitmap>(transformations.Count);

            foreach (var t in transformations)
            {
                var layers = t.Layers;
                layers = ChangePhotoLayer(layers, newX, newY, newWidth, newHeight);
                t.Layers = layers;
                bmps.Add(t.TransformImage());
            }

            return bmps;
        }

        public List<AbstractLayer> ChangePhotoLayer(List<AbstractLayer> layers, int newX, int newY, int newWidth, int newHeight)
        {
            for (var i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (!(layer is PhotoLayer)) continue;
                ((PhotoLayer)layer).X = newX;
                ((PhotoLayer)layer).Y = newY;
                ((PhotoLayer)layer).Width = newWidth;
                ((PhotoLayer)layer).Height = newHeight;
                layers[i] = layer;
            }
            return layers;
        }

        public List<AbstractLayer> ChangePhotoLayer(List<AbstractLayer> layers, int newWidth, int newHeight)
        {
            for (var i = 0; i < layers.Count; i++)
            {
                var layer = layers[i];
                if (!(layer is PhotoLayer photoLayer)) continue;
                photoLayer.Width = newWidth;
                photoLayer.Height = newHeight;
                layers[i] = layer;
            }
            return layers;
        }

        public IList<Bitmap> TransformPictures(IRespondent respondent, bool preview)
        {
            var transformations = GetTransformations(respondent, preview);
            var bmps = new List<Bitmap>(transformations.Count);

            foreach (var t in transformations)
            {
                if (t.Respondent == null)
                {
                    t.Respondent = respondent;
                }

                bmps.Add(t.TransformImage());
            }

            return bmps;
        }

        public int GetTransformationIndex(IRespondent respondent)
        {
            var nbrTranformation = _transformations.Count;

            for (var i = 0; i < nbrTranformation; i++)
            {
                var transformation = _transformations[i];

                if (transformation.IsConditionsSatisfied(respondent))
                    return i;
            }
            return -1;
        }

        public IList<Transformation> GetTransformations(IRespondent respondent, bool preview)
        {

            var nbrTranformation = _transformations.Count;
            var transfos = new List<Transformation>();

            for (var i = 0; i < nbrTranformation; i++)
            {
                var transformation = _transformations[i];

                if (!transformation.IsConditionsSatisfied(respondent)) continue;
                
                transfos.Add(transformation);

                if ((!preview || transformation.IsPreview) && (preview || !transformation.IsPreview)) continue;
                transformation.Respondent = respondent;
                break;

            }

            if (transfos.Any()) return transfos;

            throw new NoTransformationFoundException("NoTransformationFoundException");
        }

        public void Dispose()
        {
            foreach (var t in _transformations)
            {
                t.Dispose();
            }
        }

        ~TransformationsController()
        {
            Dispose();
        }

    }
}
