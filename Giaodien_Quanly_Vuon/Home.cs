using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
// Giao tiếp qua Serial
using System.IO;
using System.IO.Ports;
using System.Xml;
// Thêm ZedGraph
using ZedGraph;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace Giaodien_Quanly_Vuon
{
    public partial class Home : Form
    {
        public bool isThoat = true;

        private DateTime datetime;  //Khai báo biến thời gian

        int baudrate = 0;
        string nhietdo = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string doam = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string anhsang = String.Empty;  // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        int status = 0; // Khai báo biến để xử lý sự kiện vẽ đồ thị
        string statuslamp = String.Empty;
        string statuspump = String.Empty;
        string StrDoam = String.Empty;
        string Strlight = String.Empty;
        //Khai báo biến thời gian để vẽ đồ thị
        double m_doam = 0;
        double m_anhsang = 0;
        double m_nhietdo = 0;

        int i = 0;
        public Home()
        {
            InitializeComponent();
        }
        // Có 2 cách làm với Đăng xuất
        //public event EventHandler Dangxuat;
        private void btnDangxuat_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("Bạn có chắc muốn đăng xuất không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                //Dangxuat(this, new EventArgs());

            
            if (MessageBox.Show("Bạn có chắc muốn đăng xuất không?", "Thông báo", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                this.Hide();
                DangNhap dangNhap = new DangNhap();
                dangNhap.ShowDialog();
            } 
        }

        private void button_Thoat_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có chắc muốn thoát?", "Thoát", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
               // Application.Exit(); // Đóng ứng dụng
            }
        }

        // Hỏi xem nhấn dấu X xem có muốn đóng chương trình hay không?
        private void Home_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult answer = MessageBox.Show("Do you want to exit the program?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }
            }
        }

        private void Home_Load(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames(); // Lấy nguồn cho comboBox là tên của cổng COM
            comboBox1.Text = Properties.Settings.Default.ComName; // Lấy ComName đã làm ở bước 5 cho comboBox
            comboBox2.SelectedIndex = 2;

            // Hiển thị ngày tháng năm, giờ ở góc cuối bên phải giao diện
            DateTime tn = DateTime.Now;
            lbDate.Text = tn.ToString("dd/MM/yyyy");
            lbTime.Text = DateTime.Now.ToLongTimeString();

            // Khởi tạo ZedGraph
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Đồ thị hiển thị dữ liệu theo thời gian";
            myPane.XAxis.Title.Text = "Thời gian (s)";
            myPane.YAxis.Title.Text = "Dữ liệu";

            // Danh sách dữ liệu gồm 60000 phần tử có thể cuốn chiếu lại
            RollingPointPairList list = new RollingPointPairList(60000);
            RollingPointPairList list1 = new RollingPointPairList(60000);
            RollingPointPairList list2 = new RollingPointPairList(60000);
            // Phần đặt tên chú thích cho 3 thông số trên biểu đồ
            LineItem curve = myPane.AddCurve("Nhiệt độ", list, Color.Red, SymbolType.None);
            LineItem curve1 = myPane.AddCurve("Độ ẩm", list1, Color.Blue, SymbolType.None);
            LineItem curve2 = myPane.AddCurve("Ánh sáng", list2, Color.Yellow, SymbolType.None);

            myPane.XAxis.Scale.Min = 0;
            myPane.XAxis.Scale.Max = 30;
            myPane.XAxis.Scale.MinorStep = 1;   // bước nhảy nhỏ nhất
            myPane.XAxis.Scale.MajorStep = 5;   // bước nhảy lớn nhất
            myPane.YAxis.Scale.Min = 0;
            myPane.YAxis.Scale.Max = 100;

            myPane.AxisChange();
        }
        // Hàm Tick này sẽ bắt sự kiện cổng Serial mở hay không
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)    // cổng Com đóng
            {
                progressBar1.Value = 0;
            }
            else if (serialPort1.IsOpen)    // cổng Com mở
            {
                progressBar1.Value = 100;
                Draw();
                Data_Listview();
                status = 0;
            }
        }

        // Hàm này lưu lại cổng COM đã chọn cho lần kết nối
        private void SaveSetting()
        {
            Properties.Settings.Default.ComName = comboBox1.Text;
            Properties.Settings.Default.Save();
        }
        // Nhận và xử lý string gửi từ Serial
        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                string[] arrList = serialPort1.ReadLine().Split('|'); // Đọc một dòng của Serial, cắt chuỗi khi gặp ký tự gạch đứng
                doam = arrList[0]; // Chuỗi đầu tiên lưu vào SRealTime
                anhsang = arrList[1]; // Chuỗi thứ hai lưu vào SDatas
                nhietdo = arrList[2];
                statuslamp = arrList[3];
                statuspump = arrList[4];
                StrDoam = arrList[5];
                Strlight = arrList[6];
                i++;
                double.TryParse(doam, out m_doam); // Chuyển đổi sang kiểu double
                double.TryParse(anhsang, out m_anhsang);
                double.TryParse(nhietdo, out m_nhietdo);
                //realtime = realtime / 1000.0; // Đối ms sang s
                status = 1; // Bắt sự kiện xử lý xong chuỗi, đổi starus về 1 để hiển thị dữ liệu trong ListView và vẽ đồ thị
            }
            catch
            {
                return;
            }
        }

        // Vẽ đồ thị
        void Draw()
        {

            if (zedGraphControl1.GraphPane.CurveList.Count <= 0)
                return;

            // Khai báo đường cong lấy từ trên
            LineItem curve = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            LineItem curve1 = zedGraphControl1.GraphPane.CurveList[1] as LineItem;
            LineItem curve2 = zedGraphControl1.GraphPane.CurveList[2] as LineItem;

            if (curve == null)
                return;
            if (curve1 == null)
                return;
            if (curve2 == null)
                return;

            // Khai báo danh sách dữ liệu đường cong đồ thị
            IPointListEdit list = curve.Points as IPointListEdit;
            IPointListEdit list1 = curve1.Points as IPointListEdit;
            IPointListEdit list2 = curve2.Points as IPointListEdit;

            if (list == null)
                return;

            list.Add(i, m_nhietdo); // Thêm điểm trên đồ thị
            list1.Add(i, m_doam);
            list2.Add(i, m_anhsang);

            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (i > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = i + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }

            // Tự động Scale theo trục y
            if (m_nhietdo > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_nhietdo + yScale.MajorStep;
            }
            else if (m_nhietdo < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_nhietdo - yScale.MajorStep;
            }
            if (m_doam > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_doam + yScale.MajorStep;
            }
            else if (m_doam < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_doam - yScale.MajorStep;
            }
            if (m_anhsang > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_anhsang + yScale.MajorStep;
            }
            else if (m_anhsang < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_anhsang - yScale.MajorStep;
            }

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
            zedGraphControl1.Refresh();
        }

        // Hiển thị dữ liệu trong ListView
        private void Data_Listview()
        {
            Data_Modify dataModify = new Data_Modify();
            if (status == 0)
                return;
            else
            {
                label9.Text = nhietdo;
                label10.Text = anhsang;
                label13.Text = doam;
                textBox2.Text = StrDoam;
                textBox3.Text = Strlight;
                if (statuslamp == "1")
                {
                    button5.BackColor = Color.Yellow;
                }
                else
                {

                    button5.BackColor = Color.White;
                }
                if (statuspump == "1")
                {
                    button6.BackColor = Color.Yellow;
                }
                else
                {
                    button6.BackColor = Color.White;
                }

                //Tạo 1 chuỗi gồm thời gian hiện tại
                datetime = DateTime.Now;
                string time = datetime.Day + "/" + datetime.Month + "/" + datetime.Year + "/" + datetime.Hour + ":" + datetime.Minute + ":" + datetime.Second;
                //string DataQuery = "Insert into GreenMonitor values ('" + nhietdo + "', '" + doam + "','" + anhsang + "','" + time+ "')";
                string DataQuery = "Insert into SensorMonitor(Temperature, Humidity, Light, RealTime) values ('" + nhietdo + "', '" + doam + "','" + anhsang + "','" + time + "')";
                dataModify.SqlCommand(DataQuery);
                //Tạo listview với cột đầu tiên là thời gian
                ListViewItem item = new ListViewItem(time); // Gán biến realtime vào cột đầu tiên của ListView

                //Thêm 3 cột tiếp theo là Nhiệt độ, Ánh sáng và Độ ẩm
                item.SubItems.Add(nhietdo);
                item.SubItems.Add(anhsang);
                item.SubItems.Add(doam);
                listView1.Items.Add(item); // Gán biến datas vào cột tiếp theo của ListView

                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiển thị dòng được gán gần nhất ở ListView, tức là mình cuộn ListView theo dữ liệu gần nhất đó

                


            }
        }
        private void ResetValue()
        {
            doam = String.Empty;    // Khôi phục tất cả các biến vào trạng thái ban đầu
            nhietdo = String.Empty;
            anhsang = String.Empty;
            status = 0; // Chuyển status về 0
        }

        // Sự kiện nhấn nút button1 - Connect
        // Thường mình try - catch để kiểm tra lỗi
        private void button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen)    // cổng Com đóng
            {
                serialPort1.PortName = comboBox1.Text; // Lấy cổng COM
                int.TryParse(comboBox2.Text, out baudrate);
                serialPort1.BaudRate = baudrate; // Baudrate là 9600, trùng với baudrate của Arduino
                try
                {
                    serialPort1.Open();
                    label15.Text = "Đang kết nối";
                    label15.ForeColor = Color.Green;
                    button8.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = true;
                    toolStripStatusLabel1.Text = "Kết nối thành công cổng COM!";
                    toolStripStatusLabel1.ForeColor = Color.Green;
                }
                catch
                {
                    MessageBox.Show("Không thể mở cổng " + serialPort1.PortName, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                SaveSetting(); // Lưu cổng COM vào ComName
            }
            else
            {
                MessageBox.Show("Đang mở cổng Com");
            }
        }
        // Sự kiện nhấn nút button2 - Disconnect
        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) // cổng Com mở
            {
                serialPort1.Close();
                SaveSetting(); // Lưu cổng COM vào ComName
                progressBar1.Value = 0;
                button2.Enabled = false;
                button1.Enabled = true;
                button8.Enabled = true;
                label15.Text = "Ngắt kết nối";
                label15.ForeColor = Color.Red;
                toolStripStatusLabel1.Text = "Đã ngắt kết nối cổng COM!";
                toolStripStatusLabel1.ForeColor = Color.Red;
            }
            else
            {
                MessageBox.Show("Cổng Com đang đóng");
            }
        }
        // Sự kiện nhấn nút button3 - Auto Run
        private void button3_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("1"); //Gửi ký tự "1" qua Serial, chạy hàm tạo Random ở Arduino
                button5.Enabled = false;
                button6.Enabled = false;
                button3.BackColor = Color.Green;
                button4.BackColor = Color.Gray;
                button3.Enabled = false;
                button4.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
                button10.Enabled = true;
                button11.Enabled = true;
                button12.Enabled = true;

            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Sự kiện nhấn nút button4 - Manual - Chế độ Thủ công
        private void button4_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                button5.Enabled = true;
                button6.Enabled = true;
                button3.BackColor = Color.Gray;
                button4.BackColor = Color.Green;
                button3.Enabled = true;
                button4.Enabled = false;
                button8.Enabled = true;
                button9.Enabled = false;
                button10.Enabled = false;
                button11.Enabled = false;
                button12.Enabled = false;
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Sự kiện nhấn nút button5 - LAMP
        private void button5_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("2");
            }
            else
                MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Sự kiện nhấn nút button6 - PUMP
        private void button6_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("3");
            }
            else
                MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // Sự kiện nhấn nút button7 - Pause
        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("8");
            }
            else
                MessageBox.Show("Bạn không thể dừng khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Sự kiện nhấn nút button8 - Exit
        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có chắc muốn thoát?", "Thoát", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                //Application.Exit(); // Đóng ứng dụng
            }
        }

        //Hàm lưu dữ liệu lên Excell
        void SaveToExcel()
        {
            Microsoft.Office.Interop.Excel.Application xla = new Microsoft.Office.Interop.Excel.Application();
            xla.Visible = true;
            Microsoft.Office.Interop.Excel.Workbook wb = xla.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)xla.ActiveSheet;

            // Đặt tên cho 4 ô A1, B1, C1, D1 lần lượt là "Thời gian (s)" ; "Nhiệt độ (°C)" ; "Ánh sáng (%)" và "Độ ẩm (%)" sau đó tự động dãn độ rộng
            Microsoft.Office.Interop.Excel.Range rg = (Microsoft.Office.Interop.Excel.Range)ws.get_Range("A1", "B1");
            Microsoft.Office.Interop.Excel.Range rf = (Microsoft.Office.Interop.Excel.Range)ws.get_Range("C1", "D1");
            ws.Cells[1, 1] = "Thời gian (s)                ";
            ws.Cells[1, 2] = "Nhiệt độ (°C)";
            ws.Cells[1, 3] = "Ánh sáng (%) ";
            ws.Cells[1, 4] = "Độ ẩm (%) ";
            rg.Columns.AutoFit();
            rf.Columns.AutoFit();

            // Lưu từ ô đầu tiên của dòng thứ 2, tức ô A2
            int i = 2;
            int j = 1;

            foreach (ListViewItem comp in listView1.Items)
            {
                ws.Cells[i, j] = comp.Text.ToString();
                foreach (ListViewItem.ListViewSubItem drv in comp.SubItems)
                {
                    ws.Cells[i, j] = drv.Text.ToString();
                    j++;
                }
                j = 1;
                i++;
            }
        }

        // Hàm thiết lập nút bấm "Lưu"
        private void bt_save_Click_1(object sender, EventArgs e)
        {
            DialogResult traloi;
            traloi = MessageBox.Show("Bạn có muốn lưu số liệu?", "Lưu", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (traloi == DialogResult.OK)
            {
                SaveToExcel(); // Thực thi hàm lưu ListView sang Excel
            }
        }

        // Hàm thiết lập nút bấm "Xóa"
        private void bt_Xoa_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                DialogResult traloi;
                traloi = MessageBox.Show("Bạn có chắc muốn xóa dữ liệu?", "Xóa dữ liệu", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (traloi == DialogResult.OK)
                {
                    if (serialPort1.IsOpen)
                    {
                        //Gửi ký tự "2" qua Serial
                        serialPort1.Write("2");

                        // Xóa listview
                        listView1.Items.Clear();

                        //Xóa dữ liệu trong Form
                        ResetValue();
                    }
                    else
                        MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
                MessageBox.Show("Bạn không thể xóa khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Hàm này đơn giản là mình có thể ghi thông tin các thành viên nhóm hay lời cảm ơn với thầy cô
        private void bt_about_Click(object sender, EventArgs e)
        {
            MessageBox.Show("NHÓM 09: Hệ thống vườn thông minh giám sát, điều khiển nhiệt độ, độ ẩm và ánh sáng \n\nTHÀNH VIÊN:\n Phí Văn Hòa: 19021047 \n Hoàng Văn Thịnh: 19021117 \n Nguyễn Văn Tùng: 19021133 \n\nChân thành cảm ơn thầy TS. Hoàng Văn Mạnh đã đồng hành và giúp đỡ chúng em hoàn thành môn học !  ", "Thông tin");
        }

        private void groupBox7_Enter(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {

                serialPort1.Write("8");
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Set Độ ẩm tăng lên 1 đơn vị
        private void button9_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {

                serialPort1.Write("4");
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Set Độ ẩm giảm lên 1 đơn vị
        private void button10_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("5");
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Set Ánh sáng tăng lên 1 đơn vị
        private void button11_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("6");
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Set Ánh sáng giảm lên 1 đơn vị
        private void button12_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("7");
            }
            else
                MessageBox.Show("Bạn không thể chạy khi chưa kết nối với thiết bị", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void DBConnect_Click(object sender, EventArgs e)
        {
            
            
        }

        private void zedGraphControl1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }
    }
}
