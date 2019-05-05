using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("AssurBox SDK demo");

            /*
            Nom
DemoconsoleappClient
Client ID
KJzPheaIF4czmfKbuALs3g==
Client Secret
lfr8i9vUyWJBNfBDm8NlYbvzMv6cbFC6SsnX8Cfu6hvx0LS502v/3X4gtoeZhZD9P3tvnysyPv1dLL7E3GBHdg==
            */

            // 1 -> Authenticate 
            AssurBox.SDK.Clients.SecurityClient sClient = new AssurBox.SDK.Clients.SecurityClient(new AssurBox.SDK.AssurBoxClientOptions(AssurBox.SDK.AssurBoxEnvironments.Test));

            var tokenInfo = sClient.GetBearerToken("KJzPheaIF4czmfKbuALs3g==", "lfr8i9vUyWJBNfBDm8NlYbvzMv6cbFC6SsnX8Cfu6hvx0LS502v/3X4gtoeZhZD9P3tvnysyPv1dLL7E3GBHdg==").Result;

            Console.WriteLine(tokenInfo.access_token);

            // 2 -> list companies providing the contract termination service
            AssurBox.SDK.Clients.ContractTerminationClient ctClient = new AssurBox.SDK.Clients.ContractTerminationClient(new AssurBox.SDK.AssurBoxClientOptions
            {
                Environment = AssurBox.SDK.AssurBoxEnvironments.Test,
                ApiKey = tokenInfo.access_token
            });

            var companies = ctClient.GetInsurancesForContractTerminationAsync().Result;

            
            foreach(var company in companies)
            {
                Console.WriteLine(company.Name);
            }
            var companyInfo = companies.First();

            // 3 -> request a contract termination

            var requestInfo=  ctClient.RequestContractTerminationAsync(new AssurBox.SDK.DTO.Contract.ContractTerminationRequest
            {
                Risk = new AssurBox.SDK.DTO.Risk
                {
                    RiskType = AssurBox.SDK.DTO.RiskTypes.Vehicle,
                    IdentifierType = AssurBox.SDK.DTO.RiskIdentifierTypes.LicencePlate,
                    Identifier = "BE4987"
                },
                RecipientInsurer = companyInfo,
                TerminationDate = DateTime.Today.AddDays(5),
                TerminationReason = AssurBox.SDK.DTO.Contract.ContractTerminationReasons.BeforeDueDate,
                Communication="This is a demo"
            }).Result;

            Console.WriteLine(requestInfo.ResponseContent); // this will be the "correlation id" to save

            Console.WriteLine("Wait a little (+- 30 seconds for this simulation)");
            Console.ReadLine();

            // 4 -> Check for a response


            var response = ctClient.GetContractTerminationRequestAsync(new Guid(requestInfo.ResponseContent)).Result;

            if (response.HasResponse)
            {
                Console.WriteLine("Request accepted ?  : "+response.Response.IsTerminationRequestAccepted);
            }
            else
            {
                Console.WriteLine("No response yet");
            }



            Console.WriteLine("Enter to quit");
            Console.ReadLine();
        }
    }
}
