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
    public class PersonRepository : IPersonRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        // person and its person_type in one row - split at idPersonType, the first column of the
        // TypePerson half, to build the nested oPersonType via Dapper multi-mapping.
        private const string PersonWithTypeSelect =
            "SELECT p.id AS idPerson, p.document_number AS document, p.name, p.address, p.phone, " +
            "p.password, ISNULL(p.status, 1) AS Estado, ISNULL(p.must_change_password, 0) AS mustChangePassword, " +
            "pt.id AS idPersonType, pt.description " +
            "FROM person p INNER JOIN person_type pt ON pt.id = p.person_type_id";

        public PersonRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public int Register(Person person)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    // must_change_password = 1: a user created from the Usuarios screen gets a
                    // temporary password from the admin and must replace it on first login.
                    const string sql =
                        "INSERT INTO person(document_number, name, address, phone, password, person_type_id, must_change_password) " +
                        "VALUES (@document, @name, @address, @phone, @password, @person_type_id, 1); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    return oConnection.ExecuteScalar<int>(sql, new
                    {
                        document = person.document,
                        name = person.name,
                        address = person.address,
                        phone = person.phone,
                        password = person.password,
                        person_type_id = person.oPersonType.idPersonType
                    });
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return 0; // a person with that document already exists
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
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
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
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }

        public Person GetById(int idPerson)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Person, TypePerson, Person>(
                        PersonWithTypeSelect + " WHERE p.id = @id",
                        (person, typePerson) => { person.oPersonType = typePerson; return person; },
                        new { id = idPerson },
                        splitOn: "idPersonType")
                        .FirstOrDefault();
                }
                catch (SqlException ex) when (SqlErrorCodes.IsConnectivityError(ex))
                {
                    Logger.LogError(ex);
                    throw new DataUnavailableException(DataUnavailableException.DefaultMessage, ex);
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

        public bool SetPasswordAndFlag(int idPerson, string hashedPassword, bool mustChangePassword)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    oConnection.Execute(
                        "UPDATE person SET password = @password, must_change_password = @flag WHERE id = @id",
                        new { password = hashedPassword, flag = mustChangePassword, id = idPerson });
                    return true;
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
