namespace PixelEditor
{
    public partial class FormLighting : Form
    {
        public Image? Image = null;

        public FormLighting()
        {
            InitializeComponent();
        }

        private void FormLighting_Load(object sender, EventArgs e)
        {
            TrackBar1_Scroll(sender, e);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            Image?.Dispose();
            Image = pictureSample.Image;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            pictureSample?.Dispose();
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            if (Image != null)
            {
                lblBrightness.Text = $"{trackBar1.Value}";
                lblContrast.Text = $"{trackBar2.Value}";
                lblExposure.Text = $"{trackBar3.Value}";
                lblHighlights.Text = $"{trackBar4.Value}";
                lblShadows.Text = $"{trackBar5.Value}";
                lblVignette.Text = $"{trackBar6.Value}";
                pictureSample.Image = new Bitmap(Image);
                pictureSample.Image = LayersManipulator.ApplyLighting((Bitmap)pictureSample.Image, 
                    (float)trackBar1.Value / 100,
                    (float)trackBar2.Value / 100,
                    (float)trackBar3.Value / 100,
                    (float)trackBar5.Value / 100,
                    (float)trackBar4.Value / 100,
                    (float)trackBar6.Value / 100);
            }
        }
    }
}
