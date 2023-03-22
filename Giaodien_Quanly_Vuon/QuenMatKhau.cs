using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Giaodien_Quanly_Vuon
{
    public partial class FogotPassword : Form
    {
        public FogotPassword()
        {
            InitializeComponent();
            label2.Text = "";
        }
        Modify modify = new Modify();

        private void button_LayLaiMK_Click(object sender, EventArgs e)
        {
            string email = textBox1.Text;
            if (email.Trim() == "")
            {
                MessageBox.Show("Please enter your register email!", "Notification");
            }
            else
            {
                string query = "SELECT * FROM Account where gmail = '" + email + "'";
                if (modify.Accounts(query).Count != 0)
                {
                    label2.ForeColor = Color.Blue;
                    label2.Text = "Password: " + modify.Accounts(query)[0].Password;
                }
                else
                {
                    label2.ForeColor = Color.Red;
                    label2.Text = "This email is not registered. Please check again!";
                }
            }
        }

        // Hỏi xem nhấn dấu X xem có muốn đóng chương trình hay không?
        private void QuenMatKhau_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult answer = MessageBox.Show("Do you want to exit the program?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void FogotPassword_Load(object sender, EventArgs e)
        {

        }
    }
}
