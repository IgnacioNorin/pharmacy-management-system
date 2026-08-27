namespace PharmacySystem.Model
{
    // Mirrors the person_type table's built-in id column (is_system = 1). Custom roles use ids
    // >= 100 and have no member here. Since the permissions rework, application logic branches
    // on permissions, not on the role - the only member still referenced in code is Cliente
    // (a client is a data-only role that cannot sign in). The other three are kept as
    // documentation of the seeded ids.
    public enum PersonType
    {
        AdministradorGeneral = 1,
        Administrador = 2,
        Empleado = 3,
        Cliente = 4
    }
}
