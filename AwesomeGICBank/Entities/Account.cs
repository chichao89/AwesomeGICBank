namespace AwesomeGICBank.Entities
{
    public class Account
    {
        public string AccountId { get; private set; }
        public decimal Balance { get; private set; }
        public List<Transaction> Transactions { get; private set; }

        public Account(string accountId)
        {
            AccountId = accountId;
            Balance = 0;
            Transactions = new List<Transaction>();
        }

        public void AddTransaction(Transaction transaction)
        {
            if (transaction.Type == TransactionType.Withdrawal && (Balance - transaction.Amount) < 0)
                throw new InvalidOperationException("Insufficient funds.");

            Transactions.Add(transaction);
            Balance += transaction.Type == TransactionType.Deposit ? transaction.Amount : -transaction.Amount;
        }

        public decimal GetStartingBalanceForMonth(DateTime month)
        {
            
            DateTime lastDayOfPreviousMonth = month.Month == 1
                ? new DateTime(month.Year - 1, 12, DateTime.DaysInMonth(month.Year - 1, 12))
                : new DateTime(month.Year, month.Month - 1, DateTime.DaysInMonth(month.Year, month.Month - 1));

            // Filter transactions up to and including the last day of the previous month.
            var transactionsBeforeThisMonth = Transactions.Where(t => t.Date <= lastDayOfPreviousMonth);

            // Calculate the balance based on these transactions.
            decimal previousTransactionsTotal = transactionsBeforeThisMonth.Sum(t =>
                t.Type == TransactionType.Deposit ? t.Amount : -t.Amount);

            return Balance - Transactions.Sum(t => t.Type == TransactionType.Deposit ? t.Amount : -t.Amount) + previousTransactionsTotal;
        }

        public decimal GetEodBalance(DateTime date)
        {
            // Return the sum of transactions up to the end of the given date
            return Transactions
                .Where(t => t.Date <= date)
                .Sum(t => t.Type == TransactionType.Deposit ? t.Amount : -t.Amount);
        }

        // Generates the next transaction ID for a given date
        private string GenerateNextTransactionId(DateTime date)
        {
            int lastNumber = 1;
            var transactionsOnSameDate = Transactions.Where(t => t.Date.Date == date.Date).ToList();
            if (transactionsOnSameDate.Count > 0)
            {
                var lastTransactionId = transactionsOnSameDate.Last().TransactionId;
                var lastSuffix = lastTransactionId.Split('-')[1];
                lastNumber = int.Parse(lastSuffix) + 1;
            }

            return $"{date:yyyyMMdd}-{lastNumber:00}";
        }

        public List<Transaction> GetTransactionsForMonth(DateTime month)
        {
            return Transactions
                .Where(t => t.Date.Year == month.Year && t.Date.Month == month.Month)
                .OrderBy(t => t.Date)
                .ToList();
        }

        public string GetNewTransactionId(DateTime date)
        {
            return GenerateNextTransactionId(date);
        }
    }
}
