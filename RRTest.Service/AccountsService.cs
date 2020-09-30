using System;
using System.Collections.Generic;
using System.Linq;
using RRTest.Contracts;

namespace RRTest.Service
{
    public class AccountsService : IAccountsService
    {
        const int _collectionLength = 1000000;

        List<Account> Accounts = new List<Account>();

        public AccountsService()
        {
            for (var i = 0; i < _collectionLength; i++)
            {
                Accounts.Add(new Account
                {
                    AccountId = i,
                    Description = $"Счет клиента для всего что только нужно {i}",
                    Name = $"Расчетный счет {i}",
                    Number = "408178105" + i.ToString().PadLeft(11, '0'),
                    Balance = 100000 + i,
                    OpenDate = DateTime.Now
                });
            }
        }


        public IEnumerable<Account> GetClientAccounts(int clientID)
        {
            if (clientID > _collectionLength)
                throw new OverflowException($"Массив не более {_collectionLength} элементов");

            return Accounts.Take(clientID);
        }
    }
}
