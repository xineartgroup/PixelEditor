namespace PixelEditor
{
    public partial class FormBlur : Form
    {
        public Image? Image = null;

        public FormBlur()
        {
            InitializeComponent();
        }

        private void FormBlur_Load(object sender, EventArgs e)
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
                lblRadius.Text = $"{trackBar1.Value}";
                pictureSample.Image = new Bitmap(Image);
                pictureSample.Image = ManipulatorLighting.GaussianBlur((Bitmap)pictureSample.Image, trackBar1.Value, trackBar2.Value);
            }
        }
    }
}
