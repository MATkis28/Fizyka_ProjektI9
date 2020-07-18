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
    public partial class SimulationOutForm : Form
    {
        public SimulationOutForm()
        {
            InitializeComponent();
            CenterToScreen();
            Visible = true;
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
            TextArea.SelectionStart = TextArea.TextLength;
            TextArea.SelectionLength = 0;
            TextArea.SelectionColor = color;
            TextArea.AppendText(text);
        }
    }
}
