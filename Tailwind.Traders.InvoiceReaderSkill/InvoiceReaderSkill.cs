using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Azure.CognitiveServices.FormRecognizer;

namespace Tailwind.Traders.InvoiceReaderSkill
{
    public static class InvoiceReaderSkill
    {
        // Forms Recognizer Credentials
        static readonly ApiKeyServiceClientCredentials credentials = 
            new ApiKeyServiceClientCredentials(GetEnv("FormsRecognizerKey"));
        
        // Forms Recognizer Client
        static readonly IFormRecognizerClient formsClient = new FormRecognizerClient(credentials) {
            Endpoint = GetEnv("FormsRecognizerEndpoint")
        };

        // Forms Recognize Model
        static readonly Guid ModelId = Guid.Parse(GetEnv("ModelId"));

        [FunctionName("AnalyzeInvoice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Analyze Invoice SKill: C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            using (WebClient client = new WebClient())
            {
                foreach (var record in data.values)
                {
                    string recordId = record.recordId;
                    string url = record.data.formUrl;
                    string token = record.data.formSasToken;

                    var stream = await client.OpenReadTaskAsync($"{url}{token}");
                    var result = await formsClient.AnalyzeWithCustomModelAsync(ModelId, 
                                    stream, 
                                    contentType: "application/pdf");
                }
            }

            return new OkObjectResult(new { data = "good job!" });
        }

        public static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
