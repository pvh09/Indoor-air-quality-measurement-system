using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

namespace Giaodien_Quanly_Vuon
{
    public class Data_Modify
    {
        public Data_Modify()
        {

        }
        SqlCommand DataCommand; // dùng để truy vấn các câu lệnh insert, update, delete...
        SqlDataReader ReadData; // dùng để đọc dữ liệu trong bảng

        public List<Data> DataMonitor(string query)   // 
        {
            List<Data> dataMonitor = new List<Data>();

            using (SqlConnection sqlConnect = ConnectionData.GetSqlConnect()) 
            {
                sqlConnect.Open();
                while (ReadData.Read())
                {
                   // dataMonitor.Add(new Data(ReadData.GetString(0), ReadData.GetString(1), ReadData.GetString(2), ReadData.GetString(3)));
                    dataMonitor.Add(new Data(ReadData.GetString(1), ReadData.GetString(2), ReadData.GetString(3), ReadData.GetString(4)));
                }
                sqlConnect.Close();
            }
            return dataMonitor;
        }
        public void SqlCommand(string query)   
        {
            using (SqlConnection sqlConnect = ConnectionData.GetSqlConnect())
            {
                sqlConnect.Open();
                SqlCommand sqlCommand = new SqlCommand(query, sqlConnect);
                sqlCommand.ExecuteNonQuery(); 
                
                sqlConnect.Close();
            }
        }
    }
}
