//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SamhashoService.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductMedia
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string MediaSource { get; set; }
        public bool IsMain { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime DateCreate { get; set; }
    
        public virtual Product Product { get; set; }
    }
}
