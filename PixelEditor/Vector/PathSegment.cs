namespace PixelEditor.Vector
{
    public class PathSegment
    {
        public PathSegment()
        {
            PathType = "";
            InputPoints = [];
        }

        public PathSegment(string pathType, List<PointF> points)
        {
            PathType = pathType;
            InputPoints = points;
        }

        public string PathType { get; set; } = string.Empty; // M, L, H, V, C, Q, A, Z

        public List<PointF> InputPoints { get; set; } = [];

        public List<PointF> GetPoints()
        {
            List<PointF> points = [];
            if (InputPoints.Count == 0) return points;

            switch (PathType.ToUpper())
            {
                case "M": // MoveTo
                    points.Add(InputPoints.Count > 1 ? InputPoints[1] : InputPoints[0]);
                    break;

                case "L": // LineTo
                case "H": // Horizontal LineTo
                case "V": // Vertical LineTo
                    points.Add(InputPoints.Count > 1 ? InputPoints[1] : InputPoints[0]);
                    break;

                case "C": // Cubic Bezier Curve
                    if (InputPoints.Count < 4) break;
                    PointF startC = InputPoints[0], cp1 = InputPoints[1], cp2 = InputPoints[2], endC = InputPoints[3];
                    int cSteps = Math.Max(10, (int)Math.Min(50, Utility.VectorDistance(startC, endC) * 1.5));
                    for (int s = 1; s <= cSteps; s++)
                    {
                        float t = s / (float)cSteps;
                        float xt = (float)(Math.Pow(1 - t, 3) * startC.X + 3 * Math.Pow(1 - t, 2) * t * cp1.X + 3 * (1 - t) * t * t * cp2.X + Math.Pow(t, 3) * endC.X);
                        float yt = (float)(Math.Pow(1 - t, 3) * startC.Y + 3 * Math.Pow(1 - t, 2) * t * cp1.Y + 3 * (1 - t) * t * t * cp2.Y + Math.Pow(t, 3) * endC.Y);
                        points.Add(new PointF((float)xt, (float)yt));
                    }
                    break;

                case "Q": // Quadratic Bezier Curve
                    if (InputPoints.Count < 3) break;
                    PointF startQ = InputPoints[0], qp1 = InputPoints[1], endQ = InputPoints[2];
                    int qSteps = Math.Max(8, (int)Math.Min(40, Utility.VectorDistance(startQ, endQ) * 1.2));
                    for (int s = 1; s <= qSteps; s++)
                    {
                        float t = s / (float)qSteps;
                        float xt = (float)(Math.Pow(1 - t, 2) * startQ.X + 2 * (1 - t) * t * qp1.X + t * t * endQ.X);
                        float yt = (float)(Math.Pow(1 - t, 2) * startQ.Y + 2 * (1 - t) * t * qp1.Y + t * t * endQ.Y);
                        points.Add(new PointF((float)xt, (float)yt));
                    }
                    break;

                case "A": // Arc
                    if (InputPoints.Count < 5) break;
                    points.AddRange(CalculateArcPoints(InputPoints[0], InputPoints[1], InputPoints[2].X, (int)InputPoints[3].X, (int)InputPoints[3].Y, InputPoints[4]));
                    break;

                case "Z": // ClosePath
                    break;
            }
            return points;
        }

        private static List<PointF> CalculateArcPoints(PointF start, PointF r, float rot, int large, int sweep, PointF end)
        {
            List<PointF> arcPoints = [];
            float rx = Math.Abs(r.X), ry = Math.Abs(r.Y);
            if (rx == 0 || ry == 0) { arcPoints.Add(end); return arcPoints; }

            float rad = (float)(rot * (Math.PI / 180.0)), cosP = (float)Math.Cos(rad), sinP = (float)Math.Sin(rad);
            float dx2 = (float)(start.X - end.X) / 2.0f, dy2 = (float)(start.Y - end.Y) / 2.0f;
            float x1p = cosP * dx2 + sinP * dy2, y1p = -sinP * dx2 + cosP * dy2;
            float sq = (rx * rx * ry * ry - rx * rx * y1p * y1p - ry * ry * x1p * x1p) / (rx * rx * y1p * y1p + ry * ry * x1p * x1p);
            float coef = (float)(large == sweep ? -1 : 1) * (float)Math.Sqrt(Math.Max(0, sq));
            float cxp = coef * ((rx * y1p) / ry), cyp = coef * -((ry * x1p) / rx);
            float centX = (float)cosP * cxp - sinP * cyp + (start.X + end.X) / 2.0f, centY = (float)(sinP * cxp + cosP * cyp + (start.Y + end.Y) / 2.0f);
            float t1 = (float)Math.Atan2((y1p - cyp) / ry, (x1p - cxp) / rx), dt = (float)Math.Atan2((-y1p - cyp) / ry, (-x1p - cxp) / rx) - t1;
            if (sweep == 0 && dt > 0) dt -= 2 * (float)Math.PI; else if (sweep == 1 && dt < 0) dt += 2 * (float)Math.PI;

            int segs = Math.Max(10, (int)(Math.Abs(dt) * 8));
            for (int s = 1; s <= segs; s++)
            {
                float a = t1 + dt * (s / (float)segs);
                arcPoints.Add(new PointF((float)(cosP * rx * Math.Cos(a) - sinP * ry * Math.Sin(a) + centX), (float)(sinP * rx * Math.Cos(a) + cosP * ry * Math.Sin(a) + centY)));
            }
            return arcPoints;
        }

        public override string ToString()
        {
            return $"{PathType} {string.Join(" ", InputPoints.Select(p => $"({p.X},{p.Y})"))}";
        }
    }
}
