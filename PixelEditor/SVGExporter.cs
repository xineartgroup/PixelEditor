using PixelEditor.Vector;
using System.Drawing.Drawing2D;
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

            for (int n = layers.Count - 1; n >= 0; n--)
            {
                var layer = layers[n];
                string visibility = layer.IsVisible ? "inline" : "none";
                XElement group = new(SvgNs + "g",
                    new XAttribute("id", layer.Name),
                    new XAttribute("display", visibility));

                foreach (var stroke in layer.Shapes)
                {
                    XElement? el = null;
                    float scaleX = 1.0f;
                    float scaleY = 1.0f;

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
                                PointF startPt = pts.Count >= 2 ? pts[1] : pts[0];
                                sb.AppendFormat(CultureInfo.InvariantCulture, "M {0:0.###},{1:0.###} ", startPt.X, startPt.Y);
                                if (type == "M") continue;
                            }

                            switch (type)
                            {
                                case "M":
                                case "L":
                                    PointF dest = pts.Count >= 2 ? pts[1] : pts[0];
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} ", type, dest.X, dest.Y);
                                    break;

                                case "H":
                                    PointF hPt = pts.Count >= 2 ? pts[1] : pts[0];
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "H {0:0.###} ", hPt.X);
                                    break;

                                case "V":
                                    PointF vPt = pts.Count >= 2 ? pts[1] : pts[0];
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "V {0:0.###} ", vPt.Y);
                                    break;

                                case "C":
                                    if (pts.Count >= 4)
                                        sb.AppendFormat(CultureInfo.InvariantCulture, "C {0:0.###},{1:0.###} {2:0.###},{3:0.###} {4:0.###},{5:0.###} ",
                                            pts[1].X, pts[1].Y, pts[2].X, pts[2].Y, pts[3].X, pts[3].Y);
                                    else if (pts.Count == 3)
                                        sb.AppendFormat(CultureInfo.InvariantCulture, "C {0:0.###},{1:0.###} {2:0.###},{3:0.###} {4:0.###},{5:0.###} ",
                                            pts[0].X, pts[0].Y, pts[1].X, pts[1].Y, pts[2].X, pts[2].Y);
                                    break;

                                case "Q":
                                    if (pts.Count >= 3)
                                        sb.AppendFormat(CultureInfo.InvariantCulture, "Q {0:0.###},{1:0.###} {2:0.###},{3:0.###} ",
                                            pts[1].X, pts[1].Y, pts[2].X, pts[2].Y);
                                    else if (pts.Count == 2)
                                        sb.AppendFormat(CultureInfo.InvariantCulture, "Q {0:0.###},{1:0.###} {2:0.###},{3:0.###} ",
                                            pts[0].X, pts[0].Y, pts[1].X, pts[1].Y);
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
                        float nativeFontSize = t.FontSize;
                        float exportX = t.X;
                        float exportY = t.Y;

                        using (GraphicsPath path = new())
                        {
                            FontStyle fontStyle = GetFontStyle(t);
                            FontFamily family;
                            try { family = new FontFamily(t.FontFamily ?? "Arial"); }
                            catch { family = FontFamily.GenericSansSerif; }

                            int emHeight = family.GetEmHeight(fontStyle);
                            int cellAscent = family.GetCellAscent(fontStyle);

                            path.AddString(t.Content, family, (int)fontStyle, nativeFontSize, new PointF(0, 0), StringFormat.GenericTypographic);
                            RectangleF rawGlyphBounds = path.GetBounds();

                            if (t.Width > 0 && rawGlyphBounds.Width > 0)
                                scaleX = t.Width / rawGlyphBounds.Width;
                            if (t.Height > 0 && rawGlyphBounds.Height > 0)
                                scaleY = t.Height / rawGlyphBounds.Height;

                            float fontAscent = nativeFontSize * cellAscent / emHeight;
                            exportX = t.X - (rawGlyphBounds.X * scaleX);
                            exportY = t.Y + (fontAscent * scaleY) - (rawGlyphBounds.Y * scaleY);
                        }

                        double angleRad = t.Rotation * Math.PI / 180.0;
                        float cos = (float)Math.Cos(angleRad);
                        float sin = (float)Math.Sin(angleRad);

                        float ma = cos * scaleX;
                        float mb = sin * scaleX;
                        float mc = -sin * scaleY;
                        float md = cos * scaleY;

                        string transform = string.Format(CultureInfo.InvariantCulture,
                            "matrix({0:0.######},{1:0.######},{2:0.######},{3:0.######},{4:0.###},{5:0.###})",
                            ma, mb, mc, md, exportX, exportY);

                        XElement textElement = new(SvgNs + "text",
                            new XAttribute("x", "0"),
                            new XAttribute("y", "0"),
                            new XAttribute("font-size", $"{nativeFontSize.ToString("0.###", CultureInfo.InvariantCulture)}px"),
                            new XAttribute("font-family", t.FontFamily ?? "Arial"),
                            t.IsBold ? new XAttribute("font-weight", "bold") : null,
                            t.IsItalic ? new XAttribute("font-style", "italic") : null,
                            new XAttribute("text-rendering", "geometricPrecision"),
                            new XAttribute("transform", transform),
                            t.Content);

                        el = textElement;
                    }

                    if (el != null)
                    {
                        if (stroke is not ShapeText && stroke.Rotation != 0)
                        {
                            float cx = stroke.HasCustomRotationCenter ? stroke.RotationCenterX : 0;
                            float cy = stroke.HasCustomRotationCenter ? stroke.RotationCenterY : 0;

                            double angleRad = stroke.Rotation * Math.PI / 180.0;
                            float cos = (float)Math.Cos(angleRad);
                            float sin = (float)Math.Sin(angleRad);

                            float ma = cos;
                            float mb = sin;
                            float mc = -sin;
                            float md = cos;
                            float me = cx * (1 - cos) + cy * sin;
                            float mf = cy * (1 - cos) - cx * sin;

                            string transform = string.Format(CultureInfo.InvariantCulture,
                                "matrix({0:0.######},{1:0.######},{2:0.######},{3:0.######},{4:0.###},{5:0.###})",
                                ma, mb, mc, md, me, mf);

                            el.Add(new XAttribute("transform", transform));
                        }

                        if (el.Attribute("stroke") == null)
                            el.Add(new XAttribute("stroke", ColorToHex(stroke.LineColor)));
                        if (el.Attribute("stroke-width") == null)
                        {
                            if (stroke is ShapeText && scaleX > 0)
                                el.Add(new XAttribute("stroke-width", (stroke.LineWidth / scaleX).ToString("0.###", CultureInfo.InvariantCulture)));
                            else
                                el.Add(new XAttribute("stroke-width", stroke.LineWidth));
                        }
                        if (el.Attribute("fill") == null)
                            el.Add(new XAttribute("fill", stroke.FillColor == Color.Transparent ? "none" : ColorToHex(stroke.FillColor)));

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

        private static FontStyle GetFontStyle(ShapeText text)
        {
            FontStyle style = FontStyle.Regular;
            if (text.IsBold) style |= FontStyle.Bold;
            if (text.IsItalic) style |= FontStyle.Italic;
            return style;
        }
    }
}