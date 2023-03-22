using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Giaodien_Quanly_Vuon
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void linkLabel_QuenMatKhau_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FogotPassword forgotPass = new FogotPassword();
            forgotPass.ShowDialog();
        }

        private void linkLabel_DangKy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Resgister register = new Resgister();
            register.ShowDialog();
        }

        Modify modify = new Modify();

        private void button_DangNhap_Click(object sender, EventArgs e)
        {
            string account = textBox_TenTaiKhoan.Text;
            string password = textBox_MatKhau.Text;

            if (account.Trim() == "")
            {
                MessageBox.Show("Please enter your account!", "Notification"); 
                return;
            }
            else if (password.Trim() == "")
            {
                MessageBox.Show("Enter password!", "Notification"); 
                return;
            }
            else
            {
                string query = "SELECT * FROM Account where AccountName = '" + password + "' and UserPassword = '" + password + "'";
                if (modify.Accounts(query).Count != 0)
                {
                    MessageBox.Show("Success login!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Hide();
                    Home home = new Home();
                    home.ShowDialog();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("The username or password entered incorrect!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ckbShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbShowPassword.Checked)
            {
                textBox_MatKhau.UseSystemPasswordChar = false;
            }
            else
            {
                textBox_MatKhau.UseSystemPasswordChar = true;
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Are you sure quit?", "Exit", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                Application.Exit(); // Đóng ứng dụng
            }
        }

        // Hỏi xem nhấn dấu X xem có muốn đóng chương trình hay không?
        private void DangNhap_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult answer = MessageBox.Show("Do you want to exit the program?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void Login_Load(object sender, EventArgs e)
        {

        }
    }
}
