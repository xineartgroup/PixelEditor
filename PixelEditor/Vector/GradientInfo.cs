using System.Globalization;

namespace PixelEditor.Vector
{
    public class GradientInfo
    {
        public string Id { get; set; } = "";
        public bool IsRadial { get; set; }
        public List<GradientStop> Stops { get; set; } = [];

        // Linear gradient properties
        public float X1 { get; set; }
        public float Y1 { get; set; }
        public float X2 { get; set; }
        public float Y2 { get; set; }

        // Radial gradient properties
        public float Cx { get; set; }
        public float Cy { get; set; }
        public float R { get; set; }
        public float Fx { get; set; }
        public float Fy { get; set; }

        // Transform properties
        public bool HasTransform { get; set; }
        public float TransformA { get; set; } = 1;
        public float TransformB { get; set; }
        public float TransformC { get; set; }
        public float TransformD { get; set; } = 1;
        public float TransformE { get; set; }
        public float TransformF { get; set; }

        public bool UserSpaceOnUse { get; set; } = false;

        public System.Drawing.Drawing2D.Matrix? TransformMatrix { get; set; }

        private static readonly char[] separator = [',', ' ', '\t', '\n', '\r'];

        // Parse and store as Matrix
        public void SetTransform(string transform)
        {
            if (string.IsNullOrWhiteSpace(transform)) return;

            transform = transform.Trim();

            if (transform.StartsWith("matrix", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    int start = transform.IndexOf('(');
                    int end = transform.LastIndexOf(')');

                    if (start >= 0 && end > start)
                    {
                        string content = transform.Substring(start + 1, end - start - 1);
                        string[] parts = content.Split(separator,
                            StringSplitOptions.RemoveEmptyEntries);

                        if (parts.Length >= 6)
                        {
                            float[] elements = new float[6];
                            for (int i = 0; i < 6 && i < parts.Length; i++)
                            {
                                if (float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out float val))
                                    elements[i] = val;
                            }

                            TransformMatrix = new System.Drawing.Drawing2D.Matrix(
                                elements[0], elements[1],
                                elements[2], elements[3],
                                elements[4], elements[5]);
                            HasTransform = true;
                        }
                    }
                }
                catch { }
            }
        }
    }
}
