namespace PatanjaliTest.Models
{
    [BsonIgnoreExtraElements]
    public class Vertical : BaseModel
    {


        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("division_id")]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string DivisionId { get; set; }



        [BsonElement("user_ids")]
        public IEnumerable<string> UserIds { get; set; } = new List<string>();

        [BsonElement("distributor_ids")]
        public IEnumerable<String> DistributorIds { get; set; } = new List<string>();
    }
}
