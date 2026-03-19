using System.Security.Cryptography;

namespace PixelEditor
{
    public static class XPESaver
    {
        private const int FormatMajorVersion = 1;
        private const int FormatMinorVersion = 1;
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

                        if (layer.Image != null)
                        {
                            writer.Write(true);
                            using var msImage = new MemoryStream();
                            layer.Image.Save(msImage, System.Drawing.Imaging.ImageFormat.Png);
                            byte[] imageBytes = msImage.ToArray();
                            writer.Write(imageBytes.Length);
                            writer.Write(imageBytes);
                        }
                        else
                        {
                            writer.Write(false);
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
    }
}