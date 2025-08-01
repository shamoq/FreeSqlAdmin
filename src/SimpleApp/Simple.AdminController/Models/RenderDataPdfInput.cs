namespace Simple.AdminController.Models;

public class RenderDataPdfInput
{
    public Guid BusinessId { get; set; }
    public Guid TemplateId { get; set; }
    public string Data { get; set; }
}