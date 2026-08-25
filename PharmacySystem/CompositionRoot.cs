using PharmacySystem.Business;
using PharmacySystem.Data;
using PharmacySystem.Presentation;

namespace PharmacySystem
{
    // Manual composition instead of a DI container: with a handful of services this stays
    // readable, and a container would add a dependency plus runtime "magic" to resolve a
    // graph this small. Revisit only if the graph grows enough that this file gets unwieldy.
    internal static class CompositionRoot
    {
        internal static readonly ISqlConnectionFactory ConnectionFactory = SqlConnectionFactory.FromConfiguration();

        public static SupplierPresenter CreateSupplierPresenter(ISupplierView view)
        {
            ISupplierRepository repository = new SupplierRepository(ConnectionFactory);
            ISupplierService service = new SupplierService(repository);
            return new SupplierPresenter(view, service);
        }
    }
}
