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
    public class PersonRepository : IPersonRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        // person and its person_type in one row - split at idPersonType, the first column of the
        // TypePerson half, to build the nested oPersonType via Dapper multi-mapping.
        private const string PersonWithTypeSelect =
            "SELECT p.id AS idPerson, p.document_number AS document, p.name, p.address, p.phone, p.password, p.status AS Estado, " +
            "pt.id AS idPersonType, pt.description " +
            "FROM person p INNER JOIN person_type pt ON pt.id = p.person_type_id";

        public PersonRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public bool Register(Person person)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("document", person.document);
                    parameters.Add("name", person.name);
                    parameters.Add("address", person.address);
                    parameters.Add("phone", person.phone);
                    parameters.Add("password", person.password);
                    parameters.Add("person_type_id", person.oPersonType.idPersonType);
                    parameters.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_create_person", parameters, commandType: CommandType.StoredProcedure);

                    return Convert.ToBoolean(parameters.Get<int>("result"));
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public bool Update(Person person)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("id_person", person.idPerson);
                    parameters.Add("document", person.document);
                    parameters.Add("name", person.name);
                    parameters.Add("address", person.address);
                    parameters.Add("phone", person.phone);
                    parameters.Add("password", person.password);
                    parameters.Add("person_type_id", person.oPersonType.idPersonType);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_update_person", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<bool>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public List<Person> List()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Person, TypePerson, Person>(
                        PersonWithTypeSelect,
                        (person, typePerson) => { person.oPersonType = typePerson; return person; },
                        splitOn: "idPersonType")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Person>();
                }
            }
        }

        public Person GetByDocument(string document)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Person, TypePerson, Person>(
                        PersonWithTypeSelect + " WHERE p.document_number = @document",
                        (person, typePerson) => { person.oPersonType = typePerson; return person; },
                        new { document },
                        splitOn: "idPersonType")
                        .FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        public bool UpdatePassword(int idPerson, string hashedPassword)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Execute(
                        "UPDATE person SET password = @password WHERE id = @id",
                        new { password = hashedPassword, id = idPerson });
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public bool Delete(int idPerson)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("id_person", idPerson);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    // sp_delete_person hard-deletes when unreferenced, otherwise soft-deletes
                    // (status = 0) - same pattern as products and categories.
                    oConnection.Execute("sp_delete_person", parameters, commandType: CommandType.StoredProcedure);

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
