using OrderingApplication.Extentions;
using OrderingApplication.Utilities;
using OrderingInfrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder( args );

builder.ConfigureLogging();
builder.ConfigureInfrastructure();
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
app.UseCors();
app.UseRouting();
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