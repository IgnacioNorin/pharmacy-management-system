using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class PersonRepository : IPersonRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public PersonRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool Register(Person person)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_create_person", oConnection);
                    cmd.Parameters.AddWithValue("document", person.document);
                    cmd.Parameters.AddWithValue("name", person.name);
                    cmd.Parameters.AddWithValue("address", person.address);
                    cmd.Parameters.AddWithValue("phone", person.phone);
                    cmd.Parameters.AddWithValue("password", person.password);
                    cmd.Parameters.AddWithValue("person_type_id", person.oPersonType.idPersonType);
                    cmd.Parameters.Add("result", SqlDbType.Int).Direction = ParameterDirection.Output;
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

        public bool Update(Person person)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("sp_update_person", oConnection);
                    cmd.Parameters.AddWithValue("id_person", person.idPerson);
                    cmd.Parameters.AddWithValue("document", person.document);
                    cmd.Parameters.AddWithValue("name", person.name);
                    cmd.Parameters.AddWithValue("address", person.address);
                    cmd.Parameters.AddWithValue("phone", person.phone);
                    cmd.Parameters.AddWithValue("password", person.password);
                    cmd.Parameters.AddWithValue("person_type_id", person.oPersonType.idPersonType);
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

        public List<Person> List()
        {
            List<Person> listPerson = new List<Person>();
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT p.id AS idproduct,p.document_number,p.name,p.address,p.phone,p.password,pt.id AS person_type_id,pt.description, p.status FROM person p");
                    sb.AppendLine("INNER JOIN person_type pt on pt.id = p.person_type_id");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            listPerson.Add(new Person()
                            {
                                idPerson = Convert.ToInt32(dr["idproduct"]),
                                document = dr["document_number"].ToString(),
                                name = dr["name"].ToString(),
                                address = dr["address"].ToString(),
                                phone = dr["phone"].ToString(),
                                password = dr["password"].ToString(),
                                oPersonType = new TypePerson() { idPersonType = Convert.ToInt32(dr["person_type_id"]), description = dr["description"].ToString() },
                                Estado = Convert.ToBoolean(dr["status"])
                            });
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    listPerson = new List<Person>();
                }
            }
            return listPerson;
        }

        public Person GetByDocument(string document)
        {
            Person person = null;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("SELECT p.id AS idproduct,p.document_number,p.name,p.address,p.phone,p.password,pt.id AS person_type_id,pt.description, p.status FROM person p");
                    sb.AppendLine("INNER JOIN person_type pt on pt.id = p.person_type_id");
                    sb.AppendLine("WHERE p.document_number = @document");

                    SqlCommand cmd = new SqlCommand(sb.ToString(), oConnection);
                    cmd.Parameters.AddWithValue("@document", document);
                    cmd.CommandType = CommandType.Text;

                    oConnection.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            person = new Person()
                            {
                                idPerson = Convert.ToInt32(dr["idproduct"]),
                                document = dr["document_number"].ToString(),
                                name = dr["name"].ToString(),
                                address = dr["address"].ToString(),
                                phone = dr["phone"].ToString(),
                                password = dr["password"].ToString(),
                                oPersonType = new TypePerson() { idPersonType = Convert.ToInt32(dr["person_type_id"]), description = dr["description"].ToString() },
                                Estado = Convert.ToBoolean(dr["status"])
                            };
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    person = null;
                }
            }
            return person;
        }

        public bool UpdatePassword(int idPerson, string hashedPassword)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("UPDATE person SET password = @password WHERE id = @id", oConnection);
                    cmd.Parameters.AddWithValue("@password", hashedPassword);
                    cmd.Parameters.AddWithValue("@id", idPerson);
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

        public bool Delete(int idPerson)
        {
            bool result = true;
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    SqlCommand cmd = new SqlCommand("DELETE FROM person WHERE id = @id", oConnection);
                    cmd.Parameters.AddWithValue("@id", idPerson);
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
