using AwesomeGICBank.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AwesomeGICBank.Services
{
    public interface IBankingService
    {
        Transaction CreateTransaction(DateTime date, TransactionType type, decimal amount, string accountId);
        List<Transaction> GetTransactions(string accountId);
        bool AccountExists(string accountId);

        void AddInterestRule(DateTime date, string ruleId, decimal rate);
        List<InterestRule> GetAllInterestRules();

        decimal CalculateInterestForMonth(string accountId, DateTime month);

        decimal GetStartingBalanceForMonth(string accountId, DateTime month);

        List<Transaction> GetTransactionsForMonth(string accountId, DateTime month);



    }
}
