public class ImageAdjustment(string name, List<float> values)
{
    public string Name { get; set; } = name;

    public List<float> Values { get; set; } = values;

    public List<Point> Points { get; set; } = [
            new Point(0, 0),
            new Point(255, 255)
        ];

    public bool IsActive { get; set; } = true;

    public override string ToString()
    {
        return $"{Name}: {string.Join(", ", Values)}";
    }
}