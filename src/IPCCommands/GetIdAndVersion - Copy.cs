﻿using IPCShared.BaseStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCShared
{
    public class AcquireLeaseRequest : RequestMessageBase
    {
        public string? GrainId { get; set; }
    }

    public class AcquireLeaseResponse : ResponseMessageBase
    {        
        public Guid LeaseID { get; set; }
    }

    public class ReadLeaseIDRequest : RequestMessageBase
    {
        public string? GrainId { get; set; }
    }

    public class ReadLeaseIDResponse : ResponseMessageBase
    {
        public Guid? LeaseID { get; set; }
    }
}
