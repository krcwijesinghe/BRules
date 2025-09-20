// See https://aka.ms/new-console-template for more information
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DepartmentId { get; set; }
    public string CompanyCode { get; set; }

    // Inputs
    public double BasicSalary { get; set; }
    public double TotalOtHours { get; set; }
    public double TotalNoPayDays { get; set; }

    // Outputs
    public double NetSalary { get; set; }
    public double GrossSalary { get; set; }
    public double EpfCompany { get; set; }
    public double EpfEmployee { get; set; }
    public double Etf { get; set; }
    public double NetSalaryAfterTax { get; set; }

    public void CalculatePayroll()
    {
        Console.WriteLine($"Payroll calculated for {Name}, Basic: {BasicSalary}");
    }
}
