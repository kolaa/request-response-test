using RRTest.Contracts;
using RRTest.Service;
using SlimMessageBus;
using System.Linq;
using System.Threading.Tasks;

namespace RRTEst.Kafka.Handler.Handlers
{
    public class GetAccountsHandler : IRequestHandler<GetAccountsRequest, GetAccountsResponse>
    {
        private readonly IAccountsService _accountsService;

        public GetAccountsHandler(IAccountsService accountsService)
        {
            _accountsService = accountsService;
        }

        public Task<GetAccountsResponse> OnHandle(GetAccountsRequest request, string name)
        {
            return Task.Run(() => new GetAccountsResponse(_accountsService.GetClientAccounts(request.ClientId)));
        }
    }
}
