public class ImageAdjustment(string name, List<float> values)
{
    public string Name { get; set; } = name;

    public List<float> Values { get; set; } = values;

    public Dictionary<string, List<Point>> Curves { get; set; } = InitializeCurves();

    public bool IsActive { get; set; } = true;

    private static Dictionary<string, List<Point>> InitializeCurves()
    {
        string[] channels = ["RGB", "R", "G", "B"];
        var curves = new Dictionary<string, List<Point>>();

        foreach (string channel in channels)
        {
            curves[channel] =
            [
                new Point(0, 0),
                new Point(255, 255)
            ];
        }

        return curves;
    }

    public override string ToString()
    {
        return $"{Name}: {string.Join(", ", Values)}";
    }
}