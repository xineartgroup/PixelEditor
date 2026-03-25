namespace PixelEditor.Vector
{
    public interface IShape
    {
        float LineWidth { get; set; }
        Color LineColor { get; set; }
        Color FillColor { get; set; }
    }
}
