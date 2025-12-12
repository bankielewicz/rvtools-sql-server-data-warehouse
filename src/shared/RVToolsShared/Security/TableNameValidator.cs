namespace RVToolsShared.Security;

/// <summary>
/// SEC-006: Validates table names against the RVTools whitelist.
/// Prevents SQL injection by only allowing known table names.
/// </summary>
public static class TableNameValidator
{
    /// <summary>
    /// The 27 valid RVTools table names (sheet names from Excel export).
    /// </summary>
    public static readonly HashSet<string> ValidTableNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // VM-related
        "vInfo", "vCPU", "vMemory", "vDisk", "vPartition", "vNetwork",
        "vCD", "vUSB", "vSnapshot", "vTools",

        // Host-related
        "vHost", "vHBA", "vNIC",

        // Network-related
        "vSwitch", "vPort", "dvSwitch", "dvPort", "vSC_VMK",

        // Storage-related
        "vDatastore", "vMultiPath", "vFileInfo",

        // Cluster/Resource-related
        "vCluster", "vRP",

        // Licensing
        "vLicense",

        // Metadata/Health
        "vSource", "vHealth", "vMetaData"
    };

    /// <summary>
    /// Checks if a table name is in the allowed whitelist.
    /// </summary>
    /// <param name="tableName">Table name to validate</param>
    /// <returns>True if table name is valid</returns>
    public static bool IsValid(string tableName)
    {
        return !string.IsNullOrWhiteSpace(tableName) && ValidTableNames.Contains(tableName);
    }

    /// <summary>
    /// Validates a table name and throws if invalid.
    /// </summary>
    /// <param name="tableName">Table name to validate</param>
    /// <exception cref="ArgumentException">Thrown if table name is not in whitelist</exception>
    public static void Validate(string tableName)
    {
        if (!IsValid(tableName))
        {
            throw new ArgumentException(
                $"Table name '{tableName}' is not in the allowed RVTools table whitelist (SEC-006).",
                nameof(tableName));
        }
    }

    /// <summary>
    /// Returns the properly-cased table name from the whitelist.
    /// </summary>
    /// <param name="tableName">Table name (case-insensitive)</param>
    /// <returns>Properly-cased table name, or null if not found</returns>
    public static string? GetCanonicalName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return null;

        return ValidTableNames.FirstOrDefault(t =>
            string.Equals(t, tableName, StringComparison.OrdinalIgnoreCase));
    }
}
