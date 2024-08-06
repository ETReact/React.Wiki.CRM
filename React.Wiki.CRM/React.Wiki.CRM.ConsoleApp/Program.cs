using System;
using Microsoft.Xrm.Tooling.Connector;

namespace React.Wiki.CRM.ConsoleApp
{
    class Program
    {
        private static CrmServiceClient _client;

        public static void Main(string[] args)
        {
            // Start Here
            Utilities.RetrieveTotalRecordCount(new string[] { "quote", "quotedetail" });

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}