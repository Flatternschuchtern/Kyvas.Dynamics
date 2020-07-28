using Microsoft.Xrm.Sdk;

namespace Kyvas.Dynamics.Plugin
{
    public abstract class PluginImage
    {
        public Entity Value { get; protected set; }
        public bool HasValue { get; protected set; }
    }

    public class PluginPreImage : PluginImage
    {
        public PluginPreImage(IPluginExecutionContext context, string name)
        {
            if (context.PreEntityImages.TryGetValue(name, out var entity))
            {
                Value = entity;
                HasValue = true;
            }
            else
                HasValue = false;
        }
    }

    public class PluginPostImage : PluginImage
    {
        public PluginPostImage(IPluginExecutionContext context, string name)
        {
            if (context.PostEntityImages.TryGetValue(name, out var entity))
            {
                Value = entity;
                HasValue = true;
            }
            else
                HasValue = false;
        }
    }
}
