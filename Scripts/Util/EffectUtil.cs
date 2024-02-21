using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using TS.TSEffect.Template;

namespace TS.TSEffect.Util
{
    public static class EffectUtil
    {
        public static List<Type> GetEffectTypesByReflection()
        {
            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var all_types = assembly.GetTypes();
                foreach (var type in all_types)
                {
                    var temp_reg = type.GetCustomAttribute(typeof(TemplateRegister)) as TemplateRegister;
                    if (temp_reg != null)
                    {
                        if (typeof(TSEffectTemplate).IsAssignableFrom(type))
                        {
                            types.Add(type);
                        }
                    }
                }
            }
            return types;
        }
        public static bool TryInstantiateEffect(Type type, out TSEffectTemplate effect)
        {
            if (TSEffect.Metadata.EffectTypes.Contains(type.FullName))
            {
                effect = FormatterServices.GetUninitializedObject(type) as TSEffectTemplate;
                type.GetMethod("OnInit", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(effect, new object[0]);
                effect.OnReset();
                effect.RefreshRegisteredTargetType();
                return true;
            }
            else
            {
                effect = null;
                return false;
            }
        }
    }
}
