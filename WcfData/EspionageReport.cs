//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WcfData
{
    using System;
    using System.Collections.Generic;
    
    public partial class EspionageReport
    {
        public int ErId { get; set; }
        public Nullable<int> EspionageId { get; set; }
        public bool IsExtreme { get; set; }
        public int PlayerId { get; set; }
        public int PositionId { get; set; }
        public int Ore { get; set; }
        public int Crystal { get; set; }
        public int Hydrogen { get; set; }
        public System.DateTime ApproximateTime { get; set; }
        public Nullable<bool> HasShips { get; set; }
        public Nullable<bool> HasDefenses { get; set; }
        public Nullable<byte> UniverseId { get; set; }
    }
}