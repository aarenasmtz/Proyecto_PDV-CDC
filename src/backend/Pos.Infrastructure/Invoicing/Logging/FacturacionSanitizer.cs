using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Pos.Infrastructure.Invoicing.Logging;

public static class FacturacionSanitizer
{
    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "apikey", "api-key", "x-api-key", "x-tenant-key", "tenantkey", "tenant-key",
        "secret", "password", "contrasena", "contraseña", "privatekey", "private-key",
        "token", "bearer", "authorization", "auth", "certificate", "certificado",
        "csd", "keypassword", "passphrase", "webhooksecret", "llaveprivada"
    };

    private static readonly HashSet<string> DocumentLargeKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "base64logo", "base64cancellationacknowledgement", "acusecancelacionxml",
        "xmlcontenido", "pdfbase64", "xmlbase64", "base64pdf", "base64xml",
        "certificatecontent", "privatekeycontent"
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string? SanitizeJson(string? rawJson, int maxChars = 16384)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
            return null;

        try
        {
            var node = JsonNode.Parse(rawJson);
            if (node != null)
            {
                SanitizeNode(node);
                var result = node.ToJsonString(JsonOptions);
                return Truncate(result, maxChars);
            }
        }
        catch
        {
            // Si no es un JSON válido, sanitizar por expresiones regulares
            var scrubbed = Regex.Replace(
                rawJson,
                @"(api[-_]?key|secret|password|token|bearer|authorization)\s*[:=]\s*['""]?[^'""\s,;]+['""]?",
                "$1=***REDACTED***",
                RegexOptions.IgnoreCase);

            return Truncate(scrubbed, maxChars);
        }

        return Truncate(rawJson, maxChars);
    }

    public static string? SanitizeObject<T>(T? obj, int maxChars = 16384)
    {
        if (obj == null)
            return null;

        try
        {
            var json = JsonSerializer.Serialize(obj, JsonOptions);
            return SanitizeJson(json, maxChars);
        }
        catch
        {
            return $"[ERROR_SERIALIZACION: {obj.GetType().Name}]";
        }
    }

    public static string? SanitizeException(Exception? ex)
    {
        if (ex == null)
            return null;

        var fullMessage = $"{ex.GetType().FullName}: {ex.Message}\n{ex.StackTrace}";
        var scrubbed = Regex.Replace(
            fullMessage,
            @"(api[-_]?key|secret|password|token|bearer|authorization)\s*[:=]\s*['""]?[^'""\s,;]+['""]?",
            "$1=***REDACTED***",
            RegexOptions.IgnoreCase);

        return Truncate(scrubbed, 8192);
    }

    private static void SanitizeNode(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var keys = obj.Select(kv => kv.Key).ToList();
            foreach (var key in keys)
            {
                var val = obj[key];
                if (SensitiveKeys.Contains(key))
                {
                    obj[key] = "***REDACTED***";
                }
                else if (DocumentLargeKeys.Contains(key) && val is JsonValue jsonVal && jsonVal.TryGetValue<string>(out var strVal))
                {
                    obj[key] = $"[BASE64_DOCUMENT_DATA_EXCLUDED: length={strVal.Length}]";
                }
                else if (val is JsonValue strNode && strNode.TryGetValue<string>(out var stringContent) && stringContent.Length > 500 && IsLikelyBase64(stringContent))
                {
                    obj[key] = $"[BASE64_DATA_EXCLUDED: length={stringContent.Length}]";
                }
                else if (val != null)
                {
                    SanitizeNode(val);
                }
            }
        }
        else if (node is JsonArray arr)
        {
            foreach (var item in arr)
            {
                if (item != null)
                {
                    SanitizeNode(item);
                }
            }
        }
    }

    private static bool IsLikelyBase64(string s)
    {
        if (s.Length % 4 != 0) return false;
        var trimmed = s.TrimEnd('=');
        return trimmed.All(c => char.IsLetterOrDigit(c) || c == '+' || c == '/');
    }

    private static string Truncate(string input, int maxChars)
    {
        if (input.Length <= maxChars)
            return input;

        return input.Substring(0, maxChars) + "\n... [TRUNCATED]";
    }
}

