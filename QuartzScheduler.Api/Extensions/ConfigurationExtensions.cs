using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace QuartzScheduler.Common.Exstensions
{
    public static class ConfigurationExtensions
    {
        public static NameValueCollection ToNameValueCollection(this IConfigurationSection configurationSection)
        {
            return configurationSection.GetChildren()
                .ToDictionary(x => x.Key, x => x.Value)
                .Aggregate(new NameValueCollection(),
                (seed, current) =>
                {
                    seed.Add(current.Key, current.Value);
                    return seed;
                });
        }
    }
}
