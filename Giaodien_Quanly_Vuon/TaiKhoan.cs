using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giaodien_Quanly_Vuon
{
    public class Account
    {
        private string account;
        private string password;

        public Account()
        {
        }

        public Account(string account, string password)
        {
            this.account = account;
            this.password = password;
        }

        public string AccountName { get => account; set => account = value; }
        public string Password { get => password; set => password = value; }
    }
}
