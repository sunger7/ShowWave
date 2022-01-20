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
    public partial class ValueDialg : UserControl
    {
        public ValueDialg()
        {
            InitializeComponent();
        }
        public double Value,time;
        bool IsOk = false;
        
        private void button1_Click(object sender, EventArgs e)
        {
            Value = Convert.ToDouble( textBox1.Text);
            time = Convert.ToDouble(numericUpDown1.Value);
            this.Visible = false;
            IsOk = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            IsOk = false;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        public void SetNum(double t)
        {
            numericUpDown1.Value = Convert.ToDecimal( t );
        }
    }
}
