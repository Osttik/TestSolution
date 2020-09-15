using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace DynamicTask
{
    public class ValidatingPlugin : IPlugin
    {
        private string _logicalAccountName;
        private string _phoneNumberField;
        private string _isValidatedField;

        public ValidatingPlugin()
        {
            _logicalAccountName = "account";
            _phoneNumberField = "new_phonenumber";
            _isValidatedField = "new_isvalidated";
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.LogicalName != _logicalAccountName)
                    return;

                IOrganizationServiceFactory serviceFactory =
                    (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                try
                {
                    object phoneNumber = string.Empty;

                    bool isPhoneNumber = entity.Attributes.TryGetValue(_phoneNumberField, out phoneNumber);

                    if (isPhoneNumber && (phoneNumber as string).Length != 0)
                        isPhoneNumber = true;

                    entity.Attributes.Add(_isValidatedField, isPhoneNumber);
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occurred in Test Plugin: ", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Test Plugin: {0}", ex.ToString());
                    throw;
                }
            }
        }
    }
}
