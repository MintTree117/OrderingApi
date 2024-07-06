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
// app.UseStaticFiles(); // Uncomment if needed
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseCors();
//app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints();
app.Run();
/*
app.UseSwagger();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseEndpoints();
app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
*/