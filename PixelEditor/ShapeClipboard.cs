using PixelEditor.Vector;
using System.Text.Json;

namespace PixelEditor
{
    public static class ShapeClipboard
    {
        // A mapping of the exact classes to unique clipboard format keys
        private static readonly (Type Type, string Format)[] ShapeFormats =
        [
            (typeof(ShapeLine), "PixelEditor.Vector.ShapeLine"),
            (typeof(ShapeRect), "PixelEditor.Vector.ShapeRect"),
            (typeof(ShapeEllipse), "PixelEditor.Vector.ShapeEllipse"),
            (typeof(ShapePolygon), "PixelEditor.Vector.ShapePolygon"),
            (typeof(ShapePath), "PixelEditor.Vector.ShapePath"),
            (typeof(ShapeText), "PixelEditor.Vector.ShapeText")
        ];

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

                string json = JsonSerializer.Serialize(shape, exactType, new JsonSerializerOptions
                {
                    WriteIndented = false
                });

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
                        if (JsonSerializer.Deserialize(json, mapping.Type) is BaseShape concreteShape)
                        {
                            return concreteShape;
                        }
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