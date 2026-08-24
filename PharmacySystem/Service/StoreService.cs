
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
    class StoreService
    {
        private static StoreService instance = null;

        public StoreService()
        {

        }

        public static StoreService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StoreService();
                }

                return instance;
            }
        }

        public Store ListStore()
        {
            Store obj = new Store();
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("SELECT id,document_store, company_name, email, phone, address FROM store WHERE id = 1", oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            obj = new Store()
                            {
                                document = dr["document_store"].ToString(),
                                companyName = dr["company_name"].ToString(),
                                email = dr["email"].ToString(),
                                phone = dr["phone"].ToString(),
                                address = dr["address"].ToString()
                            };
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    obj = new Store();
                }
            }
            return obj;
        }

        public bool UpdateStore(Store obj)
        {
            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("UPDATE store SET document_number = @document,");
                    sb.AppendLine("company_name = @company_name,");
                    sb.AppendLine("email = @email,");
                    sb.AppendLine("phone = @phone,");
                    sb.AppendLine("address = @address");
                    sb.AppendLine("WHERE id = 1");
                    
                    using (SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection)) {
                        cmd.Parameters.AddWithValue("@document", obj.document);
                        cmd.Parameters.AddWithValue("@company_name", obj.companyName);
                        cmd.Parameters.AddWithValue("@email", obj.email);
                        cmd.Parameters.AddWithValue("@phone", obj.phone);
                        cmd.Parameters.AddWithValue("@address", obj.address);
                        cmd.CommandType = CommandType.Text;
                        oConnection.Open();
                        cmd.ExecuteNonQuery();
                        result = true;
                    }

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
