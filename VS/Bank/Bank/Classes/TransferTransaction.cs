using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bank.Classes
{
    public class TransferTransaction : Transaction
    {
        private double commissionRate;

        public TransferTransaction(double commissionRate)
        {
            this.commissionRate = commissionRate;
        }

        public override double CalculateTotalAmount()
        {
            return TransactionValue + (TransactionValue * commissionRate);
        }
    }

}
