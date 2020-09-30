using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using RRTest.Service;

namespace RRTest.GrpcService
{
    public class ClientAccountsService : ClientAccounts.ClientAccountsBase
    {
        private readonly ILogger<ClientAccountsService> _logger;
        private readonly IAccountsService _accountsService;

        public ClientAccountsService(ILogger<ClientAccountsService> logger, IAccountsService accountsService)
        {
            _logger = logger;
            _accountsService = accountsService;
        }

        public override Task<GetAccountsResponse> GetAccounts(GetAccountsRequest request, ServerCallContext context)
        {
            var res = new GetAccountsResponse();

            res.Accounts.AddRange(
                _accountsService.GetClientAccounts(request.ClientId).Select(a =>
                    new GetAccountsResponse.Types.Account
                    {
                        AccountId = a.AccountId,
                        Balance = DecimalValue.FromDecimal(a.Balance),
                        Number = a.Number,
                        Name = a.Name,
                        Description = a.Description,
                        OpenDate = Timestamp.FromDateTime(a.OpenDate.ToUniversalTime())
                    }
                ));


            return Task.FromResult(res);
        }
    }
}
