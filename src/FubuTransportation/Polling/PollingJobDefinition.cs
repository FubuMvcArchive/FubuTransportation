using System;
using System.Linq.Expressions;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuTransportation.Polling
{
    public class PollingJobDefinition
    {
        public Type JobType { get; set; }
        public Type SettingType { get; set; }
        public Expression IntervalSource { get; set; }

        public ObjectDef ToObjectDef()
        {
            var def = new ObjectDef(typeof (PollingJob<,>), JobType, SettingType);

            var funcType = typeof (Func<,>).MakeGenericType(SettingType, typeof (double));
            var intervalSourceType = typeof (Expression<>).MakeGenericType(funcType);
            def.DependencyByValue(intervalSourceType, IntervalSource);

            return def;
        }
    }
}