using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.Sprites;

namespace ExtendedToolbar.Utils
{
    public static class ReflectionHelper
    {
        private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> propertyCache = new();
        private static readonly ConcurrentDictionary<(Type, string), FieldInfo?> fieldCache = new();
        private static readonly ConcurrentDictionary<(Type, string), MethodInfo?> methodCache = new();

        public static PropertyInfo? FindProperty(Type type, string propertyName)
        {
            return propertyCache.GetOrAdd((type, propertyName), key =>
            {
                for (var t = key.Item1; t != null && t != typeof(object); t = t.BaseType)
                {
                    var prop = t.GetProperty(key.Item2,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (prop != null) return prop;
                }
                return null;
            });
        }

        public static FieldInfo? FindField(Type type, string fieldName)
        {
            return fieldCache.GetOrAdd((type, fieldName), key =>
            {
                for (var t = key.Item1; t != null && t != typeof(object); t = t.BaseType)
                {
                    var field = t.GetField(key.Item2,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (field != null) return field;
                }
                return null;
            });
        }

        public static MethodInfo? FindMethod(Type type, string methodName)
        {
            return methodCache.GetOrAdd((type, methodName), key =>
            {
                for (var t = key.Item1; t != null && t != typeof(object); t = t.BaseType)
                {
                    var method = t.GetMethod(key.Item2,
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (method != null) return method;
                }
                return null;
            });
        }

        public static T? GetPropertyValue<T>(object instance, string propertyName)
        {
            if (instance == null) return default;
            var prop = FindProperty(instance.GetType(), propertyName);
            return prop != null ? (T?)prop.GetValue(instance) : default;
        }

        public static T? GetFieldValue<T>(object instance, string fieldName)
        {
            if (instance == null) return default;
            var field = FindField(instance.GetType(), fieldName);
            return field != null ? (T?)field.GetValue(instance) : default;
        }

        public static Drawable? FindStatsDisplay(Drawable root)
        {
            if (root == null) return null;
            return root.ChildrenOfType<Drawable>()
                .FirstOrDefault(c => c.GetType().Name.Contains("TransientUserStatisticsUpdateDisplay", StringComparison.OrdinalIgnoreCase));
        }

        public static Box? FindFlashBox(Drawable root)
        {
            if (root == null) return null;
            return root.ChildrenOfType<Box>()
                .FirstOrDefault(b => b.RelativeSizeAxes == Axes.Both && (b.Name.Contains("flash", StringComparison.OrdinalIgnoreCase) || b.Blending == BlendingParameters.Additive));
        }
    }
}
