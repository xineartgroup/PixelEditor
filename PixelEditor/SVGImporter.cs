using PixelEditor.Vector;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Xml.Linq;

namespace PixelEditor
{
    public static class SVGImporter
    {
        private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

        public static List<BaseShape> ImportSVG(string filePath, out int width, out int height)
        {
            var shapes = new List<BaseShape>();
            var doc = XDocument.Load(filePath);
            var root = doc.Root;

            width = root != null ? (int)Math.Round(ParseDimension(root.Attribute("width")?.Value ?? "", true)) : 0;
            height = root != null ? (int)Math.Round(ParseDimension(root.Attribute("height")?.Value ?? "", true)) : 0;
            var defs = root?.Element(root.Name.Namespace + "defs");

            var viewBox = root?.Attribute("viewBox")?.Value;
            if ((width == 0 || height == 0) && !string.IsNullOrEmpty(viewBox))
            {
                var parts = viewBox.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4)
                {
                    width = (int)Math.Round(ParseDimension(parts[2]));
                    height = (int)Math.Round(ParseDimension(parts[3]));
                }
            }

            Dictionary<string, PointF> definedShapes = [];
            Dictionary<string, GradientInfo> gradients = [];

            if (defs != null)
            {
                foreach (var el in defs.Elements())
                {
                    string? id = el.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(id)) continue;

                    if (el.Name.LocalName == "rect")
                    {
                        float x = (float)GetD(el, "x");
                        float y = (float)GetD(el, "y");
                        definedShapes[id] = new PointF(x, y);
                    }
                }
            }

            if (defs != null)
            {
                foreach (var el in defs.Elements())
                {
                    string? id = el.Attribute("id")?.Value;
                    if (string.IsNullOrEmpty(id)) continue;

                    if (el.Name.LocalName == "rect")
                    {
                        float x = (float)GetD(el, "x");
                        float y = (float)GetD(el, "y");
                        definedShapes[id] = new PointF(x, y);
                    }
                    else if (el.Name.LocalName == "linearGradient" || el.Name.LocalName == "radialGradient")
                    {
                        var gradient = ParseGradient(el, gradients);
                        if (gradient != null)
                        {
                            gradients[id] = gradient;
                        }
                    }
                }
            }

