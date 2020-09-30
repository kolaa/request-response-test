using System;

namespace RRTest.Contracts
{
    public class Account
    {
        public int AccountId { get; set; }
        
        public DateTime OpenDate { get; set; }

        public string Number { get; set; }
        
        public string Name { get; set; }

        public decimal Balance { get; set; }

        public string Description { get; set; }
    }
}
