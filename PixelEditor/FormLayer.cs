namespace PixelEditor
{
    public partial class FormLayer : Form
    {
        public Layer Layer = new("Layer 1", true);

        public FormLayer()
        {
            InitializeComponent();
        }

        private void FormName_Load(object sender, EventArgs e)
        {
            textBoxName.Text = Layer.Name;
            cboBlendMode.Items.AddRange(Enum.GetNames<ImageBlending>());
            cboBlendMode.SelectedItem = Layer.BlendMode.ToString();
            opacity.Value = Layer.Opacity;
            btnBackgroundColor.BackColor = Layer.FillColor;
            cboFillWith.Text = Layer.FillType == FillType.Transparency ? "Transparency" : "Color";
            width.Value = Layer.Image?.Width ?? ManipulatorGeneral.Width;
            height.Value = Layer.Image?.Height ?? ManipulatorGeneral.Height;
            offsetX.Value = Layer.X;
            offsetY.Value = Layer.Y;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Layer.Name = textBoxName.Text;
            Layer.BlendMode = !string.IsNullOrEmpty(cboBlendMode.Text) ? Enum.Parse<ImageBlending>(cboBlendMode.Text) : ImageBlending.Normal;
            Layer.Opacity = opacity.Value;
            Layer.FillColor = cboFillWith.Text == "Transparency" ? Color.Transparent : btnBackgroundColor.BackColor;
            Layer.FillType = cboFillWith.Text == "Transparency" ? FillType.Transparency : FillType.Color;
            Layer.X = (int)offsetX.Value;
            Layer.Y = (int)offsetY.Value;

            Image image = ManipulatorGeneral.GetImage(Layer.FillColor, (int)width.Value, (int)height.Value) ?? new Bitmap((int)width.Value, (int)height.Value);
            if (Layer.Image != null)
            {
                using (Graphics g = Graphics.FromImage(image))
                {
                    float ratioX = (float)image.Width / Layer.Image.Width;
                    float ratioY = (float)image.Height / Layer.Image.Height;
                    float ratio = Math.Min(ratioX, ratioY);

                    int newWidth = (int)(Layer.Image.Width * ratio);
                    int newHeight = (int)(Layer.Image.Height * ratio);

                    int posX = (image.Width - newWidth) / 2;
                    int posY = (image.Height - newHeight) / 2;

                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                    g.DrawImage(Layer.Image, posX, posY, newWidth, newHeight);
                }

                Layer.Image.Dispose();
            }
            Layer.Image = image;

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
                Color = btnBackgroundColor.BackColor
            };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                btnBackgroundColor.BackColor = colorDialog.Color;
            }
        }

        private void BtnCenterX_Click(object sender, EventArgs e)
        {
            offsetX.Value = (ManipulatorGeneral.Width - width.Value) / 2;
        }

        private void BtnCenterY_Click(object sender, EventArgs e)
        {
            offsetY.Value = (ManipulatorGeneral.Height - height.Value) / 2;
        }

        private void BtnAutoWidth_Click(object sender, EventArgs e)
        {
            width.Value = ManipulatorGeneral.Width;
        }

        private void BtnAutoHeight_Click(object sender, EventArgs e)
        {
            height.Value = ManipulatorGeneral.Height;
        }
    }
}
