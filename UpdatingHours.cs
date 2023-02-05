using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.ServiceModel;


namespace BVIcouldthinkof
{
    public class UpdatingHours : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the tracing service
            ITracingService tracingService =
            (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Obtain the execution context from the service provider.  
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            // The InputParameters collection contains all the data passed in the message request.  

            // Obtain the target entity from the input parameters.  
            /*Entity entity = (Entity)context.InputParameters["Target"];*/

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];



                try
                {
                    ColumnSet LookupTask = new ColumnSet(new String[] { "new_tasks" });
                    Entity myEntityHavingLookup = service.Retrieve("new_itemses", entity.Id, LookupTask);
                    var TaskId = ((Microsoft.Xrm.Sdk.EntityReference)(myEntityHavingLookup.Attributes["new_tasks"])).Id;
                    /*var hoursintaskitem = entity["new_hours"];*/
                    // The name of the whole number field you want to sum the values for
                    string fieldName = "new_hours";

                    // The logical name of the lookup field
                    // The logical name of the entity you want to retrieve data from
                    /*string entityName = "new_itemses";*/

                    // FetchXML query to retrieve the top 10 records of the entity with the related tasks
                    string fetchXml = @"
                                        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                        <entity name='new_itemses'>
                                        <attribute name='new_itemsesid' />
                                        <attribute name='new_name' />
                                        <attribute name='"+fieldName+@"' />
                                        <attribute name='createdon' />
                                        <order attribute='new_name' descending='false' />
                                        <link-entity name='new_tasks' from='new_tasksid' to='new_tasks' link-type='inner' alias='ab'>
                                        <filter type='and'>
                                        <condition attribute='new_tasksid' operator='eq' uiname='Faisal' uitype='new_tasks' value='" + TaskId + @"' />
                                        </filter>
                                        </link-entity>
                                        </entity>
                                        </fetch>";


                    // Execute the FetchXML query
                    EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));

                    // Sum the values from the whole number field
                    double sum = results.Entities.Sum(x => (double)x.Attributes[fieldName]);

                    // Store the sum in a variable
                    double total = sum;


                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin.", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}

