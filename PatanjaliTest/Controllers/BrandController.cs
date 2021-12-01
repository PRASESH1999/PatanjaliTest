using Microsoft.AspNetCore.Mvc;

namespace PatanjaliTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BrandController : ControllerBase
    {
        private IMongoCollection<Brand> _brandCollection;
        private IMongoCollection<Vertical> _verticalCollection;
        private IMongoCollection<Division> _divisionCollection;
        public BrandController(IMongoDatabase database, IDatabaseSettings settings)
        {
            _brandCollection = database.GetCollection<Brand>(settings.BrandCollectionName);
            _verticalCollection = database.GetCollection<Vertical>(settings.VerticalCollectionName);
            _divisionCollection = database.GetCollection<Division>(settings.DivisionCollectionName);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken = default)
        {

            var filter = Builders<Brand>.Filter.Eq(m => m.Id, id);

            var brand = await _brandCollection
                .Find(filter)
                .FirstOrDefaultAsync();

            if (brand == null) return BadRequest("Not Found");

            return Ok(new
            {
                Id = brand.Id,
                Name = brand.Name
            });
        }

        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] int limit = 10, [FromQuery] int page = 1, [FromQuery]string sort = "created_at", [FromQuery]int sortDirection = -1, CancellationToken cancellationToken = default)
        {
            if (limit > 20) { 
                limit = 20;
            }
            try
            {   

                var sortBrand = new BsonDocument(sort, sortDirection);

                //var brandProjection = Builders<Brand>.Projection
                //    .Include(b => b.Id)
                //    .Include(b => b.Name)
                //    .Include(b => b.DivisionId)
                //    .Include(b => b.VerticalId)
                //    .Include(b => b.CreatedAt)
                //    .Include(b => b.UpdatedAt);

                var brands = await _brandCollection
                    .Find("{}")
                    .Project(x => new
                    {
                        Id = x.Id,
                        Name = x.Name,
                        DivisionId = x.DivisionId,
                        VerticalId = x.VerticalId,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                    })
                    .Sort(sortBrand)
                    .Skip(limit * (page - 1))
                    .Limit(limit)
                    .ToListAsync(cancellationToken);


                //extracting Division Id
                List<string> divisionIds = new List<string>();
                List<string> verticalIds = new List<string>();
                foreach (var brand in brands)
                {
                    Console.WriteLine(brand);
                    if (!divisionIds.Contains(brand.DivisionId))
                    {
                        divisionIds.Add(brand.DivisionId);
                    }

                    if (!divisionIds.Contains(brand.VerticalId))
                    {
                        verticalIds.Add(brand.VerticalId);
                    }
                }
                //extracting Vertical Id
                //List<string> verticalIds = new List<string>();
                //foreach (var brand in brands)
                //{
                //    if (verticalIds.Contains(brand.VerticalId))
                //    {
                //        continue;
                //    }
                //    verticalIds.Add(brand.VerticalId);
                //}
                //getting list of Divisions
                var divisionFilter = Builders<Division>.Filter.In(d => d.Id, divisionIds);
                var divProjectionFilter = Builders<Division>.Projection
                    .Include(d => d.Name);
                var divisions = await _divisionCollection
                    .Find(divisionFilter)
                    .Project<DivisionProject>(divProjectionFilter)
                    .ToListAsync();

                //getting list of verticals
                var verticalFilter = Builders<Vertical>.Filter.In(d => d.Id, verticalIds);
                var verProjectionFilter = Builders<Vertical>.Projection
                    .Include(d => d.Name)
                    .Include(d => d.DivisionId);
                var verticals = await _verticalCollection
                    .Find(verticalFilter)
                    .Project<VerticalProject>(verProjectionFilter)
                    .ToListAsync();


                //linking divisions and verticals with the brands
                List<BrandProjectionClass> finalBrand = new List<BrandProjectionClass>();
                foreach(var brand in brands)
                {
                    foreach(var vertical in verticals)
                    {
                        if(brand.VerticalId == vertical.Id)
                        {
                            foreach(var division in divisions)
                            {
                                if(vertical.DivisionId == division.Id)
                                {
                                    finalBrand.Add(new BrandProjectionClass
                                    {
                                        Id = brand.Id,
                                        Name = brand.Name,
                                        DivisionId = brand.DivisionId,
                                        VerticalId = brand.VerticalId,
                                        CreatedAt = brand.CreatedAt,
                                        UpdatedAt = brand.UpdatedAt,
                                        DivisionName = division.Name,
                                        VerticalName = vertical.Name,
                                    });
                                }
                            }

                        }
                    }
                }
 
                return Ok(new
                {
                    Page = page,
                    TotalCount = await _brandCollection.CountAsync(_ => true),
                    Data = finalBrand
                });

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Create(BrandInput inputModel)
        {
            Brand newBrand = new Brand
            {
                Name = inputModel.Name,
                DivisionId = inputModel.DivisionId,
                VerticalId = inputModel.VerticalId,
            };

            await _brandCollection.InsertOneAsync(newBrand);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, BrandInput inputModel)
        {
            var filter = Builders<Brand>.Filter.Eq(d => d.Id, id);

            var update = Builders<Brand>.Update
                .Set(d => d.Name, inputModel.Name)
                .Set(d => d.DivisionId, inputModel.DivisionId)
                .Set(d => d.VerticalId, inputModel.VerticalId)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);

            await _brandCollection.UpdateOneAsync(filter, update);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var filter = Builders<Brand>.Filter.Eq(d => d.Id, id);

            await _brandCollection.DeleteOneAsync(filter);
            return Ok();
        }

        public class BrandInput
        {
            public string Name { get; set; }
            public string DivisionId { get; set; }
            public string VerticalId { get; set; }
        }

        public class DivisionProject
        {
            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            [BsonElement("name")]
            public string Name { get; set; }
        }

        public class VerticalProject
        {

            [BsonRepresentation(BsonType.ObjectId)]
            public string Id { get; set; }
            [BsonElement("name")]
            public string Name { get; set; }
            [BsonElement("division_id")]
            public string DivisionId { get; set; }

        }

        public class BrandProjectionClass
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string DivisionId { get; set; }
            public string VerticalId { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
            public string DivisionName { get; set; }
            public string VerticalName { get; set; }

        }

    }
}
