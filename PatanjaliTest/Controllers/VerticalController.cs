using MongoDB.Driver;

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
        public async Task<IActionResult> Get(int page = 1, int itemPerPage = 10, string sort = "created_at", int sortDirection = -1, CancellationToken cancellationToken = default)
        {
            if (itemPerPage > 20)
            {
                itemPerPage = 20;
            }
            try
            {
                var skip = itemPerPage * (page - 1);
                var limit = itemPerPage;

                var sortFilter = new BsonDocument(sort, sortDirection);

                var verticals = await _verticalCollection
                    .Find("{}")
                    .Project(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DivisionId = x.DivisionId,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .Sort(sortFilter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync(cancellationToken);

                //extracting Division ID
                /*
                List<string> divisionIds = new List<string>();
                foreach (var vertical in verticals)
                {
                    if (divisionIds.Contains(vertical.DivisionId))
                    {
                        continue;
                    }
                    divisionIds.Add(vertical.DivisionId);
                }
                */

                //Doing same thing as above Loop.
                var divisionIds = (from v in verticals where v.DivisionId is not null select v.DivisionId).Distinct();

                // Does same thing as above
                var divisionIds1 = verticals.Where(v => v.DivisionId is not null).Select(v => v.DivisionId).Distinct();

                //getting list of divisions
                var filter = Builders<Division>.Filter.In(d => d.Id, divisionIds);

                var divisions = await _divisionCollection
                    .Find(filter)
                    .Project(x => new
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .ToListAsync();

                var divisionsDict = divisions.ToDictionary(x => x.Id, x => x.Name);

                //Linking divisions with the verticals
                /*
                List<VerticalProjectionClass> finalVertical = new List<VerticalProjectionClass>();
                foreach (var vertical in verticals)
                {
                    foreach (var division in divisions)
                    {
                        if (vertical.DivisionId == division.Id)
                        {
                            finalVertical.Add(new VerticalProjectionClass
                            {
                                Id = vertical.Id,
                                Name = vertical.Name,
                                DivisionId = vertical.DivisionId,
                                CreatedAt = vertical.CreatedAt,
                                UpdatedAt = vertical.UpdatedAt,
                                DivisionName = division.Name
                            });
                        }
                    }
                }
                */

                var finalVerticals = verticals.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    DivisionId = x.DivisionId,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    DivisionName = divisionsDict.ContainsKey(x.DivisionId)
                                   ? divisionsDict[x.DivisionId]
                                   : null
                });

                return Ok(new
                {
                    Page = page,
                    TotalCount = await _verticalCollection.CountAsync(_ => true),
                    Data = finalVerticals
                });
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken = default)
        {
            try
            {
                var filter = Builders<Vertical>.Filter.Eq(v => v.Id, id);
                var result = await _verticalCollection.Find(filter).FirstOrDefaultAsync();

                var divisionResult = await _divisionCollection.Find(Builders<Division>.Filter.Eq(v => v.Id, result.DivisionId)).FirstOrDefaultAsync();

                var newResult = new
                {
                    Name = result.Name,
                    Division = divisionResult?.Name
                };
                return Ok(newResult);
            }
            catch (Exception ex)
            {
                throw ex;
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

        //Extra classes
        public class divisionProjection
        {
            [BsonId]
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            [BsonElement("name")]
            public string Name { get; set; }
        }
        public class VerticalInput
        {
            public string DivisionId { get; set; }
            public string verticalName { get; set; }
        }

        public record VerticalRecord(string Id, string Name, string Divison);

        [BsonIgnoreExtraElements]
        public class VerticalWithDivision : Vertical
        {
            public string DivisionName { get; set; }
        }

        public class VerticalProjectionClass
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }

            public string Name { get; set; }

            public string DivisionId { get; set; }

            public DateTime CreatedAt { get; set; }

            public DateTime UpdatedAt { get; set; }

            public string DivisionName { get; set; }
        }
    }
}
