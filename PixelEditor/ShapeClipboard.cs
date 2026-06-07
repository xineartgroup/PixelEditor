using PixelEditor.Vector;
using System.Text.Json;

namespace PixelEditor
{
    public static class ShapeClipboard
    {
        private static readonly (Type Type, string Format)[] ShapeFormats =
        [
            (typeof(ShapeLine), "PixelEditor.Vector.ShapeLine"),
            (typeof(ShapeRect), "PixelEditor.Vector.ShapeRect"),
            (typeof(ShapeEllipse), "PixelEditor.Vector.ShapeEllipse"),
            (typeof(ShapePolygon), "PixelEditor.Vector.ShapePolygon"),
            (typeof(ShapePath), "PixelEditor.Vector.ShapePath"),
            (typeof(ShapeText), "PixelEditor.Vector.ShapeText")
        ];

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = false,
            Converters = { new ColorJsonConverter() }
        };

        public static void SetClipboardData(BaseShape? shape)
        {
            if (shape == null) return;

            try
            {
                Type exactType = shape.GetType();
                string? targetFormat = null;

                foreach (var mapping in ShapeFormats)
                {
                    if (mapping.Type == exactType)
                    {
                        targetFormat = mapping.Format;
                        break;
                    }
                }

                if (targetFormat == null) return;

                string json = JsonSerializer.Serialize(shape, exactType, SerializerOptions);

                DataObject data = new();
                data.SetData(targetFormat, false, json);

                Clipboard.SetDataObject(data, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to copy vector shape: {ex.Message}");
            }
        }

        public static BaseShape? GetClipboardData()
        {
            try
            {
                foreach (var mapping in ShapeFormats)
                {
                    if (Clipboard.TryGetData(mapping.Format, out string? json) && !string.IsNullOrEmpty(json))
                    {
                        return JsonSerializer.Deserialize(json, mapping.Type, SerializerOptions) as BaseShape;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse vector shape JSON: {ex.Message}");
            }

            return null;
        }
    }
}