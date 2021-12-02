namespace PatanjaliTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IMongoCollection<Product> _productCollection;
        private IMongoCollection<Vertical> _verticalCollection;
        private IMongoCollection<Division> _divisionCollection;
        private IMongoCollection<Unit> _unitCollection;
        private IMongoCollection<Brand> _brandCollection;

        public ProductController(IMongoDatabase database, IDatabaseSettings settings)
        {
            _productCollection = database.GetCollection<Product>(settings.ProductCollectionName);
            _verticalCollection = database.GetCollection<Vertical>(settings.VerticalCollectionName);
            _divisionCollection = database.GetCollection<Division>(settings.DivisionCollectionName);
            _unitCollection = database.GetCollection<Unit>(settings.UnitCollectionName);
            _brandCollection = database.GetCollection<Brand>(settings.BrandCollectionName);
        }

        [HttpPost]
        public async Task<IActionResult> Post(EditInputModel product)
        {
            var newProduct = new Product
            {
                Name = product.Name,
                DivisionId = product.DivisionId,
                VerticalId = product.VerticalId,
                BrandId = product.BrandId,
                UnitId = product.UnitId,
                SapCode = product.SapCode,
                DCode = product.DCode,
                DName = product.DName,
                IsFeatured = product.IsFeatured
            };
            await _productCollection.InsertOneAsync(newProduct);

            return Ok(newProduct);
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int itemPerPage = 10, string sort = "sap_code", int sortDirection = -1, CancellationToken cancellationToken = default)
        {
            //limitting the number of pages
            if (itemPerPage > 20)
            {
                itemPerPage = 20;
            }
            try
            {
                var skip = itemPerPage * (page - 1);
                var limit = itemPerPage;

                var sortFilter = new BsonDocument(sort, sortDirection);

                var products = await _productCollection
                    .Find("{}")
                    .Project(x => new
                    {
                        SapCode = x.SapCode,
                        Id = x.Id,
                        Name = x.Name,
                        Dcode = x.DCode,
                        Dname = x.DName,
                        IsActive = x.IsActive,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt,
                        DivisionId = x.DivisionId,
                        VerticalId = x.VerticalId,
                        UnitId = x.UnitId
                    })
                    .Sort(sortFilter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync(cancellationToken);

                //Getting ids to retrieve Names

                //var divisionIds = (from p in products where p.DivisionId is not null select p.DivisionId).Distinct();
                var divisionIds = products.Where(p => p.DivisionId is not null).Select(p => p.DivisionId).Distinct();

                var verticalIds = (from p in products where p.VerticalId is not null select p.VerticalId).Distinct();

                var unitIds = (from p in products where p.UnitId is not null select p.UnitId).Distinct();

                //getting list of divisions
                var divisions = await _divisionCollection
                    .Find(Builders<Division>.Filter.In(d => d.Id, divisionIds))
                    .Project(
                        x => new
                        {
                            Id = x.Id,
                            Name = x.Name
                        }
                    )
                    .ToListAsync();

                var divisionsDict = divisions.ToDictionary(x => x.Id, x => x.Name);

                var verticals = await _verticalCollection
                    .Find(Builders<Vertical>.Filter.In(d => d.Id, verticalIds))
                    .Project(
                        x => new
                        {
                            Id = x.Id,
                            Name = x.Name
                        }
                    )
                    .ToListAsync();

                var verticalDict = verticals.ToDictionary(x => x.Id, x => x.Name);


                var units = await _unitCollection
                    .Find(Builders<Unit>.Filter.In(d => d.Id, unitIds))
                    .Project(
                        x => new
                        {
                            Id = x.Id,
                            Name = x.Name
                        }
                    )
                    .ToListAsync();

                var unitDict = units.ToDictionary(x => x.Id, x => x.Name);


                var finalProduct = products.Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    Dcode = x.Dcode,
                    Dname = x.Dname,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    IsActive = x.IsActive,
                    DivisionId = x.DivisionId,
                    DivisionName = divisionsDict.ContainsKey(x.DivisionId)
                                    ? divisionsDict[x.DivisionId]
                                    : null,
                    VerticalId = x.VerticalId,
                    VerticalName = verticalDict.ContainsKey(x.VerticalId)
                                    ? verticalDict[x.VerticalId]
                                    : null,
                    UnitId = x.UnitId,
                    UnitName = unitDict.ContainsKey(x.UnitId)
                                    ? unitDict[x.UnitId]
                                    : null

                });


                return Ok(finalProduct);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var product = await _productCollection
                    .Find(Builders<Product>.Filter.Eq(x => x.Id, id))
                    .Project(x => new
                    {
                        SapCode = x.SapCode,
                        Id = x.Id,
                        Name = x.Name,
                        Dcode = x.DCode,
                        Dname = x.DName,
                        DivisionId = x.DivisionId,
                        VerticalId = x.VerticalId,
                        BrandId = x.BrandId,
                        UnitId = x.UnitId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

            var division = await _divisionCollection
                    .Find(Builders<Division>.Filter.Eq(d => d.Id, product.DivisionId))
                    .Project(
                        x => new
                        {
                            Id = x.Id,
                            Name = x.Name
                        }
                    )
                    .FirstOrDefaultAsync();

            var vertical = await _verticalCollection
                    .Find(Builders<Vertical>.Filter.Eq(d => d.Id, product.VerticalId))
                    .Project(
                        x => new
                        {
                            Id = x.Id,
                            Name = x.Name
                        }
                    )
                    .FirstOrDefaultAsync();

            var brand = await _brandCollection
                    .Find(Builders<Brand>.Filter.Eq(d => d.Id, product.BrandId))
                    .Project(
                            x => new
                            {
                                Id = x.Id,
                                Name = x.Name
                            }
                        )
                    .FirstOrDefaultAsync();

            var unit = await _unitCollection
                    .Find(Builders<Unit>.Filter.Eq(d => d.Id, product.UnitId))
                    .Project(
                        x => new
                        {
                            Id = x.Id,
                            Name = x.Name
                        }
                    )
                    .FirstOrDefaultAsync();

            var finalProduct = new
            {
                Id = product.Id,
                Name = product.Name,
                Dcode = product.Dcode,
                Dname = product.Name,
                DivisionId = product.DivisionId,
                DivisionName = division?.Name,
                VerticalId = product.VerticalId,
                VerticalName = vertical?.Name,
                BrandId = product.BrandId,
                BrandName = brand?.Name,
                UnitId = product.UnitId,
                UnitName = unit?.Name
            };

            return Ok(finalProduct);
        }

        [HttpPut]
        public async Task<IActionResult> Update(string id, EditInputModel input)
        {
            await _productCollection.UpdateOneAsync(
                    Builders<Product>.Filter.Eq(x => x.Id, id),
                    Builders<Product>.Update
                        .Set(t => t.Name, input.Name)
                        .Set(t => t.DivisionId, input.DivisionId)
                        .Set(t => t.VerticalId, input.VerticalId)
                        .Set(t => t.BrandId, input.BrandId)
                        .Set(t => t.UnitId, input.UnitId)
                        .Set(t => t.SapCode, input.SapCode)
                        .Set(t => t.DCode, input.DCode)
                        .Set(t => t.DName, input.DName)
                        .Set(t => t.IsFeatured, input.IsFeatured)
                    );
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _productCollection.DeleteOneAsync(Builders<Product>.Filter.Eq(x => x.Id, id));

            return Ok();
        }

        //Extra Classes:
        public class BaseInputModel
        {
            public string Name { get; set; }
            public string DivisionId { get; set; }
            public string VerticalId { get; set; }
            public string BrandId { get; set; }
            public string UnitId { get; set; }
            public string SapCode { get; set; }
            public string DCode { get; set; }
            public string DName { get; set; }
            public bool IsFeatured { get; set; }
        }

        public class EditInputModel : BaseInputModel { }
    }
}
