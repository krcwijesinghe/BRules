// See https://aka.ms/new-console-template for more information
public class PayrollPolicy
{
    public string CompanyCode { get; init; } = "";
    public double OtHourlyRate { get; init; }           // e.g., 300
    public double NoPayDailyRate { get; init; }         // e.g., 2500
    public double EpfEmployeePct { get; init; }         // e.g., 0.08 (8%)
    public double EpfCompanyPct { get; init; }         // e.g., 0.12 (12%)
    public double EtfPct { get; init; }         // e.g., 0.03 (3%)
}
