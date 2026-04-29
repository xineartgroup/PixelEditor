using PixelEditor.Vector;
using System.Drawing.Drawing2D;

namespace PixelEditor
{
    public partial class FormLayer : Form
    {
        public Layer Layer = new("Layer 1", true);
        public List<Layer> Layers = [];

        private GroupBox groupVectorProperties = new();
        private readonly ListBox listBoxVector = new();

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

            groupVectorProperties.Controls.Add(listBoxVector);

            Controls.Add(groupVectorProperties);
        }

        private void FormName_Load(object sender, EventArgs e)
        {
            int maskIndex = 0;
            cboType.SelectedIndex = Layer.LayerType == LayerType.Vector ? 1 : 0;
            textBoxName.Text = Layer.Name;
            cboBlendMode.Items.AddRange(Enum.GetNames<ImageBlending>());
            cboBlendMode.SelectedItem = Layer.BlendMode.ToString();
            opacity.Value = Layer.Opacity;
            btnBackgroundColor.BackColor = Layer.FillColor;
            cboFillWith.Text = Layer.FillType == FillType.Transparency ? "Transparency" : "Color";
            width.Value = Layer.Image?.Width ?? Document.Width;
            height.Value = Layer.Image?.Height ?? Document.Height;
            offsetX.Value = Layer.X;
            offsetY.Value = Layer.Y;
            cboLayers.Items.Add("None");
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

            foreach (BaseShape shape in Layer.Shapes)
            {
                listBoxVector.Items.Add(shape.GetType().Name);
            }
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

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Layer.LayerType = cboType.SelectedIndex == 1 ? LayerType.Vector : LayerType.Image;
            Layer.Name = textBoxName.Text;
            Layer.BlendMode = !string.IsNullOrEmpty(cboBlendMode.Text) ? Enum.Parse<ImageBlending>(cboBlendMode.Text) : ImageBlending.Normal;
            Layer.Opacity = opacity.Value;
            Layer.FillColor = cboFillWith.Text == "Transparency" ? Color.Transparent : btnBackgroundColor.BackColor;
            Layer.FillType = cboFillWith.Text == "Transparency" ? FillType.Transparency : FillType.Color;
            Layer.X = (int)offsetX.Value;
            Layer.Y = (int)offsetY.Value;

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
    }
}
