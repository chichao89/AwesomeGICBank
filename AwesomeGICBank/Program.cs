using AwesomeGICBank.Services;

namespace AwesomeGICBank
{
    class Program
    {
        static void Main(string[] args)
        {
            IBankingService bankingService = new BankingService();
            MenuService menuService = new MenuService(bankingService);
            menuService.Run();
        }
    }
}
