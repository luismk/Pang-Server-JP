using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PacketCreator
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            radioButton1.Checked = true;
        }

        public static string HexStringToCSharpHexArray(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("O comprimento da string hex deve ser par.");

            var sb = new StringBuilder();
            sb.Append("new byte[] { ");

            for (int i = 0; i < hex.Length; i += 2)
            {
                if (i > 0)
                    sb.Append(", ");

                string byteValue = hex.Substring(i, 2);
                sb.Append("0x" + byteValue);
            }

            sb.Append(" }");
            return sb.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtInput.Text))
            {
                MessageBox.Show("O texto nao pode ser nulo");
            }
            else
            {
                if (txtInput.Text.Contains("new byte[]"))
                {
                    MessageBox.Show("O texto ja esta escrito em byte[], click em 'Clear' para limpar o process");
                }
                else
                {
                    if (radioButton2.Checked)
                    {
                        txtInput.Text = "p.WriteBytes(" + HexStringToCSharpHexArray(txtInput.Text) + ");";
                    }
                    else if (radioButton1.Checked)
                    {
                        txtInput.Text = HexStringToCSharpHexArray(txtInput.Text);
                    }
                    else
                    {

                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            txtInput.Text = "";
        }
    }
}
