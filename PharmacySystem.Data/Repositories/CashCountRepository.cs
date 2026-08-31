using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class CashCountRepository : ICashCountRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public CashCountRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public DateTime? GetLastPeriodEnd()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.QueryFirstOrDefault<DateTime?>("SELECT MAX(period_end) FROM cash_count");
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        public DateTime? GetEarliestSaleDate()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.QueryFirstOrDefault<DateTime?>("SELECT MIN(date_registered) FROM sale");
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        public List<CashCountLine> GetExpectedTotals(DateTime start, DateTime end)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // Sum the real per-method breakdown (sale_payment), not sale.total_amount by
                    // the primary method - a mixed sale must credit each method its own share,
                    // and a credit note's negated rows net out here.
                    const string sql =
                        "SELECT sp.payment_method AS paymentMethod, ISNULL(SUM(sp.amount), 0) AS expectedAmount " +
                        "FROM sale s INNER JOIN sale_payment sp ON sp.sale_id = s.id " +
                        "WHERE s.date_registered >= @start AND s.date_registered < @end " +
                        "GROUP BY sp.payment_method";

                    return oConnection.Query<CashCountLine>(sql, new { start, end }).ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<CashCountLine>();
                }
            }
        }

        public int Register(CashCount cashCount)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                oConnection.Open();
                SqlTransaction tx = oConnection.BeginTransaction();
                try
                {
                    const string insertHeader =
                        "INSERT INTO cash_count(period_start, period_end, user_id, notes) " +
                        "VALUES (@periodStart, @periodEnd, @userId, @notes); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int id = oConnection.ExecuteScalar<int>(insertHeader, new
                    {
                        cashCount.periodStart,
                        cashCount.periodEnd,
                        cashCount.userId,
                        cashCount.notes
                    }, tx);

                    const string insertLine =
                        "INSERT INTO cash_count_line(cash_count_id, payment_method, expected_amount, counted_amount) " +
                        "VALUES (@id, @paymentMethod, @expectedAmount, @countedAmount)";

                    foreach (CashCountLine line in cashCount.lines)
                    {
                        oConnection.Execute(insertLine, new
                        {
                            id,
                            line.paymentMethod,
                            line.expectedAmount,
                            line.countedAmount
                        }, tx);
                    }

                    tx.Commit();
                    return id;
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    tx.Rollback();
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    tx.Rollback();
                    return 0;
                }
            }
        }

        public List<CashCount> History()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "SELECT c.id, c.period_start AS periodStart, c.period_end AS periodEnd, " +
                        "c.user_id AS userId, pe.name AS userName, c.notes, c.created_at AS createdAt, " +
                        "l.payment_method AS paymentMethod, l.expected_amount AS expectedAmount, l.counted_amount AS countedAmount " +
                        "FROM cash_count c " +
                        "LEFT JOIN person pe ON pe.id = c.user_id " +
                        "LEFT JOIN cash_count_line l ON l.cash_count_id = c.id " +
                        "ORDER BY c.created_at DESC, c.id DESC, l.id";

                    var byId = new Dictionary<int, CashCount>();
                    oConnection.Query<CashCount, CashCountLine, CashCount>(
                        sql,
                        (header, line) =>
                        {
                            if (!byId.TryGetValue(header.id, out CashCount current))
                            {
                                current = header;
                                current.lines = new List<CashCountLine>();
                                byId.Add(current.id, current);
                            }
                            if (line != null && line.paymentMethod != null)
                            {
                                current.lines.Add(line);
                            }
                            return current;
                        },
                        splitOn: "paymentMethod");

                    return byId.Values.ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<CashCount>();
                }
            }
        }
    }
}
