namespace PatanjaliTest.Models
{
    [BsonIgnoreExtraElements]
    public class Vertical : BaseModel
    {
        [BsonElement("division_id")]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string DivisionId { get; set; }

        [BsonElement("user_ids")]
        public List<string> UserIds { get; set; } = new List<string>();

        [BsonElement("distributor_ids")]
        public List<String> DistributorIds { get; set; } = new List<string>();
    }
}
