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
    public partial class SimulationSeriesOutForm : Form
    {
        public SimulationSeriesOutForm()
        {
            InitializeComponent();
            CenterToScreen();
            TextArea.SelectionTabs = new int[] { 100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500, 1600, 1700, 1800, 1900, 2000 };
        }

        public void appendLine(String text)
        {
            append(text + "\n", Color.Black);
        }

        public void appendLine(String text, Color color)
        {
            append(text + "\n", color);
        }

        public void append(String text)
        {
            append(text, Color.Black);
        }

        public void append(String text, Color color)
        {
            TextArea.Invoke
            (
                new MethodInvoker
                (
                    delegate ()
                    {
                        TextArea.SelectionStart = TextArea.TextLength;
                        TextArea.SelectionLength = 0;
                        TextArea.SelectionColor = color;
                        TextArea.AppendText(text);
                    }
                )
            );
            
        }

        private void Form_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                if (MessageBox.Show("Do you want to end simulation series?", Text, MessageBoxButtons.YesNo) == DialogResult.Yes)
                    Hide();
            }

        }
    }
}
