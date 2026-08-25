using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
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
            int result;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_create_category", oConnection);
                    cmd.Parameters.AddWithValue("description", obj.description);
                    cmd.Parameters.Add("result", SqlDbType.Int).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oConnection.Open();

                    cmd.ExecuteNonQuery();

                    result = Convert.ToInt32(cmd.Parameters["result"].Value);

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = 0;
                }
            }
            return result;
        }

        public bool Update(Categories obj)
        {
            bool result = false;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_update_category", oConnection);
                    cmd.Parameters.AddWithValue("category_id", obj.IdCategory);
                    cmd.Parameters.AddWithValue("description", obj.description);
                    cmd.Parameters.Add("result", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    oConnection.Open();

                    cmd.ExecuteNonQuery();

                    result = Convert.ToBoolean(cmd.Parameters["result"].Value);

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = false;
                }

            }

            return result;
        }

        public List<Categories> List()
        {
            List<Categories> List = new List<Categories>();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT id,description FROM category WHERE status = 1");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List.Add(new Categories()
                            {
                                IdCategory = Convert.ToInt32(dr["id"]),
                                description = dr["description"].ToString()
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    List = new List<Categories>();
                }
            }
            return List;
        }

        public bool Delete(int idCategory)
        {
            bool result = false;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_delete_category", oConnection);
                    cmd.Parameters.AddWithValue("category_id", idCategory);
                    cmd.Parameters.Add("result", SqlDbType.Bit).Direction = ParameterDirection.Output;

                    cmd.CommandType = CommandType.StoredProcedure;

                    oConnection.Open();

                    cmd.ExecuteNonQuery();

                    result = Convert.ToBoolean(cmd.Parameters["result"].Value);

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    result = false;
                }

            }

            return result;
        }
    }
}
