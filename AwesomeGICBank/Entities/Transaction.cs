using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBank.Entities
{
    public class Transaction
    {
        public DateTime Date { get; private set; }
        public string TransactionId { get; private set; }
        public TransactionType Type { get; private set; }
        public decimal Amount { get; private set; }

        public Transaction(DateTime date, TransactionType type, decimal amount, string transactionId)
        {
            Date = date;
            Type = type;
            Amount = amount;
            TransactionId = transactionId;
        }
    }
}
