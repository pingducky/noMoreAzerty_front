using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using noMoreAzerty_front;
using noMoreAzerty_front.Services;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


// Configuration de l'authentification avec Azure AD
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://67ee7997-1621-46ac-ada5-c49204f57d56/API.Access");
    options.ProviderOptions.LoginMode = "redirect";
});

// Enregistre un HttpClient qui enverra le token automatiquement
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri("https://localhost:7104/");
})
.AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
    .ConfigureHandler(
        authorizedUrls: new[] { "https://localhost:7104" },
        scopes: new[] { "api://67ee7997-1621-46ac-ada5-c49204f57d56/API.Access" }));

//builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<VaultService>();
builder.Services.AddMudServices();


var app = builder.Build();

// Redirection vers /vaults au démarrage
var navigationManager = app.Services.GetRequiredService<NavigationManager>();
navigationManager.NavigateTo("/vaults");

await app.RunAsync();
