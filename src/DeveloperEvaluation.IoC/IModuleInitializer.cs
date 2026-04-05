using Microsoft.AspNetCore.Builder;

namespace DeveloperEvaluation.IoC;

public interface IModuleInitializer
{
    void Initialize(WebApplicationBuilder builder);
}
