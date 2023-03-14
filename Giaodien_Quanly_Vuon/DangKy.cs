using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Giaodien_Quanly_Vuon
{
    public partial class DangKy : Form
    {
        public DangKy()
        {
            InitializeComponent();
        }

        public bool CheckAccount(string ac) // check mat khau va ten tai khoan 
        {
            return Regex.IsMatch(ac, "^[a-zA-Z0-9]{6,24}$");
        }
        public bool CheckEmail(string em)   // check email
        {
            return Regex.IsMatch(em, @"^[a-zA-Z0-9_.]{3,20}@gmail.com(.vn|)$");
        }

        Modify modify = new Modify();

        private void button_DangKy_Click(object sender, EventArgs e)
        {
            string account = textBox_TenTaiKhoan.Text;
            string password = textBox_MatKhau.Text;
            string passCheck = textBox_XNMatKhau.Text;
            string email = textBox_Email.Text;
            if (!CheckAccount(account))
            {
                MessageBox.Show("Please enter your account name 6-24 characters long with alpha characters, uppercase and lowercase!", "Notification");
                return;
            }
            if (!CheckAccount(password))
            {
                MessageBox.Show("Please enter a password name 6-24 characters long with alpha characters, uppercase and lowercase!", "Notification");
                return;
            }
            if (passCheck != password)
            {
                MessageBox.Show("Please confirm the correct password!", "Notification");
                return;
            }
            if (!CheckEmail(email))
            {
                MessageBox.Show("Please enter correct email format", "Notification");
                return;
            }
            if (modify.TaiKhoans("Select * from TaiKhoan where Email = '" + email + "'").Count != 0)
            {
                MessageBox.Show("This email esixted, please register other email!", "Notification");
                return;
            }
            try
            {
                string query = "Insert into Taikhoan values ('" + account + "', '" + password + "','" + email + "')";
                modify.Command(query);
                if (MessageBox.Show("Sign up completed! Do you want to login??", "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            catch
            {
                MessageBox.Show("This account name is registered, please register another account!", "Notification");
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Are you sure you want to exit?", "Quit", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                Application.Exit(); // Đóng ứng dụng
            }
        }

        // Hỏi xem nhấn dấu X xem có muốn đóng chương trình hay không?
        private void DangKy_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult answer = MessageBox.Show("Do you want to exit the program?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
