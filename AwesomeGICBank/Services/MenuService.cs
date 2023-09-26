using AwesomeGICBank.Entities;
using AwesomeGICBank.Services;
using System;
using System.Collections.Generic;
using System.Globalization;

public class MenuService
{
    private readonly IBankingService _bankingService;

    public MenuService(IBankingService bankingService)
    {
        _bankingService = bankingService;
    }

    public void Run()
    {
        while (true)
        {
            UserInterfaceHelper.DisplayMainMenu();
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input)) continue;

            switch (input.ToUpper()[0])
            {
                case 'I':
                    InputTransaction();
                    break;
                case 'D':
                    DefineInterestRule();
                    break;
                case 'P':
                    PrintAccountStatement();
                    break;
                case 'Q':
                    Console.WriteLine("Thank you for banking with AwesomeGIC Bank. Have a nice day!");
                    return;
            }
        }
    }

    private void InputTransaction()
    {
        Console.WriteLine("\nPlease enter transaction details in <Date>|<Account>|<Type>|<Amount> format");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) return;

        try
        {
            var (date, type, amount, accountId) = ValidationHelper.ParseTransactionInput(input);

            _bankingService.CreateTransaction(date, type, amount, accountId);

            // Here we get the starting balance for the account 
            //decimal startingBalance = _bankingService.GetStartingBalance(accountId);
            decimal startingBalance = _bankingService.GetStartingBalanceForMonth(accountId, DateTime.Now);  // or any appropriate date


            var transactions = _bankingService.GetTransactions(accountId);
            DisplayTransactions(accountId, transactions, startingBalance);
        }
        catch (Exception ex)
        {
            UserInterfaceHelper.DisplayErrorMessage(ex.Message);
        }
    }

    private decimal UpdateRunningBalance(decimal runningBalance, Transaction transaction)
    {
        switch (transaction.Type)
        {
            case TransactionType.Deposit:
                return runningBalance + transaction.Amount;
            case TransactionType.Withdrawal:
                return runningBalance - transaction.Amount;
            case TransactionType.Interest:
                return runningBalance + transaction.Amount;
            default:
                throw new InvalidOperationException($"Unknown transaction type: {transaction.Type}");
        }
    }

    private void DisplayTransactions(string accountId, List<Transaction> monthlyTransactions, decimal startingBalanceForMonth)
    {
        Console.WriteLine("\nAccount:\t" + accountId);
        Console.WriteLine("Date      | Txn Id      | Type | Amount   | Balance  |");
        Console.WriteLine("----------|-------------|------|----------|----------|");

        decimal runningBalance = startingBalanceForMonth;

        foreach (var transaction in monthlyTransactions)
        {

            runningBalance = UpdateRunningBalance(runningBalance, transaction);
            //Console.WriteLine($"{transaction.Date:yyyyMMdd}  | {transaction.TransactionId,-12} | {transaction.Type.ToString()[0]}    | {transaction.Amount,9:N2} | {runningBalance,9:N2} |");
            Console.WriteLine($"{transaction.Date:yyyyMMdd} ".PadRight(10) + $"| {transaction.TransactionId.PadRight(11)} | {transaction.Type.ToString()[0]}    | {transaction.Amount,8:N2} | {runningBalance,8:N2} |");
        }
    }



    private void DefineInterestRule()
    {
        Console.WriteLine("\nPlease enter interest rules details in <Date>|<RuleId>|<Rate in %> format");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) return;

        try
        {
            var (date, ruleId, rate) = ValidationHelper.ParseInterestRuleInput(input);
            _bankingService.AddInterestRule(date, ruleId, rate);
            DisplayInterestRules();
        }
        catch (Exception ex)
        {
            UserInterfaceHelper.DisplayErrorMessage(ex.Message);
        }
    }

    private void DisplayInterestRules()
    {
        Console.WriteLine("\nInterest rules:");
        Console.WriteLine("Date     | RuleId | Rate (%) |");
        var rules = _bankingService.GetAllInterestRules();
        foreach (var rule in rules)
        {
            Console.WriteLine($"{rule.Date:yyyyMMdd} | {rule.RuleId} | {rule.Rate,8:0.00} |");
        }
        Console.WriteLine();
    }

    private void PrintAccountStatement()
    {
        Console.WriteLine("\nPlease enter account and month to generate the statement <Account>|<Month>");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) return;

        var parts = input.Split('|');
        if (parts.Length != 2)
        {
            UserInterfaceHelper.DisplayErrorMessage("Invalid input format!");
            return;
        }

        string accountId = parts[0].Trim();
        string currentYear = DateTime.Now.Year.ToString();
        string monthString = currentYear + parts[1].Trim() + "01";

        Console.WriteLine($"Attempting to parse date: {monthString}");

        if (!DateTime.TryParseExact(monthString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime month))
        {
            UserInterfaceHelper.DisplayErrorMessage("Invalid month format!");
            return;
        }

        var transactionsForMonth = _bankingService.GetTransactionsForMonth(accountId, month);
        decimal startingBalanceForMonth = _bankingService.GetStartingBalanceForMonth(accountId, month);

        decimal interest = _bankingService.CalculateInterestForMonth(accountId, month);
        transactionsForMonth.Add(new Transaction(new DateTime(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month)), TransactionType.Interest, interest, ""));

        DisplayTransactions(accountId, transactionsForMonth, startingBalanceForMonth);
    }



}
