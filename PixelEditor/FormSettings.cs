namespace PixelEditor
{
    public partial class FormSettings : Form
    {
        private int tempLayerX = 0;
        private int tempLayerY = 0;

        public int LayerWidth = 2;
        public int LayerHeight = 2;
        public int LayerX = 0;
        public int LayerY = 0;

        public FormSettings()
        {
            InitializeComponent();
        }

        private void FormSettings_Load(object sender, EventArgs e)
        {
            canvasWidth.Value = ManipulatorGeneral.Width;
            canvasHeight.Value = ManipulatorGeneral.Height;
            layerWidth.Value = LayerWidth >= layerWidth.Minimum && LayerWidth <= layerWidth.Maximum ? LayerWidth : 2;
            layerHeight.Value = LayerHeight >= layerHeight.Minimum && LayerHeight <= layerHeight.Maximum ? LayerHeight : 2;
            tempLayerX = LayerX;
            tempLayerY = LayerY;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            ManipulatorGeneral.Width = (int)canvasWidth.Value;
            ManipulatorGeneral.Height = (int)canvasHeight.Value;
            LayerWidth = (int)layerWidth.Value;
            LayerHeight = (int)layerHeight.Value;
            if (ManipulatorGeneral.Width == LayerWidth && ManipulatorGeneral.Height == LayerHeight)
            {
                LayerX = tempLayerX;
                LayerY = tempLayerY;
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void BtnAutoResizeCanvas_Click(object sender, EventArgs e)
        {
            canvasWidth.Value = layerWidth.Value;
            canvasHeight.Value = layerHeight.Value;
            tempLayerX = 0;
            tempLayerY = 0;
        }

        private void BtnAutoResizeLayer_Click(object sender, EventArgs e)
        {
            layerWidth.Value = canvasWidth.Value;
            layerHeight.Value = canvasHeight.Value;
            tempLayerX = 0;
            tempLayerY = 0;
        }

        private void Values_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
