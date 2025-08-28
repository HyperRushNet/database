using SimpleFileDatabaseApi;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Bepaal pad naar data-bestand
var dataFile = Environment.GetEnvironmentVariable("DATA_FILE") ?? "data.json";
var db = new Database(dataFile);

// Endpoint: lijst alle personen
app.MapGet("/persons", () =>
{
    return Results.Ok(db.GetAllPersons());
});

// Endpoint: voeg een persoon toe
app.MapPost("/persons", (Person person) =>
{
    db.AddPerson(person);
    return Results.Created($"/persons/{person.Name}", person);
});

app.Run();
