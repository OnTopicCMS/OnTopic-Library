var builder = WebApplication.CreateBuilder(
  new WebApplicationOptions() {
    WebRootPath = "_site"
});

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();