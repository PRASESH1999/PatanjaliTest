namespace PatanjaliTest.Controllers;

[ApiController]
[Route("[controller]")]
public class DivisionController : ControllerBase
{

    private IMongoCollection<Division> _divisionCollection;

    //ctor
    public DivisionController(IMongoDatabase database, IDatabaseSettings settings)
    {
        _divisionCollection = database.GetCollection<Division>(settings.DivisionCollectionName);
    }

    [HttpGet("{divisionId}")]
    public async Task<IActionResult> Get(string divisionId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Division>.Filter.Eq("DivisionId", divisionId);

        var division = await _divisionCollection
            .Find(filter)
            .FirstOrDefaultAsync();

        if (division is null) return BadRequest("Not Found");

        return Ok(new
        {
            Id = division.Id,
            Name = division.Name
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        int limit = 10, [FromQuery]int page = 1, string sort = "created_at", int sortDirection = -1, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Division>.Filter.Empty;

        var sortDivision = new BsonDocument(sort, sortDirection); 

        var divisions = await _divisionCollection
            .Find(filter)
            .Skip(limit * (page-1))
            .Limit(limit)
            .Sort(sortDivision)
            .ToListAsync();

        return Ok(divisions);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddInputModel inputModel, CancellationToken cancellationToken = default)
    {
        Division division = new Division
        {
            Name = inputModel.Name,
            Abbreviation = inputModel.Abbreviation,
        };

        await _divisionCollection.InsertOneAsync(division);

        return Ok();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, AddInputModel inputModel) 
    {
        var filter = Builders<Division>.Filter.Eq(d => d.Id, id);

        var update = Builders<Division>.Update
            .Set(d => d.Name, inputModel.Name)
            .Set(d => d.Abbreviation, inputModel.Abbreviation)
            .Set(d => d.UpdatedAt, DateTime.UtcNow);

        await _divisionCollection.UpdateOneAsync(filter, update);

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var filter = Builders<Division>.Filter.Eq(d => d.Id, id);

        await _divisionCollection.DeleteOneAsync(filter);

        return Ok();
    }

    public class BaseInputModel
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
    }

    public class AddInputModel : BaseInputModel
    {
        
    }

    public class UpdateInputModel : BaseInputModel
    {

    }

}
