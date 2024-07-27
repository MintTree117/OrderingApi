using OrderingApplication.Extentions;
using OrderingApplication.Middleware;
using OrderingInfrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.ConfigureLogging();
builder.ConfigureInfrastructure();
builder.ConfigureAuthentication();
builder.ConfigureFeatures();
builder.ConfigureSwagger();
builder.ConfigureCors();

WebApplication app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors();
app.UseAuthorization();
app.UseEndpoints();
app.Run();