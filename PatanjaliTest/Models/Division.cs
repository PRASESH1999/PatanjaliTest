namespace PatanjaliTest.Models
{
    public class Division
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("abbreviation")]
        public string Abbreviation { get; set; }

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("user_ids")]
        public IEnumerable<String> UserIds { get; set; } = new List<string>();

        [BsonElement("distributor_ids")]
        public IEnumerable<String> DistributorIds { get; set; } = new List<string>();
    }
}
