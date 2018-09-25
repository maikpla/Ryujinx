using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Ryujinx.GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add("Dir: " + textBox1.Text);
            listBox1.Items.Add("Game: " + textBox2.Text);
        }
        public void runRyujinx()
        {

            string selectedGame = listBox1.GetItemText(listBox1.SelectedItem);
            var P = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"ryujinx.dll {selectedGame}",
            };

            Process.Start(P);

        }

        private void label3_Click(object sender, EventArgs e)
        {
 
        }
    }
}
