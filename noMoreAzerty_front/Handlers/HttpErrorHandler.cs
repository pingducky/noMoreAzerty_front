using MudBlazor;
using System.Net;
using System.Text.Json;

namespace noMoreAzerty_front.Handlers
{
    // Exception personnalisée pour transporter les détails d'erreur
    public class ApiException : Exception
    {
        public ProblemDetails Problem { get; }
        public ApiException(ProblemDetails problem) : base(problem.Detail ?? problem.Title)
        {
            Problem = problem;
        }
    }

    public class HttpErrorHandler : DelegatingHandler
    {
        private readonly ILogger<HttpErrorHandler> _logger;

        public HttpErrorHandler(ILogger<HttpErrorHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                await HandleErrorAsync(response);
            }

            return response;
        }

        private async Task HandleErrorAsync(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            _logger.LogWarning(
                "HTTP request failed with status {StatusCode}. Content: {Content}",
                response.StatusCode,
                content);

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Tenter de désérialiser en ValidationProblemDetails d'abord (400 Bad Request)
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var validationProblem = JsonSerializer.Deserialize<ValidationProblemDetails>(content, options);
                        if (validationProblem?.Errors != null && validationProblem.Errors.Any())
                        {
                            throw new ApiException(validationProblem);
                        }
                    }
                    catch
                    {
                        _logger.LogDebug("Failed to deserialize as ValidationProblemDetails, trying ProblemDetails");
                    }
                }

                // Désérialisation standard ProblemDetails
                var problem = JsonSerializer.Deserialize<ProblemDetails>(content, options)
                              ?? new ProblemDetails { Title = "Erreur inconnue", Detail = content };
                throw new ApiException(problem);
            }
            catch (Exception ex) when (ex is not ApiException)
            {
                _logger.LogError(ex, "Failed to deserialize error response");
                throw new ApiException(new ProblemDetails
                {
                    Title = "Erreur inattendue",
                    Detail = $"Impossible de traiter la réponse d'erreur : {ex.Message}"
                });
            }
        }
    }
}