using System.Security.Cryptography;
using PixelEditor.Vector;
using System.Drawing.Drawing2D;

namespace PixelEditor
{
    public static class XPESaver
    {
        private const int FormatMajorVersion = 1;
        private const int FormatMinorVersion = 0;
        private const string MagicString = "BNDATA";

        public static void Save(Stream destination, List<Layer> layers, int selectedLayerIndex)
        {
            try
            {
                using var ms = new MemoryStream();
                using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    writer.Write(MagicString);
                    writer.Write(FormatMajorVersion);
                    writer.Write(FormatMinorVersion);

                    writer.Write(Document.Zoom);
                    writer.Write(Document.ImageOffset.X);
                    writer.Write(Document.ImageOffset.Y);
                    writer.Write(Document.Width);
                    writer.Write(Document.Height);
                    writer.Write(selectedLayerIndex);

                    writer.Write(layers.Count);
                    foreach (var layer in layers)
                    {
                        writer.Write(layer.Name);
                        writer.Write((int)layer.LayerType);

                        if (layer.LayerType == LayerType.Vector)
                        {
                            writer.Write(layer.Shapes.Count);
                            foreach (var shape in layer.Shapes)
                            {
                                WriteShape(writer, shape);
                            }
                        }
                        else
                        {
                            writer.Write(layer.X);
                            writer.Write(layer.Y);
                            writer.Write(layer.ScaleWidth);
                            writer.Write(layer.ScaleHeight);
                            writer.Write(layer.Opacity);
                            writer.Write(layer.IsVisible);
                            writer.Write(layer.RedFilter);
                            writer.Write(layer.GreenFilter);
                            writer.Write(layer.BlueFilter);
                            writer.Write((int)layer.Channel);
                            writer.Write((int)layer.BlendMode);

                            WriteImageData(writer, layer.GetBasicImage());
                            WriteImageData(writer, layer.ImageMask);
                        }
                    }
                }

                byte[] payload = ms.ToArray();
                byte[] hash = SHA256.HashData(payload);
                destination.Write(payload, 0, payload.Length);
                destination.Write(hash, 0, hash.Length);
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving PTV file: " + ex.Message, ex);
            }
        }

        private static void WriteImageData(BinaryWriter writer, Image? img)
        {
            if (img != null)
            {
                writer.Write(true);
                using var msImage = new MemoryStream();
                img.Save(msImage, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = msImage.ToArray();
                writer.Write(imageBytes.Length);
                writer.Write(imageBytes);
            }
            else
            {
                writer.Write(false);
            }
        }

        private static void WriteShape(BinaryWriter writer, BaseShape shape)
        {
            int typeId = shape switch
            {
                ShapeLine => 1,
                ShapeRect => 2,
                ShapeEllipse => 3,
                ShapePolygon => 4,
                ShapeText => 5,
                _ => 0
            };
            writer.Write(typeId);

            writer.Write(shape.LineWidth);
            writer.Write(shape.LineColor.ToArgb());
            writer.Write(shape.FillColor.ToArgb());
            writer.Write((int)shape.DashStyle);
            writer.Write(shape.Opacity);
            writer.Write(shape.Rotation);

            writer.Write(shape.HasGradientFill);
            writer.Write(shape.HasGradientStroke);
            WriteGradient(writer, shape.GradientFill);
            WriteGradient(writer, shape.GradientStroke);

            switch (shape)
            {
                case ShapeLine line:
                    writer.Write(line.StartPoint.X);
                    writer.Write(line.StartPoint.Y);
                    writer.Write(line.EndPoint.X);
                    writer.Write(line.EndPoint.Y);
                    break;

                case ShapeRect rect:
                    writer.Write(rect.X);
                    writer.Write(rect.Y);
                    writer.Write(rect.Width);
                    writer.Write(rect.Height);
                    writer.Write(rect.Rx);
                    writer.Write(rect.Ry);
                    break;

                case ShapeEllipse ellipse:
                    writer.Write(ellipse.X);
                    writer.Write(ellipse.Y);
                    writer.Write(ellipse.Width);
                    writer.Write(ellipse.Height);
                    break;

                case ShapePolygon poly:
                    writer.Write(poly.IsClosed);
                    writer.Write(poly.Points.Count);
                    foreach (var p in poly.Points)
                    {
                        writer.Write(p.X);
                        writer.Write(p.Y);
                    }
                    break;

                case ShapeText text:
                    writer.Write(text.Content ?? string.Empty);
                    writer.Write(text.X);
                    writer.Write(text.Y);
                    writer.Write(text.Width);
                    writer.Write(text.Height);
                    writer.Write(text.FontSize);
                    writer.Write(text.FontFamily ?? "Arial");
                    writer.Write(text.IsBold);
                    writer.Write(text.IsItalic);
                    break;

                case ShapePath path:
                    writer.Write(path.PathSegments.Count);
                    foreach (var segment in path.PathSegments)
                    {
                        WritePathSegment(writer, segment);
                    }
                    break;
            }
        }

        private static void WritePathSegment(BinaryWriter writer, PathSegment segment)
        {
            writer.Write(segment.PathType ?? string.Empty);
            writer.Write(segment.InputPoints.Count);
            foreach (var p in segment.InputPoints)
            {
                writer.Write(p.X);
                writer.Write(p.Y);
            }
        }

        private static void WriteGradient(BinaryWriter writer, GradientInfo? gradient)
        {
            if (gradient == null)
            {
                writer.Write(false);
                return;
            }

            writer.Write(true);
            writer.Write(gradient.IsRadial);
            writer.Write(gradient.UserSpaceOnUse);
            writer.Write(gradient.HasTransform);

            if (gradient.HasTransform)
            {
                writer.Write(gradient.TransformA);
                writer.Write(gradient.TransformB);
                writer.Write(gradient.TransformC);
                writer.Write(gradient.TransformD);
                writer.Write(gradient.TransformE);
                writer.Write(gradient.TransformF);
            }

            if (gradient.IsRadial)
            {
                writer.Write(gradient.Cx);
                writer.Write(gradient.Cy);
                writer.Write(gradient.R);
                writer.Write(gradient.Fx);
                writer.Write(gradient.Fy);
            }
            else
            {
                writer.Write(gradient.X1);
                writer.Write(gradient.Y1);
                writer.Write(gradient.X2);
                writer.Write(gradient.Y2);
            }

            writer.Write(gradient.Stops.Count);
            foreach (var stop in gradient.Stops)
            {
                writer.Write(stop.Offset);
                writer.Write(stop.Color.ToArgb());
            }
        }
    }
}