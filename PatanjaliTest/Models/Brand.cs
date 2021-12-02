namespace PatanjaliTest.Models
{
    [BsonIgnoreExtraElements]
    public class Brand : BaseModel
    {

        [BsonElement("division_id")]
        public string DivisionId { get; set; }

        [BsonElement("vertical_id")]
        public string VerticalId { get; set; }



    }
}
