using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;

namespace MonJobs
{
    public static class DynamicExtensions
    {
        public static dynamic ToDynamic<TValue>(this TValue value) where TValue : IDictionary
        {
            return value.ToExpando();
        }

        public static ExpandoObject ToExpando(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
                expando.Add(property.Name, property.GetValue(value));

            return expando as ExpandoObject;
        }

        public static JobResult ToJobResult(this object value)
        {
            return new JobResult(value.ToExpando().ToDictionary(x => x.Key, x => x.Value));
        }

        public static JobAttributes ToJobAttributes(this object value)
        {
            return new JobAttributes(value.ToExpando().ToDictionary(x => x.Key, x => x.Value));
        }

        public static JobReport ToJobReport(this object value)
        {
            return new JobReport(value.ToExpando().ToDictionary(x => x.Key, x => x.Value));
        }
    }
}