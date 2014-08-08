using System;
using FubuMVC.Core.Registration.ObjectGraph;

namespace FubuTransportation.ScheduledJob
{
    public class ScheduledJobDefinition
    {
        public Type JobType { get; set; }
        public Type SchedulerType { get; set; }
        
        public ObjectDef ToObjectDef()
        {
            var def = new ObjectDef(typeof (ScheduledJobInitializer<>), JobType);
            def.DependencyByType<IJobScheduler>(new ObjectDef(SchedulerType));

            return def;
        }
    }
}