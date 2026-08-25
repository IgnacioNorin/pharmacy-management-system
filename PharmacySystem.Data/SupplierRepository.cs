using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    // Same SQL as the original PharmacySystem.Logical.SupplierService, moved here unchanged so
    // this migration step is a relocation, not a rewrite. The connection now comes from an
    // injected factory instead of the static Connection.CN.
    public class SupplierRepository : ISupplierRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public SupplierRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Supplier obj)
        {
            int result = 0;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_create_supplier", oConnection);
                    cmd.Parameters.AddWithValue("document", obj.document);
                    cmd.Parameters.AddWithValue("company_name", obj.companyName);
                    cmd.Parameters.AddWithValue("email", obj.email);
                    cmd.Parameters.AddWithValue("phone", obj.phone);
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

        public bool Update(Supplier obj)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_update_supplier", oConnection);
                    cmd.Parameters.AddWithValue("id_supplier", obj.idSupplier);
                    cmd.Parameters.AddWithValue("document", obj.document);
                    cmd.Parameters.AddWithValue("company_name", obj.companyName);
                    cmd.Parameters.AddWithValue("email", obj.email);
                    cmd.Parameters.AddWithValue("phone", obj.phone);
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

        public List<Supplier> List()
        {
            List<Supplier> List = new List<Supplier>();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT id,document_number,company_name,email,phone FROM supplier");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List.Add(new Supplier()
                            {
                                idSupplier = Convert.ToInt32(dr["id"]),
                                document = dr["document_number"].ToString(),
                                companyName = dr["company_name"].ToString(),
                                email = dr["email"].ToString(),
                                phone = dr["phone"].ToString()
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    List = new List<Supplier>();
                }
            }
            return List;
        }

        public bool Delete(int idSupplier)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM supplier WHERE id = @id_supplier", oConnection);
                    cmd.Parameters.AddWithValue("@id_supplier", idSupplier);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();

                    cmd.ExecuteNonQuery();

                    result = true;

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
