using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyLib
{
    public class BankingCard
    {
        public int ID_Card { get; set; }
        public string CardNumber { get; set; }
        public string CvvCode { get; set; }
        public DateTime CardDate { get; set; }
        public string PaySystem { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
    }
}
