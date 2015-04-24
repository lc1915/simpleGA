using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace simpleGA
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Text = app.value.ToString();
            app.value = int.Parse(textBox1.Text);
            app.value0 = int.Parse(textBox2.Text);
            this.Hide();
        }
    }
}
