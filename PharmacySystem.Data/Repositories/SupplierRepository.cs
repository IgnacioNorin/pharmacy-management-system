using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public SupplierRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Supplier obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "INSERT INTO supplier(document_number, company_name, email, phone) " +
                        "VALUES (@document, @company_name, @email, @phone); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    return oConnection.ExecuteScalar<int>(sql, new
                    {
                        document = obj.document,
                        company_name = obj.companyName,
                        email = obj.email,
                        phone = obj.phone
                    });
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return 0; // a supplier with that document already exists
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public bool Update(Supplier obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "UPDATE supplier SET document_number = @document, company_name = @company_name, " +
                        "email = @email, phone = @phone WHERE id = @id_supplier;";

                    oConnection.Execute(sql, new
                    {
                        id_supplier = obj.idSupplier,
                        document = obj.document,
                        company_name = obj.companyName,
                        email = obj.email,
                        phone = obj.phone
                    });

                    return true;
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return false; // another supplier already uses that document
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        private const string SupplierSelectColumns =
            "id AS idSupplier, document_number AS document, company_name AS companyName, email, phone ";

        private const string SupplierWhere = "FROM supplier WHERE ISNULL(status, 1) = 1";

        public List<Supplier> List()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Supplier>("SELECT " + SupplierSelectColumns + SupplierWhere).ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Supplier>();
                }
            }
        }

        // One page of active suppliers, filtered by a term that matches company name, document
        // or email. Count and page come back from a single command.
        public PagedResult<Supplier> ListPaged(int pageNumber, int pageSize, string search)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = PagedResult<Supplier>.DefaultPageSize;

            const string filter =
                " AND (@search = '' OR company_name LIKE @like OR document_number LIKE @like OR email LIKE @like)";

            string sql =
                "SELECT COUNT(*) " + SupplierWhere + filter + ";" +
                "SELECT " + SupplierSelectColumns + SupplierWhere + filter +
                " ORDER BY company_name, id OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY;";

            string term = (search ?? string.Empty).Trim();
            var param = new
            {
                search = term,
                like = "%" + term + "%",
                offset = (pageNumber - 1) * pageSize,
                take = pageSize
            };

            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    using (SqlMapper.GridReader multi = oConnection.QueryMultiple(sql, param))
                    {
                        int total = multi.ReadFirst<int>();
                        var items = multi.Read<Supplier>().ToList();

                        return new PagedResult<Supplier>
                        {
                            Items = items,
                            TotalCount = total,
                            PageNumber = pageNumber,
                            PageSize = pageSize
                        };
                    }
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return PagedResult<Supplier>.Empty(pageSize);
                }
            }
        }

        public bool Delete(int idSupplier)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("id_supplier", idSupplier);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // sp_delete_supplier hard-deletes when unreferenced, otherwise soft-deletes
                    // (status = 0) - same pattern as products, categories and persons.
                    oConnection.Execute("sp_delete_supplier", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }
    }
}
