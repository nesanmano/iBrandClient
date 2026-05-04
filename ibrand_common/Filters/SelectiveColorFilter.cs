using NomadLogic.iBrand.Common.Util;
using System.Collections.Generic;
using System.Xml;

namespace NomadLogic.iBrand.Transformation.Filters
{
    class SelectiveColorFilter : ImageFilter
    {
        readonly List<SelectiveColorLayer> _listeLayers;

        public SelectiveColorFilter() 
        {
            _listeLayers = new List<SelectiveColorLayer>();
        }
        
        protected override byte[] FilterRgb(byte alpha, byte r, byte g, byte b)
        {
            var argb = new byte[4];
            argb[0] = alpha;

            if (alpha == 0)
            {
                argb[0] = alpha;
                argb[1] = r;
                argb[2] = g;
                argb[3] = b;
            }
            else
            {
                float rMean = 0;
                float gMean = 0;
                float bMean = 0;
                var nbModifs = 0;

                foreach (var layer in _listeLayers)
                {
                    var transformedRgb = layer.FilterColor(alpha, r, g, b);
                    if (transformedRgb[0] == alpha && transformedRgb[1] == r && transformedRgb[2] == g &&
                        transformedRgb[3] == b) continue;
                    rMean += transformedRgb[1];
                    gMean += transformedRgb[2];
                    bMean += transformedRgb[3];
                    nbModifs++;
                }
                if (nbModifs != 0)
                {
                    argb[1] = ImageUtil.ColorClamp((int)(rMean / nbModifs));
                    argb[2] = ImageUtil.ColorClamp((int)(gMean / nbModifs));
                    argb[3] = ImageUtil.ColorClamp((int)(bMean / nbModifs));
                }
                else
                {
                    argb[1] = r;
                    argb[2] = g;
                    argb[3] = b;
                }
            }
            return argb;
        }

        public override void ParseXml(XmlTextReader reader)
        {
            if (!reader.IsEmptyElement)
            {
                var filtreFinished = false;

                while (!filtreFinished)
                {
                    reader.Read();
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            SelectiveColorLayer layer = null;
                            if (reader.Name.Equals("reds"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.REDS);
                            }
                            else if (reader.Name.Equals("greens"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.GREENS);
                            }
                            else if (reader.Name.Equals("blues"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.BLUES);
                            }
                            else if (reader.Name.Equals("cyans"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.CYANS);
                            }
                            else if (reader.Name.Equals("magentas"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.MAGENTAS);
                            }
                            else if (reader.Name.Equals("yellows"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.YELLOWS);
                            }
                            else if (reader.Name.Equals("whites"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.WHITES);
                            }
                            else if (reader.Name.Equals("neutrals"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.NEUTRALS);
                            }
                            else if (reader.Name.Equals("blacks"))
                            {
                                layer = new SelectiveColorLayer(SelectiveColorLayer.BLACKS);
                            }
                            if (layer != null)
                            {
                                var stringCyan = reader.GetAttribute("cyan");
                                if (stringCyan != null && !stringCyan.Equals(""))
                                    layer.Cyan = float.Parse(stringCyan);

                                var stringMagenta = reader.GetAttribute("magenta");
                                if (stringMagenta != null && !stringMagenta.Equals(""))
                                    layer.Magenta = float.Parse(stringMagenta);

                                var stringYellow = reader.GetAttribute("yellow");
                                if (stringYellow != null && !stringYellow.Equals(""))
                                    layer.Yellow = float.Parse(stringYellow);

                                var stringBlack = reader.GetAttribute("black");
                                if (stringBlack != null && !stringBlack.Equals(""))
                                    layer.Black = float.Parse(stringBlack);
                            }
                            _listeLayers.Add(layer);

                            break;

                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("image_filter"))
                            {
                                filtreFinished = true;
                            }
                            break;
                    }
                }
            } // if has parameters
        }
    }
}
