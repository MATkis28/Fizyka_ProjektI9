using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fizyka_smietnik
{
    public partial class SimulationSeriesInForm : Form
    {
        public bool runSS = false;
        public char simulatedVariable;
        public int[] simulatedVariableValues;
        public int numberOfOutValues;

        public SimulationSeriesInForm()
        {
            InitializeComponent();
            CenterToScreen();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedItem = "N";
        }

        private void deleteValue(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void valueUp(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex > 0)
            {
                listBox1.Items.Insert(listBox1.SelectedIndex - 1, listBox1.Items[listBox1.SelectedIndex]);
                int index = listBox1.SelectedIndex;
                listBox1.Items.RemoveAt(index);
                listBox1.SelectedIndex = index-2;
            }
        }

        private void valueDown(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < listBox1.Items.Count - 1 && listBox1.SelectedIndex != -1)
            {
                listBox1.Items.Insert(listBox1.SelectedIndex + 2, listBox1.Items[listBox1.SelectedIndex]);
                int index = listBox1.SelectedIndex;
                listBox1.Items.RemoveAt(index);
                listBox1.SelectedIndex = index + 1;
            }
        }

        private void addValue(object sender, EventArgs e)
        {
            try
            {
                int iItem = Convert.ToInt32(textBox1.Text);
                if (iItem <= 0)
                    throw new Exception("Value should be positive.");
                listBox1.Items.Add(iItem);
                textBox1.Clear();
            }
            catch (Exception excep)
            {
                MessageBox.Show("Wrong value.\n" + excep.Message, Text, MessageBoxButtons.OK);
            }
            textBox1.Focus();
        }

        private void keyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                addValue(this, null);
                e.Handled = true;
            }
        }

        private void run(object sender, EventArgs e)
        {
            if(listBox1.Items.Count < 1)
            {
                MessageBox.Show("There should be at least one variable value.", Text, MessageBoxButtons.OK);
                return;
            }
            runSS = true;
            simulatedVariable = ((string)comboBox1.SelectedItem)[0];
            simulatedVariableValues = listBox1.Items.Cast<int>().ToArray();
            numberOfOutValues = Convert.ToInt32(numericUpDown1.Value);
            Dispose();
        }
    }
}
