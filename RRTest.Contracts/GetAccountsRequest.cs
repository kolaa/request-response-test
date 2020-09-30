using SlimMessageBus;

namespace RRTest.Contracts
{
    public class GetAccountsRequest : IRequestMessage<GetAccountsResponse>
    {
        public int ClientId { get; set; }

        public GetAccountsRequest() { }

        public GetAccountsRequest( int clientId)
        {
            ClientId = clientId;            
        }
    }
}
