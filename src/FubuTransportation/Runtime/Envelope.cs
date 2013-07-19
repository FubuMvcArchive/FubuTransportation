using System;
using System.Collections.Specialized;
using FubuCore.Util;

namespace FubuTransportation.Runtime
{
    public abstract class Envelope
    {
        public static readonly string Id = "Id";
        public static readonly string OriginalId = "OriginalId";
        public static readonly string ParentId = "ParentId";

        public abstract NameValueCollection Headers { get; } 

        public object[] Messages;

        // TODO -- do routing slip tracking later
        
        public Uri Source;

        public abstract void MarkSuccessful();
        public abstract void MarkFailed();
        
    }


}