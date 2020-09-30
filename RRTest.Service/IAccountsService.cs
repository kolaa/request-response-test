using RRTest.Contracts;
using System.Collections.Generic;

namespace RRTest.Service
{
    public interface IAccountsService
    {
        IEnumerable<Account> GetClientAccounts(int clientID);
    }
}