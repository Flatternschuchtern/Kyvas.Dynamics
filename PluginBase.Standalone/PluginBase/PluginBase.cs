using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using PluginBase.Standalone.Logic;

namespace PluginBase.Standalone.PluginBase
{
    public abstract class PluginBase : IPlugin
    {
        protected IServiceProvider ServiceProvider;
        protected IPluginExecutionContext Context;
        protected ITracingService Tracer;
        protected IOrganizationService Service;
        protected IOrganizationService AdminService;

        private FieldInfo[] _outputs;

        protected abstract void ExecuteBody();
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                ServiceProvider = serviceProvider;
                Context = ServiceProvider.Get<IPluginExecutionContext>();
                Tracer = ServiceProvider.Get<ITracingService>();

                var factory = ServiceProvider.Get<IOrganizationServiceFactory>();
                Service = factory.CreateOrganizationService(Context.UserId);
                AdminService = factory.CreateOrganizationService(null);

                _outputs = GetOutputs().ToArray(); // we use these on initialization and setting in the end, keep value
                InitializeParameters();

                ExecuteBody();

                SetOutputs();
            }
            catch (Exception e)
            {
                throw new InvalidPluginExecutionException(e.Message);
            }
        }

        private void InitializeParameters()
        {
            foreach (var input in GetInputs())
            {
                var pluginInput = new PluginInput(Context, StringValueAttributeReader.GetFirstValueOrName(input));
                input.SetValue(this, pluginInput.ToGeneric(input.FieldType.GetGenericArguments().Single()));
            }

            foreach (var output in _outputs)
            {
                var pluginOutput = new PluginOutput();
                output.SetValue(this, pluginOutput.ToGeneric(output.FieldType.GetGenericArguments().Single()));
            }

            foreach (var preImage in GetPreImages())
            {
                preImage.SetValue(this, new PluginPreImage(Context, StringValueAttributeReader.GetFirstValueOrName(preImage)));
            }

            foreach (var postImage in GetPostImages())
            {
                postImage.SetValue(this, new PluginPostImage(Context, StringValueAttributeReader.GetFirstValueOrName(postImage)));
            }
        }

        private void SetOutputs()
        {
            foreach (var output in _outputs)
            {
                var property = PluginOutput.ToNonGeneric(output.GetValue(this));
                
                if (property.HasValue)
                    Context.OutputParameters[StringValueAttributeReader.GetFirstValueOrName(output)] = property.Value;
            }
        }

        private IEnumerable<FieldInfo> GetInputs() =>
            GetType()
                .GetFields()
                .Where(p =>
                    p.FieldType.IsGenericType
                    && p.FieldType.GetGenericTypeDefinition() == typeof(PluginInput<>));

        private IEnumerable<FieldInfo> GetOutputs() =>
            GetType()
                .GetFields()
                .Where(p =>
                    p.FieldType.IsGenericType
                    && p.FieldType.GetGenericTypeDefinition() == typeof(PluginOutput<>));

        private IEnumerable<FieldInfo> GetPreImages() =>
            GetType()
                .GetFields()
                .Where(f => f.FieldType == typeof(PluginPreImage));

        private IEnumerable<FieldInfo> GetPostImages() =>
            GetType()
                .GetFields()
                .Where(f => f.FieldType == typeof(PluginPostImage));
    }
}
