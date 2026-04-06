using System;
using System.Windows.Forms;

namespace PixelEditor
{
    public partial class FormRotate : Form
    {
        public float RotationAngle = 0.0f;

        public FormRotate()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            RotationAngle = (float)numAngle.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}