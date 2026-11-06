using System.Data;
using Dapper;
using PgManagement.Application.Interfaces;
using PgManagement.Infrastructure.Data;

namespace PgManagement.Infrastructure.Services
{
    public class NoSeriesService : INoSeriesService
    {
        private readonly IDbConnectionFactory _factory;
        private readonly IIdentityInformationService _identity;

        public NoSeriesService(IDbConnectionFactory factory, IIdentityInformationService identity)
        {
            _factory = factory;
            _identity = identity;
        }

        public async Task<string> GetNextCodeAsync(string seriesKey, Guid? userId = null)
        {
            var effectiveUserId = userId ?? _identity.UserId;
            using var conn = _factory.CreateConnection();
            return await conn.ExecuteScalarAsync<string>(
                "SELECT config.next_doc_no(@SeriesKey, @UserId);",
                new { SeriesKey = seriesKey, UserId = effectiveUserId }
            ) ?? string.Empty;
        }

        public async Task<string> GetNextCodeWithTransactionAsync(IDbConnection conn, IDbTransaction tran, string seriesKey, Guid? userId = null)
        {
            var effectiveUserId = userId ?? _identity.UserId;
            return await conn.ExecuteScalarAsync<string>(
                "SELECT config.next_doc_no(@SeriesKey, @UserId);",
                new { SeriesKey = seriesKey, UserId = effectiveUserId },
                transaction: tran
            ) ?? string.Empty;
        }
    }
}
