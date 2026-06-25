namespace PixelEditor
{
    public class ImageAdjustment(string name, List<float> values)
    {
        public string Name { get; set; } = name;

        public List<float> Values { get; set; } = values;

        public bool IsActive { get; set; } = true;

        public override string ToString()
        {
            return $"{Name}: {string.Join(", ", Values)}";
        }
    }
}
