using Microsoft.Crm.Sdk.Messages;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace React.Wiki.CRM.ConsoleApp
{
    internal class Utilities
    {
        private static CrmServiceClient _client;

        /// <summary>
        /// Restituisce il numero di record su CRM per ogni entità.
        /// </summary>
        public static void RetrieveTotalRecordCount(string[] entityNames)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                var r = new RetrieveTotalRecordCountRequest();
                r.EntityNames = entityNames;

                var count = ((RetrieveTotalRecordCountResponse)_service.Execute(r)).EntityRecordCountCollection;

                foreach (var e in r.EntityNames)
                {
                    Console.WriteLine(e + ": " + count[e]);
                }
            }
        }

        /// <summary>
        /// Scarica in locale una copia di un Document Template di Dynamics.
        /// </summary>
        /// <param name="templateId"></param>
        public static void DocumentTemplateDownloader(Guid templateId, string path)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                Entity objDocTemplate = _service.Retrieve("documenttemplate", templateId, new ColumnSet(true));

                if (objDocTemplate != null && objDocTemplate.Attributes.Contains("content"))
                {
                    string docName = objDocTemplate["name"].ToString();
                    Console.WriteLine(docName);

                    var ms = new MemoryStream(Convert.FromBase64String(objDocTemplate["content"].ToString()));
                    var fs = new FileStream($"{path}/{docName}.docx", FileMode.CreateNew);
                    ms.CopyTo(fs);
                    fs.Flush();
                    fs.Close();
                }
                Console.WriteLine("done.");
            }
        }

        /// <summary>
        /// Restituisce i metadati per un'entità del CRM.
        /// </summary>
        /// <param name="entityType"></param>
        /// <param name="primaryKey"></param>
        public static void GetEntityMetadata(string entityType, string primaryKey)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.All,
                    LogicalName = entityType
                };
                RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)_service.Execute(retrieveEntityRequest);
                EntityMetadata _entity = retrieveEntityResponse.EntityMetadata;

                Console.WriteLine("Entity metadata:");
                Console.WriteLine(_entity.SchemaName);
                Console.WriteLine(_entity.DisplayName.UserLocalizedLabel.Label);
                Console.WriteLine(_entity.EntityColor);

                Console.WriteLine("Entity attributes:");
                foreach (object attribute in _entity.Attributes)
                {
                    AttributeMetadata a = (AttributeMetadata)attribute;
                    Console.WriteLine(a.LogicalName);
                }

                StringAttributeMetadata name = (StringAttributeMetadata)_entity.Attributes.Where(a => a.LogicalName == primaryKey).FirstOrDefault();
                Console.WriteLine(name.DisplayName.UserLocalizedLabel.Label);
                Console.WriteLine(name.MaxLength);
            }
        }

        /// <summary>
        /// Modifica la lunghezza massima di un campo di tipo Testo a partire da un elenco di Entità in formato .csv.
        /// </summary>
        /// <param name="newLength"></param>
        public static void ModifyMaxLength(int newLength, string path)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                using (var reader = new StreamReader(path))
                {
                    for (int i = 0; !reader.EndOfStream; i++)
                    {
                        string riga = reader.ReadLine();
                        var dati = riga.Split(';');
                        if (dati[0] != "Entity Display Name")
                        {
                            try
                            {
                                RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest
                                {
                                    EntityFilters = EntityFilters.All,
                                    LogicalName = dati[1]
                                };
                                RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)_service.Execute(retrieveEntityRequest);
                                EntityMetadata metadata = retrieveEntityResponse.EntityMetadata;

                                StringAttributeMetadata name = (StringAttributeMetadata)metadata.Attributes.Where(a => a.LogicalName == dati[2]).FirstOrDefault();

                                Console.WriteLine(metadata.LogicalName);
                                Console.WriteLine(name.MaxLength);
                                name.MaxLength = newLength;
                                Console.WriteLine(name.MaxLength);

                                UpdateAttributeRequest updReq = new UpdateAttributeRequest
                                {
                                    Attribute = name,
                                    EntityName = metadata.LogicalName,
                                    MergeLabels = true
                                };

                                _service.Execute(updReq);

                            }
                            catch (Exception)
                            {
                                Console.WriteLine($"Errore riga {i} {dati[0]}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Esegue un update su CRM leggendo i dati a partire da un file .csv.
        /// </summary>
        public static void CsvReader(string path)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                using (var reader = new StreamReader(path))
                {
                    for (int i = 0; !reader.EndOfStream; i++)
                    {
                        string riga = reader.ReadLine();
                        var dati = riga.Split(';');
                        if (dati[0] != "ID")
                        {
                            try
                            {
                                Entity myEntity = new Entity("rct_entity");
                                myEntity.Id = new Guid(dati[0]);
                                myEntity["rct_field"] = dati[1];
                                _service.Update(myEntity);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine($"{i} {dati[0]} {dati[1]}");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recupera un Bearer token di accesso ai servizi di Dynamics utilizzando l'autenticazione Service Principal
        /// </summary>
        /// <param name="tenant">contoso.onmicrosoft.com</param>
        /// <param name="resourceId">https://contoso.crm4.dynamics.com/</param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <returns>Access Token</returns>
        public static string GetAccessToken(string tenant, string resourceId, string clientId, string clientSecret)
        {
            string authString = "https://login.windows.net/" + tenant;

            var authenticationContext = new Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext(authString, false);
            ClientCredential clientCred = new ClientCredential(clientId, clientSecret);
            AuthenticationResult authenticationResult = null;
            Task runTask = Task.Run(async () => authenticationResult = await authenticationContext.AcquireTokenAsync(resourceId, clientCred));
            runTask.Wait();
            return authenticationResult.AccessToken;
        }

        /// <summary>
        /// Esegue una chiamata HTTP di tipo POST utilizzando il metodo di serializzazione di un JSON con "System.Text.Json".
        /// </summary>
        /// <param name="url"></param>
        public static void HTTPPostRequest_V1(string url)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //usando Text.JSON, creando un nuovo oggetto vuoto
                    string json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        Id = "testID",
                        Field1 = "Description",
                        Field2 = "Description_2"
                    });

                    //oppure
                    var jsonObj = new
                    {
                        Id = "testID",
                        Field1 = "Description",
                        Field2 = "Description_2"
                    };
                    string json_alt = System.Text.Json.JsonSerializer.Serialize(jsonObj);

                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    Console.WriteLine(result);
                }
            }
        }

        /// <summary>
        /// Esegue una chiamata HTTP di tipo POST utilizzando il metodo di serializzazione di un JSON con "JavaScriptSerializer".
        /// </summary>
        /// <param name="url"></param>
        public static void HTTPPostRequest_V2(string url)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //usando JS serializer
                    string json = new JavaScriptSerializer().Serialize(new
                    {
                        Id = "Foo",
                        Descrizione = "Baz",
                        LookupId = "..."
                    });

                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    Console.WriteLine(result);
                }
            }
        }

        /// <summary>
        /// Esegue una chiamata HTTP di tipo POST utilizzando il metodo di serializzazione di un JSON con "System.Text.Json" usando una classe custom.
        /// </summary>
        /// <param name="url"></param>
        public static void HTTPPostRequest_V3(string url)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //usando Text.JSON, creando ua nuova istanza di una classe si è legati ad un oggetto fortemente tipizzato
                    string json = System.Text.Json.JsonSerializer.Serialize(new OggettoCRM()
                    {
                        Id = Guid.NewGuid(),
                        Descrizione = "DescriptionTest",
                        Lookup = new EntityReference("systemuser", new Guid("..."))
                    });

                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    Console.WriteLine(result);
                }
            }
        }

        public class OggettoCRM
        {
            public Guid Id { get; set; }
            public string Descrizione { get; set; }
            public EntityReference Lookup { get; set; }
        }

        /// <summary>
        /// Esegue una chiamata HTTP di tipo POST utilizzando il metodo di serializzazione di un JSON con "JObject".
        /// </summary>
        /// <param name="url"></param>
        public static void HTTPPostRequest_V4(string url)
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    //usando JObject
                    JObject jobj = new JObject();
                    jobj.Add("Id", "testID");
                    jobj.Add("Field1", "Description");
                    jobj.Add("Field2", "Description_2");

                    streamWriter.Write(jobj);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    Console.WriteLine(result);
                }
            }
        }

        /// <summary>
        /// Esegue una chiamata HTTP di tipo PUT utilizzando il pacchetto "RestSharp".
        /// </summary>
        /// <param name="url"></param>
        public async static void RestSharpHTTPPutRequest(string url)
        {
            var options = new RestClientOptions(url)
            {
                Timeout = new TimeSpan(0, 2, 0)
            };
            var client = new RestClient(options);
            var request = new RestRequest("/api/apimethod", Method.Put);
            request.AddHeader("Content-Type", "application/json");
            var body = @"{
            " + "\n" +
                        @"    ""objID"": ""123456""
            " + "\n" +
            @"}";
            request.AddStringBody(body, DataFormat.Json);
            RestResponse response = await client.ExecuteAsync(request);
            Console.WriteLine(response.Content);
        }
    }
}