            foreach (var el in doc.Descendants())
            {
                if (IsInsideDefs(el))
                    continue;

                string localName = el.Name.LocalName.ToLower();
                BaseShape? shape = null;

                if (localName == "rect")
                {
                    shape = new ShapeRect
                    {
                        X = GetD(el, "x"),
                        Y = GetD(el, "y"),
                        Width = GetD(el, "width"),
                        Height = GetD(el, "height"),
                        Rx = GetD(el, "rx"),
                        Ry = GetD(el, "ry")
                    };
                }
                else if (localName == "circle")
                {
                    shape = new ShapeEllipse
                    {
                        X = GetD(el, "cx") - GetD(el, "r"),
                        Y = GetD(el, "cy") - GetD(el, "r"),
                        Width = GetD(el, "r") * 2,
                        Height = GetD(el, "r") * 2
                    };
                }
                else if (localName == "ellipse")
                {
                    shape = new ShapeEllipse
                    {
                        X = GetD(el, "cx") - GetD(el, "rx"),
                        Y = GetD(el, "cy") - GetD(el, "ry"),
                        Width = GetD(el, "rx") * 2,
                        Height = GetD(el, "ry") * 2
                    };
                }
                else if (localName == "line")
                {
                    shape = new ShapeLine
                    {
                        StartPoint = new PointF((float)GetD(el, "x1"), (float)GetD(el, "y1")),
                        EndPoint = new PointF((float)GetD(el, "x2"), (float)GetD(el, "y2"))
                    };
                }
                else if (localName == "polyline")
                {
                    shape = new ShapePolygon
                    {
                        IsClosed = false,
                        Points = ParsePts(el.Attribute("points")?.Value ?? "")
                    };
                }
                else if (localName == "polygon")
                {
                    shape = new ShapePolygon
                    {
                        IsClosed = true,
                        Points = ParsePts(el.Attribute("points")?.Value ?? "")
                    };
                }
                else if (localName == "path")
                {
                    shape = ParsePathElement(el);
                }
                else if (localName == "text")
                {
                    shape = ParseTextElement(el);
                }
                else if (localName == "g")
                {
                }

                if (shape != null)
                {
                    float visualStrokeWidth = 1.0f;
                    if (shape is ShapeText textShape)
                    {
                        visualStrokeWidth = textShape.LineWidth;
                    }
                    ApplyStyles(el, shape, gradients);
                    ApplyAllTransforms(el, shape);
                    if (shape is ShapeText textShapeD)
                    {
                        textShapeD.LineWidth *= visualStrokeWidth;
                    }
                    shapes.Add(shape);
                }
            }
            return shapes;
        }

        private static bool IsInsideDefs(XElement element)
        {
            var current = element.Parent;
            while (current != null)
            {
                if (current.Name.LocalName == "defs")
                    return true;
                current = current.Parent;
            }
            return false;
        }

        private static void ApplyAllTransforms(XElement element, BaseShape shape)
        {
            using Matrix finalMatrix = new();
            var current = element;
            var transformAttributes = new List<string>();

            while (current != null)
            {
                if (shape is not ShapeText)
                {
                    var transformAttr = current.Attribute("transform")?.Value;
                    if (!string.IsNullOrEmpty(transformAttr))
                    {
                        transformAttributes.Add(transformAttr);
                    }
                }
                current = current.Parent;
            }

            transformAttributes.Reverse();

            foreach (var transformStr in transformAttributes)
            {
                var transformParts = transformStr.Split([')'], StringSplitOptions.RemoveEmptyEntries);
                for (int i = transformParts.Length - 1; i >= 0; i--)
                {
                    var part = transformParts[i];
                    if (!part.Contains('(')) continue;

                    string function = part[..part.IndexOf('(')].Trim().ToLower();
                    string contentStr = part[(part.IndexOf('(') + 1)..];
                    string[] values = contentStr.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                    switch (function)
                    {
                        case "matrix":
                            if (values.Length >= 6)
                            {
                                float ma = float.Parse(values[0], CultureInfo.InvariantCulture);
                                float mb = float.Parse(values[1], CultureInfo.InvariantCulture);
                                float mc = float.Parse(values[2], CultureInfo.InvariantCulture);
                                float md = float.Parse(values[3], CultureInfo.InvariantCulture);
                                float me = float.Parse(values[4], CultureInfo.InvariantCulture);
                                float mf = float.Parse(values[5], CultureInfo.InvariantCulture);
                                using Matrix em = new(ma, mb, mc, md, me, mf);
                                finalMatrix.Multiply(em, MatrixOrder.Append);
                            }
                            break;
                        case "translate":
                            if (values.Length > 0)
                            {
                                float tx = float.Parse(values[0], CultureInfo.InvariantCulture);
                                float ty = values.Length > 1 ? float.Parse(values[1], CultureInfo.InvariantCulture) : 0;
                                finalMatrix.Translate(tx, ty, MatrixOrder.Append);
                            }
                            break;
                        case "scale":
                            if (values.Length > 0)
                            {
                                float sx = float.Parse(values[0], CultureInfo.InvariantCulture);
                                float sy = values.Length > 1 ? float.Parse(values[1], CultureInfo.InvariantCulture) : sx;
                                finalMatrix.Scale(sx, sy, MatrixOrder.Append);
                            }
                            break;
                        case "rotate":
                            if (values.Length > 0)
                            {
                                float angle = float.Parse(values[0], CultureInfo.InvariantCulture);
                                if (values.Length >= 3)
                                {
                                    float cx = float.Parse(values[1], CultureInfo.InvariantCulture);
                                    float cy = float.Parse(values[2], CultureInfo.InvariantCulture);
                                    finalMatrix.Translate(cx, cy, MatrixOrder.Append);
                                    finalMatrix.Rotate(angle, MatrixOrder.Append);
                                    finalMatrix.Translate(-cx, -cy, MatrixOrder.Append);
                                }
                                else
                                {
                                    finalMatrix.Rotate(angle, MatrixOrder.Append);
                                }
                            }
                            break;
                    }
                }
            }

            float[] mElements = finalMatrix.Elements;
            float scaleX = (float)Math.Sqrt(mElements[0] * mElements[0] + mElements[1] * mElements[1]);
            float scaleY = (float)Math.Sqrt(mElements[2] * mElements[2] + mElements[3] * mElements[3]);

            float angleInRadians = (float)Math.Atan2(mElements[1], mElements[0]);
            float calculatedRotation = angleInRadians * (180.0f / (float)Math.PI);

            if (shape is not ShapeText)
            {
                shape.Rotation = calculatedRotation;
            }

            if (shape is ShapeRect rect)
            {
                float origCenterX = rect.X + rect.Width / 2f;
                float origCenterY = rect.Y + rect.Height / 2f;

                PointF[] pts = [
                    new PointF(rect.X, rect.Y),
        new PointF(origCenterX, origCenterY)
                ];
                finalMatrix.TransformPoints(pts);

                rect.X = pts[1].X - rect.Width / 2f;
                rect.Y = pts[1].Y - rect.Height / 2f;
                rect.Width *= scaleX;
                rect.Height *= scaleY;

                rect.RotationCenterX = pts[1].X;
                rect.RotationCenterY = pts[1].Y;
                rect.HasCustomRotationCenter = true;
            }
            else if (shape is ShapeEllipse ellipse)
            {
                float origCenterX = ellipse.X + ellipse.Width / 2f;
                float origCenterY = ellipse.Y + ellipse.Height / 2f;

                PointF[] pts = [
                    new PointF(ellipse.X, ellipse.Y),
        new PointF(origCenterX, origCenterY)
                ];
                finalMatrix.TransformPoints(pts);

                ellipse.X = pts[1].X - ellipse.Width / 2f;
                ellipse.Y = pts[1].Y - ellipse.Height / 2f;
                ellipse.Width *= scaleX;
                ellipse.Height *= scaleY;

                ellipse.RotationCenterX = pts[1].X;
                ellipse.RotationCenterY = pts[1].Y;
                ellipse.HasCustomRotationCenter = true;
            }
            else if (shape is ShapeLine line)
            {
                PointF[] pts = [line.StartPoint, line.EndPoint];
                finalMatrix.TransformPoints(pts);
                line.StartPoint = pts[0];
                line.EndPoint = pts[1];
            }
            else if (shape is ShapePolygon polygon)
            {
                if (polygon.Points.Count > 0)
                {
                    PointF[] pts = [.. polygon.Points];
                    finalMatrix.TransformPoints(pts);
                    for (int i = 0; i < pts.Length; i++)
                    {
                        polygon.Points[i] = pts[i];
                    }
                }
            }
            else if (shape is ShapePath path)
            {
                for (int i = 0; i < path.PathSegments.Count; i++)
                {
                    if (path.PathSegments[i].InputPoints.Count > 0)
                    {
                        PointF[] pts = [.. path.PathSegments[i].InputPoints];
                        finalMatrix.TransformPoints(pts);
                        for (int j = 0; j < pts.Length; j++)
                        {
                            path.PathSegments[i].InputPoints[j] = pts[j];
                        }
                    }
                }
            }
            else if (shape is ShapeText text)
            {
                ////Already handled translation, rotation and scaling for text
                //PointF[] pts = [new PointF(text.X, text.Y)];
                //finalMatrix.TransformPoints(pts);
                //text.X = pts[0].X;
                //text.Y = pts[0].Y;
            }
        }

        private static ShapePath ParsePathElement(XElement el)
        {
            var shape = new ShapePath();
            string d = el.Attribute("d")?.Value ?? "";
            var tokens = TokenizePathData(d);

            float cx = 0, cy = 0, startX = 0, startY = 0;
            float lastCtrlX = 0, lastCtrlY = 0;
            char currentCmd = ' ';
            int i = 0;

            while (i < tokens.Count)
            {
                string token = tokens[i];
                if (char.IsLetter(token[0]) && token[0] != '-' && token[0] != '.')
                {
                    currentCmd = token[0];
                    i++;
                }

                char cmdType = char.ToUpper(currentCmd);
                bool isRel = char.IsLower(currentCmd);
                var p = new PathSegment { PathType = cmdType.ToString() };

                p.InputPoints.Add(new PointF(cx, cy));

                switch (cmdType)
                {
                    case 'M':
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        startX = cx; startY = cy;
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        currentCmd = isRel ? 'l' : 'L';
                        break;

                    case 'L':
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        currentCmd = isRel ? 'l' : 'L';
                        break;

                    case 'H':
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        break;

                    case 'V':
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        break;

                    case 'C':
                        float cp1x = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        float cp1y = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        float cp2x = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        float cp2y = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)cp1x, (float)cp1y));
                        p.InputPoints.Add(new PointF((float)cp2x, (float)cp2y));
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        lastCtrlX = cp2x; lastCtrlY = cp2y;
                        break;

                    case 'S':
                        float sCp1x = (shape.PathSegments.Count > 0 && "CS".Contains(shape.PathSegments.Last().PathType))
                                        ? 2 * cx - lastCtrlX : cx;
                        float sCp1y = (shape.PathSegments.Count > 0 && "CS".Contains(shape.PathSegments.Last().PathType))
                                        ? 2 * cy - lastCtrlY : cy;
                        float sCp2x = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        float sCp2y = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)sCp1x, (float)sCp1y));
                        p.InputPoints.Add(new PointF((float)sCp2x, (float)sCp2y));
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        lastCtrlX = sCp2x; lastCtrlY = sCp2y;
                        break;

                    case 'Q':
                        float qCp1x = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        float qCp1y = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)qCp1x, (float)qCp1y));
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        lastCtrlX = qCp1x; lastCtrlY = qCp1y;
                        break;

                    case 'T':
                        float tCp1x = (shape.PathSegments.Count > 0 && "QT".Contains(shape.PathSegments.Last().PathType))
                                        ? 2 * cx - lastCtrlX : cx;
                        float tCp1y = (shape.PathSegments.Count > 0 && "QT".Contains(shape.PathSegments.Last().PathType))
                                        ? 2 * cy - lastCtrlY : cy;
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)tCp1x, (float)tCp1y));
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        lastCtrlX = tCp1x; lastCtrlY = tCp1y;
                        break;

                    case 'A':
                        p.InputPoints.Add(new PointF((float)ParseD(tokens[i++]), (float)ParseD(tokens[i++])));
                        p.InputPoints.Add(new PointF((float)ParseD(tokens[i++]), 0));
                        p.InputPoints.Add(new PointF((float)ParseD(tokens[i++]), (float)ParseD(tokens[i++])));
                        cx = isRel ? cx + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        cy = isRel ? cy + ParseD(tokens[i++]) : ParseD(tokens[i++]);
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        break;

                    case 'Z':
                        cx = startX; cy = startY;
                        p.InputPoints.Add(new PointF((float)cx, (float)cy));
                        currentCmd = ' ';

                        break;

                    default:
                        i++;
                        continue;
                }

                shape.PathSegments.Add(p);

                //if (i < tokens.Count && !char.IsLetter(tokens[i][0]))
                //{
                //    if (cmdType == 'Z') i++;
                //}
            }
            //foreach (var seg in shape.PathSegments)
            //{
            //    Console.WriteLine($"Segment {seg}.");
            //}
            return shape;
        }

        private static List<string> TokenizePathData(string d)
        {
            var tokens = new List<string>();
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < d.Length; i++)
            {
                char c = d[i];

                if (char.IsLetter(c))
                {
                    // Handle scientific notation (e.g., 1e-5)
                    if ((c == 'e' || c == 'E') && sb.Length > 0 && char.IsDigit(sb[^1]))
                    {
                        sb.Append(c);
                        // Peek ahead for the sign of the exponent
                        if (i + 1 < d.Length && (d[i + 1] == '-' || d[i + 1] == '+'))
                        {
                            sb.Append(d[i + 1]);
                            i++;
                        }
                        continue;
                    }

                    if (sb.Length > 0) tokens.Add(sb.ToString());
                    tokens.Add(c.ToString());
                    sb.Clear();
                }
                else if (c == '-' || c == ',' || char.IsWhiteSpace(c))
                {
                    if (sb.Length > 0) tokens.Add(sb.ToString());
                    sb.Clear();
                    if (c == '-') sb.Append(c); // Start a new negative number
                }
                else if (c == '.' || char.IsDigit(c))
                {
                    // Handle the "float decimal" case: 10.5.5 -> 10.5 and .5
                    if (c == '.' && sb.ToString().Contains('.'))
                    {
                        tokens.Add(sb.ToString());
                        sb.Clear();
                    }
                    sb.Append(c);
                }
            }
            if (sb.Length > 0) tokens.Add(sb.ToString());
            return tokens;
        }

        private static void ApplyStyles(XElement el, BaseShape s, Dictionary<string, GradientInfo> gradients)
        {
            try
            {
                string style = el.Attribute("style")?.Value ?? "";
                s.LineWidth = 1.0f; // Default line width
                if (!string.IsNullOrEmpty(style))
                {
                    var styleParts = style.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var part in styleParts)
                    {
                        var kv = part.Split(':', 2);
                        if (kv.Length != 2) continue;

                        string k = kv[0].Trim();
                        string v = kv[1].Trim();

                        if (k == "stroke")
                        {
                            if (v.StartsWith("url(#"))
                            {
                                string gradientId = ExtractGradientId(v);

                                if (!string.IsNullOrEmpty(gradientId) &&
                                    gradients.TryGetValue(gradientId, out GradientInfo? gradient))
                                {
                                    s.GradientStroke = gradient;  // ← Need separate property
                                    s.LineColor = gradient.Stops.Count > 0 ? gradient.Stops[0].Color : Color.Black;
                                }
                                else
                                {
                                    s.LineColor = Color.Black;
                                }
                            }
                            else if (v != "none")
                            {
                                try
                                {
                                    s.LineColor = ColorTranslator.FromHtml(v);
                                }
                                catch
                                {
                                    s.LineColor = Color.Black;
                                }
                            }
                        }

                        if (k == "stroke-width")
                        {
                            if (float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float sw))
                            {
                                s.LineWidth = (float)sw;
                            }
                        }

                        if (k == "fill")
                        {
                            if (v == "none")
                            {
                                s.FillColor = Color.Transparent;
                            }
                            else if (v.StartsWith("url(#"))
                            {
                                string gradientId = ExtractGradientId(v);

                                if (!string.IsNullOrEmpty(gradientId) &&
                                    gradients.TryGetValue(gradientId, out GradientInfo? gradient))
                                {
                                    s.GradientFill = gradient;  // ← Store the actual gradient
                                    s.HasGradientFill = true;
                                    s.FillColor = gradient.Stops.Count > 0 ? gradient.Stops[0].Color : Color.Gray;
                                }
                            }
                            else
                            {
                                try
                                {
                                    s.FillColor = ColorTranslator.FromHtml(v);
                                }
                                catch
                                {
                                    s.FillColor = Color.Black;
                                }
                            }
                        }

                        if (k == "fill-opacity" && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float fo))
                            s.Opacity = fo;
                        if (k == "stroke-opacity" && float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float so))
                            s.StrokeOpacity = so;
                    }
                }

                // Handle fill attribute
                var fillAttr = el.Attribute("fill");
                if (fillAttr != null)
                {
                    string v = fillAttr.Value;
                    if (v.StartsWith("url(#"))
                    {
                        string gradientId = ExtractGradientId(v);

                        if (!string.IsNullOrEmpty(gradientId) &&
                            gradients.TryGetValue(gradientId, out GradientInfo? gradient))
                        {
                            s.GradientFill = gradient;
                            s.HasGradientFill = true;
                            s.FillColor = gradient.Stops.Count > 0 ? gradient.Stops[0].Color : Color.Gray;
                        }
                    }
                    else if (v == "none")
                    {
                        s.FillColor = Color.Transparent;
                    }
                    else
                    {
                        try
                        {
                            s.FillColor = ColorTranslator.FromHtml(v);
                        }
                        catch
                        {
                            s.FillColor = Color.Black;
                        }
                    }
                }

                // Handle stroke attribute
                var strokeAttr = el.Attribute("stroke");
                if (strokeAttr != null && strokeAttr.Value != "none")
                {
                    try
                    {
                        s.LineColor = ColorTranslator.FromHtml(strokeAttr.Value);
                    }
                    catch
                    {
                        s.LineColor = Color.Black;
                    }
                }

                var strokeWidthAttr = el.Attribute("stroke-width");
                if (strokeWidthAttr != null)
                {
                    if (float.TryParse(strokeWidthAttr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float sw))
                    {
                        s.LineWidth = (float)sw;
                    }
                }
            }
            catch { }
        }

        private static string ExtractGradientId(string urlValue)
        {
            int hashIndex = urlValue.IndexOf('#');
            if (hashIndex >= 0)
            {
                int closeParen = urlValue.IndexOf(')', hashIndex);
                if (closeParen > hashIndex)
                {
                    return urlValue.Substring(hashIndex + 1, closeParen - hashIndex - 1);
                }
            }
            return string.Empty;
        }

        private static float ParseD(string s)
        {
            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out float d))
            {
                return d;
            }

            return 0;
        }

        private static float GetD(XElement el, string a)
        {
            var val = el?.Attribute(a)?.Value;
            if (val == null)
                return 0;
            return ParseDimension(val);
        }

        private static List<PointF> ParsePts(string v)
        {
            var points = new List<PointF>();
            if (string.IsNullOrWhiteSpace(v)) return points;

            string[] coords = v.Split([' ', ',', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i + 1 < coords.Length; i += 2)
            {
                if (float.TryParse(coords[i], NumberStyles.Any, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(coords[i + 1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y))
                {
                    points.Add(new PointF(x, y));
                }
            }

            return points;
        }

        private static float ParseDimension(string v, bool ignoreFactor = false)
        {
            if (string.IsNullOrEmpty(v)) return 0;
            float factor = 1.0f;
            if (v.EndsWith("mm"))
            {
                factor = 3.7795275591f; v = v[..^2];
            }
            else if (v.EndsWith("cm"))
            {
                factor = 37.795275591f; v = v[..^2];
            }
            else if (v.EndsWith("pt"))
            {
                factor = 1.3333333333f; v = v[..^2];
            }
            else if (v.EndsWith("px"))
            {
                v = v[..^2];
                factor = 1.0f;
            }
            else if (v.EndsWith("cm")) { factor = 37.795275591f; v = v[..^2]; }
            else if (v.EndsWith("pt")) { factor = 1.3333333333f; v = v[..^2]; }

            if (ignoreFactor)
            {
                factor = 1.0f;
            }

            return float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float res) ? res * factor : 0;
        }

        private static ShapeText? ParseTextElement(XElement el)
        {
            XElement? tspan = null;
            XElement? current = el;
            while (true)
            {
                var childTspan = current.Elements(current.Name.Namespace + "tspan").FirstOrDefault();
                if (childTspan == null) break;
                tspan = childTspan;
                current = childTspan;
            }

            string content = el.Value.Trim();
            if (tspan != null && !string.IsNullOrEmpty(tspan.Value.Trim()))
            {
                content = tspan.Value.Trim();
            }
            else if (string.IsNullOrEmpty(content) && tspan != null)
            {
                content = tspan.Value.Trim();
            }

            if (string.IsNullOrEmpty(content)) return null;

            float x = 0f, y = 0f, width = 0f, height = 0f;
            bool hasDefinedBounds = false;
            float fontSize = 12f;
            string fontFamily = "Arial";
            bool isBold = false;
            bool isItalic = false;

            string style = el.Attribute("style")?.Value ?? "";

            var tspanChain = new List<XElement>();
            current = tspan;
            while (current != null && current != el)
            {
                tspanChain.Insert(0, current);
                current = current.Parent;
            }

            foreach (var tspanElement in tspanChain)
            {
                string tspanStyle = tspanElement.Attribute("style")?.Value ?? "";
                if (!string.IsNullOrEmpty(tspanStyle))
                {
                    var styleParts = style.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
                    var tspanParts = tspanStyle.Split(';', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var tspanPart in tspanParts)
                    {
                        var kv = tspanPart.Split(':', 2);
                        if (kv.Length == 2)
                        {
                            string key = kv[0].Trim();
                            for (int i = 0; i < styleParts.Count; i++)
                            {
                                if (styleParts[i].TrimStart().StartsWith(key + ":"))
                                {
                                    styleParts.RemoveAt(i);
                                    break;
                                }
                            }
                            styleParts.Add(tspanPart);
                        }
                    }
                    style = string.Join(";", styleParts);
                }
            }

            int fontSizeIndex = style.IndexOf("font-size:");
            if (fontSizeIndex != -1)
            {
                int start = fontSizeIndex + 10;
                int end = style.IndexOf(';', start);
                if (end == -1) end = style.Length;
                string fontSizeStr = style[start..end].Trim();
                fontSize = ParseDimension(fontSizeStr);
            }

            int fontFamilyIdx = style.IndexOf("font-family:");
            if (fontFamilyIdx != -1)
            {
                int start = fontFamilyIdx + 12;
                int end = style.IndexOf(';', start);
                if (end == -1) end = style.Length;
                fontFamily = style[start..end].Trim().Replace("'", "").Replace("\"", "");
            }

            if (style.Contains("font-weight: bold") || style.Contains("font-weight:700") ||
                style.Contains("font-weight:800") || style.Contains("font-weight:900"))
            {
                isBold = true;
            }

            if (style.Contains("font-style: italic"))
            {
                isItalic = true;
            }

            var fontSizeAttr = el.Attribute("font-size");
            var fontFamilyAttr = el.Attribute("font-family");
            var fontWeightAttr = el.Attribute("font-weight");
            var fontStyleAttr = el.Attribute("font-style");

            foreach (var tspanElement in tspanChain.AsEnumerable().Reverse())
            {
                fontSizeAttr ??= tspanElement.Attribute("font-size");
                fontFamilyAttr ??= tspanElement.Attribute("font-family");
                fontWeightAttr ??= tspanElement.Attribute("font-weight");
                fontStyleAttr ??= tspanElement.Attribute("font-style");
            }

            if (fontSizeAttr != null) fontSize = ParseDimension(fontSizeAttr.Value);
            if (fontFamilyAttr != null) fontFamily = fontFamilyAttr.Value;
            if (fontWeightAttr != null)
            {
                string fw = fontWeightAttr.Value;
                isBold = fw == "bold" || fw == "700" || fw == "800" || fw == "900";
            }
            if (fontStyleAttr != null) isItalic = fontStyleAttr.Value == "italic";

            XElement sourceEl = tspan ?? el;

            int shapeInsideIdx = style.IndexOf("shape-inside");
            if (shapeInsideIdx != -1)
            {
                int urlStart = style.IndexOf("url(", shapeInsideIdx);
                if (urlStart != -1)
                {
                    int hashStart = style.IndexOf('#', urlStart);
                    int parenEnd = style.IndexOf(')', urlStart);

                    if (hashStart != -1 && parenEnd != -1 && hashStart < parenEnd)
                    {
                        string rectId = style.Substring(hashStart + 1, parenEnd - hashStart - 1).Trim();
                        XElement? defsRect = el.Document?.Descendants(el.Name.Namespace + "rect")
                            .FirstOrDefault(r => r.Attribute("id")?.Value == rectId);

                        if (defsRect != null)
                        {
                            x = GetD(defsRect, "x");
                            y = GetD(defsRect, "y");
                            width = GetD(defsRect, "width");
                            height = GetD(defsRect, "height");
                            hasDefinedBounds = true;
                        }
                    }
                }
            }

            if (hasDefinedBounds)
            {
                XElement? coordEl = tspan;
                while (coordEl != null && coordEl != el.Parent && coordEl.Attribute("x") == null && coordEl.Attribute("y") == null)
                {
                    coordEl = coordEl.Parent;
                }

                if (coordEl != null && (coordEl.Attribute("x") != null || coordEl.Attribute("y") != null))
                {
                    x = GetD(coordEl, "x");
                    y = GetD(coordEl, "y");
                    hasDefinedBounds = false;
                }
            }
            else
            {
                x = GetD(sourceEl, "x");
                y = GetD(sourceEl, "y");
            }

            string dominantBaseline = el.Attribute("dominant-baseline")?.Value ?? "";
            if (string.IsNullOrEmpty(dominantBaseline) && style.Contains("dominant-baseline:"))
            {
                int dbStart = style.IndexOf("dominant-baseline:");
                if (dbStart != -1)
                {
                    int dbValStart = dbStart + 18;
                    int dbEnd = style.IndexOf(';', dbValStart);
                    if (dbEnd == -1) dbEnd = style.Length;
                    dominantBaseline = style[dbValStart..dbEnd].Trim();
                }
            }

            float matrixScaleX = 1.0f;
            float matrixScaleY = 1.0f;
            float geoX = x;
            float geoY = y;
            float calculatedRotation = 0.0f;

            string transformStr = el.Attribute("transform")?.Value ?? "";
            using (Matrix matrix = new ())
            {
                if (!string.IsNullOrWhiteSpace(transformStr))
                {
                    var transformParts = transformStr.Split([')'], StringSplitOptions.RemoveEmptyEntries);
                    for (int i = transformParts.Length - 1; i >= 0; i--)
                    {
                        var part = transformParts[i];
                        if (!part.Contains('(')) continue;

                        string function = part[..part.IndexOf('(')].Trim().ToLower();
                        string contentStr = part[(part.IndexOf('(') + 1)..];
                        string[] values = contentStr.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                        switch (function)
                        {
                            case "matrix":
                                if (values.Length >= 6)
                                {
                                    float ma = float.Parse(values[0], CultureInfo.InvariantCulture);
                                    float mb = float.Parse(values[1], CultureInfo.InvariantCulture);
                                    float mc = float.Parse(values[2], CultureInfo.InvariantCulture);
                                    float md = float.Parse(values[3], CultureInfo.InvariantCulture);
                                    float me = float.Parse(values[4], CultureInfo.InvariantCulture);
                                    float mf = float.Parse(values[5], CultureInfo.InvariantCulture);
                                    using Matrix em = new(ma, mb, mc, md, me, mf);
                                    matrix.Multiply(em, MatrixOrder.Append);
                                }
                                break;
                            case "translate":
                                if (values.Length > 0)
                                {
                                    float tx = float.Parse(values[0], CultureInfo.InvariantCulture);
                                    float ty = values.Length > 1 ? float.Parse(values[1], CultureInfo.InvariantCulture) : 0;
                                    matrix.Translate(tx, ty, MatrixOrder.Append);
                                }
                                break;
                            case "scale":
                                if (values.Length > 0)
                                {
                                    float sx = float.Parse(values[0], CultureInfo.InvariantCulture);
                                    float sy = values.Length > 1 ? float.Parse(values[1], CultureInfo.InvariantCulture) : sx;
                                    matrix.Scale(sx, sy, MatrixOrder.Append);
                                }
                                break;
                            case "rotate":
                                if (values.Length > 0)
                                {
                                    float angle = float.Parse(values[0], CultureInfo.InvariantCulture);
                                    if (values.Length >= 3)
                                    {
                                        float cx = float.Parse(values[1], CultureInfo.InvariantCulture);
                                        float cy = float.Parse(values[2], CultureInfo.InvariantCulture);
                                        matrix.Translate(cx, cy, MatrixOrder.Append);
                                        matrix.Rotate(angle, MatrixOrder.Append);
                                        matrix.Translate(-cx, -cy, MatrixOrder.Append);
                                    }
                                    else
                                    {
                                        matrix.Rotate(angle, MatrixOrder.Append);
                                    }
                                }
                                break;
                        }
                    }
                }

                float[] mElements = matrix.Elements;
                matrixScaleX = (float)Math.Sqrt(mElements[0] * mElements[0] + mElements[1] * mElements[1]);
                matrixScaleY = (float)Math.Sqrt(mElements[2] * mElements[2] + mElements[3] * mElements[3]);

                PointF[] points = [new PointF(x, y)];
                matrix.TransformPoints(points);
                geoX = points[0].X;
                geoY = points[0].Y;

                float angleInRadians = (float)Math.Atan2(mElements[1], mElements[0]);
                calculatedRotation = angleInRadians * (180.0f / (float)Math.PI);

                Console.WriteLine($"Matrix elements: {string.Join(", ", matrix.Elements.Select(e => e.ToString("F4")))}");
                Console.WriteLine($"tspan x={GetD(sourceEl, "x")} y={GetD(sourceEl, "y")}");
                Console.WriteLine($"Raw x={x} y={y} before transform");
                PointF[] testPt = [new PointF(x, y)];
                matrix.TransformPoints(testPt);
                Console.WriteLine($"After transform: {testPt[0].X}, {testPt[0].Y}");
            }

            float geoWidth = width * matrixScaleX;
            float geoHeight = height * matrixScaleY;

            float innerX, innerY, innerWidth, innerHeight;

            using (GraphicsPath path = new())
            {
                FontStyle fontStyle = FontStyle.Regular;
                if (isBold) fontStyle |= FontStyle.Bold;
                if (isItalic) fontStyle |= FontStyle.Italic;

                FontFamily family;
                try
                {
                    family = new FontFamily(fontFamily);
                }
                catch
                {
                    family = FontFamily.GenericSansSerif;
                }

                int emHeight = family.GetEmHeight(fontStyle);
                int cellAscent = family.GetCellAscent(fontStyle);

                path.AddString(content, family, (int)fontStyle, fontSize, new PointF(0, 0), StringFormat.GenericTypographic);
                RectangleF rawGlyphBounds = path.GetBounds();

                float scaledGlyphWidth = rawGlyphBounds.Width * matrixScaleX;
                float scaledGlyphHeight = rawGlyphBounds.Height * matrixScaleY;

                float fontAscent = fontSize * cellAscent / emHeight;

                float marginX = rawGlyphBounds.X * matrixScaleX;
                float marginY = scaledGlyphHeight - (fontAscent * matrixScaleY) + (rawGlyphBounds.Y * matrixScaleY);

                if (!string.IsNullOrEmpty(dominantBaseline))
                {
                    if (dominantBaseline.Equals("hanging", StringComparison.CurrentCultureIgnoreCase))
                    {
                        innerY = geoY - marginY;
                    }
                    else if (dominantBaseline.Equals("middle", StringComparison.CurrentCultureIgnoreCase))
                    {
                        innerY = geoY - scaledGlyphHeight / 2 - marginY;
                    }
                    else if (dominantBaseline.Equals("central", StringComparison.CurrentCultureIgnoreCase))
                    {
                        innerY = geoY - scaledGlyphHeight / 2;
                    }
                    else
                    {
                        innerY = geoY + marginY - scaledGlyphHeight;
                    }
                }
                else
                {
                    innerY = geoY + marginY - scaledGlyphHeight;
                }

                innerX = geoX + marginX;
                innerWidth = scaledGlyphWidth;
                innerHeight = scaledGlyphHeight;

                Console.WriteLine($"rawGlyphBounds: X={rawGlyphBounds.X} Y={rawGlyphBounds.Y} W={rawGlyphBounds.Width} H={rawGlyphBounds.Height}");
                Console.WriteLine($"fontAscent={fontAscent}");
                Console.WriteLine($"scaledGlyphWidth={scaledGlyphWidth} scaledGlyphHeight={scaledGlyphHeight}");
                Console.WriteLine($"marginX={marginX} marginY={marginY}");
                Console.WriteLine($"innerX={innerX} innerY={innerY}");
            }

            string unit = "px";
            XElement? svgEl = el.Document?.Root;
            if (svgEl != null)
            {
                string svgWidthAttr = svgEl.Attribute("width")?.Value ?? "";
                if (svgWidthAttr.EndsWith("mm")) unit = "mm";
                else if (svgWidthAttr.EndsWith("cm")) unit = "cm";
                else if (svgWidthAttr.EndsWith("in")) unit = "in";
                else if (svgWidthAttr.EndsWith("pt")) unit = "pt";
                else if (svgWidthAttr.EndsWith("pc")) unit = "pc";
            }

            float textCenterX = geoX; // already the transformed position of the text origin
            float textCenterY = geoY;

            var text = new ShapeText
            {
                Content = content,
                MeasurementUnit = unit,
                X = innerX,
                Y = innerY,
                Width = innerWidth,
                Height = innerHeight,
                FontSize = fontSize,
                TransformScale = Math.Min(matrixScaleX, matrixScaleY),
                FontFamily = fontFamily,
                IsBold = isBold,
                IsItalic = isItalic,
                Rotation = calculatedRotation,
                RotationCenterX = geoX,
                RotationCenterY = geoY,
                HasCustomRotationCenter = true
            };

            text.FontSize *= Math.Min(matrixScaleX, matrixScaleY);
            text.LineWidth = ((matrixScaleX + matrixScaleY) / 2.0f);

            Console.WriteLine($"geoX={geoX} geoY={geoY}");
            Console.WriteLine($"innerX={innerX} innerY={innerY}");
            Console.WriteLine($"innerWidth={innerWidth} innerHeight={innerHeight}");
            Console.WriteLine($"matrixScaleX={matrixScaleX} matrixScaleY={matrixScaleY}");
            Console.WriteLine($"calculatedRotation={calculatedRotation}");

            return text;
        }

        private static void ParseGradientTransform(string transform, GradientInfo gradient)
        {
            if (string.IsNullOrWhiteSpace(transform)) return;

            if (transform.Contains("matrix"))
            {
                try
                {
                    int start = transform.IndexOf('(');
                    if (start < 0) return;

                    int end = transform.IndexOf(')', start + 1);
                    if (end < 0) return;

                    start += 1;
                    string content = transform[start..end];
                    string[] parts = content.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 6)
                    {
                        if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float a))
                            gradient.TransformA = a;
                        if (float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                            gradient.TransformB = b;
                        if (float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float c))
                            gradient.TransformC = c;
                        if (float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float d))
                            gradient.TransformD = d;
                        if (float.TryParse(parts[4], NumberStyles.Float, CultureInfo.InvariantCulture, out float e))
                            gradient.TransformE = e;
                        if (float.TryParse(parts[5], NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
                            gradient.TransformF = f;

                        gradient.HasTransform = true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to parse gradient transform: {transform}. {ex.Message}");
                }
            }
        }

        private static GradientInfo? ParseGradient(XElement el, Dictionary<string, GradientInfo> allGradients)
        {
            string? id = el.Attribute("id")?.Value;
            if (string.IsNullOrEmpty(id)) return null;

            Console.WriteLine($"Parsing gradient: '{id}'");

            var xlinkRef = el.Attribute(XName.Get("href", "http://www.w3.org/1999/xlink"))?.Value;
            GradientInfo? baseGradient = null;

            if (!string.IsNullOrEmpty(xlinkRef) && xlinkRef.StartsWith('#'))
            {
                string baseId = xlinkRef[1..];
                Console.WriteLine($"Gradient '{id}' references base gradient '{baseId}'");

                if (allGradients.TryGetValue(baseId, out baseGradient))
                {
                    Console.WriteLine($"Found base gradient '{baseId}'");
                }
            }

            var gradient = new GradientInfo
            {
                IsRadial = el.Name.LocalName == "radialGradient",
                Id = id
            };

            if (baseGradient != null)
            {
                gradient.Stops = [.. baseGradient.Stops];
                Console.WriteLine($"Copied {gradient.Stops.Count} stops from base gradient");
            }

            if (gradient.IsRadial)
            {
                gradient.Cx = GetD(el, "cx");
                gradient.Cy = GetD(el, "cy");
                gradient.R = GetD(el, "r");
                gradient.Fx = GetD(el, "fx");
                gradient.Fy = GetD(el, "fy");

                if (gradient.Fx == 0) gradient.Fx = gradient.Cx;
                if (gradient.Fy == 0) gradient.Fy = gradient.Cy;
            }
            else
            {
                gradient.X1 = GetD(el, "x1");
                gradient.Y1 = GetD(el, "y1");
                gradient.X2 = GetD(el, "x2");
                gradient.Y2 = GetD(el, "y2");
            }

            var transform = el.Attribute("gradientTransform")?.Value;
            if (!string.IsNullOrEmpty(transform))
            {
                ParseGradientTransform(transform, gradient);
            }

            var units = el.Attribute("gradientUnits")?.Value;
            gradient.UserSpaceOnUse = units == "userSpaceOnUse";

            foreach (var stopEl in el.Elements(el.Name.Namespace + "stop"))
            {
                var stop = new GradientStop
                {
                    Offset = ParseOffset(stopEl.Attribute("offset")?.Value)
                };

                var style = stopEl.Attribute("style")?.Value;
                if (!string.IsNullOrEmpty(style))
                {
                    foreach (var part in style.Split(';'))
                    {
                        var kv = part.Split(':');
                        if (kv.Length != 2) continue;
                        string k = kv[0].Trim();
                        string v = kv[1].Trim();

                        if (k == "stop-color")
                        {
                            try
                            {
                                stop.Color = ColorTranslator.FromHtml(v);
                            }
                            catch
                            {
                                stop.Color = Color.Black;
                            }
                        }
                        else if (k == "stop-opacity")
                        {
                            if (float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float opacity1))
                                stop.Color = Color.FromArgb((int)(opacity1 * 255), stop.Color);
                        }
                    }
                }

                var colorAttr = stopEl.Attribute("stop-color");
                if (colorAttr != null)
                {
                    try
                    {
                        stop.Color = ColorTranslator.FromHtml(colorAttr.Value);
                    }
                    catch { }
                }

                var opacityAttr = stopEl.Attribute("stop-opacity");
                if (opacityAttr != null && float.TryParse(opacityAttr.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out float opacity))
                {
                    stop.Color = Color.FromArgb((int)(opacity * 255), stop.Color);
                }

                gradient.Stops.Add(stop);
            }

            gradient.Stops = [.. gradient.Stops.OrderBy(s => s.Offset)];

            Console.WriteLine($"Finished gradient '{id}'. Final stops: {gradient.Stops.Count}");
            return gradient.Stops.Count > 0 ? gradient : null;
        }

        private static float ParseOffset(string? offsetValue)
        {
            if (string.IsNullOrEmpty(offsetValue)) return 0;

            if (offsetValue.EndsWith('%'))
            {
                if (float.TryParse(offsetValue.TrimEnd('%'), NumberStyles.Any, CultureInfo.InvariantCulture, out float percent))
                    return percent / 100f;
            }
            else
            {
                if (float.TryParse(offsetValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float value))
                    return value;
            }

            return 0;
        }
    }
}