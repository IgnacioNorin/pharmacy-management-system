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
    // Same shape as SupplierRepository: inline parameterised SQL, a filtered/paged list, and a
    // soft-delete stored procedure. Clients used to be person rows with person_type_id = 4.
    public class ClientRepository : IClientRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ClientRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Client client)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "INSERT INTO client(document_number, name, address, phone, business_name, activity, commune, email, is_company) " +
                        "VALUES (@document, @name, @address, @phone, @business_name, @activity, @commune, @email, @is_company); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    return oConnection.ExecuteScalar<int>(sql, new
                    {
                        document = client.document,
                        name = client.name,
                        address = client.address,
                        phone = client.phone,
                        business_name = client.businessName,
                        activity = client.activity,
                        commune = client.commune,
                        email = client.email,
                        is_company = client.isCompany
                    });
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return 0; // a client with that document already exists
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

        public bool Update(Client client)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    const string sql =
                        "UPDATE client SET document_number = @document, name = @name, address = @address, " +
                        "phone = @phone, business_name = @business_name, activity = @activity, commune = @commune, " +
                        "email = @email, is_company = @is_company WHERE id = @id_client;";

                    oConnection.Execute(sql, new
                    {
                        id_client = client.idClient,
                        document = client.document,
                        name = client.name,
                        address = client.address,
                        phone = client.phone,
                        business_name = client.businessName,
                        activity = client.activity,
                        commune = client.commune,
                        email = client.email,
                        is_company = client.isCompany
                    });

                    return true;
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return false; // another client already uses that document
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

        private const string ClientSelectColumns =
            "id AS idClient, document_number AS document, name, address, phone, " +
            "business_name AS businessName, activity, commune, email, is_company AS isCompany ";

        private const string ClientWhere = "FROM client WHERE ISNULL(status, 1) = 1";

        public List<Client> ListClients()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Client>("SELECT " + ClientSelectColumns + ClientWhere + " ORDER BY name, id").ToList();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Client>();
                }
            }
        }

        // One page of active clients, filtered by a term that matches name, document, business
        // name or email. Count and page come back from a single command.
        public PagedResult<Client> ListClientsPaged(int pageNumber, int pageSize, string search)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = PagedResult<Client>.DefaultPageSize;

            const string filter =
                " AND (@search = '' OR name LIKE @like OR document_number LIKE @like " +
                "OR business_name LIKE @like OR email LIKE @like)";

            string sql =
                "SELECT COUNT(*) " + ClientWhere + filter + ";" +
                "SELECT " + ClientSelectColumns + ClientWhere + filter +
                " ORDER BY name, id OFFSET @offset ROWS FETCH NEXT @take ROWS ONLY;";

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
                        var items = multi.Read<Client>().ToList();

                        return new PagedResult<Client>
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
                    return PagedResult<Client>.Empty(pageSize);
                }
            }
        }

        public bool Delete(int idClient)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("id_client", idClient);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // sp_delete_client hard-deletes when unreferenced, otherwise soft-deletes
                    // (status = 0) - same pattern as suppliers.
                    oConnection.Execute("sp_delete_client", parameters, commandType: CommandType.StoredProcedure);

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
