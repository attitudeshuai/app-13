namespace FridgeWatch.Application.DTOs;

public class DataExportRequestDto
{
    public int HouseholdId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
