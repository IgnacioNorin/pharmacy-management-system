using PharmacySystem.Helpers;
using PharmacySystem.Model;
using PharmacySystem.Presentation;
using Xunit;

namespace PharmacySystem.Tests.Presentation
{
    public class LoginPresenterTests
    {
        private static LoginPresenter CreatePresenter(FakeLoginView view, FakePersonService service)
            => new LoginPresenter(view, service);

        [Fact]
        public void OnLogin_UnknownDocument_ShowsError()
        {
            var view = new FakeLoginView { Document = "999", Password = "irrelevant" };
            var service = new FakePersonService { GetByDocumentResult = null };

            CreatePresenter(view, service).OnLogin();

            Assert.Equal("No se econtraron coincidencias del usuario", view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_ClientRole_IsAlwaysRejected()
        {
            var view = new FakeLoginView { Document = "123", Password = "secret" };
            var service = new FakePersonService
            {
                GetByDocumentResult = new Person { idPerson = 1, password = "secret", oPersonType = new TypePerson { idPersonType = 4 } } // Cliente
            };

            CreatePresenter(view, service).OnLogin();

            Assert.Equal("No se econtraron coincidencias del usuario", view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_WrongPassword_ShowsError()
        {
            var view = new FakeLoginView { Document = "123", Password = "wrong" };
            var service = new FakePersonService
            {
                GetByDocumentResult = new Person { idPerson = 1, password = PasswordHasher.Hash("correct"), oPersonType = new TypePerson { idPersonType = 1 } }
            };

            CreatePresenter(view, service).OnLogin();

            Assert.Equal("No se econtraron coincidencias del usuario", view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_HashedPasswordMatches_LogsIn()
        {
            var person = new Person { idPerson = 1, password = PasswordHasher.Hash("correct"), Estado = true, oPersonType = new TypePerson { idPersonType = 1 } };
            var view = new FakeLoginView { Document = "123", Password = "correct" };
            var service = new FakePersonService { GetByDocumentResult = person };

            CreatePresenter(view, service).OnLogin();

            Assert.Same(person, view.LoggedInPerson);
            Assert.Null(view.ShownError);
            Assert.Null(service.UpdatedPasswordForId); // already hashed - no migration needed
        }

        [Fact]
        public void OnLogin_LegacyPlainTextPasswordMatches_LogsInAndMigratesToHash()
        {
            var person = new Person { idPerson = 7, password = "plain-text", Estado = true, oPersonType = new TypePerson { idPersonType = 1 } };
            var view = new FakeLoginView { Document = "123", Password = "plain-text" };
            var service = new FakePersonService { GetByDocumentResult = person };

            CreatePresenter(view, service).OnLogin();

            Assert.Same(person, view.LoggedInPerson);
            Assert.Equal(7, service.UpdatedPasswordForId);
            Assert.True(PasswordHasher.IsHashed(service.UpdatedPasswordHash));
        }

        [Fact]
        public void OnLogin_DeactivatedPerson_IsRejectedEvenWithCorrectPassword()
        {
            var person = new Person { idPerson = 1, password = PasswordHasher.Hash("correct"), Estado = false, oPersonType = new TypePerson { idPersonType = 1 } };
            var view = new FakeLoginView { Document = "123", Password = "correct" };
            var service = new FakePersonService { GetByDocumentResult = person };

            CreatePresenter(view, service).OnLogin();

            Assert.Equal("No se econtraron coincidencias del usuario", view.ShownError);
            Assert.Null(view.LoggedInPerson);
        }

        [Fact]
        public void OnLogin_DocumentIsTrimmedBeforeLookup()
        {
            var view = new FakeLoginView { Document = "  123  ", Password = "x" };
            var service = new FakePersonService { GetByDocumentResult = null };

            CreatePresenter(view, service).OnLogin();

            Assert.Equal("123", service.RequestedDocument);
        }
    }
}
