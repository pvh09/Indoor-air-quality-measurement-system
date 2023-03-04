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
            string tentk = textBox_TenTaiKhoan.Text;
            string matkhau = textBox_MatKhau.Text;
            string xnmatkhau = textBox_XNMatKhau.Text;
            string email = textBox_Email.Text;
            if (!CheckAccount(tentk))
            {
                MessageBox.Show("Vui lòng nhập tên tài khoản dài 6-24 ký tự với các ký tự chữ và số, chữ hoa và chữ thường!", "Thông báo");
                return;
            }
            if (!CheckAccount(matkhau))
            {
                MessageBox.Show("Vui lòng nhập tên mật khẩu dài 6-24 ký tự với các ký tự chữ và số, chữ hoa và chữ thường!", "Thông báo");
                return;
            }
            if (xnmatkhau != matkhau)
            {
                MessageBox.Show("Vui lòng xác nhận mật khẩu chính xác!", "Thông báo");
                return;
            }
            if (!CheckEmail(email))
            {
                MessageBox.Show("Vui lòng nhập đúng định dạng email!", "Thông báo");
                return;
            }
            if (modify.TaiKhoans("Select * from TaiKhoan where Email = '" + email + "'").Count != 0)
            {
                MessageBox.Show("Email này đã được đăng ký, vui lòng đăng ký email khác!", "Thông báo");
                return;
            }
            try
            {
                string query = "Insert into Taikhoan values ('" + tentk + "', '" + matkhau + "','" + email + "')";
                modify.Command(query);
                if (MessageBox.Show("Đăng ký thành công! Bạn có muốn đăng nhập luôn không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    this.Close();
                }
            }
            catch
            {
                MessageBox.Show("Tên tài khoản này đã được đăng ký, vui lòng đăng ký tên tài khoản khác!", "Thông báo");
            }
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có chắc muốn thoát?", "Thoát", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
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
    }
}
