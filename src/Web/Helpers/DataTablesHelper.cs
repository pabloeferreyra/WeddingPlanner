namespace WeddingPlanner.Web.Helpers;


public static class DataTablesHelper
{
    /// <summary>
    /// Extracts DataTables draw/start/length parameters from the HTTP request.
    /// </summary>
    public static (string draw, int pageSize, int skip) GetDataTableParams(HttpRequest request)
    {
        var draw = request.Form["draw"].FirstOrDefault() ?? "1";
        var start = request.Form["start"].FirstOrDefault();
        var length = request.Form["length"].FirstOrDefault();

        int pageSize = length != null ? int.Parse(length) : 0;
        int skip = start != null ? int.Parse(start) : 0;

        return (draw, pageSize, skip);
    }

    public static List<T> ApplySorting<T>(List<T> data, HttpRequest request)
    {
        var orderColumnIndex = request.Form["order[0][column]"].FirstOrDefault();
        var sortColumn = !string.IsNullOrEmpty(orderColumnIndex)
            ? request.Form[$"columns[{orderColumnIndex}][name]"].FirstOrDefault()
            : null;
        var sortDir = request.Form["order[0][dir]"].FirstOrDefault();

        if (!string.IsNullOrEmpty(sortColumn))
        {
            try
            {
                if (string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase))
                    data = [.. data.OrderBy(v => GetPropValue(v, sortColumn))];
                else
                    data = [.. data.OrderByDescending(v => GetPropValue(v, sortColumn))];
            }
            catch
            {
                // Invalid sort column — return unsorted
            }
        }

        return data;
    }

        public static List<T> ApplyPaging<T>(List<T> data, int pageSize, int skip)
    {
        if (data == null) return [];
        if (pageSize == -1) return data;
        if (skip != 0 && pageSize > 0) return [.. data.Skip(skip).Take(pageSize)];
        if (pageSize > 0) return [.. data.Take(pageSize)];
        return data;
    }

    private static object? GetPropValue(object? obj, string propName)
    {
        if (obj == null || string.IsNullOrWhiteSpace(propName)) return null;
        var prop = obj.GetType().GetProperty(propName);
        return prop?.GetValue(obj);
    }
}