using AwesomeGICBank.Entities;
using System;
using System.Globalization;

public static class ValidationHelper
{
    public static bool IsValidDateFormat(string dateString)
    {
        return DateTime.TryParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
    }

    public static bool IsValidTransactionType(string typeString)
    {
        typeString = typeString.ToUpper();
        return typeString == "D" || typeString == "W";
    }

    public static (DateTime date, TransactionType type, decimal amount, string accountId) ParseTransactionInput(string input)
    {
        var parts = input.Split('|');
        if (parts.Length != 4)
        {
            throw new ArgumentException("Invalid format! Please ensure you're using the correct format.");
        }

        if (!IsValidDateFormat(parts[0]))
        {
            throw new ArgumentException("Invalid date format! Use the YYYYMMdd format.");
        }

        var date = DateTime.ParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture);
        var accountId = parts[1];

        if (!IsValidTransactionType(parts[2]))
        {
            throw new ArgumentException("Invalid transaction type! Use D for deposit and W for withdrawal.");
        }

        var type = parts[2].ToUpper() == "D" ? TransactionType.Deposit : TransactionType.Withdrawal;

        if (!decimal.TryParse(parts[3], out var amount) || amount <= 0)
        {
            throw new ArgumentException("Invalid amount! Ensure it's a positive number.");
        }

        return (date, type, amount, accountId);
    }

    public static void ValidateTransactionCreation(TransactionType type, decimal amount, bool accountExists)
    {
        // 1. Check if account exists for withdrawals
        if (type == TransactionType.Withdrawal && !accountExists)
            throw new InvalidOperationException("Cannot create a withdrawal transaction for a new account.");

        //check for amount < 0
        if (amount <= 0)
            throw new ArgumentException($"{(type == TransactionType.Deposit ? "Deposit" : "Withdrawal")} amount must be greater than 0.");
    }

    public static (DateTime date, string ruleId, decimal rate) ParseInterestRuleInput(string input)
    {
        var parts = input.Split('|');
        if (parts.Length != 3)
        {
            throw new ArgumentException("Invalid format! Please ensure you're using the correct format.");
        }

        if (!IsValidDateFormat(parts[0]))
        {
            throw new ArgumentException("Invalid date format! Use the YYYYMMdd format.");
        }

        var date = DateTime.ParseExact(parts[0], "yyyyMMdd", CultureInfo.InvariantCulture);
        var ruleId = parts[1];

        if (!decimal.TryParse(parts[2], out var rate) || rate <= 0 || rate >= 100)
        {
            throw new ArgumentException("Invalid rate! Ensure it's between 0 and 100.");
        }

        return (date, ruleId, rate);
    }


}
