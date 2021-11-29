namespace PatanjaliTest.Models
{
    [BsonIgnoreExtraElements]
    public class Vertical
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("division_id")]
        //[BsonRepresentation(BsonType.ObjectId)]
        public string DivisionId { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("user_ids")]
        public IEnumerable<string> UserIds { get; set; } = new List<string>();

        [BsonElement("distributor_ids")]
        public IEnumerable<String> DistributorIds { get; set; } = new List<string>();
    }
}
