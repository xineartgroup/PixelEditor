using System.Security.Cryptography;
using PixelEditor.Vector;
using System.Drawing.Drawing2D;

namespace PixelEditor
{
    public static class XPELoader
    {
        private const string MagicString = "BNDATA";

        public static void Load(Stream source, out List<Layer> layers, out int selectedLayerIndex)
        {
            try
            {
                byte[] allBytes;
                using (var tempMs = new MemoryStream()) { source.CopyTo(tempMs); allBytes = tempMs.ToArray(); }

                if (allBytes.Length < 32) throw new InvalidDataException("File too small.");
                byte[] payload = new byte[allBytes.Length - 32];
                byte[] storedHash = new byte[32];
                Array.Copy(allBytes, 0, payload, 0, payload.Length);
                Array.Copy(allBytes, allBytes.Length - 32, storedHash, 0, 32);

                if (!SHA256.HashData(payload).SequenceEqual(storedHash))
                    throw new CryptographicException("Checksum mismatch! Data corrupted.");

                using var reader = new BinaryReader(new MemoryStream(payload), System.Text.Encoding.UTF8);

                if (reader.ReadString() != MagicString) throw new InvalidDataException("Invalid format.");
                reader.ReadInt32();
                reader.ReadInt32();

                Document.Zoom = reader.ReadSingle();
                Document.ImageOffset = new PointF(reader.ReadSingle(), reader.ReadSingle());
                Document.Width = reader.ReadInt32();
                Document.Height = reader.ReadInt32();

                selectedLayerIndex = reader.ReadInt32();

                int layerCount = reader.ReadInt32();
                layers = [];
                for (int i = 0; i < layerCount; i++)
                {
                    string name = reader.ReadString();
                    LayerType type = (LayerType)reader.ReadInt32();

                    if (type == LayerType.Vector)
                    {
                        Layer layer = new(name, true) { LayerType = LayerType.Vector };
                        int shapeCount = reader.ReadInt32();
                        for (int j = 0; j < shapeCount; j++)
                        {
                            layer.Shapes.Add(ReadShape(reader));
                        }
                        layers.Add(layer);
                    }
                    else
                    {
                        int x = reader.ReadInt32();
                        int y = reader.ReadInt32();
                        int scaleWidth = reader.ReadInt32();
                        int scaleHeight = reader.ReadInt32();
                        int opacity = reader.ReadInt32();
                        bool isVisible = reader.ReadBoolean();
                        bool redFilter = reader.ReadBoolean();
                        bool greenFilter = reader.ReadBoolean();
                        bool blueFilter = reader.ReadBoolean();
                        LayerChannel channel = (LayerChannel)reader.ReadInt32();
                        ImageBlending blendMode = (ImageBlending)reader.ReadInt32();

                        Layer layer = new(name, isVisible)
                        {
                            LayerType = LayerType.Image,
                            X = x,
                            Y = y,
                            ScaleWidth = scaleWidth,
                            ScaleHeight = scaleHeight,
                            Opacity = opacity,
                            RedFilter = redFilter,
                            GreenFilter = greenFilter,
                            BlueFilter = blueFilter,
                            Channel = channel,
                            BlendMode = blendMode,
                            Image = ReadLayerImageData(reader),
                            ImageMask = ReadLayerImageData(reader)
                        };

                        layers.Add(layer);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to load PTV file.", ex);
            }
        }

        private static Image? ReadLayerImageData(BinaryReader reader)
        {
            if (reader.ReadBoolean())
            {
                int imageLength = reader.ReadInt32();
                byte[] imageBytes = reader.ReadBytes(imageLength);
                using var msImage = new MemoryStream(imageBytes);
                return Image.FromStream(msImage);
            }
            return null;
        }

        private static BaseShape ReadShape(BinaryReader reader)
        {
            int typeId = reader.ReadInt32();

            BaseShape shape = typeId switch
            {
                1 => new ShapeLine(),
                2 => new ShapeRect(),
                3 => new ShapeEllipse(),
                4 => new ShapePolygon(),
                5 => new ShapeText(),
                _ => throw new InvalidDataException("Unknown shape type.")
            };

            shape.LineWidth = reader.ReadSingle();
            shape.LineColor = Color.FromArgb(reader.ReadInt32());
            shape.FillColor = Color.FromArgb(reader.ReadInt32());
            shape.DashStyle = (DashStyle)reader.ReadInt32();
            shape.Opacity = reader.ReadSingle();
            shape.Rotation = reader.ReadSingle();

            shape.HasGradientFill = reader.ReadBoolean();
            shape.HasGradientStroke = reader.ReadBoolean();
            shape.GradientFill = ReadGradient(reader);
            shape.GradientStroke = ReadGradient(reader);

            switch (shape)
            {
                case ShapeLine line:
                    line.StartPoint = new PointF(reader.ReadSingle(), reader.ReadSingle());
                    line.EndPoint = new PointF(reader.ReadSingle(), reader.ReadSingle());
                    break;

                case ShapeRect rect:
                    rect.X = reader.ReadSingle();
                    rect.Y = reader.ReadSingle();
                    rect.Width = reader.ReadSingle();
                    rect.Height = reader.ReadSingle();
                    rect.Rx = reader.ReadSingle();
                    rect.Ry = reader.ReadSingle();
                    break;

                case ShapeEllipse ellipse:
                    ellipse.X = reader.ReadSingle();
                    ellipse.Y = reader.ReadSingle();
                    ellipse.Width = reader.ReadSingle();
                    ellipse.Height = reader.ReadSingle();
                    break;

                case ShapePolygon poly:
                    poly.IsClosed = reader.ReadBoolean();
                    int pointsCount = reader.ReadInt32();
                    poly.Points = [];
                    for (int i = 0; i < pointsCount; i++)
                    {
                        poly.Points.Add(new PointF(reader.ReadSingle(), reader.ReadSingle()));
                    }
                    break;

                case ShapeText text:
                    text.Content = reader.ReadString();
                    text.X = reader.ReadSingle();
                    text.Y = reader.ReadSingle();
                    text.Width = reader.ReadSingle();
                    text.Height = reader.ReadSingle();
                    text.FontSize = reader.ReadSingle();
                    text.FontFamily = reader.ReadString();
                    text.IsBold = reader.ReadBoolean();
                    text.IsItalic = reader.ReadBoolean();
                    break;

                case ShapePath path:
                    int segmentCount = reader.ReadInt32();
                    path.PathSegments = [];
                    for (int i = 0; i < segmentCount; i++)
                    {
                        path.PathSegments.Add(ReadPathSegment(reader));
                    }
                    break;
            }

            return shape;
        }

        private static PathSegment ReadPathSegment(BinaryReader reader)
        {
            PathSegment segment = new()
            {
                PathType = reader.ReadString()
            };

            int pointCount = reader.ReadInt32();
            segment.InputPoints = [];
            for (int i = 0; i < pointCount; i++)
            {
                segment.InputPoints.Add(new PointF(reader.ReadSingle(), reader.ReadSingle()));
            }

            return segment;
        }

        private static GradientInfo? ReadGradient(BinaryReader reader)
        {
            bool exists = reader.ReadBoolean();
            if (!exists) return null;

            GradientInfo gradient = new()
            {
                IsRadial = reader.ReadBoolean(),
                UserSpaceOnUse = reader.ReadBoolean(),
                HasTransform = reader.ReadBoolean()
            };

            if (gradient.HasTransform)
            {
                gradient.TransformA = reader.ReadSingle();
                gradient.TransformB = reader.ReadSingle();
                gradient.TransformC = reader.ReadSingle();
                gradient.TransformD = reader.ReadSingle();
                gradient.TransformE = reader.ReadSingle();
                gradient.TransformF = reader.ReadSingle();

                gradient.TransformMatrix = new System.Drawing.Drawing2D.Matrix(
                    gradient.TransformA, gradient.TransformB,
                    gradient.TransformC, gradient.TransformD,
                    gradient.TransformE, gradient.TransformF
                );
            }

            if (gradient.IsRadial)
            {
                gradient.Cx = reader.ReadSingle();
                gradient.Cy = reader.ReadSingle();
                gradient.R = reader.ReadSingle();
                gradient.Fx = reader.ReadSingle();
                gradient.Fy = reader.ReadSingle();
            }
            else
            {
                gradient.X1 = reader.ReadSingle();
                gradient.Y1 = reader.ReadSingle();
                gradient.X2 = reader.ReadSingle();
                gradient.Y2 = reader.ReadSingle();
            }

            int stopCount = reader.ReadInt32();
            for (int i = 0; i < stopCount; i++)
            {
                gradient.Stops.Add(new GradientStop
                {
                    Offset = reader.ReadSingle(),
                    Color = Color.FromArgb(reader.ReadInt32())
                });
            }

            return gradient;
        }
    }
}