namespace PixelEditor
{
    public partial class FormColorBalance : Form
    {
        public Image? Image = null;

        public FormColorBalance()
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
                lblBrightness.Text = $"{trackBar1.Value}";
                lblWarmth.Text = $"{trackBar2.Value}";
                lblTint.Text = $"{trackBar3.Value}";
                pictureSample.Image = new Bitmap(Image);
                pictureSample.Image = ImageManipulator.AdjustColorBalance((Bitmap)pictureSample.Image, 
                    (float)trackBar1.Value / trackBar1.Maximum, 
                    (float)trackBar2.Value - 50, 
                    (float)trackBar3.Value - 50);
            }
        }
    }
}
