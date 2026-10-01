using System.Text.Json;

namespace BGTA.Server.Helpers
{
    public static class JsonConversionExtensions
    {
        public static int ToInt(this object? obj)
        {
            if (obj == null || obj is DBNull) return 0;
            if (obj is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var i)) return i;
                if (element.ValueKind == JsonValueKind.String && int.TryParse(element.GetString(), out var res)) return res;
                return 0;
            }
            return int.TryParse(obj.ToString(), out var result) ? result : 0;
        }

        public static decimal ToDecimal(this object? obj)
        {
            if (obj == null || obj is DBNull) return 0;
            if (obj is JsonElement element)
            {
                if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var d)) return d;
                if (element.ValueKind == JsonValueKind.String && decimal.TryParse(element.GetString(), out var res)) return res;
                return 0;
            }
            return decimal.TryParse(obj.ToString(), out var result) ? result : 0;
        }

       public static DateTime? ToExcelDate(this object? obj)
       {
         if (obj == null || obj is DBNull) return null; // Retourne null au lieu de string.Empty
       if (obj is JsonElement element)
      {
        if (element.ValueKind == JsonValueKind.String && DateTime.TryParse(element.GetString(), out var dt)) 
            return dt;
        return null;
       }
    return DateTime.TryParse(obj.ToString(), out var res) ? res : (DateTime?)null;
      }
        public static string ToStringSafe(this object? obj)
        {
            if (obj == null || obj is DBNull) return string.Empty;
            if (obj is JsonElement element) return element.GetString() ?? string.Empty;
            return obj.ToString() ?? string.Empty;
        }
    }
}