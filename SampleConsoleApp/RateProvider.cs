public class RateProvider
{
    private readonly Dictionary<string, PayrollPolicy> _policies = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DEFAULT"] = new PayrollPolicy
        {
            CompanyCode = "DEFAULT",
            OtHourlyRate = 300,
            NoPayDailyRate = 2500,
            EpfEmployeePct = 0.08,
            EpfCompanyPct = 0.12,
            EtfPct = 0.03
        },
        ["ACME"] = new PayrollPolicy
        {
            CompanyCode = "ACME",
            OtHourlyRate = 350,
            NoPayDailyRate = 2600,
            EpfEmployeePct = 0.08,
            EpfCompanyPct = 0.12,
            EtfPct = 0.03
        }
    };

    public PayrollPolicy GetPolicy(string companyCode)
        => _policies.TryGetValue(companyCode, out var p) ? p : _policies["DEFAULT"];
}