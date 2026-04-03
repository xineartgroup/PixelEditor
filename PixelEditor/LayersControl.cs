using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PixelEditor
{
    public partial class LayersControl : UserControl
    {
        private const int IMAGE_SIZE = 24;

        private readonly List<Layer> imageLayers = [];
        private int selectedLayerIndex = -1;
        public event EventHandler<LayerChangedEventArgs>? LayerChanged;
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

            ContextMenuStrip contextMenu = new();
            ToolStripMenuItem menuEdit = new("Edit Layer", null, (s, e) => BtnEditCaption_Click(panelLayer, e));
            ToolStripMenuItem menuDelete = new("Delete Layer", null, (s, e) => ContextMenu_Delete_Click(panelLayer));
            ToolStripMenuItem menuRaise = new("Raise Layer", null, (s, e) => ContextMenu_Raise_Click(panelLayer));
            ToolStripMenuItem menuLower = new("Lower Layer", null, (s, e) => ContextMenu_Lower_Click(panelLayer));
            ToolStripMenuItem menuDuplicate = new("Duplicate Layer", null, (s, e) => ContextMenu_Duplicate_Click(panelLayer));
            ToolStripMenuItem menuMerge = new("Merge Down", null, (s, e) => ContextMenu_MergeDown_Click(panelLayer));

            contextMenu.Items.AddRange([
                menuEdit, menuDuplicate, new ToolStripSeparator(),
                menuRaise, menuLower, new ToolStripSeparator(),
                menuMerge, new ToolStripSeparator(),
                menuDelete
            ]);

            panelLayer.ContextMenuStrip = contextMenu;

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
                Tag = "preview",
                ContextMenuStrip = contextMenu
            };

            PictureBox maskBox = new()
            {
                Image = layer.ImageMask,
                Size = picSize,
                Location = new Point(pictureBox.Right + 5, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Tag = "mask",
                Visible = layer.ImageMask != null,
                ContextMenuStrip = contextMenu
            };

            Label lblName = new()
            {
                Text = layer.Name,
                AutoSize = true,
                Location = new Point((maskBox.Visible ? maskBox.Right : pictureBox.Right) + 5, 15),
                Tag = "name",
                ContextMenuStrip = contextMenu
            };

            chkVisible.Click += ChkVisible_Click;

            panelLayer.MouseDown += Layer_MouseDown;
            pictureBox.MouseDown += Layer_MouseDown;
            maskBox.MouseDown += Layer_MouseDown;
            lblName.MouseDown += Layer_MouseDown;

            panelLayer.DoubleClick += BtnEditCaption_Click;
            pictureBox.DoubleClick += BtnEditCaption_Click;
            maskBox.DoubleClick += BtnEditCaption_Click;
            lblName.DoubleClick += BtnEditCaption_Click;

            panelLayer.Controls.Add(chkVisible);
            panelLayer.Controls.Add(pictureBox);
            panelLayer.Controls.Add(maskBox);
            panelLayer.Controls.Add(lblName);

            return panelLayer;
        }

        private void Layer_MouseDown(object? sender, MouseEventArgs e)
        {
            Control? clickedControl = sender as Control;
            Panel? targetPanel = clickedControl as Panel;

            if (targetPanel == null && clickedControl?.Parent is Panel p)
            {
                targetPanel = p;
            }

            if (targetPanel != null)
            {
                int index = flowLayers.Controls.IndexOf(targetPanel);
                if (index >= 0 && index != selectedLayerIndex)
                {
                    int oldSelectedIndex = selectedLayerIndex;
                    selectedLayerIndex = index;
                    UpdateLayerSelection(index);
                    OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, imageLayers[selectedLayerIndex]));
                }
            }
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
                LayerType layerType = frm.Layer.LayerType;
                if (frm.ShowDialog() == DialogResult.OK)
                {
                    UpdateLayer(selectedLayerIndex, frm.Layer);
                    OnLayerChanged(new LayerChangedEventArgs(imageLayers[selectedLayerIndex], frm.Layer.LayerType != layerType));
                }
            }
        }

        private void ContextMenu_Delete_Click(Panel panel)
        {
            int index = flowLayers.Controls.IndexOf(panel);
            if (index >= 0)
            {
                RemoveLayerAt(index);
            }
        }

        private void ContextMenu_Raise_Click(Panel panel)
        {
            int index = flowLayers.Controls.IndexOf(panel);
            if (index == selectedLayerIndex)
            {
                MoveLayerUp();
            }
        }

        private void ContextMenu_Lower_Click(Panel panel)
        {
            int index = flowLayers.Controls.IndexOf(panel);
            if (index == selectedLayerIndex)
            {
                MoveLayerDown();
            }
        }

        private void ContextMenu_Duplicate_Click(Panel panel)
        {
            int index = flowLayers.Controls.IndexOf(panel);
            if (index >= 0 && index < imageLayers.Count)
            {
                Layer sourceLayer = imageLayers[index];

                Layer duplicatedLayer = sourceLayer.Clone();
                duplicatedLayer.Name = sourceLayer.Name + " Copy";

                InsertLayer(index + 1, duplicatedLayer);
            }
        }

        private void ContextMenu_MergeDown_Click(Panel panel)
        {
            int index = flowLayers.Controls.IndexOf(panel);

            if (index >= 0 && index < imageLayers.Count - 1)
            {
                Layer upperLayer = imageLayers[index];
                Layer lowerLayer = imageLayers[index + 1];

                if (lowerLayer.Image != null && upperLayer.Image != null)
                {
                    using Graphics g = Graphics.FromImage(lowerLayer.Image);
                    g.DrawImage(upperLayer.Image, 0, 0);
                }

                RemoveLayerAt(index);
                SetSelectedLayerIndex(index);
            }
            else
            {
                MessageBox.Show("There is no layer below this one to merge with.", "Cannot Merge", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        protected virtual void OnLayerChanged(LayerChangedEventArgs e)
        {
            LayerChanged?.Invoke(this, e);
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

            HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

            imageLayers.Insert(index, layer);
            RefreshLayersDisplay();
            int oldSelectedIndex = selectedLayerIndex;
            selectedLayerIndex = index;
            if (oldSelectedIndex != selectedLayerIndex)
            {
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(oldSelectedIndex, selectedLayerIndex, layer));
            }
            OnLayersCountChanged(new LayersCountChangedEventArgs(imageLayers.Count - 1, imageLayers.Count));

            HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
        }

        public void UpdateLayer(int index, Layer layer)
        {
            if (index < 0 || index >= imageLayers.Count) return;

            HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

            imageLayers[index] = layer;
            RefreshLayersDisplay();
            if (index == selectedLayerIndex)
            {
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(selectedLayerIndex, selectedLayerIndex, layer));
            }

            HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
        }

        public void RemoveLayerAt(int index)
        {
            if (index < 0 || index >= imageLayers.Count) return;

            HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

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

            HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
        }

        public void MoveLayerUp()
        {
            if (selectedLayerIndex <= 0) return;

            int currentIndex = selectedLayerIndex;
            int newIndex = currentIndex - 1;

            var layerToMove = GetLayer(currentIndex);

            if (layerToMove != null)
            {
                HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));

                HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
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
                HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));

                HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
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
                HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));

                HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
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
                HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

                imageLayers.RemoveAt(currentIndex);
                imageLayers.Insert(newIndex, layerToMove);
                selectedLayerIndex = newIndex;
                RefreshLayersDisplay();
                OnSelectedLayerChanged(new SelectedLayerChangedEventArgs(currentIndex, selectedLayerIndex, layerToMove));

                HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
            }
        }

        public void RemoveSelectedLayer()
        {
            if (selectedLayerIndex >= 0 && selectedLayerIndex < imageLayers.Count)
            {
                HistoryManager.RecordState(new HistoryItem(imageLayers, selectedLayerIndex));

                RemoveLayerAt(selectedLayerIndex);

                HistoryManager.CurrentState(new HistoryItem(imageLayers, selectedLayerIndex));
            }
        }

        public void ClearLayers()
        {
            // DO NOT RECORD HISTORY FOR THIS ACTION, AS IT IS USED WHEN LOADING A NEW IMAGE OR CREATING A NEW DOCUMENT OR WHEN UNDOING/REDOING AN ACTION.
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

            Size newPicSize = new((int)(Document.Width * (float)IMAGE_SIZE / Document.Height), IMAGE_SIZE);

            for (int i = 0; i < imageLayers.Count; i++)
            {
                Layer layer = imageLayers[i];

                if (i < flowLayers.Controls.Count)
                {
                    if (flowLayers.Controls[i] is Panel existingPanel)
                    {
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

            PictureBox? mainPic = null;
            PictureBox? maskPic = null;
            Label? lblName = null;

            foreach (Control child in p.Controls)
            {
                switch (child.Tag?.ToString())
                {
                    case "visibility":
                        ((CheckBox)child).Checked = layer.IsVisible;
                        break;
                    case "preview":
                        mainPic = (PictureBox)child;
                        mainPic.Image = layer.GetBasicImage();
                        mainPic.Size = picSize;
                        break;
                    case "mask":
                        maskPic = (PictureBox)child;
                        maskPic.Image = layer.ImageMask;
                        maskPic.Size = picSize;
                        maskPic.Visible = layer.ImageMask != null;
                        maskPic.Location = new Point(mainPic!.Right + 5, 10);
                        break;
                    case "name":
                        lblName = (Label)child;
                        lblName.Text = layer.Name;
                        break;
                }
            }

            if (lblName != null && mainPic != null && maskPic != null)
            {
                int labelX = (maskPic.Visible ? maskPic.Right : mainPic.Right) + 5;
                lblName.Location = new Point(labelX, 15);
            }
        }
    }

    public class LayerChangedEventArgs(Layer layer, bool layerTypeChanged) : EventArgs
    {
        public Layer Layer { get; } = layer;
        public bool LayerTypeChanged { get; } = layerTypeChanged;
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