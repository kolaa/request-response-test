using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RRTest.Client.Api;
using RRTest.Contracts;
using RRTest.GrpcService;
using SlimMessageBus;
using GetAccountsRequest = RRTest.Contracts.GetAccountsRequest;

namespace RPClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private IRequestResponseBus _bus;
        private readonly ClientAccounts.ClientAccountsClient _grpcClient;
        private readonly ClientAccountsRestClient _restClient;

        public AccountsController(ILogger<AccountsController> logger,
            IRequestResponseBus bus,
            ClientAccounts.ClientAccountsClient grpcClient,
            ClientAccountsRestClient restClient)
        {
            _logger = logger;
            _bus = bus;
            _grpcClient = grpcClient;
            _restClient = restClient;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("ByBus/{clientId}")]
        public async Task<ActionResult<List<Account>>> GetClientAccountsByBus(int clientId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _bus.Send(new GetAccountsRequest(clientId), cancellationToken).ConfigureAwait(false);
                return response.Accounts;
            }
            catch (RequestHandlerFaultedMessageBusException)
            {
                // The request handler for GenerateThumbnailRequest failed
                return NotFound();
            }
            catch (OperationCanceledException)
            {
                // The request was cancelled (HTTP connection cancelled, or request timed out)
                return StatusCode(StatusCodes.Status503ServiceUnavailable, "The request was cancelled");
            }            
        }

        [HttpGet]
        [Route("ByGrpc/{clientId}")]
        public async Task<ActionResult<List<Account>>> GetClientAccountsByGrpc(int clientId)
        {
            var res = await _grpcClient.GetAccountsAsync(new RRTest.GrpcService.GetAccountsRequest
            {
                ClientId = clientId
            });

            return res.Accounts.Select(a => new Account
            {
                AccountId = a.AccountId,
                Balance = a.Balance.ToDecimal(),
                Description = a.Description,
                Name = a.Name,
                Number = a.Number,
                OpenDate = a.OpenDate.ToDateTime()
            }).ToList();
        }

        [HttpGet]
        [Route("ByRest/{clientId}")]
        public async Task<IEnumerable<Account>> GetClientAccountsByRest(int clientId)
        {
            return await _restClient.GetAccountsAsync(clientId);
        }
    }
}
