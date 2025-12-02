using RazorLight;
using Service.Interfaces;

namespace Service.Implementations;

public class EmailTemplateLoaderService: IEmailTemplateLoaderService
{
    private readonly RazorLightEngine _engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(EmailTemplateLoaderService).Assembly, "Service.Templates")
            .UseMemoryCachingProvider()
            .EnableDebugMode(true)
            .Build();

    public async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        return await _engine.CompileRenderAsync(templateName, model);
    }
}