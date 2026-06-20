using AutoMapper;
using ClosedXML.Excel;
using FridgeWatch.Application.DTOs;
using FridgeWatch.Application.Interfaces;
using FridgeWatch.Domain.Entities;
using FridgeWatch.Domain.Enums;
using FridgeWatch.Domain.Interfaces;
using FridgeWatch.Domain.Common;
using System.Globalization;
using System.Text;

namespace FridgeWatch.Application.Services;

public class FoodItemService : IFoodItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IExpiryAlertSyncService _alertSyncService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;

    public FoodItemService(IUnitOfWork unitOfWork, IMapper mapper, IExpiryAlertSyncService alertSyncService, IFileStorageService fileStorageService, IAuditLogService auditLogService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _alertSyncService = alertSyncService;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<FoodItemDto>> GetListAsync(FoodItemQueryParametersDto parameters, int? householdId = null, int? userId = null)
    {
        var queryParams = _mapper.Map<FoodItemQueryParameters>(parameters);

        var resolvedHouseholdId = await ResolveHouseholdIdAsync(householdId, userId);
        var result = await _unitOfWork.FoodItems.GetFilteredAsync(queryParams, resolvedHouseholdId);

        return _mapper.Map<PagedResultDto<FoodItemDto>>(result);
    }

    private async Task<int?> ResolveHouseholdIdAsync(int? householdId, int? userId)
    {
        if (householdId.HasValue)
        {
            return householdId.Value;
        }

        if (userId.HasValue)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user != null && user.DefaultHouseholdId.HasValue)
            {
                return user.DefaultHouseholdId.Value;
            }
        }

        return null;
    }

    public async Task<FoodItemDetailDto> GetByIdAsync(int id)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        return _mapper.Map<FoodItemDetailDto>(foodItem);
    }

    public async Task<FoodItemDto> CreateAsync(FoodItemCreateDto dto, int userId, Stream? photoStream = null, string? photoFileName = null)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(dto.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法添加食材");
        }

        var foodItem = _mapper.Map<FoodItem>(dto);
        foodItem.CreatedByUserId = userId;
        foodItem.Status = FoodStatusHelper.CalculateStatus(foodItem.ExpiryDate, foodItem.Quantity);

        if (photoStream != null && !string.IsNullOrEmpty(photoFileName))
        {
            var (photoUrl, thumbnailUrl) = await _fileStorageService.SaveAsync(photoStream, photoFileName, "image/*");
            foodItem.PhotoUrl = photoUrl;
            foodItem.ThumbnailUrl = thumbnailUrl;
        }

        await _unitOfWork.FoodItems.AddAsync(foodItem);
        await _unitOfWork.SaveChangesAsync();

        await _alertSyncService.SyncAlertsForFoodItemAsync(foodItem);

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("FoodItem", foodItem.Id, "Create", userId, operatorName, foodItem.HouseholdId, $"添加食材「{foodItem.Name}」，数量 {foodItem.Quantity}{foodItem.Unit}");

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    public async Task<FoodItemDto> UpdateAsync(int id, FoodItemUpdateDto dto, int userId, Stream? photoStream = null, string? photoFileName = null)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改食材");
        }

        _mapper.Map(dto, foodItem);

        if (photoStream != null && !string.IsNullOrEmpty(photoFileName))
        {
            if (!string.IsNullOrEmpty(foodItem.PhotoUrl) || !string.IsNullOrEmpty(foodItem.ThumbnailUrl))
            {
                await _fileStorageService.DeleteAsync(foodItem.PhotoUrl ?? "", foodItem.ThumbnailUrl ?? "");
            }

            var (photoUrl, thumbnailUrl) = await _fileStorageService.SaveAsync(photoStream, photoFileName, "image/*");
            foodItem.PhotoUrl = photoUrl;
            foodItem.ThumbnailUrl = thumbnailUrl;
        }

        if (dto.ExpiryDate.HasValue)
        {
            foodItem.Status = FoodStatusHelper.CalculateStatus(dto.ExpiryDate.Value, foodItem.Quantity);
        }

        await _unitOfWork.FoodItems.UpdateAsync(foodItem);
        await _unitOfWork.SaveChangesAsync();

        await _alertSyncService.SyncAlertsForFoodItemAsync(foodItem);

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("FoodItem", foodItem.Id, "Update", userId, operatorName, foodItem.HouseholdId, $"修改食材「{foodItem.Name}」");

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    public async Task DeleteAsync(int id, int userId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法删除食材");
        }

        if (!string.IsNullOrEmpty(foodItem.PhotoUrl) || !string.IsNullOrEmpty(foodItem.ThumbnailUrl))
        {
            await _fileStorageService.DeleteAsync(foodItem.PhotoUrl ?? "", foodItem.ThumbnailUrl ?? "");
        }

        await _unitOfWork.FoodItems.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("FoodItem", id, "Delete", userId, operatorName, foodItem.HouseholdId, $"删除食材「{foodItem.Name}」");
    }

    public async Task<FoodItemDto> UpdateStatusAsync(int id, FoodStatus status, int userId)
    {
        var foodItem = await _unitOfWork.FoodItems.GetByIdAsync(id);
        if (foodItem == null)
        {
            throw new BusinessException("食材不存在");
        }

        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(foodItem.HouseholdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法修改食材状态");
        }

        foodItem.Status = status;
        foodItem.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.FoodItems.UpdateAsync(foodItem);
        await _unitOfWork.SaveChangesAsync();

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("FoodItem", foodItem.Id, "UpdateStatus", userId, operatorName, foodItem.HouseholdId, $"更新食材「{foodItem.Name}」状态为 {status}");

        return _mapper.Map<FoodItemDto>(foodItem);
    }

    public async Task<FoodItemImportResultDto> BulkImportAsync(int householdId, Stream fileStream, string fileName, int userId)
    {
        if (!await _unitOfWork.HouseholdMembers.IsHouseholdMemberAsync(householdId, userId))
        {
            throw new UnauthorizedAccessException("您不是该家庭的成员，无法导入食材");
        }

        if (fileStream == null || fileStream.Length == 0)
        {
            throw new BusinessException("请上传有效的文件");
        }

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".csv")
        {
            throw new BusinessException("仅支持 .xlsx 或 .csv 格式的文件");
        }

        var rows = ext == ".csv"
            ? ParseCsvFile(fileStream)
            : ParseExcelFile(fileStream);

        var result = new FoodItemImportResultDto
        {
            TotalRows = rows.Count
        };

        var existingItems = (await _unitOfWork.FoodItems
            .FindAsync(f => f.HouseholdId == householdId))
            .ToList();

        foreach (var row in rows)
        {
            try
            {
                var (isValid, errorMessage, createDto) = ValidateAndMapRow(row, householdId);
                if (!isValid || createDto == null)
                {
                    result.Errors.Add(new FoodItemImportErrorDto
                    {
                        RowNumber = row.RowNumber,
                        ErrorMessage = errorMessage ?? "数据校验失败"
                    });
                    result.FailedCount++;
                    continue;
                }

                var existingItem = existingItems.FirstOrDefault(f =>
                    f.Name.Equals(createDto.Name, StringComparison.OrdinalIgnoreCase) &&
                    f.Category.Equals(createDto.Category, StringComparison.OrdinalIgnoreCase));

                FoodItem processedItem;
                if (existingItem != null)
                {
                    existingItem.StorageLocation = createDto.StorageLocation;
                    existingItem.PurchaseDate = createDto.PurchaseDate;
                    existingItem.ExpiryDate = createDto.ExpiryDate;
                    existingItem.Quantity = createDto.Quantity;
                    existingItem.Unit = createDto.Unit;
                    existingItem.Status = FoodStatusHelper.CalculateStatus(createDto.ExpiryDate, createDto.Quantity);
                    existingItem.UpdatedAt = DateTime.UtcNow;

                    await _unitOfWork.FoodItems.UpdateAsync(existingItem);
                    processedItem = existingItem;
                    result.UpdatedCount++;
                }
                else
                {
                    var foodItem = _mapper.Map<FoodItem>(createDto);
                    foodItem.CreatedByUserId = userId;
                    foodItem.Status = FoodStatusHelper.CalculateStatus(foodItem.ExpiryDate, foodItem.Quantity);
                    await _unitOfWork.FoodItems.AddAsync(foodItem);
                    existingItems.Add(foodItem);
                    processedItem = foodItem;
                    result.CreatedCount++;
                }

                await _unitOfWork.SaveChangesAsync();
                await _alertSyncService.SyncAlertsForFoodItemAsync(processedItem);
                result.ImportedItems.Add(_mapper.Map<FoodItemDto>(processedItem));
            }
            catch (Exception ex)
            {
                result.Errors.Add(new FoodItemImportErrorDto
                {
                    RowNumber = row.RowNumber,
                    ErrorMessage = $"处理失败：{ex.Message}"
                });
                result.FailedCount++;
            }
        }

        var operatorName = (await _unitOfWork.Users.GetByIdAsync(userId))?.Username ?? userId.ToString();
        await _auditLogService.LogAsync("FoodItem", 0, "BulkImport", userId, operatorName, householdId,
            $"批量导入食材：新增 {result.CreatedCount} 条，更新 {result.UpdatedCount} 条，失败 {result.FailedCount} 条",
            $"总计 {result.TotalRows} 行");

        return result;
    }

    public async Task<byte[]> DownloadTemplateAsync()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("食材导入模板");

        var headers = new[] { "名称", "分类", "存放位置", "购买日期", "保质期", "数量", "单位" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
            worksheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        }

        var examples = new[]
        {
            new object[] { "牛奶", "乳制品", "冷藏", "2026-06-15", "2026-06-25", 2, "盒" },
            new object[] { "鸡胸肉", "肉类", "冷冻", "2026-06-10", "2026-07-10", 1.5, "kg" },
            new object[] { "大米", "主食", "常温", "2026-01-01", "2026-12-31", 10, "kg" }
        };
        for (int i = 0; i < examples.Length; i++)
        {
            for (int j = 0; j < examples[i].Length; j++)
            {
                worksheet.Cell(i + 2, j + 1).Value = XLCellValue.FromObject(examples[i][j]);
            }
        }

        var noteSheet = workbook.AddWorksheet("填写说明");
        noteSheet.Cell(1, 1).Value = "填写说明";
        noteSheet.Cell(1, 1).Style.Font.Bold = true;
        noteSheet.Cell(1, 1).Style.Font.FontSize = 14;

        var notes = new[]
        {
            "1. 名称：必填，食材名称，系统将根据【名称+分类】判断是新增还是更新已有食材",
            "2. 分类：必填，食材所属分类（如：乳制品、肉类、蔬菜、水果等）",
            "3. 存放位置：必填，可选值为【冷藏/冷冻/常温】，不区分大小写",
            "4. 购买日期：必填，格式为 YYYY-MM-DD，如 2026-06-15",
            "5. 保质期：必填，格式为 YYYY-MM-DD，如 2026-06-25",
            "6. 数量：必填，数字格式，支持小数",
            "7. 单位：必填，如：盒、kg、个、袋等",
            "",
            "存放位置对应关系：",
            "  冷藏 -> Fridge（冰箱冷藏）",
            "  冷冻 -> Freezer（冰箱冷冻）",
            "  常温 -> Pantry（储物柜/ pantry）"
        };
        for (int i = 0; i < notes.Length; i++)
        {
            noteSheet.Cell(i + 3, 1).Value = notes[i];
        }

        worksheet.Columns().AdjustToContents();
        noteSheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return await Task.FromResult(stream.ToArray());
    }

    private List<FoodItemImportRowDto> ParseExcelFile(Stream fileStream)
    {
        var rows = new List<FoodItemImportRowDto>();
        fileStream.Position = 0;

        using var workbook = new XLWorkbook(fileStream);
        var worksheet = workbook.Worksheet(1);
        var range = worksheet.RangeUsed();
        if (range == null || range.RowCount() < 2)
        {
            return rows;
        }

        for (int rowNum = 2; rowNum <= range.LastRow().RowNumber(); rowNum++)
        {
            var row = worksheet.Row(rowNum);
            if (row.IsEmpty()) continue;

            rows.Add(new FoodItemImportRowDto
            {
                RowNumber = rowNum,
                Name = GetCellString(row.Cell(1)),
                Category = GetCellString(row.Cell(2)),
                StorageLocation = GetCellString(row.Cell(3)),
                PurchaseDate = GetCellString(row.Cell(4)),
                ExpiryDate = GetCellString(row.Cell(5)),
                Quantity = GetCellString(row.Cell(6)),
                Unit = GetCellString(row.Cell(7))
            });
        }

        return rows;
    }

    private List<FoodItemImportRowDto> ParseCsvFile(Stream fileStream)
    {
        var rows = new List<FoodItemImportRowDto>();
        fileStream.Position = 0;

        using var reader = new StreamReader(fileStream, Encoding.UTF8);
        var allLines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            allLines.Add(line);
        }

        var parsedRecords = ParseCsvRecords(allLines);

        for (int i = 0; i < parsedRecords.Count; i++)
        {
            var values = parsedRecords[i];
            if (values.All(string.IsNullOrWhiteSpace)) continue;

            rows.Add(new FoodItemImportRowDto
            {
                RowNumber = i + 1,
                Name = values.Length > 0 ? values[0] : string.Empty,
                Category = values.Length > 1 ? values[1] : string.Empty,
                StorageLocation = values.Length > 2 ? values[2] : string.Empty,
                PurchaseDate = values.Length > 3 ? values[3] : string.Empty,
                ExpiryDate = values.Length > 4 ? values[4] : string.Empty,
                Quantity = values.Length > 5 ? values[5] : string.Empty,
                Unit = values.Length > 6 ? values[6] : string.Empty
            });
        }

        if (rows.Count > 0 && IsHeaderRow(rows[0]))
        {
            rows.RemoveAt(0);
            foreach (var row in rows)
            {
                row.RowNumber--;
            }
        }

        return rows;
    }

    internal static bool IsHeaderRow(FoodItemImportRowDto row)
    {
        var headerKeywords = new[] { "名称", "分类", "存放位置", "购买日期", "保质期", "数量", "单位" };
        var fields = new[] { row.Name, row.Category, row.StorageLocation, row.PurchaseDate, row.ExpiryDate, row.Quantity, row.Unit };
        var matchCount = fields.Count(f => headerKeywords.Any(k => f.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0));
        return matchCount >= 3;
    }

    internal static List<string[]> ParseCsvRecords(List<string> lines)
    {
        var records = new List<string[]>();
        var currentFields = new List<string>();
        var currentField = new StringBuilder();
        bool inQuotes = false;
        int i = 0;

        while (i < lines.Count)
        {
            var line = lines[i];
            int pos = 0;

            while (pos < line.Length)
            {
                if (inQuotes)
                {
                    if (line[pos] == '"')
                    {
                        if (pos + 1 < line.Length && line[pos + 1] == '"')
                        {
                            currentField.Append('"');
                            pos += 2;
                        }
                        else
                        {
                            inQuotes = false;
                            pos++;
                        }
                    }
                    else
                    {
                        currentField.Append(line[pos]);
                        pos++;
                    }
                }
                else
                {
                    if (line[pos] == '"')
                    {
                        inQuotes = true;
                        pos++;
                    }
                    else if (line[pos] == ',')
                    {
                        currentFields.Add(currentField.ToString());
                        currentField.Clear();
                        pos++;
                    }
                    else
                    {
                        currentField.Append(line[pos]);
                        pos++;
                    }
                }
            }

            if (inQuotes)
            {
                currentField.Append('\n');
                i++;
            }
            else
            {
                currentFields.Add(currentField.ToString());
                currentField.Clear();
                records.Add(currentFields.ToArray());
                currentFields.Clear();
                i++;
            }
        }

        if (inQuotes || currentFields.Count > 0 || currentField.Length > 0)
        {
            currentFields.Add(currentField.ToString());
            records.Add(currentFields.ToArray());
        }

        return records;
    }

    private static string GetCellString(IXLCell cell)
    {
        if (cell == null || cell.Value.IsBlank) return string.Empty;
        if (cell.Value.IsDateTime)
        {
            return cell.GetDateTime().ToString("yyyy-MM-dd");
        }
        return cell.Value.ToString()?.Trim() ?? string.Empty;
    }

    private (bool IsValid, string? ErrorMessage, FoodItemCreateDto? Dto) ValidateAndMapRow(
        FoodItemImportRowDto row, int householdId)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(row.Name))
            errors.Add("名称不能为空");
        if (string.IsNullOrWhiteSpace(row.Category))
            errors.Add("分类不能为空");
        if (string.IsNullOrWhiteSpace(row.StorageLocation))
            errors.Add("存放位置不能为空");
        if (string.IsNullOrWhiteSpace(row.PurchaseDate))
            errors.Add("购买日期不能为空");
        if (string.IsNullOrWhiteSpace(row.ExpiryDate))
            errors.Add("保质期不能为空");
        if (string.IsNullOrWhiteSpace(row.Quantity))
            errors.Add("数量不能为空");
        if (string.IsNullOrWhiteSpace(row.Unit))
            errors.Add("单位不能为空");

        if (errors.Count > 0)
            return (false, string.Join("；", errors), null);

        StorageLocation location;
        var locStr = row.StorageLocation.Trim().ToLowerInvariant();
        switch (locStr)
        {
            case "冷藏":
            case "fridge":
            case "冰箱":
                location = StorageLocation.Fridge;
                break;
            case "冷冻":
            case "freezer":
                location = StorageLocation.Freezer;
                break;
            case "常温":
            case "pantry":
            case "储物柜":
            case "干货":
                location = StorageLocation.Pantry;
                break;
            default:
                errors.Add($"存放位置无效：{row.StorageLocation}，可选值：冷藏/冷冻/常温");
                return (false, string.Join("；", errors), null);
        }

        if (!DateTime.TryParseExact(row.PurchaseDate.Trim(),
            new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd", "yyyy-M-d", "yyyy/M/d" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var purchaseDate))
        {
            errors.Add($"购买日期格式无效：{row.PurchaseDate}，请使用 YYYY-MM-DD 格式");
        }

        if (!DateTime.TryParseExact(row.ExpiryDate.Trim(),
            new[] { "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd", "yyyy-M-d", "yyyy/M/d" },
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiryDate))
        {
            errors.Add($"保质期格式无效：{row.ExpiryDate}，请使用 YYYY-MM-DD 格式");
        }

        if (!decimal.TryParse(row.Quantity.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var quantity)
            || quantity < 0)
        {
            errors.Add($"数量无效：{row.Quantity}，必须为非负数");
        }

        if (errors.Count > 0)
            return (false, string.Join("；", errors), null);

        if (expiryDate < purchaseDate)
            return (false, "保质期不能早于购买日期", null);

        var dto = new FoodItemCreateDto
        {
            HouseholdId = householdId,
            Name = row.Name.Trim(),
            Category = row.Category.Trim(),
            StorageLocation = location,
            PurchaseDate = DateTime.SpecifyKind(purchaseDate, DateTimeKind.Utc),
            ExpiryDate = DateTime.SpecifyKind(expiryDate, DateTimeKind.Utc),
            Quantity = quantity,
            Unit = row.Unit.Trim()
        };

        return (true, null, dto);
    }
}
