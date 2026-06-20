using ClosedXML.Excel;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Common;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;

namespace FridgeWatch.Application.Services;

public class DataExportService : IDataExportService
{
    private readonly IUnitOfWork _unitOfWork;

    public DataExportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<byte[]> ExportAsync(DataExportRequestDto dto, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(dto.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法导出数据");
        }

        if (dto.StartDate > dto.EndDate)
        {
            throw new BusinessException("开始日期不能晚于结束日期");
        }

        var startDate = DateTime.SpecifyKind(dto.StartDate, DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(dto.EndDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

        var foodItems = await _unitOfWork.FoodItems.FindAsync(
            f => f.HouseholdId == dto.HouseholdId
                 && f.CreatedAt >= startDate
                 && f.CreatedAt <= endDate);

        var consumptionRecords = await _unitOfWork.ConsumptionRecords.FindAsync(
            cr => cr.FoodItem!.HouseholdId == dto.HouseholdId
                  && cr.ConsumedAt >= startDate
                  && cr.ConsumedAt <= endDate);

        var expiryAlerts = await _unitOfWork.ExpiryAlerts.FindAsync(
            ea => ea.UserId == userId
                  && ea.FoodItem!.HouseholdId == dto.HouseholdId
                  && ea.AlertDate >= startDate
                  && ea.AlertDate <= endDate);

        using var workbook = new XLWorkbook();

        BuildFoodItemsSheet(workbook, foodItems);
        BuildConsumptionRecordsSheet(workbook, consumptionRecords);
        BuildExpiryAlertsSheet(workbook, expiryAlerts);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private void BuildFoodItemsSheet(XLWorkbook workbook, List<Domain.Entities.FoodItem> foodItems)
    {
        var ws = workbook.AddWorksheet("食材清单");

        var headers = new[] { "ID", "名称", "分类", "存放位置", "购买日期", "保质期", "数量", "单位", "状态", "创建时间" };
        WriteHeaders(ws, headers);

        for (int i = 0; i < foodItems.Count; i++)
        {
            var item = foodItems[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = item.Id;
            ws.Cell(row, 2).Value = item.Name;
            ws.Cell(row, 3).Value = item.Category;
            ws.Cell(row, 4).Value = GetStorageLocationText(item.StorageLocation);
            ws.Cell(row, 5).Value = item.PurchaseDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 6).Value = item.ExpiryDate.ToString("yyyy-MM-dd");
            ws.Cell(row, 7).Value = item.Quantity;
            ws.Cell(row, 8).Value = item.Unit;
            ws.Cell(row, 9).Value = GetFoodStatusText(item.Status);
            ws.Cell(row, 10).Value = item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        ws.Columns().AdjustToContents();
    }

    private void BuildConsumptionRecordsSheet(XLWorkbook workbook, List<Domain.Entities.ConsumptionRecord> records)
    {
        var ws = workbook.AddWorksheet("消耗记录");

        var headers = new[] { "ID", "食材名称", "消耗数量", "单位", "消耗时间", "备注", "消耗人", "记录时间" };
        WriteHeaders(ws, headers);

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = record.Id;
            ws.Cell(row, 2).Value = record.FoodItem?.Name ?? "";
            ws.Cell(row, 3).Value = record.ConsumedQuantity;
            ws.Cell(row, 4).Value = record.FoodItem?.Unit ?? "";
            ws.Cell(row, 5).Value = record.ConsumedAt.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 6).Value = record.Note ?? "";
            ws.Cell(row, 7).Value = record.User?.Username ?? "";
            ws.Cell(row, 8).Value = record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        ws.Columns().AdjustToContents();
    }

    private void BuildExpiryAlertsSheet(XLWorkbook workbook, List<Domain.Entities.ExpiryAlert> alerts)
    {
        var ws = workbook.AddWorksheet("提醒记录");

        var headers = new[] { "ID", "食材名称", "提醒类型", "提醒日期", "是否已读", "创建时间" };
        WriteHeaders(ws, headers);

        for (int i = 0; i < alerts.Count; i++)
        {
            var alert = alerts[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = alert.Id;
            ws.Cell(row, 2).Value = alert.FoodItem?.Name ?? "";
            ws.Cell(row, 3).Value = GetAlertTypeText(alert.AlertType);
            ws.Cell(row, 4).Value = alert.AlertDate.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(row, 5).Value = alert.IsRead ? "已读" : "未读";
            ws.Cell(row, 6).Value = alert.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        ws.Columns().AdjustToContents();
    }

    private static void WriteHeaders(IXLWorksheet ws, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
        }
    }

    private static string GetStorageLocationText(StorageLocation location) => location switch
    {
        StorageLocation.Fridge => "冷藏",
        StorageLocation.Freezer => "冷冻",
        StorageLocation.Pantry => "常温",
        _ => location.ToString()
    };

    private static string GetFoodStatusText(FoodStatus status) => status switch
    {
        FoodStatus.Fresh => "新鲜",
        FoodStatus.NearExpiry => "即将过期",
        FoodStatus.Expired => "已过期",
        FoodStatus.Consumed => "已消耗",
        FoodStatus.Archived => "已归档",
        _ => status.ToString()
    };

    private static string GetAlertTypeText(AlertType alertType) => alertType switch
    {
        AlertType.NearExpiry => "即将过期",
        AlertType.Expired => "已过期",
        AlertType.Custom => "自定义",
        _ => alertType.ToString()
    };
}
