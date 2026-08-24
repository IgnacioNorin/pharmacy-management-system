
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
                    SqlCommand cmd = new SqlCommand("SELECT id,document_store, company_name, email, phone, address, currency_culture FROM store WHERE id = 1", oConnection);
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
                                address = dr["address"].ToString(),
                                currencyCulture = dr["currency_culture"] == DBNull.Value ? null : dr["currency_culture"].ToString()
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

        // True once the pharmacy has real sales or purchases on record. Used to lock the
        // currency setting: switching it doesn't convert any stored amount, it only changes
        // how numbers are formatted, so allowing it after real operations exist would make
        // every historical price/total/report display under a currency that was never
        // actually used for them.
        public bool HasOperationalData()
        {
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(
                        "SELECT CASE WHEN EXISTS (SELECT 1 FROM sale) OR EXISTS (SELECT 1 FROM purchase) THEN 1 ELSE 0 END",
                        oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    // Fail closed: if this can't be verified, assume there is operational data
                    // so a currency change is never silently let through on a DB hiccup.
                    return true;
                }
            }
        }

        public bool UpdateStore(Store obj)
        {
            Store currentStore = ListStore();
            bool isChangingCurrency = !string.Equals(currentStore?.currencyCulture, obj.currencyCulture, StringComparison.OrdinalIgnoreCase);
            if (isChangingCurrency && HasOperationalData())
            {
                return false;
            }

            bool result = true;
            using (SqlConnection oConnection = new SqlConnection(Connection.CN))
            {
                try
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine("UPDATE store SET document_store = @document,");
                    sb.AppendLine("company_name = @company_name,");
                    sb.AppendLine("email = @email,");
                    sb.AppendLine("phone = @phone,");
                    sb.AppendLine("address = @address,");
                    sb.AppendLine("currency_culture = @currency_culture");
                    sb.AppendLine("WHERE id = 1");

                    using (SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection)) {
                        cmd.Parameters.AddWithValue("@document", obj.document);
                        cmd.Parameters.AddWithValue("@company_name", obj.companyName);
                        cmd.Parameters.AddWithValue("@email", obj.email);
                        cmd.Parameters.AddWithValue("@phone", obj.phone);
                        cmd.Parameters.AddWithValue("@address", obj.address);
                        cmd.Parameters.AddWithValue("@currency_culture", (object)obj.currencyCulture ?? DBNull.Value);
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
