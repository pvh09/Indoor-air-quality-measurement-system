﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Runtime.Remoting.Contexts;

namespace Giaodien_Quanly_Vuon
{
    public class ConnectionData
    {
        public static string stringCon = @"Data Source=HOAPV1\SQLEXPRESS;Initial Catalog=AirQualityData;Persist Security Info=True;User ID=sa;Password=112233";
        public static SqlConnection GetSqlConnect()
        {
            return new SqlConnection(stringCon);
        }
    }
}

