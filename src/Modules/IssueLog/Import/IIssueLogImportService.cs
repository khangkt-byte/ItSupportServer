using ItSupportServer.src.Shared.Base;

namespace ItSupportServer.src.Modules.IssueLog
{
    /// <summary>
    /// Issue log import/export service interface
    /// Pattern: ServiceNow data import API
    /// Reference: Separation of concerns (SoC)
    /// </summary>
    public interface IIssueLogImportService
    {
        /// <summary>
        /// Step 1: Validate Excel file and return preview + errors
        /// </summary>
        /// <param name="excelStream">Excel file stream</param>
        /// <returns>Validation result with errors, warnings, duplicates</returns>
        Task<ImportValidationResultDto> ValidateImportAsync(Stream excelStream);
        
        /// <summary>
        /// Step 2: Import with user's decisions on how to handle errors
        /// </summary>
        /// <param name="excelStream">Excel file stream</param>
        /// <param name="options">Import configuration</param>
        /// <returns>Import execution result</returns>
        Task<ImportResultDto> ImportFromExcelAsync(Stream excelStream, ImportOptionsDto options);
        
        /// <summary>
        /// Export to Excel
        /// </summary>
        /// <param name="parameters">Query parameters (filter, search)</param>
        /// <returns>Excel file as byte array</returns>
        Task<byte[]> ExportToExcelAsync(QueryParameters? parameters = null);
    }
}