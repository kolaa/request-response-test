using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RRTest.Contracts;

namespace RRTest.Service.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly IAccountsService _accountsService;

        public AccountsController(ILogger<AccountsController> logger, IAccountsService accountsService)
        {
            _logger = logger;
            _accountsService = accountsService;
        }

        [HttpGet("{clientId}")]
        public ActionResult<List<Account>> GetClientAccounts(int clientId)
        {
            return _accountsService.GetClientAccounts(clientId).ToList();
        }
    }
}
