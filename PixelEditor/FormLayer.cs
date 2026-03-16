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
            width.Value = Layer.Image?.Width ?? LayersManipulator.Width;
            height.Value = Layer.Image?.Height ?? LayersManipulator.Height;
            offsetX.Value = Layer.X;
            offsetY.Value = Layer.Y;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Layer.Name = textBoxName.Text;
            Layer.BlendMode = !string.IsNullOrEmpty(cboBlendMode.Text) ? Enum.Parse<ImageBlending>(cboBlendMode.Text) : ImageBlending.Normal;
            Layer.Opacity = opacity.Value;
            Layer.FillColor = btnBackgroundColor.BackColor;
            Layer.FillType = cboFillWith.Text == "Transparency" ? FillType.Transparency : FillType.Color;
            Layer.Image = (Layer.Image != null) ?
                ImageManipulator.CropFromCenter(Layer.Image, (int)width.Value, (int)height.Value) :
                ImageManipulator.GetImage(Layer.FillType == FillType.Transparency ? Color.Transparent : Layer.FillColor, (int)width.Value, (int)height.Value);
            Layer.X = (int)offsetX.Value;
            Layer.Y = (int)offsetY.Value;

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
            offsetX.Value = (LayersManipulator.Width - width.Value) / 2;
        }

        private void BtnCenterY_Click(object sender, EventArgs e)
        {
            offsetY.Value = (LayersManipulator.Height - height.Value) / 2;
        }
    }
}
