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
                        "SELECT id AS Id, code AS Code, section AS Section, description AS Description, parent_code AS ParentCode " +
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

        public List<int> GetRolesGranting(string permissionCode)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<int>(
                        "SELECT DISTINCT rp.person_type_id FROM role_permission rp " +
                        "INNER JOIN permission p ON p.id = rp.permission_id " +
                        "WHERE p.code = @permissionCode",
                        new { permissionCode })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<int>();
                }
            }
        }

        public List<TypePerson> GetRoles()
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<TypePerson>(
                        "SELECT id AS idPersonType, description, is_system AS IsSystem " +
                        "FROM person_type ORDER BY id")
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<TypePerson>();
                }
            }
        }

        public List<int> GetPermissionIdsForRole(int personTypeId)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    return oConnection.Query<int>(
                        "SELECT permission_id FROM role_permission WHERE person_type_id = @personTypeId",
                        new { personTypeId })
                        .ToList();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return new List<int>();
                }
            }
        }

        public bool SetRolePermissions(int personTypeId, IEnumerable<int> permissionIds)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    string csv = string.Join(",", (permissionIds ?? Enumerable.Empty<int>()).Distinct());

                    var parameters = new DynamicParameters();
                    parameters.Add("person_type_id", personTypeId);
                    parameters.Add("permission_ids", csv);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_set_role_permissions", parameters,
                        commandType: CommandType.StoredProcedure);

                    // false = the procedure refused the save (it would strip roles.gestionar from
                    // the last role that has it).
                    return parameters.Get<bool>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return false;
                }
            }
        }

        public int CreateRole(string description)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("description", description);
                    parameters.Add("result", dbType: DbType.Int32, direction: ParameterDirection.Output);

                    oConnection.Execute("sp_create_person_type", parameters, commandType: CommandType.StoredProcedure);

                    return parameters.Get<int>("result");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    return 0;
                }
            }
        }

        public bool RenameRole(int personTypeId, string description)
        {
            return ExecuteRoleBitProc("sp_update_person_type", p =>
            {
                p.Add("id", personTypeId);
                p.Add("description", description);
            });
        }

        public bool DeleteRole(int personTypeId)
        {
            return ExecuteRoleBitProc("sp_delete_person_type", p => p.Add("id", personTypeId));
        }

        private bool ExecuteRoleBitProc(string procName, Action<DynamicParameters> addInputs)
        {
            using (SqlConnection oConnection = _connectionFactory.Create())
            {
                try
                {
                    var parameters = new DynamicParameters();
                    addInputs(parameters);
                    parameters.Add("result", dbType: DbType.Boolean, direction: ParameterDirection.Output);

                    oConnection.Execute(procName, parameters, commandType: CommandType.StoredProcedure);

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
