using System;
using PharmacySystem.Data;
using PharmacySystem.Helpers;
using PharmacySystem.Model;

namespace PharmacySystem.Business
{
    public class PasswordChangeService : IPasswordChangeService
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILoginAttemptRepository _attempts;

        public PasswordChangeService(IPersonRepository personRepository, ILoginAttemptRepository attempts)
        {
            _personRepository = personRepository;
            _attempts = attempts;
        }

        public PasswordChangeResult ChangeOwnPassword(int personId, string currentPlain, string newPlain)
        {
            Person person = _personRepository.GetById(personId);

            if (person == null || !CurrentMatches(person, currentPlain))
            {
                return PasswordChangeResult.WrongCurrent;
            }

            if ((newPlain ?? string.Empty).Length < PasswordRules.MinLength)
            {
                return PasswordChangeResult.TooShort;
            }

            if (newPlain == currentPlain)
            {
                return PasswordChangeResult.SameAsOld;
            }

            _personRepository.SetPasswordAndFlag(personId, PasswordHasher.Hash(newPlain), false);
            return PasswordChangeResult.Ok;
        }

        public string AdminReset(int targetId, int actorId)
        {
            string tempPassword = PasswordGenerator.Generate();

            _personRepository.SetPasswordAndFlag(targetId, PasswordHasher.Hash(tempPassword), true);

            Person target = _personRepository.GetById(targetId);
            _attempts.Record(target?.document ?? string.Empty, true, "admin_reset", actorId, Environment.MachineName);

            return tempPassword;
        }

        private static bool CurrentMatches(Person person, string enteredCurrent)
        {
            return PasswordHasher.IsHashed(person.password)
                ? PasswordHasher.Verify(enteredCurrent, person.password)
                : person.password == enteredCurrent;
        }
    }
}
