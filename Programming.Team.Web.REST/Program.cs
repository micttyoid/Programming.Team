var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

    app.UseSwagger();
    app.UseSwaggerUI();
var defaultFileOptions = new DefaultFilesOptions();
defaultFileOptions.DefaultFileNames.Clear(); // Clear existing defaults
defaultFileOptions.DefaultFileNames.Add("index.html"); // Add custom default file

app.UseDefaultFiles(defaultFileOptions);
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
