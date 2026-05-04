using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using NomadLogic.Common.Util.Math;

namespace NomadLogic.Multimedia.XmlTransformation.Filters
{
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    [PublicAPI]
    public class CurvesFilter : ImageFilter
    {
        public const int RED_CURVE = 0;
        public const int GREEN_CURVE = 1;
        public const int BLUE_CURVE = 2;

        private readonly List<Tuple<float, float>> _redCurve;
        private readonly List<Tuple<float, float>> _greenCurve;
        private readonly List<Tuple<float, float>> _blueCurve;

        private Dictionary<byte, byte> _reds;
        private Dictionary<byte, byte> _greens;
        private Dictionary<byte, byte> _blues;

        private bool _beziersCalculated;

        public CurvesFilter()
        {
            _redCurve = new List<Tuple<float, float>> {new Tuple<float, float>(0, 0), new Tuple<float, float>(255, 255)};
            _greenCurve = new List<Tuple<float, float>>
            {
                new Tuple<float, float>(0, 0),
                new Tuple<float, float>(255, 255)
            };
            _blueCurve = new List<Tuple<float, float>>
            {
                new Tuple<float, float>(0, 0),
                new Tuple<float, float>(255, 255)
            };
            _reds = new Dictionary<byte, byte>();
            _greens = new Dictionary<byte, byte>();
            _blues = new Dictionary<byte, byte>();
        }

        public void AddNode(float x, float y, int curve)
        {
            switch (curve)
            {
                case RED_CURVE: _redCurve.Add(new Tuple<float, float>(x, y));
                    _redCurve.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                    break;
                case GREEN_CURVE: _greenCurve.Add(new Tuple<float, float>(x, y));
                    _greenCurve.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                    break;
                case BLUE_CURVE: _blueCurve.Add(new Tuple<float, float>(x, y));
                    _blueCurve.Sort((a, b) => a.Item1.CompareTo(b.Item1));
                    break;
            }
        }

