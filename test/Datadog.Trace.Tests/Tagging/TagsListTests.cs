using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Datadog.Trace.ClrProfiler.Integrations.AdoNet;
using Datadog.Trace.Tagging;
using Xunit;

namespace Datadog.Trace.Tests.Tagging
{
    public class TagsListTests
    {
        [Fact]
        public void CheckProperties()
        {
            var assemblies = new[] { typeof(TagsList).Assembly, typeof(SqlTags).Assembly };

            foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
            {
                if (!typeof(TagsList).IsAssignableFrom(type))
                {
                    continue;
                }

                if (type.IsInterface || type.IsAbstract)
                {
                    continue;
                }

                var random = new Random();

                ValidateProperties<string>(type, "GetAdditionalTags", () => Guid.NewGuid().ToString());
                ValidateProperties<double?>(type, "GetAdditionalMetrics", () => random.NextDouble());
            }
        }

        private void ValidateProperties<T>(Type type, string methodName, Func<T> valueGenerator)
        {
            var instance = (ITags)Activator.CreateInstance(type);

            var allTags = (IProperty<T>[])type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(instance, null);

            var tags = allTags.Where(t => !t.IsReadOnly).ToArray();
            var readonlyTags = allTags.Where(t => t.IsReadOnly).ToArray();

            var allProperties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.PropertyType == typeof(T))
                .ToArray();

            var properties = allProperties.Where(p => p.CanWrite).ToArray();
            var readonlyProperties = allProperties.Where(p => !p.CanWrite).ToArray();

            Assert.True(properties.Length == tags.Length, $"Mismatch between readonly properties and tags count for type {type}");
            Assert.True(readonlyProperties.Length == readonlyTags.Length, $"Mismatch between readonly properties and tags count for type {type}");

            // ---------- Test read-write properties
            var testValues = Enumerable.Range(0, tags.Length).Select(_ => valueGenerator()).ToArray();

            // Check for each tag that the getter and the setter are mapped on the same property
            for (int i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];

                tag.Setter(instance, testValues[i]);

                Assert.True(testValues[i].Equals(tag.Getter(instance)), $"Getter and setter mismatch for tag {tag.Key} of type {type.Name}");
            }

            // Check that all read/write properties were mapped
            var remainingValues = new HashSet<T>(testValues);

            foreach (var property in properties)
            {
                Assert.True(remainingValues.Remove((T)property.GetValue(instance)), $"Property {property.Name} of type {type.Name} is not mapped");
            }

            // ---------- Test readonly properties
            remainingValues = new HashSet<T>(readonlyProperties.Select(p => (T)p.GetValue(instance)));

            foreach (var tag in readonlyTags)
            {
                Assert.True(remainingValues.Remove(tag.Getter(instance)), $"Tag {tag.Key} of type {type.Name} is not mapped");
            }
        }
    }
}
