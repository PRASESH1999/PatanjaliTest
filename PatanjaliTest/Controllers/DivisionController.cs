<<<<<<< HEAD
﻿namespace PatanjaliTest.Controllers;
=======
﻿using Microsoft.AspNetCore.Mvc;

namespace PatanjaliTest.Controllers;
>>>>>>> 34065605a465c518202d71bee3707d59cca2c29a

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

    // Get Division by Id
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

    // Get Divisions
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

    // Create new Division
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

    // Update Division
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

    //Delete division
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var filter = Builders<Division>.Filter.Eq(d => d.Id, id);

        await _divisionCollection.DeleteOneAsync(filter);

        return Ok();
    }

    // Base or parent model class for create and update division
    public class BaseInputModel
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
    }

    // Child model class for create division
    public class AddInputModel : BaseInputModel
    {
        
    }

    // Child model class for update division
    public class UpdateInputModel : BaseInputModel
    {

    }

}
