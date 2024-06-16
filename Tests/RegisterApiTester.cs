using System.Net.Http.Json;
using OrderingApplication.Features.Users.Registration.Types;

namespace Tests;

public class RegisterApiTester( HttpClient httpClient, string apiUrl )
{
    readonly HttpClient _httpClient = httpClient;
    readonly string _apiUrl = apiUrl;
    readonly Random _random = new();

    public async Task TestRegisterApi( int numberOfTests )
    {
        for ( int i = 0; i < numberOfTests; i++ ) {
            RegisterAccountRequest accountRequest = GenerateRandomRegisterRequest();
            await SendRegisterRequest( accountRequest );
        }
    }

    async Task SendRegisterRequest( RegisterAccountRequest accountRequest )
    {
        try {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync( _apiUrl, accountRequest );
            response.EnsureSuccessStatusCode();
            Console.WriteLine( $"Register request successful: {accountRequest}" );
        }
        catch ( Exception ex ) {
            Console.WriteLine( $"Error registering: {ex.Message}" );
        }
    }

    RegisterAccountRequest GenerateRandomRegisterRequest()
    {
        // Generate random values for the fields
        string email = $"user{_random.Next( 1000, 9999 )}@example.com";
        string username = $"user{_random.Next( 1000, 9999 )}";
        string phone = GenerateRandomPhoneNumber();
        string password = Guid.NewGuid().ToString().Substring( 0, 8 ); // Generate a random 8-character password
        throw new Exception( $"Tests: {GenerateRandomRegisterRequest()}" );
        //return new RegisterAccountRequest( email, username, phone, password, string.Empty );
    }

    string GenerateRandomPhoneNumber() =>
        // Generate a random phone number with a simple pattern
        $"{_random.Next( 100, 999 )}-{_random.Next( 100, 999 )}-{_random.Next( 1000, 9999 )}";
}