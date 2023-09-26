namespace AwesomeGICBank.Entities
{
    public class InterestRule
    {
        public DateTime Date { get; private set; }
        public string RuleId { get; private set; }
        public decimal Rate { get; private set; }

        public InterestRule(DateTime date, string ruleId, decimal rate)
        {
            Date = date;
            RuleId = ruleId;
            Rate = rate;
        }
    }
}
