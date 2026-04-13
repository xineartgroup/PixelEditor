using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PixelEditor
{
    public static class SelectionTransformer
    {
        /// <summary>
        /// Rotates a list of points by a specified angle around a pivot point.
        /// </summary>
        /// <param name="points">The original list of points (e.g., from your poly.Points).</param>
        /// <param name="angleDegrees">The angle to rotate by in degrees.</param>
        /// <param name="pivot">The point to rotate around (e.g., the center of the polygon).</param>
        /// <returns>A new list of rotated PointF points.</returns>
        public static List<PointF> RotatePolygon(List<PointF> points, float angleDegrees, PointF pivot)
        {
            if (points == null || points.Count == 0) return [];

            // Convert angle to radians
            double angleRadians = angleDegrees * (Math.PI / 180.0);
            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            List<PointF> rotatedPoints = [];

            foreach (var point in points)
            {
                // Translate point back to origin relative to pivot
                double translatedX = point.X - pivot.X;
                double translatedY = point.Y - pivot.Y;

                // Apply rotation matrix
                double rotatedX = (translatedX * cosTheta) - (translatedY * sinTheta);
                double rotatedY = (translatedX * sinTheta) + (translatedY * cosTheta);

                // Translate point back to pivot position
                rotatedPoints.Add(new PointF(
                    (float)(rotatedX + pivot.X),
                    (float)(rotatedY + pivot.Y)
                ));
            }

            return rotatedPoints;
        }

        /// <summary>
        /// Helper to calculate the bounding box center of a polygon to use as a default pivot.
        /// </summary>
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