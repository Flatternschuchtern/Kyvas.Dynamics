# Kyvas.Dynamics
Quality of life update for Dynamics 365 plugin creation based on reflection.
# Features
- Automatically retrieve frequently required services;
- Automatically retrieve, map & set input and output parameters;
- Retrieve & map entity images;
- Throw InvalidPluginExecution on error;
- Code activity NOT supported.
# Installation
Download and reference this project manually OR download it using NuGet:

```PM> Install-Package Kyvas.Dynamics -Version 1.0.0```
# Usage
## Implementation
Just inherit abstract class PluginBase instead of IPlugin, and implement abstract ExecuteBody() method:
```C#
public class PluginExample : PluginBase
    {
        protected override void ExecuteBody()
        {
            // your code
        }
    }
```
## Auto Retrieved Services
Your derived class will have some frequently used properties pre-retrieved:
``` C#
protected IServiceProvider ServiceProvider; // service provider passed to the plugin.
protected IPluginExecutionContext Context; // current plugin execution context.
protected ITracingService Tracer; // tracing service.
protected IOrganizationService Service; // organization service for current user.
protected IOrganizationService AdminService; // organization service for instance administrator.
```
## Get & Set Parameters
Derived class will automatically search and map input and output parameters from execution context. To retrieve & set parameters, it will either use field names, or names specified in attributes.

For example, this implementation:
``` C#
public class PluginExample : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // manually retrieve services
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = serviceProvider.Get<IOrganizationServiceFactory>();
            var service = factory.CreateOrganizationService(context.UserId);
            
            // check if input exists and if it of desired type
            if (!context.InputParameters.TryGetValue("Target", out var targetObj) || !(targetObj is Entity target))
                throw new InvalidPluginExecutionException(nameof(target) + " is missing.");
            
            // check if output exists
            if (!context.OutputParameters.ContainsKey("Response"))
                throw new InvalidPluginExecutionException("Response is missing.");
            
            // set output manually
            context.OutputParameters["Response"] = target.Id.ToString("D");
         }
     }
```
Can be easily transformed into:
``` C#
public class PostCreate : PluginBase
    {
        public PluginInput<Entity> Target; // PluginInput generic field to get & map input parameters
        public PluginOutput<string> Response; // PluginOutput generic field to map & set output parameters
        
        protected override void ExecuteBody()
        {
            // mapped automatically
            if (!Target.HasValue)
              throw new ArgumentNullException(nameof(Target)); // will also be rethrown into InvalidPluginExecutionException automatically
             
            // output will be set automatically in the end too!
            Response.Value = Target.Value.Id.ToString("D");
        }
    }
```
## Get & Map Entity Images
Same goes for entity images. Transform this:
``` C#
public class PostDelete: IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            
            if (!context.PreEntityImages.ContainsKey("PreImage") || !(context.PreEntityImages["PreImage"] is Entity preImage))
                throw new InvalidPluginExecutionException(nameof(preImage) + " is missing.");
                    
            if (!context.PostEntityImages.ContainsKey("PostImage") || !(context.PostEntityImages["PostImage"] is Entity postImage))
                throw new InvalidPluginExecutionException(nameof(postImage) + " is missing.");
        }
    }
```
Into
``` C#
public class PostCreate : PluginBase
    {
        public PluginPreImage PreImage;
        public PluginPostImage PostImage;
        
        protected override void ExecuteBody()
        {
            if (!PreImage.HasValue)
              throw new ArgumentNullException(nameof(PreImage)); // will also be rethrown into InvalidPluginExecutionException automatically
            if (!PostImage.HasValue)
              throw new ArgumentNullException(nameof(PostImage)); // will also be rethrown into InvalidPluginExecutionException automatically
        }
    }
```

### Important!
Make your parameters and images public, otherwise they will not be available for reflection.
## Name Attribute
If you have ugly or repeating image or parameter name, just attribute their declaration:
``` C#
public class PostCreate : PluginBase
    {
        // images with same name
        [StringValue("Image")] public PluginPreImage PreImage;
        [StringValue("Image")] public PluginPostImage PostImage;
        
        // non-conventionally named input
        [StringValue("bad_parameter_name")]
        public PluginInput<string> GoodParameterName;
        
        protected override void ExecuteBody()
        {
            ...
        }
    }
```
