
namespace PixelEditor
{
    public class HistoryItem(float zoom, PointF offset, List<Layer> layers)
    {
        public float Zoom { get; set; } = zoom;

        public PointF Offset { get; set; } = offset;

        public List<Layer> Layers { get; set; } = CopyLayers(layers);

        private static List<Layer> CopyLayers(List<Layer> copyLayers)
        {
            List<Layer> copy = [];
            copy.AddRange(copyLayers);
            return copy;
        }

        public HistoryItem GetCopy()
        {
            HistoryItem item = new(Zoom, Offset, CopyLayers(Layers));
            return item;
        }
    }
}
