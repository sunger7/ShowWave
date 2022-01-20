using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShowWave
{
    public partial class myDialog : Form
    {
        Action<Reflectivity,bool> UpdateReflect = null;

        public myDialog(Action<Reflectivity,bool> UpdateReflect)
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
            button2.DialogResult = DialogResult.Cancel;
            this.UpdateReflect = UpdateReflect;

        }

        private void myDialog_Load(object sender, EventArgs e)
        {

        }
        public void SetValue(double t,double strength)
        {
            numericUpDown1.Value = Convert.ToDecimal( t );
            textBox1.Text = strength.ToString();
        }



        private void myDialog_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                button2.PerformClick();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                button2.PerformClick();
        }



        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                button2.PerformClick();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            UpdateReflect(new Reflectivity(Math.Round( Convert.ToDouble(numericUpDown1.Value),3), Convert.ToDouble(textBox1.Text)),true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateReflect(new Reflectivity(Math.Round( Convert.ToDouble(numericUpDown1.Value),3), Convert.ToDouble(textBox1.Text)),false);
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if(numericUpDown1.Focused)
                UpdateReflect(new Reflectivity(Math.Round(Convert.ToDouble(numericUpDown1.Value), 3), Convert.ToDouble(textBox1.Text)), true);

        }
    }
}
