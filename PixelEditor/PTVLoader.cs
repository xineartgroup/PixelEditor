using System.Security.Cryptography;

namespace PixelEditor
{
    public static class PTVLoader
    {
        private const string MagicString = "BNDATA";

        public static void Load(Stream source, out float zoom, out PointF offset, out List<Layer> layers, out int selectedLayerIndex)
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
                int versionMajor = reader.ReadInt32();
                int versionMinor = reader.ReadInt32();

                zoom = reader.ReadSingle();
                offset = new PointF(reader.ReadSingle(), reader.ReadSingle());
                selectedLayerIndex = reader.ReadInt32();

                int layerCount = reader.ReadInt32();
                layers = new List<Layer>(layerCount);
                for (int i = 0; i < layerCount; i++)
                {
                    string name = reader.ReadString();
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
                        X = x,
                        Y = y,
                        ScaleWidth = scaleWidth,
                        ScaleHeight = scaleHeight,
                        Opacity = opacity,
                        RedFilter = redFilter,
                        GreenFilter = greenFilter,
                        BlueFilter = blueFilter,
                        Channel = channel,
                        BlendMode = blendMode
                    };

                    bool hasImage = reader.ReadBoolean();
                    if (hasImage)
                    {
                        int imageLength = reader.ReadInt32();
                        byte[] imageBytes = reader.ReadBytes(imageLength);
                        using var msImage = new MemoryStream(imageBytes);
                        layer.Image = Image.FromStream(msImage);
                    }

                    layers.Add(layer);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Failed to load PTV file.", ex);
            }
        }
    }
}