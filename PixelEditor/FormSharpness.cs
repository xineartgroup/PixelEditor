namespace PixelEditor
{
    public partial class FormSharpness : Form
    {
        public Image? Image = null;

        public FormSharpness()
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
                lblSharpness.Text = $"{trackBar6.Value}";
                pictureSample.Image = new Bitmap(Image);
                pictureSample.Image = ManipulatorLighting.AdjustSharpness((Bitmap)pictureSample.Image, (float)trackBar6.Value / 100);
            }
        }
    }
}
