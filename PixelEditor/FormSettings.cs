namespace PixelEditor
{
    public partial class FormSettings : Form
    {
        public int LayerWidth = 2;
        public int LayerHeight = 2;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            canvasWidth.Value = LayersManipulator.Width;
            canvasHeight.Value = LayersManipulator.Height;
            layerWidth.Value = LayerWidth >= layerWidth.Minimum && LayerWidth <= layerWidth.Maximum ? LayerWidth : 2;
            layerHeight.Value = LayerHeight >= layerHeight.Minimum && LayerHeight <= layerHeight.Maximum ? LayerHeight : 2;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            LayersManipulator.Width = (int)canvasWidth.Value;
            LayersManipulator.Height = (int)canvasHeight.Value;
            LayerWidth = (int)layerWidth.Value;
            LayerHeight = (int)layerHeight.Value;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
