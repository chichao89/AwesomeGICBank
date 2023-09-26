using Xunit;
using AwesomeGICBank.Services;
using AwesomeGICBank.Entities;
using System;

namespace AwesomeGICBank.Tests
{
    public class BankingServiceTests
    {
        private readonly BankingService _service = new BankingService();

        [Fact]
        public void CreateTransaction_DepositForExistingAccount_AddsTransaction()
        {
            // Arrange
            string accountId = "AC001";
            decimal initialBalance = _service.GetOrCreateAccount(accountId).Balance;

            // Act
            var transaction = _service.CreateTransaction(DateTime.Now, TransactionType.Deposit, 50, accountId);

            // Assert
            var account = _service.GetOrCreateAccount(accountId);
            Assert.Equal(initialBalance + 50, account.Balance);
            Assert.Contains(transaction, account.Transactions);
        }

        [Fact]
        public void CreateTransaction_WithdrawalInsufficientFunds_ThrowsException()
        {
            // Arrange
            string accountId = "AC001";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _service.CreateTransaction(DateTime.Now, TransactionType.Withdrawal, 10000, accountId));
        }

        [Fact]
        public void CreateTransaction_WithdrawalForNewAccount_ThrowsException()
        {
            // Arrange
            string accountId = "AC_NEW";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _service.CreateTransaction(DateTime.Now, TransactionType.Withdrawal, 50, accountId));
        }

     

        [Fact]
        public void CreateTransaction_MultipleTransactionsSameDay_GeneratesUniqueTransactionIds()
        {
            // Arrange
            string accountId = "AC001";

            // Act
            var txn1 = _service.CreateTransaction(new DateTime(2024, 1, 1), TransactionType.Deposit, 50, accountId);
            var txn2 = _service.CreateTransaction(new DateTime(2024, 1, 1), TransactionType.Deposit, 50, accountId);

            // Assert
            Assert.NotEqual(txn1.TransactionId, txn2.TransactionId);
        }
    }
}
