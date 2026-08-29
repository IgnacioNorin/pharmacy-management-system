using PharmacySystem.Business;
using PharmacySystem.Helpers;
using PharmacySystem.Infrastructure;
using PharmacySystem.Model;

namespace PharmacySystem.Presentation
{
    // Ported from Login.cs. Cliente is refused here regardless of password. VerifyPassword also
    // preserves the legacy plain-text migration path: a matching plain-text password logs the
    // user in and rewrites it as a hash before returning.
    public class LoginPresenter
    {
        private readonly ILoginView _view;
        private readonly IPersonService _personService;

        public LoginPresenter(ILoginView view, IPersonService personService)
        {
            _view = view;
            _personService = personService;
        }

        public void OnLogin()
        {
            try
            {
                Person person = _personService.GetByDocument(_view.Document?.Trim());

                if (person != null && person.Estado && person.oPersonType.idPersonType != (int)PersonType.Cliente && VerifyPassword(person, _view.Password))
                {
                    _view.LoginSucceeded(person);
                }
                else
                {
                    _view.ShowError("No se encontraron coincidencias del usuario");
                }
            }
            catch (DataUnavailableException ex)
            {
                _view.ShowError(ex.Message);
            }
        }

        private bool VerifyPassword(Person person, string enteredPassword)
        {
            if (PasswordHasher.IsHashed(person.password))
            {
                return PasswordHasher.Verify(enteredPassword, person.password);
            }

            // Legacy plain-text password: validate directly and migrate it to a hash on successful login.
            if (person.password == enteredPassword)
            {
                _personService.UpdatePassword(person.idPerson, PasswordHasher.Hash(enteredPassword));
                return true;
            }

            return false;
        }
    }
}
