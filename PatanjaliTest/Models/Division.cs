namespace PatanjaliTest.Models
{
    public class Division : BaseModel
    {

        [BsonElement("abbreviation")]
        public string Abbreviation { get; set; }

        [BsonElement("user_ids")]
        public IEnumerable<String> UserIds { get; set; } = new List<string>();

        [BsonElement("distributor_ids")]
        public IEnumerable<String> DistributorIds { get; set; } = new List<string>();
    }
}
