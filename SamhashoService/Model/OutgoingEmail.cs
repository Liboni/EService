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
    
    public partial class OutgoingEmail
    {
        public long Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string Reference { get; set; }
        public string Destination { get; set; }
        public byte Status { get; set; }
        public System.DateTime Date { get; set; }
    }
}
