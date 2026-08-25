using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
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
                    var parameters = new DynamicParameters();
                    parameters.Add("document", obj.document);
                    parameters.Add("company_name", obj.companyName);
                    parameters.Add("email", obj.email);
                    parameters.Add("phone", obj.phone);
                    parameters.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_create_supplier", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<int>("result");
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
                    var parameters = new DynamicParameters();
                    parameters.Add("id_supplier", obj.idSupplier);
                    parameters.Add("document", obj.document);
                    parameters.Add("company_name", obj.companyName);
                    parameters.Add("email", obj.email);
                    parameters.Add("phone", obj.phone);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_update_supplier", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
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
                        "SELECT id AS idSupplier, document_number AS document, company_name AS companyName, email, phone FROM supplier")
                        .ToList();
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
                    oConnection.Execute("DELETE FROM supplier WHERE id = @id_supplier", new { id_supplier = idSupplier });
                    return true;
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
