namespace PixelEditor
{
    public static class HistoryManager
    {
        private static readonly Stack<byte[]> undoStack = new();
        private static readonly Stack<byte[]> redoStack = new();
        private static byte[] current = [];

        public static void RecordState(HistoryItem history)
        {
            try
            {
                if (current.Length == 0)
                {
                    using MemoryStream ms = new();
                    XPESaver.Save(ms, history.Layers, history.SelectedLayerIndex);
                    undoStack.Push(ms.ToArray());
                    redoStack.Clear();
                }
                else
                {
                    undoStack.Push(current);
                    redoStack.Clear();
                }
            }
            catch
            {
            }
        }

        public static void CurrentState(HistoryItem history)
        {
            try
            {
                using MemoryStream ms = new();
                XPESaver.Save(ms, history.Layers, history.SelectedLayerIndex);
                current = ms.ToArray();
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

                redoStack.Push(current);
                current = undoStack.Pop();

                using MemoryStream restoreMs = new(current);
                XPELoader.Load(restoreMs, out List<Layer> layers, out int selectedLayerIndex);

                return new HistoryItem(layers, selectedLayerIndex);
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

                undoStack.Push(current);
                current = redoStack.Pop();

                using MemoryStream restoreMs = new(current);
                XPELoader.Load(restoreMs, out List<Layer> layers, out int selectedLayerIndex);

                return new HistoryItem(layers, selectedLayerIndex);
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
            current = [];
        }

        public static bool CanUndo => undoStack.Count > 0;

        public static bool CanRedo => redoStack.Count > 0;
    }
}