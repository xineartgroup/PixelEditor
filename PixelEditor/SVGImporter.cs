using PixelEditor.Vector;
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

            width = root != null ? (int)Math.Round(ParseDimension(root.Attribute("width")?.Value ?? "")) : 0;
            height = root != null ? (int)Math.Round(ParseDimension(root.Attribute("height")?.Value ?? "")) : 0;
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
                    shape = ParseTextElement(el, gradients);
                }
                else if (localName == "g")
                {
                }

                if (shape != null)
                {
                    // Text elements handle their own style application (including tspan overrides) in ParseTextElement
                    if (shape is not ShapeText)
                    {
                        ApplyStyles(el, shape, gradients);
                    }
                    ApplyGroupTransforms(el, shape);
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

        private static void ApplyGroupTransforms(XElement element, BaseShape shape)
        {
            var transforms = new List<(float scaleX, float scaleY, float offsetX, float offsetY)>();

            var current = element.Parent;
            while (current != null)
            {
                if (current.Name.LocalName == "g")
                {
                    var transformAttr = current.Attribute("transform")?.Value;
                    if (!string.IsNullOrEmpty(transformAttr))
                    {
                        ApplyFullMatrixTransform(transformAttr, out float sx, out float sy, out float ox, out float oy);
                        transforms.Add((sx, sy, ox, oy));
                    }
                }
                current = current.Parent;
            }

            transforms.Reverse();

            foreach (var (scaleX, scaleY, offsetX, offsetY) in transforms)
            {
                if (shape is ShapeRect rect)
                {
                    rect.X = (rect.X * scaleX) + offsetX;
                    rect.Y = (rect.Y * scaleY) + offsetY;
                    rect.Width *= scaleX;
                    rect.Height *= scaleY;
                    rect.Rx *= scaleX;
                    rect.Ry *= scaleY;
                }
                else if (shape is ShapeEllipse ellipse)
                {
                    ellipse.X = (ellipse.X * scaleX) + offsetX;
                    ellipse.Y = (ellipse.Y * scaleY) + offsetY;
                    ellipse.Width *= scaleX;
                    ellipse.Height *= scaleY;
                }
                else if (shape is ShapeLine line)
                {
                    line.StartPoint = new PointF(
                        (float)((line.StartPoint.X * scaleX) + offsetX),
                        (float)((line.StartPoint.Y * scaleY) + offsetY));
                    line.EndPoint = new PointF(
                        (float)((line.EndPoint.X * scaleX) + offsetX),
                        (float)((line.EndPoint.Y * scaleY) + offsetY));
                }
                else if (shape is ShapePolygon polygon)
                {
                    for (int i = 0; i < polygon.Points.Count; i++)
                    {
                        polygon.Points[i] = new PointF(
                            (float)((polygon.Points[i].X * scaleX) + offsetX),
                            (float)((polygon.Points[i].Y * scaleY) + offsetY));
                    }
                }
                else if (shape is ShapePath path)
                {
                    for (int i = 0; i < path.PathSegments.Count; i++)
                    {
                        for (int j = 0; j < path.PathSegments[i].InputPoints.Count; j++)
                        {
                            path.PathSegments[i].InputPoints[j] = new PointF(
                                (float)((path.PathSegments[i].InputPoints[j].X * scaleX) + offsetX),
                                (float)((path.PathSegments[i].InputPoints[j].Y * scaleY) + offsetY));
                        }
                    }
                }
                else if (shape is ShapeText text)
                {
                    text.X = (text.X * scaleX) + offsetX;
                    text.Y = (text.Y * scaleY) + offsetY;
                }
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
                p.InputPoints.Add(new PointF((float)cx, (float)cy));

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

                if (i < tokens.Count && !char.IsLetter(tokens[i][0]))
                {
                    if (cmdType == 'Z') i++;
                }
            }
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

                        if (s is ShapeText t)
                        {
                            if (k == "font-size")
                            {
                                t.FontSize = ParseDimension(v);
                            }
                            if (k == "font-family")
                            {
                                t.FontFamily = v.Replace("'", "").Replace("\"", "");
                            }
                            if (k == "font-weight" && (v == "bold" || v == "700" || v == "800" || v == "900"))
                            {
                                t.IsBold = true;
                            }
                            if (k == "font-style" && v == "italic")
                            {
                                t.IsItalic = true;
                            }
                        }
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

        private static float ParseDimension(string v)
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
            return float.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out float res) ? res * factor : 0;
        }

        private static ShapeText? ParseTextElement(XElement el, Dictionary<string, GradientInfo> gradients)
        {
            string content = el.Value.Trim();
            XElement? tspan = el.Element(el.Name.Namespace + "tspan");

            if (string.IsNullOrEmpty(content) && tspan != null)
            {
                content = tspan.Value.Trim();
            }

            if (string.IsNullOrEmpty(content)) return null;

            float rawX;
            float rawY;

            if (tspan != null)
            {
                rawX = GetD(tspan, "x");
                rawY = GetD(tspan, "y");
            }
            else
            {
                rawX = GetD(el, "x");
                rawY = GetD(el, "y");
            }

            ApplyFullMatrixTransform(el.Attribute("transform")?.Value ?? "",
                out float matrixScaleX, out float matrixScaleY, out float matrixOffsetX, out float matrixOffsetY);

            float matrixX = (rawX * matrixScaleX) + matrixOffsetX;
            float matrixY = (rawY * matrixScaleY) + matrixOffsetY;

            var text = new ShapeText
            {
                Content = content,
                X = matrixX,
                Y = matrixY,
                FontSize = 12
            };

            ApplyStyles(el, text, gradients);

            if (tspan != null)
            {
                ApplyStyles(tspan, text, gradients);

                var nestedTspan = tspan.Element(tspan.Name.Namespace + "tspan");
                if (nestedTspan != null)
                {
                    ApplyStyles(nestedTspan, text, gradients);
                }
            }

            if (matrixScaleY > 0)
            {
                text.FontSize *= (float)matrixScaleY;
            }

            return text;
        }

        private static void ApplyFullMatrixTransform(string transform, out float scaleX, out float scaleY, out float offsetX, out float offsetY)
        {
            scaleX = 1.0f;
            scaleY = 1.0f;
            offsetX = 0;
            offsetY = 0;
            if (string.IsNullOrWhiteSpace(transform)) return;

            if (transform.Contains("matrix"))
            {
                try
                {
                    int start = transform.IndexOf('(') + 1;
                    int end = transform.IndexOf(')');
                    string content = transform[start..end];
                    string[] parts = content.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 6)
                    {
                        float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out scaleX);
                        float.TryParse(parts[3], NumberStyles.Any, CultureInfo.InvariantCulture, out scaleY);
                        float.TryParse(parts[4], NumberStyles.Any, CultureInfo.InvariantCulture, out offsetX);
                        float.TryParse(parts[5], NumberStyles.Any, CultureInfo.InvariantCulture, out offsetY);
                    }
                }
                catch { }
            }
            else if (transform.Contains("scale"))
            {
                try
                {
                    int start = transform.IndexOf('(') + 1;
                    int end = transform.IndexOf(')');
                    string content = transform[start..end];
                    string[] parts = content.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out scaleX);
                        float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out scaleY);
                    }
                    else if (parts.Length == 1)
                    {
                        float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out scaleX);
                        scaleY = scaleX; // If only one value, use it for both X and Y
                    }
                }
                catch { }
            }
            else if (transform.Contains("translate"))
            {
                try
                {
                    int start = transform.IndexOf('(') + 1;
                    int end = transform.IndexOf(')');
                    string content = transform[start..end];
                    string[] parts = content.Split([',', ' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2)
                    {
                        float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out offsetX);
                        float.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out offsetY);
                    }
                    else if (parts.Length == 1)
                    {
                        float.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out offsetX);
                        offsetY = 0; // For translate, if only one value, Y is 0
                    }
                }
                catch { }
            }
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