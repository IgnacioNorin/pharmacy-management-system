using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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

        public List<Supplier> List()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Supplier>(
                        "SELECT id AS idSupplier, document_number AS document, company_name AS companyName, email, phone " +
                        "FROM supplier WHERE ISNULL(status, 1) = 1")
                        .ToList();
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
