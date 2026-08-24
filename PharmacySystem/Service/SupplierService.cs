using PharmacySystem.Helpers;
using PharmacySystem.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacySystem.Logical
{
    public class SupplierService
    {
        private static SupplierService instance = null;

        public SupplierService()
        {

        }

        public static SupplierService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new SupplierService();
                }

                return instance;
            }
        }


        public int RegisterSupplier(Supplier obj)
        {
            int result = 0;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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

        public bool UpdateSupplier(Supplier obj)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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


        public List<Supplier> ListSupplier()
        {
            List<Supplier> List = new List<Supplier>();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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

        public bool DeleteSupplier(int idSupplier)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
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