        #region ImageFilter Members

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alpha"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <returns>format: ARGB</returns>
        protected override byte[] FilterRgb(byte alpha, byte red, byte green, byte blue)
        {
            var color = new byte[4];
            if (alpha == 0)
            {
                color[0] = 0;
                color[1] = 0;
                color[2] = 0;
                color[3] = 0;
            }
            else
            {
                color[0] = alpha;
                if (!_beziersCalculated)
                {
                    _reds = CalculateBezier(_redCurve);
                    _greens = CalculateBezier(_greenCurve);
                    _blues = CalculateBezier(_blueCurve);
                    _beziersCalculated = true;
                }
                color[1] = _reds[red];
                color[2] = _greens[green];
                color[3] = _blues[blue];
            }


            return color;
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
                            if (reader.Name.Equals("redNode"))
                            {
                                var xString = reader.GetAttribute("x");
                                var yString = reader.GetAttribute("y");

                                if (xString != null && yString != null)
                                {
                                    var x = Int32.Parse(xString);
                                    var y = Int32.Parse(yString);
                                    AddNode(x, y, RED_CURVE);
                                }
                            }
                            else if (reader.Name.Equals("greenNode"))
                            {
                                var xString = reader.GetAttribute("x");
                                var yString = reader.GetAttribute("y");

                                if (xString != null && yString != null)
                                {
                                    var x = Int32.Parse(xString);
                                    var y = Int32.Parse(yString);
                                    AddNode(x, y, GREEN_CURVE);
                                }
                            }
                            else if (reader.Name.Equals("blueNode"))
                            {
                                var xString = reader.GetAttribute("x");
                                var yString = reader.GetAttribute("y");

                                if (xString != null && yString != null)
                                {
                                    var x = Int32.Parse(xString);
                                    var y = Int32.Parse(yString);
                                    AddNode(x, y, BLUE_CURVE);
                                }
                            }
                            else if (reader.Name.Equals("rgbNode"))
                            {
                                var xString = reader.GetAttribute("x");
                                var yString = reader.GetAttribute("y");

                                if (xString != null && yString != null)
                                {
                                    var x = Int32.Parse(xString);
                                    var y = Int32.Parse(yString);
                                    AddNode(x, y, RED_CURVE);
                                    AddNode(x, y, GREEN_CURVE);
                                    AddNode(x, y, BLUE_CURVE);
                                }
                            }
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

        #endregion

        

        /// <summary>
        /// Get the 2 end points and the 2 handles of the bézier curve where the point is located.
        /// </summary>
        /// <param name="originalCurveEndPoints">The list of points for the curve.</param>
        /// <param name="position">The X position in the graph.</param>
        /// <returns>The list of points.  The order is P1, H1, H2, P2</returns>
        private List<Tuple<float, float>> GetBezierPoints(List<Tuple<float, float>> originalCurveEndPoints, int position)
        {
            var bezierCurve = new List<Tuple<float, float>>();
            Tuple<float, float> h0;
            Tuple<float, float> h1;

            var p0 = originalCurveEndPoints[position];
            var p1 = position >= originalCurveEndPoints.Count - 1 ? originalCurveEndPoints.Last() : originalCurveEndPoints[position+1];

            /*foreach (Tuple<float, float> point in originalCurveEndPoints)
            {
                if(point.Item1 <= position)
                {
                    p0 = point;
                } else if (point.Item1 >= position && p1 == null)
                {
                    p1 = point;
                }
            }*/

            var indexP0 = originalCurveEndPoints.IndexOf(p0);
            var indexP1 = originalCurveEndPoints.IndexOf(p1);

            // On est direct sur un point
            if (indexP0 == indexP1)
            {
                h0 = p0;
                h1 = p0;
            }
            else
            {

                if (indexP0 == 0)
                {
                    if (indexP1 == originalCurveEndPoints.Count - 1)
                    {
                        h0 = p0;
                        h1 = p1;
                    }
                    else
                    {
                        h1 = GetLeftHandle(p0, p1, originalCurveEndPoints[indexP1 + 1]);
                        h0 = h1;
                    }
                    
                    
                }
                else
                {
                    h0 = GetRightHandle(originalCurveEndPoints[indexP0 - 1], p0, p1);
                    h1 = indexP1 == originalCurveEndPoints.Count - 1 ? h0 : GetLeftHandle(p0, p1, originalCurveEndPoints[indexP1 + 1]);
                }
            }
            
            bezierCurve.Add(p0);
            bezierCurve.Add(h0);
            bezierCurve.Add(h1);
            bezierCurve.Add(p1);
            return bezierCurve;
        }

        private Tuple<float, float> GetRightHandle(Tuple<float, float> before, Tuple<float, float> center, Tuple<float, float> after)
        {
            // On calcule le handle sur une droite parrallèle aux deux points adjacents où la 
            // position X est à mi-chemin entre les deux points
            // y = mx + b
            // m = deltaY / deltaX
            // b = y - mx
            var deltaX = (after.Item1 - before.Item1);
            var m = (after.Item2 - before.Item2) / deltaX;
            var b = center.Item2 - (m * center.Item1);
            deltaX = (after.Item1 - center.Item1);
            var x = center.Item1 + (deltaX / 2);
            var y = m * x + b;
            return new Tuple<float, float>(x, y);
        }

        private Tuple<float, float> GetLeftHandle(Tuple<float, float> before, Tuple<float, float> center, Tuple<float, float> after)
        {
            // On calcule le handle sur une droite parrallèle aux deux points adjacents où la 
            // position X est à mi-chemin entre les deux points
            // y = mx + b
            // m = deltaY / deltaX
            // b = y - mx
            var deltaX = (after.Item1 - before.Item1);
            var m = (after.Item2 - before.Item2) / deltaX;
            var b = center.Item2 - (m * center.Item1);
            deltaX = (center.Item1 - before.Item1);
            var x = center.Item1 - (deltaX / 2);
            var y = m * x + b;
            return new Tuple<float, float>(x, y);
        }
        
        private Tuple<float, float> CalculatePoint(float t, List<Tuple<float, float>> curve, int position)
        {
            
            var bezier = GetBezierPoints(curve, position);

            var square = t * t;
            var cube = square * t;
            var t1 = (1.0f - t);
            var squareT1 = t1 * t1;
            var cubeT1 = square * t1;
            float y;
            float x;

            if(bezier[0].Item1 == bezier[1].Item1 && bezier[0].Item2 == bezier[1].Item2 && 
                bezier[2].Item1 == bezier[3].Item1 && bezier[2].Item2 == bezier[3].Item2) 
            {
                y = t1 * (bezier[0].Item2 / 255) + t * (bezier[3].Item2 / 255);
                x = t1 * (bezier[0].Item1 / 255) + t * (bezier[3].Item1 / 255);
            } 
            else if (bezier[1].Item1 == bezier[2].Item1 && bezier[1].Item2 == bezier[2].Item2)
            {
                // B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
                y = squareT1 * (bezier[0].Item2 / 255) + 2 * t1 * t * (bezier[1].Item2 / 255) + square * (bezier[3].Item2 / 255);
                x = squareT1 * (bezier[0].Item1 / 255) + 2 * t1 * t * (bezier[1].Item1 / 255) + square * (bezier[3].Item1 / 255);
            }
            else
            {
                // B(t) = (1-t)^3 * P0 + 3(1-t)^2 * t * P1 + 3(1-t) * t^2 * P2 + t^3 * P3
                y = cubeT1 * (bezier[0].Item2 / 255) + 3 * squareT1 * t * (bezier[1].Item2 / 255) + 3 * t1 * square * (bezier[2].Item2 / 255) + cube * (bezier[3].Item2 / 255);

                x = cubeT1 * (bezier[0].Item1 / 255) + 3 * squareT1 * t * (bezier[1].Item1 / 255) + 3 * t1 * square * (bezier[2].Item1 / 255) + cube * (bezier[3].Item1 / 255);
            }
            

            var point = new Tuple<float, float>(x * 255, y * 255);

            return point;
        }

        private Dictionary<byte, byte> CalculateBezier(List<Tuple<float, float>> curve)
        {
            var colors = new Dictionary<byte,byte>();

            for (var position = 0; position < curve.Count-1; position++)
            {
                for (var i = 0f; i <= 1; i += 0.002f)
                {
                    var point = CalculatePoint(i, curve, position);
                    var x = MathUtil.ColorClamp((int)(point.Item1));
                    var y = MathUtil.ColorClamp((int)(point.Item2));

                    if (!colors.ContainsKey(x))
                    {
                        colors.Add(x, y);
                    }

                }
            }
            for (byte i = 0; colors.Count < 256; i++)
            {
                if (colors.ContainsKey(i)) continue;
                switch (i)
                {
                    case 0:
                        for (byte j = 1; j <= 255 && colors.ContainsKey(i); j++)
                        {
                            if (colors.ContainsKey(j))
                            {
                                colors.Add(0, colors[j]);
                            }
                        }
                        break;
                    case 255:
                        for (byte j = 254; !colors.ContainsKey(i); j--)
                        {
                            if (colors.ContainsKey(j))
                            {
                                colors.Add(255, colors[j]);
                            }
                        }
                        break;
                    default:
                        byte prevColor = 0;
                        byte nextColor = 255;

                        for (var j = i; j <= 255 && nextColor == 255; j++)
                        {
                            if (colors.ContainsKey(j))
                            {
                                nextColor = colors[j];
                            }
                        }

                        for (var j = i; prevColor == 0; j--)
                        {
                            if (colors.ContainsKey(j))
                            {
                                prevColor = colors[j];
                            }
                        }

                        var color = (prevColor + nextColor) / 2;

                        colors.Add(i, (byte)(color));
                        break;
                }
            }
            return colors;  
        }


    }
}
