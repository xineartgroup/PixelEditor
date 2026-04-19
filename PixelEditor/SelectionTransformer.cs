namespace PixelEditor
{
    public static class SelectionTransformer
    {
        public static List<PointF> RotatePolygon(List<PointF> points, float angleDegrees, PointF pivot)
        {
            if (points == null || points.Count == 0) return [];

            double angleRadians = angleDegrees * (Math.PI / 180.0);
            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            List<PointF> rotatedPoints = [];

            foreach (var point in points)
            {
                double translatedX = point.X - pivot.X;
                double translatedY = point.Y - pivot.Y;

                double rotatedX = (translatedX * cosTheta) - (translatedY * sinTheta);
                double rotatedY = (translatedX * sinTheta) + (translatedY * cosTheta);

                rotatedPoints.Add(new PointF(
                    (float)(rotatedX + pivot.X),
                    (float)(rotatedY + pivot.Y)
                ));
            }

            return rotatedPoints;
        }

        public static PointF GetPolygonCenter(List<PointF> points)
        {
            if (points == null || points.Count == 0) return PointF.Empty;

            float minX = points.Min(p => p.X);
            float maxX = points.Max(p => p.X);
            float minY = points.Min(p => p.Y);
            float maxY = points.Max(p => p.Y);

            return new PointF((minX + maxX) / 2f, (minY + maxY) / 2f);
        }
    }
}