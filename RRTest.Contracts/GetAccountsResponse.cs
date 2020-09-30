using System.Collections.Generic;
using System.Linq;

namespace RRTest.Contracts
{
    public class GetAccountsResponse
    {
        public List<Account> Accounts { get; set; }

        public GetAccountsResponse(IEnumerable<Account> accounts)
        {
            Accounts = accounts.ToList();
        }

    }
}
