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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public CategoryRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Categories obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("description", obj.description);
                    parameters.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_create_category", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<int>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public bool Update(Categories obj)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("category_id", obj.IdCategory);
                    parameters.Add("description", obj.description);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_update_category", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public List<Categories> List()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Categories>(
                        "SELECT id AS IdCategory, description FROM category WHERE status = 1")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Categories>();
                }
            }
        }

        public bool Delete(int idCategory)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("category_id", idCategory);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_delete_category", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
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
