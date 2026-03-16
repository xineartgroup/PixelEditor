namespace PixelEditor
{
    public static class HistoryManager
    {
        private static readonly Stack<byte[]> undoStack = new();
        private static readonly Stack<byte[]> redoStack = new();

        public static void RecordState(HistoryItem history)
        {
            try
            {
                using MemoryStream ms = new();
                XPESaver.Save(ms, history.Zoom, history.Offset, history.Layers, history.SelectedLayerIndex);
                undoStack.Push(ms.ToArray());
                redoStack.Clear();
            }
            catch
            {
            }
        }

        public static HistoryItem? Undo()
        {
            try
            {
                if (undoStack.Count == 0) return null;

                if (redoStack.Count == 0)
                {
                    redoStack.Push(undoStack.Pop()); //remove the current state from undo and put it in redo
                }

                using MemoryStream ms = new();
                byte[] data = undoStack.Pop();
                using MemoryStream restoreMs = new(data);
                XPELoader.Load(restoreMs, out float zoom, out PointF offset, out List<Layer> layers, out int selectedLayerIndex);
                redoStack.Push(data);
                return new HistoryItem(zoom, offset, layers, selectedLayerIndex);
            }
            catch
            {
                return null;
            }
        }

        public static HistoryItem? Redo()
        {
            try
            {
                if (redoStack.Count == 0) return null;

                if (undoStack.Count == 0)
                {
                    undoStack.Push(redoStack.Pop()); //remove the first state from redo and put it in undo
                }

                using MemoryStream ms = new();
                byte[] data = redoStack.Pop();
                using MemoryStream restoreMs = new(data);
                XPELoader.Load(restoreMs, out float zoom, out PointF offset, out List<Layer> layers, out int selectedLayerIndex);
                undoStack.Push(data);
                return new HistoryItem(zoom, offset, layers, selectedLayerIndex);
            }
            catch
            {
                return null;
            }
        }

        public static void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }

        public static bool CanUndo => undoStack.Count > 0;

        public static bool CanRedo => redoStack.Count > 0;
    }
}