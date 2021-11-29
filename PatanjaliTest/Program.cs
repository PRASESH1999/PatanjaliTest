var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

//Initializing Mongo Client

//Database Settings
builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(nameof(DatabaseSettings)));

builder.Services.AddSingleton<IDatabaseSettings>(sp =>
    sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

// Connection to database
builder.Services.AddSingleton<IMongoClient, MongoClient>(
        sp =>
        {
            DatabaseSettings settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        }
    );

//Getting Database
builder.Services.AddSingleton<IMongoDatabase>(
        sp =>
        {
            IDatabaseSettings settings = sp.GetRequiredService<IOptions<DatabaseSettings>>().Value;
            IMongoClient client = sp.GetRequiredService<IMongoClient>();

            return client.GetDatabase(settings.DatabaseName);
        }
    );


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
