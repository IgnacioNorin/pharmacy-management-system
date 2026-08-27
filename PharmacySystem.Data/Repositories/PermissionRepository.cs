using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Data
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public PermissionRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public List<Permission> GetAll()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<Permission>(
                        "SELECT id AS Id, code AS Code, section AS Section, description AS Description " +
                        "FROM permission ORDER BY section, code")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<Permission>();
                }
            }
        }

        public List<string> GetCodesForRole(int personTypeId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<string>(
                        "SELECT p.code FROM role_permission rp " +
                        "INNER JOIN permission p ON p.id = rp.permission_id " +
                        "WHERE rp.person_type_id = @personTypeId",
                        new { personTypeId })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<string>();
                }
            }
        }
    }
}
