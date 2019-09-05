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
        static readonly string modelId = GetEnv("ModelId");

        [FunctionName("AnalyzeInvoice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Analyze Invoice SKill: C# HTTP trigger function processed a request.");

            // forms endpoint
            var uri = $"https://{formsRecognizerEndpoint}/formrecognizer/v1.0-preview/custom/models/{modelId}/analyze";

            Func<byte[], HttpRequestMessage> message = bits => {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new ByteArrayContent(bits);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                request.Headers.Add("Ocp-Apim-Subscription-Key", formsRecognizerKey);
                return request;
            };

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
            List<object> records = new List<object>();
            
            using (WebClient client = new WebClient())
            {
                foreach (var record in data.values)
                {
                    string recordId = record.recordId;
                    string url = record.data.formUrl;
                    string token = record.data.formSasToken;
                    try
                    {
                        var pdfBits = await client.DownloadDataTaskAsync($"{url}{token}");
                        using (var formsClient = new HttpClient())
                        using (var formsRequest = message(pdfBits))
                        {
                            var response = await formsClient.SendAsync(formsRequest);
                            var formsResponse = await response.Content.ReadAsStringAsync();
                            var invoice = Parser.Parse(formsResponse);
                            records.Add(new
                            {
                                recordId = recordId,
                                data = new
                                {
                                    formUrl = url,
                                    invoice = Parser.Parse(formsResponse),
                                    error = new { }
                                }
                            });
                        }
                    }
                    catch (Exception error)
                    {
                        records.Add(new
                        {
                            recordId = recordId,
                            data = new
                            {
                                formUrl = url,
                                invoice = new { },
                                error =  new { message = error.Message }
                            }
                        });
                    }
                }
            }

            return new OkObjectResult(new { values = records.ToArray() });
        }

        public static string GetEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
