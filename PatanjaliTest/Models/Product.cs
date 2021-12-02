namespace PatanjaliTest.Models
{
    public class Product : BaseModel
    {
        [BsonElement("division_id")]
        public string DivisionId { get; set; }

        [BsonElement("vertical_id")]
        public string VerticalId { get; set; }

        [BsonElement("brand_id")]
        public string BrandId { get; set; }

        [BsonElement("unit_id")]
        public string UnitId { get; set; }

        [BsonElement("sap_code")]
        public string SapCode { get; set; }

        [BsonElement("dcode")]
        public string DCode { get; set; }

        [BsonElement("dname")]
        public string DName { get; set; }

        [BsonElement("distributorsellingprice")]
        public float DistributorSellingPrice { get; set; }

        [BsonElement("gst")]
        public float GST { get; set; }

        [BsonElement("mrp")]
        public float MRP { get; set; }

        [BsonElement("retailersellingprice")]
        public float RetailerSellingPrice { get; set; }

        [BsonElement("superdistributorlandingprice")]
        public float SuperDistributorLandingPrice { get; set; }

        [BsonElement("superdistributorsellingprice")]
        public float SuperDistributorSellingPrice { get; set; }

        [BsonElement("is_featured")]
        public bool IsFeatured { get; set; }

        [BsonElement("is_active")]
        public bool IsActive { get; set; }
    }
}
