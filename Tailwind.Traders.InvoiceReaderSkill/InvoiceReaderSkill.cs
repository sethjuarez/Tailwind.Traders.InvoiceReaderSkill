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


namespace Tailwind.Traders.InvoiceReaderSkill
{
    public static class InvoiceReaderSkill
    {
        // Forms Recognizer Credentials
        static readonly string formsRecognizerKey = GetEnv("FormsRecognizerKey");
        static readonly string formsRecognizerEndpoint = GetEnv("FormsRecognizerEndpoint");
        static readonly string modelId = GetEnv("FormsRecognizerEndpoint");

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

                    var pdfBits = await client.DownloadDataTaskAsync($"{url}{token}");
                    
                    
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
