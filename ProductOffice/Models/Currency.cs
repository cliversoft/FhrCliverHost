//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cliver.ProductOffice.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Currency
    {
        public Currency()
        {
            this.ImportMaps = new HashSet<ImportMap>();
            this.Prices = new HashSet<Price>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
    
        public virtual ICollection<ImportMap> ImportMaps { get; set; }
        public virtual ICollection<Price> Prices { get; set; }
    }
}
