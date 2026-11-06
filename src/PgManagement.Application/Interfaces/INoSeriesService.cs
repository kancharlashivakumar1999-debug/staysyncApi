namespace PgManagement.Application.Interfaces
{
    public interface INoSeriesService
    {
        Task<string> GetNextCodeAsync(string seriesKey, Guid? userId = null);
        Task<string> GetNextCodeWithTransactionAsync(System.Data.IDbConnection conn, System.Data.IDbTransaction tran, string seriesKey, Guid? userId = null);
    }
}
