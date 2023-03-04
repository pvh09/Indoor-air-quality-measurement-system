using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giaodien_Quanly_Vuon
{
    public class Data
    {
        //private int ID;
        private string temp;
        private string humi;
        private string light;
        private string realTime;

        public Data()
        {
        }

        public Data(string temp, string humi, string light, string realTime)
        {
           // this.ID = ID;
            this.temp = temp;
            this.humi = humi;
            this.light = light;
            this.realTime = realTime;
        }

        //public int id { get => id; set => id = value; }
        public string Temp { get => temp; set => temp = value; }
        public string Humi { get => humi; set => humi = value; }
        public string Light { get => light; set => light = value; }
        public string RealTime { get => realTime; set => realTime = value; }
    }
}