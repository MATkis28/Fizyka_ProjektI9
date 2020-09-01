using System;
using System.Drawing;
using System.Windows.Forms;

namespace Fizyka_Czasteczki
{
    public partial class SimulationSeriesOutForm : Form
    {
        public SimulationSeriesOutForm()
        {
            InitializeComponent();
            CenterToScreen();
            int[] temp = new int[30];
            for( int i=0; i<30; i++)
                temp[i] = i * 100;
            TextArea.SelectionTabs = temp;
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
