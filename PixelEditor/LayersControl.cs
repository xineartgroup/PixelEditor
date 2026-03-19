namespace PixelEditor
{
    public partial class LayersControl : UserControl
    {
        private const int IMAGE_SIZE = 24;

        private readonly List<Layer> imageLayers = [];
        private int selectedLayerIndex = -1;
        public event EventHandler<LayerVisibilityChangedEventArgs>? LayerVisibilityChanged;
        public event EventHandler<SelectedLayerChangedEventArgs>? SelectedLayerChanged;
        public event EventHandler<LayersCountChangedEventArgs>? LayerCountChanged;

        public LayersControl()
        {
            InitializeComponent();
        }

        private Panel GetLayerPanel(Layer layer)
        {
            Size picSize = new((int)(Document.Width * (float)IMAGE_SIZE / Document.Height), IMAGE_SIZE);

            Panel panelLayer = new()
            {
                Name = layer.Name,
                Size = new Size(160, 50),
                Margin = new Padding(5, 2, 2, 2),
                Tag = layer,
                BorderStyle = BorderStyle.None
            };

            CheckBox chkVisible = new()
            {
                Checked = layer.IsVisible,
                Location = new Point(5, 15),
                Size = new Size(15, 15),
                Tag = "visibility"
            };

            PictureBox pictureBox = new()
            {
                Image = layer.Image,
                Size = picSize,
                Location = Point.Add(new Point(30, 10), new Size(0, 0)),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = "preview"
            };

            Label lblName = new()
            {
                Text = layer.Name,
                AutoSize = true,
                Location = new Point(pictureBox.Left + pictureBox.Width + 5, 15),
                Tag = "name"
            };

            chkVisible.Click += ChkVisible_Click;
            panelLayer.Click += GroupLayer_Click;
            pictureBox.Click += GroupLayer_Click;
            lblName.Click += GroupLayer_Click;

            panelLayer.DoubleClick += BtnEditCaption_Click;
            pictureBox.DoubleClick += BtnEditCaption_Click;
            lblName.DoubleClick += BtnEditCaption_Click;

            panelLayer.Controls.Add(chkVisible);
            panelLayer.Controls.Add(pictureBox);
            panelLayer.Controls.Add(lblName);

            return panelLayer;
        }

        private void ChkVisible_Click(object? sender, EventArgs e)
        {
            if (sender is CheckBox chk)
            {
                int layerIndex = -1;
                foreach (Control control in flowLayers.Controls)
                {
                    if (control is Panel panel)
                    {
                        layerIndex++;
                        if (panel.Controls.Contains(chk))
                        {
                            bool oldValue = imageLayers[layerIndex].IsVisible;
                            bool newValue = chk.Checked;

                            if (oldValue != newValue)
                            {
                                imageLayers[layerIndex].IsVisible = newValue;
                                OnLayerVisibilityChanged(new LayerVisibilityChangedEventArgs(
                                    layerIndex,
                                    imageLayers[layerIndex],
                                    oldValue,
                                    newValue
                                ));
                            }

                            int oldSelectedIndex = selectedLayerIndex;
                            selectedLayerIndex = layerIndex;
                            UpdateLayerSelection(layerIndex);
                            if (oldSelectedIndex != selectedLayerIndex)
                            {
                                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, imageLayers[selectedLayerIndex]));
                            }
                            break;
                        }
                    }
                }
            }
        }

        private void GroupLayer_Click(object? sender, EventArgs e)
        {
            int layerIndex = -1;
            foreach (Control control in flowLayers.Controls)
            {
                if (control is Panel panel)
                {
                    layerIndex++;
                    if (panel == sender ||
                        (sender is PictureBox pic && panel.Controls.Contains(pic)) ||
                        (sender is Label lbl && panel.Controls.Contains(lbl)))
                    {
                        int oldSelectedIndex = selectedLayerIndex;
                        selectedLayerIndex = layerIndex;
                        UpdateLayerSelection(layerIndex);
                        if (oldSelectedIndex != selectedLayerIndex)
                        {
                            OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, imageLayers[selectedLayerIndex]));
                        }
                        break;
                    }
                }
            }
        }

        private void BtnEditCaption_Click(object? sender, EventArgs e)
        {
            FormLayer frm = new()
            {
                StartPosition = FormStartPosition.CenterParent
            };

            var layer = GetLayer(selectedLayerIndex);

            if (layer != null)
            {
                frm.Layer = layer;
                frm.Layers = imageLayers;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    HistoryManager.RecordState(new HistoryItem(GetLayers(), GetSelectedLayerIndex()));
                    layer = frm.Layer;
                    UpdateLayer(selectedLayerIndex, layer);
                    HistoryManager.CurrentState(new HistoryItem(GetLayers(), GetSelectedLayerIndex()));
                }
            }
        }

        private void UpdateLayerSelection(int selectedIndex)
        {
            int layerIndex = -1;
            foreach (Control control in flowLayers.Controls)
            {
                if (control is Panel panel)
                {
                    layerIndex++;
                    panel.BackColor = (layerIndex == selectedIndex) ? Color.LightBlue : Color.Transparent;
                }
            }
        }

        protected virtual void OnLayerVisibilityChanged(LayerVisibilityChangedEventArgs e)
        {
            LayerVisibilityChanged?.Invoke(this, e);
        }

        protected virtual void OnSelectedLayerChanged(SelectedLayerChangedEventArgs e)
        {
            SelectedLayerChanged?.Invoke(this, e);
        }

        protected virtual void OnLayersCountChanged(LayersCountChangedEventArgs e)
        {
            LayerCountChanged?.Invoke(this, e);
        }

        public List<Layer> GetLayers() => imageLayers;

        public Layer? GetLayer(int index)
        {
            if (index < 0 || index >= imageLayers.Count) return null;
            return imageLayers[index];
        }

        public int GetSelectedLayerIndex() => selectedLayerIndex;

        public void SetSelectedLayerIndex(int index)
        {
            if (index < 0 || index >= imageLayers.Count) return;
            int oldIndex = selectedLayerIndex;
            selectedLayerIndex = index;
            UpdateLayerSelection(index);
            OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldIndex, index, imageLayers[index]));
        }

        public void AddLayer(Layer layer)
        {
            imageLayers.Add(layer);
            RefreshLayersDisplay();
            int oldSelectedIndex = selectedLayerIndex;
            selectedLayerIndex = imageLayers.Count - 1;
            if (oldSelectedIndex != selectedLayerIndex)
            {
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, layer));
            }
            OnLayersCountChanged(new LayersCountChangedEventArgs(imageLayers.Count - 1, imageLayers.Count));
        }

        public void AddRange(List<Layer> layers)
        {
            if (layers.Count > 0)
            {
                imageLayers.AddRange(layers);
                RefreshLayersDisplay();
                int oldSelectedIndex = selectedLayerIndex;
                selectedLayerIndex = imageLayers.Count - 1;
                if (oldSelectedIndex != selectedLayerIndex && layers.Count > 0)
                {
                    OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, layers.Last()));
                }
                if (layers.Count > 0)
                {
                    OnLayersCountChanged(new LayersCountChangedEventArgs(imageLayers.Count - layers.Count, imageLayers.Count));
                }
            }
        }

        public void InsertLayer(int index, Layer layer)
        {
            if (index < 0 || index > imageLayers.Count) return;
            imageLayers.Insert(index, layer);
            RefreshLayersDisplay();
            int oldSelectedIndex = selectedLayerIndex;
            selectedLayerIndex = index;
            if (oldSelectedIndex != selectedLayerIndex)
            {
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, layer));
            }
            OnLayersCountChanged(new LayersCountChangedEventArgs(imageLayers.Count - 1, imageLayers.Count));
        }

        public void UpdateLayer(int index, Layer layer)
        {
            if (index < 0 || index >= imageLayers.Count) return;
            imageLayers[index] = layer;
            RefreshLayersDisplay();
            if (index == selectedLayerIndex)
            {
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(selectedLayerIndex, selectedLayerIndex, layer));
            }
        }

        public void RemoveLayerAt(int index)
        {
            if (index < 0 || index >= imageLayers.Count) return;
            imageLayers.RemoveAt(index);
            RefreshLayersDisplay();
            int oldSelectedIndex = selectedLayerIndex;
            selectedLayerIndex = imageLayers.Count > 0 ? Math.Min(index, imageLayers.Count - 1) : -1;
            if (oldSelectedIndex != selectedLayerIndex)
            {
                Layer? newSelectedLayer = selectedLayerIndex >= 0 ? imageLayers[selectedLayerIndex] : null;
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, newSelectedLayer));
            }
            OnLayersCountChanged(new LayersCountChangedEventArgs(imageLayers.Count + 1, imageLayers.Count));
        }

        public void MoveLayerUp()
        {
            if (selectedLayerIndex <= 0) return;

            int currentIndex = selectedLayerIndex;
            int newIndex = currentIndex - 1;

            var layerToMove = GetLayer(currentIndex);

            if (layerToMove != null)
            {
                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));
            }
        }

        public void MoveLayerDown()
        {
            if (selectedLayerIndex < 0 || selectedLayerIndex >= imageLayers.Count - 1) return;

            int currentIndex = selectedLayerIndex;
            int newIndex = currentIndex + 1;

            var layerToMove = GetLayer(currentIndex);

            if (layerToMove != null)
            {
                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));
            }
        }

        public void MoveLayerToTop()
        {
            if (selectedLayerIndex < 0) return;

            int currentIndex = selectedLayerIndex;
            int newIndex = 0;

            var layerToMove = GetLayer(currentIndex);

            if (layerToMove != null)
            {
                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));
            }
        }

        public void MoveLayerToBottom()
        {
            if (selectedLayerIndex < 0 || selectedLayerIndex >= imageLayers.Count - 1) return;

            int currentIndex = selectedLayerIndex;
            int newIndex = imageLayers.Count - 1;

            var layerToMove = GetLayer(currentIndex);

            if (layerToMove != null)
            {
                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));
            }
        }

        public void RemoveSelectedLayer()
        {
            if (selectedLayerIndex >= 0 && selectedLayerIndex < imageLayers.Count)
            {
                RemoveLayerAt(selectedLayerIndex);
            }
        }

        public void ClearLayers()
        {
            int oldCount = imageLayers.Count;
            imageLayers.Clear();
            flowLayers.Controls.Clear();
            int oldSelectedIndex = selectedLayerIndex;
            selectedLayerIndex = -1;
            if (oldSelectedIndex != selectedLayerIndex)
            {
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, null));
            }
            if (oldCount != imageLayers.Count)
            {
                OnLayersCountChanged(new LayersCountChangedEventArgs(oldCount, imageLayers.Count));
            }
        }

        public void RefreshLayersDisplay()
        {
            flowLayers.SuspendLayout();

            // Calculate the new target size for the preview thumbnails once
            // to avoid repeating the math inside the loop.
            Size newPicSize = new((int)(Document.Width * (float)IMAGE_SIZE / Document.Height), IMAGE_SIZE);

            for (int i = 0; i < imageLayers.Count; i++)
            {
                Layer layer = imageLayers[i];

                if (i < flowLayers.Controls.Count)
                {
                    if (flowLayers.Controls[i] is Panel existingPanel)
                    {
                        // Pass the new size to the update method
                        UpdatePanelContent(existingPanel, layer, newPicSize);
                    }
                }
                else
                {
                    flowLayers.Controls.Add(GetLayerPanel(layer));
                }
            }

            while (flowLayers.Controls.Count > imageLayers.Count)
            {
                flowLayers.Controls.RemoveAt(flowLayers.Controls.Count - 1);
            }

            UpdateLayerSelection(selectedLayerIndex);
            flowLayers.ResumeLayout();
        }

        private static void UpdatePanelContent(Panel p, Layer layer, Size picSize)
        {
            p.Tag = layer;
            p.Name = layer.Name;

            PictureBox? pictureBox = null;
            Label? nameLabel = null;

            foreach (Control child in p.Controls)
            {
                if (child is CheckBox chk && child.Tag?.ToString() == "visibility")
                {
                    chk.Checked = layer.IsVisible;
                }
                else if (child is PictureBox pic && child.Tag?.ToString() == "preview")
                {
                    pic.Image = layer.Image;
                    pic.Size = picSize; // Update the width/height based on document aspect ratio
                    pictureBox = pic;
                }
                else if (child is Label lbl && child.Tag?.ToString() == "name")
                {
                    lbl.Text = layer.Name;
                    nameLabel = lbl;
                }
            }

            // Adjust the label position in case the PictureBox width changed
            if (pictureBox != null && nameLabel != null)
            {
                nameLabel.Location = new Point(pictureBox.Left + pictureBox.Width + 5, 15);
            }
        }
    }

    public class LayerVisibilityChangedEventArgs(int layerIndex, Layer layer, bool oldValue, bool newValue) : EventArgs
    {
        public int LayerIndex { get; } = layerIndex;
        public Layer Layer { get; } = layer;
        public bool OldValue { get; } = oldValue;
        public bool NewValue { get; } = newValue;
    }

    public class SelectedLayerChangedEventArgs(int oldIndex, int newIndex, Layer? newLayer) : EventArgs
    {
        public int OldIndex { get; } = oldIndex;
        public int NewIndex { get; } = newIndex;
        public Layer? NewLayer { get; } = newLayer;
    }

    public class LayersCountChangedEventArgs(int oldCount, int newCount) : EventArgs
    {
        public int OldCount { get; } = oldCount;
        public int NewCount { get; } = newCount;
    }
}