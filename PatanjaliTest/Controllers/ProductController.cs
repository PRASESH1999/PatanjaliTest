using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PatanjaliTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : Controller
    {
        private IMongoCollection<Product> _productCollection;

        public ProductController(IMongoDatabase database, IDatabaseSettings settings)
        {
            _productCollection = database.GetCollection<Product>("products");
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ProductInputModel product)
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

            return View(newProduct);
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int itemPerPage = 10, [FromQuery] string sort = "sap_code", [FromQuery] int sortDirection = -1, CancellationToken cancellationToken = default)
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
                        updatedAt = x.UpdatedAt,
                        DivisionId = x.DivisionId,
                        VerticalId = x.VerticalId
                    })
                    .Sort(sortFilter)
                    .Skip(skip)
                    .Limit(limit)
                    .ToListAsync(cancellationToken);

                //extracting division_id
                List<string> divisionIds = new List<string>();
                foreach (var product in products)
                {
                    if (divisionIds.Contains(product.DivisionId))
                    {
                        continue;
                    }
                    divisionIds.Add(product.DivisionId);
                }

                //extracting vertical_id
                List<string> verticalIds = new List<string>();
                foreach (var product in products)
                {
                    if (verticalIds.Contains(product.VerticalId))
                    {
                        continue;
                    }
                    verticalIds.Add(product.VerticalId);
                }



                return Ok(products);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //extra classes
        public class ProductInputModel
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
    }
}
