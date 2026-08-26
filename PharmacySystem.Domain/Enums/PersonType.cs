namespace PharmacySystem.Model
{
    // Mirrors the person_type table's id column. Renumbered once already (Administrador General
    // introduced as 1, pushing Administrador/Empleado/Cliente down) - that renumbering required
    // hand-editing idPersonType comparisons across seven files because none of them referenced a
    // shared name. Compare against these members instead of the raw int from here on, so the next
    // change to this table only touches this file.
    public enum PersonType
    {
        AdministradorGeneral = 1,
        Administrador = 2,
        Empleado = 3,
        Cliente = 4
    }
}
