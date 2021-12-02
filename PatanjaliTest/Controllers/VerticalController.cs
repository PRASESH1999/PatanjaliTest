using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PatanjaliTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerticalController : ControllerBase
    {
        private IMongoCollection<Vertical> _verticalCollection;
        private IMongoCollection<Division> _divisionCollection;

        //ctor
        public VerticalController(IMongoDatabase database, IDatabaseSettings settings)
        {
            _verticalCollection = database.GetCollection<Vertical>(settings.VerticalCollectionName);
            _divisionCollection = database.GetCollection<Division>(settings.DivisionCollectionName);
        }

        //Create a Vertical
        [HttpPost]
        public async Task<IActionResult> Post(VerticalInput input)
        {
            var dateNow = DateTime.UtcNow;
            var verticalName = input.verticalName;
            var divisionId = input.DivisionId;

            var newVertical = new Vertical
            {
                Name = verticalName,
                DivisionId = divisionId,
            };

            await _verticalCollection.InsertOneAsync(newVertical);

            return Ok(newVertical);
        }

        // Get Every Data in Verticals Colelction
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int itemPerPage = 10, [FromQuery] string sort = "created_at", [FromQuery] int sortDirection = -1, CancellationToken cancellationToken = default)
        {
            try
            {
                var skip = itemPerPage * (page - 1);
                var limit = itemPerPage;

                var sortFilter = new BsonDocument(sort, sortDirection);

                var verticals = await _divisionCollection
                    .Aggregate()
                    .Lookup<Division, Vertical, VerticalWithDivision>(
                        _verticalCollection,
                        d => d.Id,
                        v => v.DivisionId,
                        l => l.vertical
                    )
                    .Limit(limit)
                    .Skip(skip)
                    .Sort(sortFilter)
                    .ToListAsync(cancellationToken);

                return Ok(verticals);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        //Update an existing Vertical
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, VerticalInput input)
        {
            var filter = Builders<Vertical>.Filter.Eq(v => v.Id, id);

            await _verticalCollection.UpdateOneAsync(
                filter,
                Builders<Vertical>.Update
                    .Set(t => t.DivisionId, input.DivisionId)
                    .Set(t => t.Name, input.verticalName)
                    .Set(t => t.UpdatedAt, DateTime.UtcNow)
            );
            return Ok(input);
        }

        // Delete an Existing vertical
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var filter = Builders<Vertical>.Filter.Where(
                    c => c.Id == id
                );
            await _verticalCollection.DeleteOneAsync(filter);

            return Ok();
        }

        public class VerticalInput
        {
            public string DivisionId { get; set; }
            public string verticalName { get; set; }
        }
        public class VerticalWithDivision : Division 
        { 
            public List<Vertical> vertical { get; set; }
        }
    }
}
