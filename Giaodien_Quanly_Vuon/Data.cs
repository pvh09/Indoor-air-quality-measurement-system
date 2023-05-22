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
        private string co2;
        private string pm25;
        private string voc;
        private string co;
        private string realTime;

        public Data()
        {
        }

        public Data(string temp, string humi, string co2, string pm25, string voc, string o3, string realTime)
        {
           // this.ID = ID;
            this.temp = temp;
            this.humi = humi;
            this.co2 = co2;
            this.pm25 = pm25;
            this.voc = voc;
            this.co = co;
            this.realTime = realTime;
        }

        //public int id { get => id; set => id = value; }
        public string Temp { get => temp; set => temp = value; }
        public string Humi { get => humi; set => humi = value; }
        public string Co2 { get => co2; set => co2 = value; }
        public string Pm25 { get => pm25; set => pm25 = value; }
        public string Voc { get => voc; set => voc = value; }
        public string CO { get => co; set => co = value; }
        public string RealTime { get => realTime; set => realTime = value; }
    }
}