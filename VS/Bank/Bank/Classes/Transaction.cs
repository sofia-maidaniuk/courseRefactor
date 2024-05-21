using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Classes
{
    public abstract class Transaction
    {
        public string TransactionType { get; set; }
        public string TransactionDestination { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionNumber { get; set; }
        public double TransactionValue { get; set; }
        public double Commission { get; set; } 

        public abstract double CalculateTotalAmount();
    }

}
