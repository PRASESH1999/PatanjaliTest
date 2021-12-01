namespace PatanjaliTest.Settings
{
    public class DatabaseSettings : IDatabaseSettings
    {
        public string DatabaseName  { get; set; }
        public string ConnectionString { get; set; }
        public string VerticalCollectionName { get; set; }
        public string DivisionCollectionName { get; set; }
        public string BrandCollectionName { get; set; }
    }

    public interface IDatabaseSettings
    {
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
        string VerticalCollectionName { get; set; }
        string DivisionCollectionName { get; set; }
        string BrandCollectionName { get; set; }
    }
}
