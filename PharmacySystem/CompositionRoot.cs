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

        public static NotificationConfigPresenter CreateNotificationConfigPresenter(INotificationConfigView view)
        {
            INotificationConfigRepository repository = new NotificationConfigRepository(ConnectionFactory);
            INotificationConfigService service = new NotificationConfigService(repository);
            return new NotificationConfigPresenter(view, service);
        }

        public static SupplierPickerPresenter CreateSupplierPickerPresenter(ISupplierPickerView view)
        {
            ISupplierRepository repository = new SupplierRepository(ConnectionFactory);
            ISupplierService service = new SupplierService(repository);
            return new SupplierPickerPresenter(view, service);
        }

        public static ClientPresenter CreateClientPresenter(IClientView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new ClientPresenter(view, service);
        }

        public static UserPresenter CreateUserPresenter(IUserView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new UserPresenter(view, service);
        }

        public static ClientPickerPresenter CreateClientPickerPresenter(IClientPickerView view)
        {
            IPersonRepository repository = new PersonRepository(ConnectionFactory);
            IPersonService service = new PersonService(repository);
            return new ClientPickerPresenter(view, service);
        }

        public static ProductPickerPresenter CreateProductPickerPresenter(IProductPickerView view, string origin)
        {
            IProductRepository repository = new ProductRepository(ConnectionFactory);
            IProductService service = new ProductService(repository);
            return new ProductPickerPresenter(view, service, origin);
        }
    }
}
