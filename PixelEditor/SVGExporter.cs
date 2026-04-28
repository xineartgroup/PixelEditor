using PixelEditor.Vector;
using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace PixelEditor
{
    public static class SVGExporter
    {
        private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

        public static void ExportSVG(string filePath, int width, int height, List<Layer> layers)
        {
            XElement root = new(SvgNs + "svg",
                new XAttribute("width", width),
                new XAttribute("height", height),
                new XAttribute("viewBox", $"0 0 {width} {height}"),
                new XAttribute("xmlns", SvgNs.NamespaceName)
            );

            foreach (var layer in layers)
            {
                string visibility = layer.IsVisible ? "inline" : "none";
                XElement group = new(SvgNs + "g",
                    new XAttribute("id", layer.Name),
                    new XAttribute("display", visibility));

                foreach (var stroke in layer.Shapes)
                {
                    XElement? el = null;

                    if (stroke is ShapeRect r)
                    {
                        el = new XElement(SvgNs + "rect",
                            new XAttribute("x", r.X.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("y", r.Y.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("width", r.Width.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("height", r.Height.ToString("0.###", CultureInfo.InvariantCulture)));
                        if (r.Rx > 0) el.Add(new XAttribute("rx", r.Rx.ToString("0.###", CultureInfo.InvariantCulture)));
                        if (r.Ry > 0) el.Add(new XAttribute("ry", r.Ry.ToString("0.###", CultureInfo.InvariantCulture)));
                    }
                    else if (stroke is ShapeEllipse e)
                    {
                        el = new XElement(SvgNs + "ellipse",
                            new XAttribute("cx", e.Cx.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("cy", e.Cy.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("rx", e.Rx.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("ry", e.Ry.ToString("0.###", CultureInfo.InvariantCulture)));
                    }
                    else if (stroke is ShapeLine l)
                    {
                        el = new XElement(SvgNs + "line",
                            new XAttribute("x1", l.StartPoint.X.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("y1", l.StartPoint.Y.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("x2", l.EndPoint.X.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("y2", l.EndPoint.Y.ToString("0.###", CultureInfo.InvariantCulture)));
                    }
                    else if (stroke is ShapePolygon pg)
                    {
                        el = new XElement(SvgNs + "polygon",
                            new XAttribute("points", GetPointsString(pg.Points)));
                    }
                    else if (stroke is ShapePath pa)
                    {
                        StringBuilder sb = new();
                        for (int i = 0; i < pa.PathSegments.Count; i++)
                        {
                            var pathSegment = pa.PathSegments[i];
                            string type = pathSegment.PathType.ToUpper();
                            var pts = pathSegment.InputPoints;

                            if (pts.Count == 0 && type != "Z") continue;

                            if (i == 0)
                            {
                                sb.AppendFormat(CultureInfo.InvariantCulture, "M {0:0.###},{1:0.###} ", pts[0].X, pts[0].Y);

                                if (type == "M") continue;
                            }

                            switch (type)
                            {
                                case "M":
                                case "L":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} ", type, pts[^1].X, pts[^1].Y);
                                    break;

                                case "H":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "H {0:0.###} ", pts[^1].X);
                                    break;

                                case "V":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "V {0:0.###} ", pts[^1].Y);
                                    break;

                                case "C":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "C {0:0.###},{1:0.###} {2:0.###},{3:0.###} {4:0.###},{5:0.###} ",
                                        pts[1].X, pts[1].Y, pts[2].X, pts[2].Y, pts[3].X, pts[3].Y);
                                    break;

                                case "Q":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "Q {0:0.###},{1:0.###} {2:0.###},{3:0.###} ",
                                        pts[1].X, pts[1].Y, pts[2].X, pts[2].Y);
                                    break;

                                case "A":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "A {0:0.###},{1:0.###} {2:0.###} {3},{4} {5:0.###},{6:0.###} ",
                                        pts[1].X, pts[1].Y, pts[2].X, (int)pts[3].X, (int)pts[3].Y, pts[4].X, pts[4].Y);
                                    break;

                                case "Z":
                                    sb.Append("Z ");
                                    break;
                            }
                        }
                        el = new XElement(SvgNs + "path", new XAttribute("d", sb.ToString().Trim()));
                    }
                    else if (stroke is ShapeText t)
                    {
                        float scaledFontSize = CalculateScaledFontSize(t);

                        XElement textElement = new XElement(SvgNs + "text",
                            new XAttribute("x", t.X.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("y", t.Y.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("font-size", $"{scaledFontSize.ToString("0.###", CultureInfo.InvariantCulture)}px"),
                            new XAttribute("font-family", t.FontFamily ?? "Arial"),
                            t.IsBold ? new XAttribute("font-weight", "bold") : null,
                            t.IsItalic ? new XAttribute("font-style", "italic") : null,
                            new XAttribute("dominant-baseline", "hanging"),
                            new XAttribute("text-rendering", "geometricPrecision"),
                            t.Content);

                        if (t.Rotation != 0)
                        {
                            textElement.Add(new XAttribute("transform",
                                $"rotate({t.Rotation.ToString("0.###", CultureInfo.InvariantCulture)}, {t.X.ToString("0.###", CultureInfo.InvariantCulture)}, {t.Y.ToString("0.###", CultureInfo.InvariantCulture)})"));
                        }

                        el = textElement;
                    }

                    if (el != null)
                    {
                        if (el.Attribute("stroke") == null)
                            el.Add(new XAttribute("stroke", ColorToHex(stroke.LineColor)));
                        if (el.Attribute("stroke-width") == null)
                            el.Add(new XAttribute("stroke-width", stroke.LineWidth));
                        if (el.Attribute("fill") == null)
                            el.Add(new XAttribute("fill", (stroke.FillColor == Color.Transparent) ? "none" : ColorToHex(stroke.FillColor)));

                        group.Add(el);
                    }
                }
                root.Add(group);
            }

            new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root).Save(filePath);
        }

        private static string ColorToHex(Color c)
        {
            return $"#{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private static string GetPointsString(List<PointF> points)
        {
            StringBuilder sb = new();
            foreach (var p in points)
            {
                if (sb.Length > 0) sb.Append(' ');
                sb.Append(p.X.ToString("0.###", CultureInfo.InvariantCulture));
                sb.Append(',');
                sb.Append(p.Y.ToString("0.###", CultureInfo.InvariantCulture));
            }
            return sb.ToString();
        }

        private static float CalculateScaledFontSize(ShapeText text)
        {
            using Font tempFont = new(text.FontFamily, text.FontSize, GetFontStyle(text));

            using StringFormat stringFormat = new()
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Near,
                Trimming = StringTrimming.None
            };

            using Graphics g = Graphics.FromImage(new Bitmap(1, 1));

            SizeF textSize = g.MeasureString(text.Content, tempFont, new SizeF(text.Width, text.Height), stringFormat);

            float widthScale = text.Width > 0 ? text.Width / textSize.Width : 1f;
            float heightScale = text.Height > 0 ? text.Height / textSize.Height : 1f;

            float scale = Math.Min(widthScale, heightScale);
            float scaledFontSize = Math.Max(4, Math.Min(text.FontSize * scale, 4000));

            return scaledFontSize;
        }

        private static FontStyle GetFontStyle(ShapeText text)
        {
            FontStyle style = FontStyle.Regular;
            if (text.IsBold) style |= FontStyle.Bold;
            if (text.IsItalic) style |= FontStyle.Italic;
            return style;
        }
    }
}
