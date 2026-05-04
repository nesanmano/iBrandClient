using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using NLog;
using NomadLogic.iBrand.Common.Respondent;
using NomadLogic.iBrand.Transformation.Exception;
using NomadLogic.iBrand.Transformation.Layer;

namespace NomadLogic.iBrand.Transformation.image
{
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

        public Bitmap TransformPicture(IRespondent respondent, int newX, int newY,
                int newWidth, int newHeight)
        {
            var transformation = GetTransformation(respondent, false);

            var layers = transformation.Layers;
            layers = ChangePhotoLayer(layers, newX, newY, newWidth, newHeight);
            transformation.Layers = layers;
            return transformation.TransformImage();
        }

        public List<AbstractLayer> ChangePhotoLayer(List<AbstractLayer> layers,
                int newX, int newY, int newWidth, int newHeight)
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
                var photoLayer = layer as PhotoLayer;
                if (photoLayer == null) continue;
                photoLayer.Width = newWidth;
                photoLayer.Height = newHeight;
                layers[i] = layer;
            }
            return layers;
        }

        public Bitmap TransformPicture(IRespondent respondent, bool preview)
        {
            var transformation = GetTransformation(respondent, preview);

            if (transformation.Respondent == null)
            {
                transformation.Respondent = respondent;
            }

            return transformation.TransformImage();
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

        public Transformation GetTransformation(IRespondent respondent, bool preview)
        {

            var nbrTranformation = _transformations.Count;
            Transformation transfo = null;

            for (var i = 0; i < nbrTranformation; i++)
            {
                var transformation = _transformations[i];

                if (transformation.IsConditionsSatisfied(respondent))
                {
                    // si la transformation a l'état preview qu'on désire, on fait juste la retourner
                    if ((preview && transformation.IsPreview) || (!preview && !transformation.IsPreview))
                        return transformation;
                    // si la transformation a pas l'état preview qu'on veut mais qui réponds quand même aux conditions,
                    // on la garde en mémoire pour la retourner au cas où on trouve pas la transformation avec l'état qu'on veut
                    else if (transfo == null && ((preview && !transformation.IsPreview) || (!preview && transformation.IsPreview)))
                    {
                        transfo = transformation;
                        transfo.Respondent = respondent;
                    }
                }

            }

            if (transfo != null)
                return transfo;

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
