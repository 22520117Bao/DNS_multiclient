using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TryForBetter
{
    public partial class ThongKeForm : Form
    {
        public ThongKeForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ServerForm form1 = new ServerForm();
            form1.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Client2Form form2 = new Client2Form();  
            form2.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Client4Form form3 = new Client4Form();
            form3.Show();
        }
    }
}
