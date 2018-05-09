
namespace SamhashoService
{
    using System.Runtime.Serialization;
    
    [DataContract]
    public class ProductRequest
    {
        [DataMember]
        public long ProductId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ShortDescription { get; set; }
        [DataMember]
        public int Catergory { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public string Per { get; set; }
        [DataMember]
        public string Manufacturer { get; set; }
        [DataMember]
        public bool IsMain { get; set; }
        [DataMember]
        public bool IsActive { get; set; }
    }

  [DataContract]
    public class CreateProductResponse
    {
        [DataMember]
        public long ProductId { get; set; }

        [DataMember]
        public string MediaSource { get; set; }

        [DataMember]
        public string Catergory { get; set; }

        [DataMember]
        public string DateCreated { get; set; }

    }

    [DataContract]
    public class ProductResponse
    {
        [DataMember]
        public bool? IsMain { get; set; }

        [DataMember]
        public bool? IsActive { get; set; }
        [DataMember]
        public long ProductId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string ShortDescription { get; set; }
        [DataMember]
        public int CatergoryId { get; set; }
        [DataMember]
        public string Catergory { get; set; }
        [DataMember]
        public decimal Price { get; set; }
        [DataMember]
        public string Per { get; set; }
        [DataMember]
        public string Manufacturer { get; set; }
        [DataMember]
        public string Code { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string CreatedDate { get; set; }
        [DataMember]
        public bool IsDeleted { get; set; }
        [DataMember]
        public string ModifiedDate { get; set; }
        [DataMember]
        public string MediaSource { get; set; }
    }

    [DataContract]
    public class ProductMediaResponse
    {
        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public string MediaSource { set; get; }

    }

    [DataContract]
    public class SearchProductRequest
    {
        [DataMember]
        public string Search { set; get; }

        [DataMember]
        public int Catergory { set; get; }
    }
}