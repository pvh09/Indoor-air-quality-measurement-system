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
using Microsoft.Office.Interop.Excel;
using System.Xml.Linq;

namespace Giaodien_Quanly_Vuon
{
    public partial class Home : Form
    {
        public bool isThoat = true;

        private DateTime datetime;  //Khai báo biến thời gian

        int baudrate = 0;
        string temp = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string humi = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string co2 = String.Empty;  // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string pm25 = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string voc = String.Empty; // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        string o3 = String.Empty;  // Khai báo chuỗi để lưu dữ liệu cảm biến gửi qua Serial
        int status = 0; // Khai báo biến để xử lý sự kiện vẽ đồ thị
        //Khai báo biến thời gian để vẽ đồ thị
        double m_temp = 0;
        double m_humi = 0;
        double m_co2 = 0;
        double m_pm25 = 0;
        double m_voc = 0;
        double m_o3 = 0;

        int i = 0;
        public Home()
        {
            InitializeComponent();
        }
        // Có 2 cách làm với Đăng xuất
        //public event EventHandler Dangxuat;
        private void btnDangxuat_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to sign out?", "Notification", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                this.Hide();
                Login login = new Login();
                login.ShowDialog();
            }
        }

        private void button_Thoat_Click(object sender, EventArgs e)
        {
            DialogResult ans;
            ans = MessageBox.Show("Are you sure you want to quit?", "Quit", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (ans == DialogResult.OK)
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

            // Khởi tạo ZedGraph
            GraphPane myPane = zedGraphControl1.GraphPane;
            myPane.Title.Text = "Real-time data visualization";
            myPane.XAxis.Title.Text = "Time (s)";
            myPane.YAxis.Title.Text = "Data";

            // Danh sách dữ liệu gồm 60000 phần tử có thể cuốn chiếu lại
            RollingPointPairList list = new RollingPointPairList(60000);
            RollingPointPairList list1 = new RollingPointPairList(60000);
            RollingPointPairList list2 = new RollingPointPairList(60000);
            RollingPointPairList list3 = new RollingPointPairList(60000);
            RollingPointPairList list4 = new RollingPointPairList(60000);
            RollingPointPairList list5 = new RollingPointPairList(60000);
            // Phần đặt tên chú thích cho 3 thông số trên biểu đồ
            LineItem curve = myPane.AddCurve("Temperature", list, Color.Red, SymbolType.None);
            LineItem curve1 = myPane.AddCurve("Humidity", list1, Color.Blue, SymbolType.None);
            LineItem curve2 = myPane.AddCurve("CO2 Concentration", list2, Color.Chocolate, SymbolType.None);
            LineItem curve3 = myPane.AddCurve("PM2.5 Concentration", list3, Color.Violet, SymbolType.None);
            LineItem curve4 = myPane.AddCurve("VOC Concentration", list4, Color.Yellow, SymbolType.None);
            LineItem curve5 = myPane.AddCurve("O3 Concentration", list5, Color.Green, SymbolType.None);

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
                temp = arrList[0]; // Chuỗi đầu tiên lưu vào SRealTime
                humi = arrList[1]; // Chuỗi thứ hai lưu vào SDatas
                co2 = arrList[2];
                pm25 = arrList[3];
                voc = arrList[4];
                o3 = arrList[5];
                i++;
                double.TryParse(temp, out m_temp); // Chuyển đổi sang kiểu double
                double.TryParse(humi, out m_humi);
                double.TryParse(co2, out m_co2);
                double.TryParse(pm25, out m_pm25); // Chuyển đổi sang kiểu double
                double.TryParse(voc, out m_voc);
                double.TryParse(o3, out m_o3);
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
            LineItem curve3 = zedGraphControl1.GraphPane.CurveList[3] as LineItem;
            LineItem curve4 = zedGraphControl1.GraphPane.CurveList[4] as LineItem;
            LineItem curve5 = zedGraphControl1.GraphPane.CurveList[5] as LineItem;

            if (curve == null)
                return;
            if (curve1 == null)
                return;
            if (curve2 == null)
                return;
            if (curve3 == null)
                return;
            if (curve4 == null)
                return;
            if (curve5 == null)
                return;

            // Khai báo danh sách dữ liệu đường cong đồ thị
            IPointListEdit list = curve.Points as IPointListEdit;
            IPointListEdit list1 = curve1.Points as IPointListEdit;
            IPointListEdit list2 = curve2.Points as IPointListEdit;
            IPointListEdit list3 = curve3.Points as IPointListEdit;
            IPointListEdit list4 = curve4.Points as IPointListEdit;
            IPointListEdit list5 = curve5.Points as IPointListEdit;

            if (list == null)
                return;

            list.Add(i, m_temp); // Thêm điểm trên đồ thị
            list1.Add(i, m_humi);
            list2.Add(i, m_co2);
            list3.Add(i, m_pm25); // Thêm điểm trên đồ thị
            list4.Add(i, m_voc);
            list5.Add(i, m_o3);

            Scale xScale = zedGraphControl1.GraphPane.XAxis.Scale;
            Scale yScale = zedGraphControl1.GraphPane.YAxis.Scale;

            // Tự động Scale theo trục x
            if (i > xScale.Max - xScale.MajorStep)
            {
                xScale.Max = i + xScale.MajorStep;
                xScale.Min = xScale.Max - 30;
            }

            // Tự động Scale theo trục y
            if (m_temp > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_temp + yScale.MajorStep;
            }
            else if (m_temp < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_temp - yScale.MajorStep;
            }

            if (m_humi > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_humi + yScale.MajorStep;
            }
            else if (m_humi < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_humi - yScale.MajorStep;
            }

            if (m_co2 > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_co2 + yScale.MajorStep;
            }
            else if (m_co2 < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_co2 - yScale.MajorStep;
            }

            if (m_pm25 > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_pm25 + yScale.MajorStep;
            }
            else if (m_pm25 < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_pm25 - yScale.MajorStep;
            }

            if (m_voc > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_voc + yScale.MajorStep;
            }
            else if (m_voc < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_voc - yScale.MajorStep;
            }

            if (m_o3 > yScale.Max - yScale.MajorStep)
            {
                yScale.Max = m_o3 + yScale.MajorStep;
            }
            else if (m_o3 < yScale.Min + yScale.MajorStep)
            {
                yScale.Min = m_o3 - yScale.MajorStep;
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
                label9.Text = temp;
                label13.Text = humi;
                label10.Text = co2;
                label22.Text = pm25;
                label25.Text = voc;
                label10.Text = pm25;
                label28.Text = o3;

                //Tạo 1 chuỗi gồm thời gian hiện tại
                datetime = DateTime.Now;
                string time = datetime.Day + "/" + datetime.Month + "/" + datetime.Year + "/" + datetime.Hour + ":" + datetime.Minute + ":" + datetime.Second;
                //string DataQuery = "Insert into GreenMonitor values ('" + nhietdo + "', '" + doam + "','" + anhsang + "','" + time+ "')";
                string DataQuery = "INSERT INTO SensorMonitorData(Temperature, Humidity, co2, pm25, voc, o3, realTime) values ('" + temp + "', '" + humi + "','" + co2 + "','" + pm25 + "', '" + voc + "', '" + o3 + "', '" + time + "')";
                dataModify.SqlCommand(DataQuery);
                //Tạo listview với cột đầu tiên là thời gian
                ListViewItem item = new ListViewItem(time); // Gán biến realtime vào cột đầu tiên của ListView

                //Thêm 3 cột tiếp theo là Nhiệt độ, Ánh sáng và Độ ẩm
                item.SubItems.Add(temp);
                item.SubItems.Add(humi);
                item.SubItems.Add(co2);
                item.SubItems.Add(pm25);
                item.SubItems.Add(voc);
                item.SubItems.Add(o3);
                listView1.Items.Add(item); // Gán biến datas vào cột tiếp theo của ListView

                listView1.Items[listView1.Items.Count - 1].EnsureVisible(); // Hiển thị dòng được gán gần nhất ở ListView, tức là mình cuộn ListView theo dữ liệu gần nhất đó
            }
        }
        private void ResetValue()
        {
            temp = String.Empty;    // Khôi phục tất cả các biến vào trạng thái ban đầu
            humi = String.Empty;
            co2 = String.Empty;
            pm25 = String.Empty;
            voc = String.Empty;
            o3 = String.Empty;
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
                    label15.Text = "Connecting...";
                    label15.ForeColor = Color.Green;
                    button8.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = true;
                    toolStripStatusLabel1.Text = "Connecting COM sucessful!";
                    toolStripStatusLabel1.ForeColor = Color.Green;
                }
                catch
                {
                    MessageBox.Show("Cannot COM gate " + serialPort1.PortName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                SaveSetting(); // Lưu cổng COM vào ComName
            }
            else
            {
                MessageBox.Show("Openning COM gate");
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
                label15.Text = "Disconnect";
                label15.ForeColor = Color.Red;
                toolStripStatusLabel1.Text = "Disconnected COM gate!";
                toolStripStatusLabel1.ForeColor = Color.Red;
            }
            else
            {
                MessageBox.Show("GOM gate disable");
            }
        }
        private void button7_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Write("8");
            }
            else
                MessageBox.Show("Can not stop until the device is connected", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Sự kiện nhấn nút button8 - Exit
        private void button8_Click(object sender, EventArgs e)
        {
            DialogResult ans;
            ans = MessageBox.Show("Are you sure you want to quit?", "Quit", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (ans == DialogResult.OK)
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

            ws.get_Range("A1", "G1").Font.Bold = true;
            ws.get_Range("A1", "G1").VerticalAlignment =
                Microsoft.Office.Interop.Excel.XlVAlign.xlVAlignCenter;

            ws.Cells[1, 1] = "Time (s)                ";
            ws.Cells[1, 2] = "Temperature (°C)";
            ws.Cells[1, 3] = "Humidity (%)";
            ws.Cells[1, 4] = "CO2 Concentration(ppm)";
            ws.Cells[1, 5] = "PM2.5 Concentration(ppm)";
            ws.Cells[1, 6] = "VOC Concentration(ppm)";
            ws.Cells[1, 7] = "O3 Concentration(ppm)";
            //rg.Columns.AutoFit();
            //rf.Columns.AutoFit();
            //ry.Columns.AutoFit();
            //rz.Columns.AutoFit();

            ws.Columns.AutoFit();
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
            DialogResult ans;
            ans = MessageBox.Show("Do you want to save the data?", "Save", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (ans == DialogResult.OK)
            {
                SaveToExcel(); // Thực thi hàm lưu ListView sang Excel
            }
        }

        // Hàm thiết lập nút bấm "Xóa"
        private void bt_Xoa_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                DialogResult ans;
                ans = MessageBox.Show("Are you sure you want to delete the data?", "Delete data", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

                if (serialPort1.IsOpen == true)
                {

                    if (checkBox1.Checked == true)
                    {
                        try
                        {
                            DialogResult ans1;
                            SqlConnection sqlConn = new SqlConnection(ConnectionData.stringCon);
                            sqlConn.Open();
                            ans1 = MessageBox.Show("Do you want to save the data before deleting Database?", "Save", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                            if (ans1 == DialogResult.OK)
                            {
                                SaveToExcel(); // Thực thi hàm lưu ListView sang Excel
                            }
                            string DeleteQuery = @"DELETE FROM SensorMonitorData where ID = 5";
                            SqlCommand cmd = new SqlCommand(DeleteQuery, sqlConn);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Deleted sucessfull");

                        }
                        catch (Exception x)
                        {
                            MessageBox.Show(" Not Deleted" + x.Message);
                        }
                    }
                    else
                    {
                        //Gửi ký tự "2" qua Serial
                        serialPort1.Write("2");

                        // Xóa listview
                        listView1.Items.Clear();

                        //Xóa dữ liệu trong Form
                        ResetValue();
                    }
                }
                else
                    MessageBox.Show("Cannot run without connecting to the device", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            else
                MessageBox.Show("Cannot delete without connecting to the device", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        // Hàm này đơn giản là mình có thể ghi thông tin các thành viên nhóm hay lời cảm ơn với thầy cô
        private void bt_about_Click(object sender, EventArgs e)
        {
            MessageBox.Show("PROJECT: INTEGRATED SENSOR SYSTEM FOR INDOOR AIR QUALITY MONITOR \n \nStudent name: Phi Van Hoa     Phone: 0967924460    Instructor: Dr. Tran Cuong Hung", "Information");
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

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void lbDate_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }

        private void label28_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void all_click(object sender, EventArgs e)
        {

        }
    }
}
