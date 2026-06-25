using PixelEditor.Vector;
using System.Drawing.Drawing2D;

namespace PixelEditor
{
    public partial class FormLayer : Form
    {
        public Layer Layer = new("Layer 1", true);
        public List<Layer> Layers = [];

        private readonly GroupBox groupVectorProperties = new();
        private readonly CheckedListBox listBoxVector = new();

        public FormLayer()
        {
            InitializeComponent();
            InitializeVectorGroupComponent();
        }

        private void InitializeVectorGroupComponent()
        {
            groupVectorProperties.Location = groupRasterProperties.Location;
            groupVectorProperties.Size = groupRasterProperties.Size;
            groupVectorProperties.TabIndex = 45;
            groupVectorProperties.TabStop = false;
            groupVectorProperties.Text = "Vector Properties";

            listBoxVector.Location = new Point(22, 32);
            listBoxVector.Size = new Size(300, 200);
            listBoxVector.TabIndex = 0;
            listBoxVector.SelectedIndexChanged += ListBoxVector_SelectedIndexChanged_Handler;
            listBoxVector.ItemCheck += ListBoxVector_ItemCheck_Handler;

            groupVectorProperties.Controls.Add(listBoxVector);

            Controls.Add(groupVectorProperties);
        }

        private void FormName_Load(object sender, EventArgs e)
        {
            int maskIndex = 0;
            cboType.SelectedIndex = Layer.LayerType == LayerType.Vector ? 1 : 0;
            textBoxName.Text = Layer.Name;
            btnBackgroundColor.BackColor = Layer.FillColor;
            cboFillWith.Text = Layer.FillType == FillType.Transparency ? "Transparency" : "Color";
            width.Value = Layer.Image?.Width ?? Document.Width;
            height.Value = Layer.Image?.Height ?? Document.Height;
            offsetX.Value = Layer.X;
            offsetY.Value = Layer.Y;
            cboLayers.Items.Add("None");

            foreach (ImageAdjustment adjustment in Layer.Adjustments)
            {
                listBoxAdjustments.Items.Add(adjustment.Name);
            }

            int index = 0;
            foreach (Layer layer in Layers)
            {
                if (layer.Name != Layer.Name)
                {
                    index++;
                    cboLayers.Items.Add(layer.Name);
                    if (ManipulatorGeneral.AreBitmapsEqual(layer.Image, Layer.ImageMask))
                    {
                        maskIndex = index;
                    }
                }
            }
            cboLayers.SelectedIndex = maskIndex;
            pictureMask.Image = Layer.ImageMask;

            for (int i = 0; i < Layer.Shapes.Count; i++)
            {
                BaseShape shape = Layer.Shapes[i];
                listBoxVector.Items.Add(shape.GetType().Name, shape.Visible);
                if (shape == Layer.CurrentShape)
                {
                    listBoxVector.SelectedIndex = i;
                }
            }

            layerImage.Image = Layer.Image;
        }

