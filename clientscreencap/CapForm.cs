using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientScreenCap
{
    public partial class CapForm : Form
    {
        public CapForm()
        {
            InitializeComponent();

        }

        private void CapForm_Load(object sender, EventArgs e)
        {

        }

        private void CapForm_Paint(object sender, PaintEventArgs e)
        {
            var hb = new SolidBrush(Color.FromArgb(32, 255, 255, 255));

            e.Graphics.FillRectangle(hb, this.DisplayRectangle);
        }
    }
}
