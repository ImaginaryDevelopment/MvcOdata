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
    
    public partial class EspionageBuilding
    {
        public EspionageBuilding()
        {
            this.Espionages = new HashSet<Espionage>();
        }
    
        public int EspionageBuildingId { get; set; }
        public Nullable<byte> ShipYard { get; set; }
        public Nullable<byte> Capitol { get; set; }
        public Nullable<byte> ResearchLab { get; set; }
        public Nullable<byte> MissileSilo { get; set; }
        public Nullable<byte> Factory { get; set; }
        public Nullable<byte> OreWarehouse { get; set; }
        public Nullable<byte> CrystalWarehouse { get; set; }
        public Nullable<byte> HydrogenStorage { get; set; }
        public Nullable<byte> Foundry { get; set; }
        public Nullable<byte> OreMine { get; set; }
        public Nullable<byte> CrystalMine { get; set; }
        public Nullable<byte> HydrogenSynthesizer { get; set; }
    
        public virtual ICollection<Espionage> Espionages { get; set; }
    }
}