        private void CboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboType.SelectedIndex == 0)
            {
                groupRasterProperties.Visible = true;
                groupVectorProperties.Visible = false;
            }
            else
            {
                groupRasterProperties.Visible = false;
                groupVectorProperties.Visible = true;
            }
        }

        private void ListBoxVector_SelectedIndexChanged_Handler(object? sender, EventArgs e)
        {
            if (listBoxVector.SelectedIndex >= 0 && listBoxVector.SelectedIndex < Layer.Shapes.Count)
            {
                Layer.CurrentShape = Layer.Shapes[listBoxVector.SelectedIndex];
            }
        }

        private void ListBoxVector_ItemCheck_Handler(object? sender, ItemCheckEventArgs e)
        {
            if (e.Index >= 0 && e.Index < Layer.Shapes.Count)
            {
                Layer.Shapes[e.Index].Visible = e.NewValue == CheckState.Checked;
            }
        }

        private void BtnBackgroundColor_Click(object sender, EventArgs e)
        {
            ColorDialog colorDialog = new()
            {
                Color = btnBackgroundColor.BackColor,
                FullOpen = true
            };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                btnBackgroundColor.BackColor = colorDialog.Color;
            }
        }

        private void BtnCenterX_Click(object sender, EventArgs e)
        {
            offsetX.Value = (Document.Width - width.Value) / 2;
        }

        private void BtnCenterY_Click(object sender, EventArgs e)
        {
            offsetY.Value = (Document.Height - height.Value) / 2;
        }

        private void BtnAutoWidth_Click(object sender, EventArgs e)
        {
            width.Value = Document.Width;
        }

        private void BtnAutoHeight_Click(object sender, EventArgs e)
        {
            height.Value = Document.Height;
        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboLayers.SelectedIndex == 0)
            {
                pictureMask.Image = null;
            }
            else if (cboLayers.SelectedIndex - 1 >= 0 && cboLayers.SelectedIndex - 1 < Layers.Count)
            {
                pictureMask.Image = Layers[cboLayers.SelectedIndex - 1].Image;
            }
            else
            {
                pictureMask.Image = null;
            }
        }

        private void CboFillWith_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboFillWith.Text == "Transparency")
            {
                btnBackgroundColor.Enabled = false;
                btnBackgroundColor.BackColor = Color.Transparent;
            }
            else
            {
                btnBackgroundColor.Enabled = true;
                btnBackgroundColor.BackColor = Layer.FillColor;
            }
        }

        private void CboAdjustments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(cboAdjustments.Text.Trim()))
            {
                listBoxAdjustments.Items.Add(cboAdjustments.Text);
                if (cboAdjustments.Text == "Blur")
                {
                    Layer.Adjustments.Add(new ImageAdjustment(cboAdjustments.Text, [trackBarAdjustmentValue1.Value / 100f, trackBarAdjustmentValue2.Value / 100f]));
                    lblAdjustmentValue1.Text = $"{trackBarAdjustmentValue1.Value / (float)trackBarAdjustmentValue1.Maximum:F2}";
                    lblAdjustmentValue2.Text = $"{trackBarAdjustmentValue2.Value / (float)trackBarAdjustmentValue2.Maximum:F2}";
                    trackBarAdjustmentValue1.Visible = true;
                    lblAdjustmentValue1.Visible = true;
                    trackBarAdjustmentValue2.Visible = true;
                    lblAdjustmentValue2.Visible = true;
                }
                else
                {
                    Layer.Adjustments.Add(new ImageAdjustment(cboAdjustments.Text, [trackBarAdjustmentValue1.Value / 100f]));
                    lblAdjustmentValue1.Text = $"{trackBarAdjustmentValue1.Value / (float)trackBarAdjustmentValue1.Maximum:F2}";
                    trackBarAdjustmentValue1.Visible = true;
                    lblAdjustmentValue1.Visible = true;
                }
                cboAdjustments.SelectedIndex = 0;
                listBoxAdjustments.SelectedIndex = listBoxAdjustments.Items.Count - 1;
                layerImage.Image = Layer.Image;
            }
        }

        private void TrackBarAdjustmentValue_Scroll(object sender, EventArgs e)
        {
            lblAdjustmentValue1.Text = $"{trackBarAdjustmentValue1.Value / (float)trackBarAdjustmentValue1.Maximum:F2}";
            lblAdjustmentValue2.Text = $"{trackBarAdjustmentValue2.Value / (float)trackBarAdjustmentValue2.Maximum:F2}";
            if (listBoxAdjustments.SelectedIndex >= 0 && listBoxAdjustments.SelectedIndex < Layer.Adjustments.Count)
            {
                if (Layer.Adjustments[listBoxAdjustments.SelectedIndex].Values.Count > 0)
                {
                    Layer.Adjustments[listBoxAdjustments.SelectedIndex].Values[0] = (float)trackBarAdjustmentValue1.Value / 100;
                }

                if (Layer.Adjustments[listBoxAdjustments.SelectedIndex].Values.Count > 1)
                {
                    Layer.Adjustments[listBoxAdjustments.SelectedIndex].Values[1] = (float)trackBarAdjustmentValue2.Value / 100;
                }

                layerImage.Image = Layer.Image;
            }
        }

        private void ListBoxAdjustments_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxAdjustments.SelectedIndex >= 0 && listBoxAdjustments.SelectedIndex < Layer.Adjustments.Count)
            {
                ImageAdjustment selectedAdjustment = Layer.Adjustments[listBoxAdjustments.SelectedIndex];
                if (selectedAdjustment.Values.Count > 0)
                {
                    trackBarAdjustmentValue1.Visible = true;
                    lblAdjustmentValue1.Visible = true;
                    trackBarAdjustmentValue1.Value = (int)(selectedAdjustment.Values[0] * 100);
                    lblAdjustmentValue1.Text = $"{trackBarAdjustmentValue1.Value / (float)trackBarAdjustmentValue1.Maximum:F2}";
                }
                else
                {
                    trackBarAdjustmentValue1.Visible = false;
                    lblAdjustmentValue1.Visible = false;
                }

                if (selectedAdjustment.Values.Count > 1)
                {
                    trackBarAdjustmentValue2.Visible = true;
                    lblAdjustmentValue2.Visible = true;
                    trackBarAdjustmentValue2.Value = (int)(selectedAdjustment.Values[1] * 100);
                    lblAdjustmentValue2.Text = $"{trackBarAdjustmentValue2.Value / (float)trackBarAdjustmentValue2.Maximum:F2}";
                }
                else
                {
                    trackBarAdjustmentValue2.Visible = false;
                    lblAdjustmentValue2.Visible = false;
                }
            }
        }

        private void BtnRemoveAdjustment_Click(object sender, EventArgs e)
        {
            if (listBoxAdjustments.SelectedIndex >= 0 && listBoxAdjustments.SelectedIndex < listBoxAdjustments.Items.Count)
            {
                Layer.Adjustments.RemoveAt(listBoxAdjustments.SelectedIndex);
                listBoxAdjustments.Items.RemoveAt(listBoxAdjustments.SelectedIndex);
                listBoxAdjustments.SelectedIndex = -1;
                layerImage.Image = Layer.Image;
                trackBarAdjustmentValue1.Visible = false;
                trackBarAdjustmentValue2.Visible = false;
            }
        }

        private void BtnMoveAdjustmentUp_Click(object sender, EventArgs e)
        {
            if (listBoxAdjustments.SelectedIndex > 0)
            {
                int selectedIndex = listBoxAdjustments.SelectedIndex;
                object selectedItem = listBoxAdjustments.SelectedItem;
                listBoxAdjustments.Items.RemoveAt(selectedIndex);
                listBoxAdjustments.Items.Insert(selectedIndex - 1, selectedItem);
                listBoxAdjustments.SelectedIndex = selectedIndex - 1;
                layerImage.Image = Layer.Image;
            }
        }

        private void BtnMoveAdjustmentDown_Click(object sender, EventArgs e)
        {
            if (listBoxAdjustments.SelectedIndex < listBoxAdjustments.Items.Count - 1)
            {
                int selectedIndex = listBoxAdjustments.SelectedIndex;
                object selectedItem = listBoxAdjustments.SelectedItem;
                listBoxAdjustments.Items.RemoveAt(selectedIndex);
                listBoxAdjustments.Items.Insert(selectedIndex + 1, selectedItem);
                listBoxAdjustments.SelectedIndex = selectedIndex + 1;
                layerImage.Image = Layer.Image;
            }
        }

        private void BtnAdjustmentVisibility_Click(object sender, EventArgs e)
        {
            if (listBoxAdjustments.SelectedIndex >= 0 && listBoxAdjustments.SelectedIndex < Layer.Adjustments.Count)
            {
                ImageAdjustment selectedAdjustment = Layer.Adjustments[listBoxAdjustments.SelectedIndex];
                selectedAdjustment.IsActive = !selectedAdjustment.IsActive;
                layerImage.Image = Layer.Image;

            }
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Layer.LayerType = cboType.SelectedIndex == 1 ? LayerType.Vector : LayerType.Image;
            Layer.Name = textBoxName.Text;
            Layer.FillColor = cboFillWith.Text == "Transparency" ? Color.Transparent : btnBackgroundColor.BackColor;
            Layer.FillType = cboFillWith.Text == "Transparency" ? FillType.Transparency : FillType.Color;
            Layer.X = (int)offsetX.Value;
            Layer.Y = (int)offsetY.Value;

            for (int i = 0; i < Layer.Shapes.Count && i < listBoxVector.Items.Count; i++)
            {
                Layer.Shapes[i].Visible = listBoxVector.GetItemChecked(i);
            }

            if (Layer.CurrentShape != null && !Layer.CurrentShape.Visible)
            {
                Layer.CurrentShape = null;
            }

            Image image = ManipulatorGeneral.GetImage(Layer.FillColor, (int)width.Value, (int)height.Value) ?? new Bitmap((int)width.Value, (int)height.Value);
            Image? basicImage = Layer.GetBasicImage();
            if (basicImage != null)
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    float ratioX = (float)image.Width / basicImage.Width;
                    float ratioY = (float)image.Height / basicImage.Height;
                    float ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(basicImage.Width * ratio);
                    int newHeight = (int)(basicImage.Height * ratio);

                    int posX = (image.Width - newWidth) / 2;
                    int posY = (image.Height - newHeight) / 2;

                    g.InterpolationMode = InterpolationMode.NearestNeighbor; //InterpolationMode.HighQualityBicubic;

                    g.DrawImage(basicImage, posX, posY, newWidth, newHeight);
                }

                basicImage.Dispose();
            }
            Layer.Image = image;

            if (pictureMask.Image != null)
            {
                Layer.ImageMask = new Bitmap(pictureMask.Image);
            }
            else
            {
                Layer.ImageMask = null;
            }

            DialogResult = DialogResult.OK;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}