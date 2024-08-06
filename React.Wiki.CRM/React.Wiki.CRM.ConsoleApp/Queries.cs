using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System;
using System.Configuration;

namespace React.Wiki.CRM.ConsoleApp
{
    internal class Queries
    {
        private static CrmServiceClient _client;

        /// <summary>
        /// Retrieve Multiple con FetchXML
        /// </summary>
        public static void FetchXMLQuery()
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                string fetchXml = @"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                  <entity name='rct_entityname'>
                    <attribute name='rct_stringfield' />
                    <attribute name='rct_lookupfield' />
                    <attribute name='rct_optionsetfield' />
                    <filter type='and'>
                      <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                  </entity>
                </fetch>";

                FetchExpression fxml = new FetchExpression(fetchXml);
                EntityCollection retrieved = _service.RetrieveMultiple(fxml);

                Console.WriteLine("#### RETRIEVED ####");
                Console.WriteLine();

                foreach (var r in retrieved.Entities)
                {
                    string stringField = r.Attributes["rct_stringfield"].ToString();
                    string lookupFieldName = ((EntityReference)r.Attributes["rct_lookupfield"]).Name;
                    string optionSetLabel = r.FormattedValues["rct_optionsetfield"];

                    Console.WriteLine(stringField);
                    Console.WriteLine(lookupFieldName);
                    Console.WriteLine(optionSetLabel);
                }
            }
        }

        /// <summary>
        /// Retrieve Multiple con QueryByAttribute
        /// </summary>
        public static void QueryByAttribute()
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                QueryByAttribute queryByAttr = new QueryByAttribute("rct_entityname");

                //ColumnSet
                queryByAttr.ColumnSet = new ColumnSet("rct_stringfield", "rct_lookupfield", "rct_optionsetfield");
                //Attribute to query      
                queryByAttr.Attributes.AddRange("rct_stringfield");
                //Value of queried attribute to return      
                queryByAttr.Values.AddRange("value");

                //Query passed to the service proxy      
                EntityCollection retrieved = _service.RetrieveMultiple(queryByAttr);

                Console.WriteLine("#### RETRIEVED ####");
                Console.WriteLine();

                foreach (var r in retrieved.Entities)
                {
                    string stringField = r.Attributes["rct_stringfield"].ToString();
                    string lookupFieldName = ((EntityReference)r.Attributes["rct_lookupfield"]).Name;
                    string optionSetLabel = r.FormattedValues["rct_optionsetfield"];

                    Console.WriteLine(stringField);
                    Console.WriteLine(lookupFieldName);
                    Console.WriteLine(optionSetLabel);
                }
            }
        }

        /// <summary>
        /// Retrieve Multiple con QueryExpression
        /// </summary>
        public static void QueryExpression()
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                QueryExpression query = new QueryExpression()
                {
                    Distinct = false,
                    EntityName = "rct_entityname",
                    ColumnSet = new ColumnSet("rct_stringfield", "rct_lookupfield", "rct_optionsetfield"),
                    Criteria =
                    {
                        Filters =
                        {
                            new FilterExpression
                            {
                                Conditions =
                                {
                                    new ConditionExpression("rct_stringfield", ConditionOperator.Equal, "value")
                                }
                            }
                        }
                    }
                };

                EntityCollection retrieved = _service.RetrieveMultiple(query);

                Console.WriteLine("#### RETRIEVED ####");
                Console.WriteLine();

                foreach (var r in retrieved.Entities)
                {
                    string stringField = r.Attributes["rct_stringfield"].ToString();
                    string lookupFieldName = ((EntityReference)r.Attributes["rct_lookupfield"]).Name;
                    string optionSetLabel = r.FormattedValues["rct_optionsetfield"];

                    Console.WriteLine(stringField);
                    Console.WriteLine(lookupFieldName);
                    Console.WriteLine(optionSetLabel);
                }
            }
        }

        /// <summary>
        /// Retrieve Multiple con QueryExpression, sintassi alternativa
        /// </summary>
        public static void QueryExpressionAlt()
        {
            using (_client = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRMConnectionString"].ConnectionString))
            {
                var _service = _client.OrganizationWebProxyClient;

                ConditionExpression condition = new ConditionExpression();
                condition.AttributeName = "rct_stringfield";
                condition.Operator = ConditionOperator.Equal;
                condition.Values.Add("value");

                FilterExpression filter = new FilterExpression();
                filter.Conditions.Add(condition);

                QueryExpression query = new QueryExpression("rct_entityname");
                query.ColumnSet.AddColumns("rct_stringfield", "rct_lookupfield", "rct_optionsetfield");
                query.Criteria.AddFilter(filter);

                EntityCollection retrieved = _service.RetrieveMultiple(query);

                Console.WriteLine("#### RETRIEVED ####");
                Console.WriteLine();

                foreach (var r in retrieved.Entities)
                {
                    string stringField = r.Attributes["rct_stringfield"].ToString();
                    string lookupFieldName = ((EntityReference)r.Attributes["rct_lookupfield"]).Name;
                    string optionSetLabel = r.FormattedValues["rct_optionsetfield"];

                    Console.WriteLine(stringField);
                    Console.WriteLine(lookupFieldName);
                    Console.WriteLine(optionSetLabel);
                }
            }
        }
    }
}
