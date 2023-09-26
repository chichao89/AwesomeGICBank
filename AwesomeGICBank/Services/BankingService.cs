using AwesomeGICBank.Entities;
using System.Data;

namespace AwesomeGICBank.Services
{
    public class BankingService : IBankingService
    {
        private readonly Dictionary<string, Account> _accounts = new Dictionary<string, Account>();
        private readonly Dictionary<DateTime, InterestRule> _interestRules = new Dictionary<DateTime, InterestRule>();
        private DateTime? GetNextRuleChangeDate(DateTime currentDate)
        {
            var nextDate = _interestRules.Keys
                                         .Where(date => date > currentDate)
                                         .OrderBy(date => date)
                                         .FirstOrDefault();

            // Check if nextDate is default, which would be DateTime.MinValue for DateTime type.
            if (nextDate == DateTime.MinValue)
            {
                return null;
            }

            return nextDate;
        }

        private void SeedData()
        {
            // Seeding Accounts and Transactions
            var account1 = new Account("AC001");
            account1.AddTransaction(new Transaction(new DateTime(2023, 5, 5), TransactionType.Deposit, 100.00m, "20230505-01"));
            account1.AddTransaction(new Transaction(new DateTime(2023, 6, 1), TransactionType.Deposit, 150.00m, "20230601-01"));
            account1.AddTransaction(new Transaction(new DateTime(2023, 6, 26), TransactionType.Withdrawal, 20.00m, "20230626-01"));
            account1.AddTransaction(new Transaction(new DateTime(2023, 6, 26), TransactionType.Withdrawal, 100.00m, "20230626-02"));

            _accounts[account1.AccountId] = account1;
            // Seeding Interest Rules
            var interestRule1 = new InterestRule(new DateTime(2023, 1, 1), "RULE01", 1.95m);
            var interestRule2 = new InterestRule(new DateTime(2023, 5, 20), "RULE02", 1.90m);
            var interestRule3 = new InterestRule(new DateTime(2023, 6, 15), "RULE03", 2.20m);
            

            _interestRules[interestRule1.Date] = interestRule1;
            _interestRules[interestRule2.Date] = interestRule2;
            _interestRules[interestRule3.Date] = interestRule3;
        }

        public BankingService()
        {
            SeedData();
        }

        public Account GetOrCreateAccount(string accountId)
        {
            if (!_accounts.TryGetValue(accountId, out var account))
            {
                account = new Account(accountId);
                _accounts[accountId] = account;
            }
            return account;
        }

        public bool AccountExists(string accountId)
        {
            return _accounts.ContainsKey(accountId);
        }

        public Transaction CreateTransaction(DateTime date, TransactionType type, decimal amount, string accountId)
        {
            // Check if date is in the future
            if (date > DateTime.Now)
            {
                throw new InvalidOperationException("Transaction date cannot be in the future.");
            }

            bool accountExists = AccountExists(accountId);
            ValidationHelper.ValidateTransactionCreation(type, amount, accountExists);

            var account = GetOrCreateAccount(accountId);

            var transactionId = account.GetNewTransactionId(date);
            var transaction = new Transaction(date, type, amount, transactionId);
            account.AddTransaction(transaction);
            return transaction;
        }


        public List<Transaction> GetTransactions(string accountId)
        {
            var account = GetOrCreateAccount(accountId);
            return account.Transactions;
        }

        public void AddInterestRule(DateTime date, string ruleId, decimal rate)
        {
            var interestRule = new InterestRule(date, ruleId, rate);
            _interestRules[date] = interestRule;

            Console.WriteLine($"Added Rule: {interestRule.RuleId} on {interestRule.Date:yyyy-MM-dd} with rate {interestRule.Rate}%");
        }


        public List<InterestRule> GetAllInterestRules()
        {
            return _interestRules.Values.OrderBy(r => r.Date).ToList();
        }

        public decimal CalculateInterestForMonth(string accountId, DateTime month)
        {
            Console.WriteLine($"Calculating interest for account {accountId} for {month:yyyy-MM}");

            var account = GetOrCreateAccount(accountId);
            var endOfMonth = new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month));

            decimal totalAnnualizedInterest = 0;
            DateTime startOfPeriod = new DateTime(month.Year, month.Month, 1);
            decimal eodBalance = account.GetStartingBalanceForMonth(month);

            var orderedTransactions = account.GetTransactionsForMonth(month).OrderBy(t => t.Date).ToList();

            while (startOfPeriod <= endOfMonth)
            {
                try
                {
                    DateTime? nextRuleDate = GetNextRuleChangeDate(startOfPeriod);
                    DateTime? nextTransactionDate = orderedTransactions.Where(t => t.Date > startOfPeriod).Select(t => t.Date).FirstOrDefault();

                    DateTime endOfPeriod = endOfMonth;

                    if (nextRuleDate.HasValue && nextRuleDate.Value != DateTime.MinValue && (nextTransactionDate == null || nextRuleDate < nextTransactionDate))
                    {
                        endOfPeriod = nextRuleDate.Value.AddDays(-1);
                    }
                    else if (nextTransactionDate.HasValue && nextTransactionDate.Value != DateTime.MinValue)
                    {
                        endOfPeriod = nextTransactionDate.Value.AddDays(-1);
                    }


                    var currentRule = GetRuleForDate(startOfPeriod);

                    decimal annualizedInterest = CalculateInterestForPeriod(startOfPeriod, endOfPeriod, eodBalance, currentRule);

                    totalAnnualizedInterest += annualizedInterest;

                    if (endOfPeriod == endOfMonth)
                    {
                        break;
                    }

                    startOfPeriod = endOfPeriod.AddDays(1);
                    eodBalance = account.GetEodBalance(startOfPeriod);
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine(ex.Message);
                    break;
                }
            }

            Console.WriteLine($"Total Annualized Interest for {month:yyyy-MM}: {totalAnnualizedInterest}");
            return Math.Round(totalAnnualizedInterest / 365, 2);
        }






        private decimal CalculateInterestForPeriod(DateTime start, DateTime end, decimal balance, InterestRule rule)
        {
            int days = (end - start).Days + 1;
            decimal annualizedInterest = balance * rule.Rate / 100 * days;
            Console.WriteLine($"From {start:yyyy-MM-dd} to {end:yyyy-MM-dd}, Balance: {balance}, Rate: {rule.Rate}%, Days: {days}, Annualized Interest: {annualizedInterest}");
            return annualizedInterest;
        }

        private InterestRule GetRuleForDate(DateTime date)
        {
            var applicableRule = _interestRules
                .Where(r => r.Key <= date)
                .OrderByDescending(r => r.Key)
                .FirstOrDefault();

            if (applicableRule.Value == null)
            {
                Console.WriteLine($"No rule found for date {date:yyyy-MM-dd}");
                throw new InvalidOperationException($"No interest rule found for date {date:yyyy-MM-dd}");
            }

            Console.WriteLine($"Rule {applicableRule.Value.RuleId} applied for date {date:yyyy-MM-dd}");
            return applicableRule.Value;
        }

        public decimal GetStartingBalanceForMonth(string accountId, DateTime month)
        {
            var account = GetOrCreateAccount(accountId); // Assuming you have this method or another way to get the account
            return account.GetStartingBalanceForMonth(month);
        }

        public List<Transaction> GetTransactionsForMonth(string accountId, DateTime month)
        {
            if (!_accounts.TryGetValue(accountId, out var account))
            {
                throw new InvalidOperationException("Account not found.");
            }

            return account.GetTransactionsForMonth(month);
        }




    }
}
