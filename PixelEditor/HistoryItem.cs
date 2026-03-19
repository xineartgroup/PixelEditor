
namespace PixelEditor
{
    public class HistoryItem(List<Layer> layers, int selectedLayerIndex)
    {
        public float Zoom { get; set; } = Document.Zoom;

        public PointF Offset { get; set; } = Document.ImageOffset;

        public int SelectedLayerIndex { get; set; } = selectedLayerIndex;

        public List<Layer> Layers { get; set; } = CopyLayers(layers);

        private static List<Layer> CopyLayers(List<Layer> copyLayers)
        {
            List<Layer> copy = [];
            copy.AddRange(copyLayers);
            return copy;
        }

        public HistoryItem GetCopy()
        {
            HistoryItem item = new(CopyLayers(Layers), SelectedLayerIndex);
            return item;
        }
    }
}
