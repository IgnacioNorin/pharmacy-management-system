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
            "p.business_name AS businessName, p.activity, p.commune, p.email, p.is_company AS isCompany, " +
            "p.password, ISNULL(p.status, 1) AS Estado, " +
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
                    const string sql =
                        "INSERT INTO person(document_number, name, address, phone, business_name, activity, commune, email, is_company, password, person_type_id) " +
                        "VALUES (@document, @name, @address, @phone, @business_name, @activity, @commune, @email, @is_company, @password, @person_type_id); " +
                        "SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    return oConnection.ExecuteScalar<int>(sql, new
                    {
                        document = person.document,
                        name = person.name,
                        address = person.address,
                        phone = person.phone,
                        business_name = person.businessName,
                        activity = person.activity,
                        commune = person.commune,
                        email = person.email,
                        is_company = person.isCompany,
                        password = person.password,
                        person_type_id = person.oPersonType.idPersonType
                    });
                }
                catch (Exception ex) when (SqlErrorCodes.IsUniqueViolation(ex))
                {
                    return 0; // a person with that document already exists
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
                    parameters.Add("business_name", person.businessName);
                    parameters.Add("activity", person.activity);
                    parameters.Add("commune", person.commune);
                    parameters.Add("email", person.email);
                    parameters.Add("is_company", person.isCompany);
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
