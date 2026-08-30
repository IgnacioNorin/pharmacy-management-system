namespace PharmacySystem.Model
{
    // Mirrors the person_type table's built-in id column (is_system = 1). Custom roles use ids
    // >= 100 and have no member here. Since the permissions rework, application logic branches
    // on permissions, not on the role; these members are kept as documentation of the seeded
    // ids. Clients live in their own `client` table now - there is no client role.
    public enum PersonType
    {
        AdministradorGeneral = 1,
        Administrador = 2,
        Empleado = 3
    }
}
