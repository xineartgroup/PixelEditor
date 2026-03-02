namespace PixelEditor
{
    public partial class FormName : Form
    {
        public string StrokeName = "";

        public FormName()
        {
            InitializeComponent();
        }

        private void FormName_Load(object sender, EventArgs e)
        {
            textBoxName.Text = StrokeName.Replace("☑ ", "").Replace("☐ ", "");
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            StrokeName = textBoxName.Text;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            StrokeName = "";
        }
    }
}
