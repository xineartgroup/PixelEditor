using PixelEditor.Vector;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
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
                        foreach (var pathSegment in pa.PathSegments)
                        {
                            string type = pathSegment.PathType; // M, L, H, V, C, Q, A, Z
                            var pts = pathSegment.InputPoints;

                            if (pts.Count == 0 && !type.Equals("Z", StringComparison.CurrentCultureIgnoreCase)) continue;

                            switch (type.ToUpper())
                            {
                                case "M":
                                case "L":
                                    // M and L both use the last point in InputPoints as the destination
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} ", type, pts[^1].X, pts[^1].Y);
                                    break;

                                case "H":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###} ", type, pts[^1].X);
                                    break;

                                case "V":
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###} ", type, pts[^1].Y);
                                    break;

                                case "C":
                                    // Cubic: Control1, Control2, EndPoint
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} {3:0.###},{4:0.###} {5:0.###},{6:0.###} ",
                                        type, pts[1].X, pts[1].Y, pts[2].X, pts[2].Y, pts[3].X, pts[3].Y);
                                    break;

                                case "Q":
                                    // Quadratic: Control1, EndPoint
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} {3:0.###},{4:0.###} ",
                                        type, pts[1].X, pts[1].Y, pts[2].X, pts[2].Y);
                                    break;

                                case "A":
                                    // Arc: Radii, Rot, LargeArc, Sweep, EndPoint
                                    // Note: InputPoints[3] stores LargeArc in X and Sweep in Y
                                    sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} {3:0.###} {4},{5} {6:0.###},{7:0.###} ",
                                        type, pts[1].X, pts[1].Y, pts[2].X, (int)pts[3].X, (int)pts[3].Y, pts[4].X, pts[4].Y);
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
                        el = new XElement(SvgNs + "text",
                            new XAttribute("x", t.X.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("y", t.Y.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("font-size", t.FontSize.ToString("0.###", CultureInfo.InvariantCulture)),
                            new XAttribute("font-family", t.FontFamily ?? "Arial"),
                            t.IsBold ? new XAttribute("font-weight", "bold") : null,
                            t.IsItalic ? new XAttribute("font-style", "italic") : null,
                            t.Content);
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

        private static string ExtractIdFromStyle(string style, string key)
        {
            var pattern = $@"{key}:url\(#([^)]+)\)";
            var match = Regex.Match(style, pattern);
            return match.Success ? match.Groups[1].Value : "";
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

        private static string PointsToPathData(List<PointF> points, bool isClosed)
        {
            if (points.Count == 0) return "";

            StringBuilder sb = new();
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                char cmd = (i == 0) ? 'M' : 'L';
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0} {1:0.###},{2:0.###} ", cmd, p.X, p.Y);
            }

            // Close the path if it's a polygon
            if (isClosed && points.Count >= 3)
            {
                sb.Append(" Z");
            }

            return sb.ToString();
        }
    }
}
