namespace PixelEditor
{
    public partial class FormSelections : Form
    {
        public int PixelAmount = 0;

        public FormSelections()
        {
            InitializeComponent();
        }

        private void FormSelections_Load(object sender, EventArgs e)
        {
            TrackBar1_Scroll(sender, e);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            PixelAmount = trackBar1.Value;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            lblAmount.Text = $"{trackBar1.Value}";
        }
    }
}
