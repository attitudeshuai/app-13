using FridgeWatch.Application.DTOs;

namespace FridgeWatch.Application.Interfaces;

public interface IDataExportService
{
    Task<byte[]> ExportAsync(DataExportRequestDto dto, int userId);
}
