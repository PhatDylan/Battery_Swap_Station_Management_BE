namespace Service.Interfaces;

public interface IEmailTemplateLoaderService
{
    public Task<string> RenderTemplateAsync<T>(string templateName, T model);
}