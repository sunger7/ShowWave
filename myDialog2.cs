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
    public partial class myDialog2 : Form
    {
        Action<List<Reflectivity>, bool> UpdateReflect = null;
        List<Reflectivity> selects;
        double last_value;

        public myDialog2(List<Reflectivity> selects, Action<List<Reflectivity>, bool> UpdateReflectMult)
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
            button2.DialogResult = DialogResult.Cancel;
            this.UpdateReflect = UpdateReflectMult;
            this.selects = selects;
            last_value = Math.Round(Convert.ToDouble(numericUpDown1.Value), 3);
        }


        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
                button2.PerformClick();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double value = Math.Round(Convert.ToDouble(numericUpDown1.Value), 3);
            if (value > last_value)
            {
                for (int i = 0; i < selects.Count; i++)
                    selects[i].x += 0.002F;
            }
            else
            {
                for (int i = 0; i < selects.Count; i++)
                    selects[i].x -= 0.002F;
            }
            last_value = value;
            UpdateReflect(selects, true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double value = Math.Round(Convert.ToDouble(numericUpDown1.Value), 3);
            if (value > last_value)
            {
                for (int i = 0; i < selects.Count; i++)
                    selects[i].x += 0.002F;
            }
            else
            {
                for (int i = 0; i < selects.Count; i++)
                    selects[i].x -= 0.002F;
            }
            last_value = value;
            UpdateReflect(selects, false);
            this.Close();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            double value = Math.Round(Convert.ToDouble(numericUpDown1.Value), 3);
            if (value > last_value)
            {
                for (int i = 0; i < selects.Count; i++)
                    selects[i].x += 0.002F;
            }
            else
            {
                for (int i = 0; i < selects.Count; i++)
                    selects[i].x -=0.002F;
            }
            last_value = value;
            UpdateReflect(selects, true);
        }
    }
}
