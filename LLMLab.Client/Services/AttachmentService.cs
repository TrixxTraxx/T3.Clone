using System.Net.Http.Json;
using LLMLab.Dtos.Messages;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace LLMLab.Client.Services;

public class AttachmentService
{
    private readonly HttpClient _httpClient;
    private readonly ISnackbar _snackbar;

    public AttachmentService(HttpClient httpClient, ISnackbar snackbar)
    {
        _httpClient = httpClient;
        _snackbar = snackbar;
    }

    public async Task<AttachmentDto?> UploadAttachment(IBrowserFile file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 1024 * 1024)); // 1MB limit
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
            
            content.Add(fileContent, "file", file.Name);

            var response = await _httpClient.PostAsync($"api/attachement/upload?fileName={Uri.EscapeDataString(file.Name)}&contentType={Uri.EscapeDataString(file.ContentType)}", content);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AttachmentDto>();
                _snackbar.Add($"File '{file.Name}' uploaded successfully", Severity.Success);
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _snackbar.Add($"Failed to upload file: {error}", Severity.Error);
                return null;
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Error uploading file: {ex.Message}", Severity.Error);
            return null;
        }
    }

    public async Task<AttachmentDto?> GetAttachment(int attachmentId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/attachement/{attachmentId}");
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AttachmentDto>();
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting attachment: {ex.Message}");
            return null;
        }
    }

    public string GetAttachmentContentUrl(int attachmentId)
    {
        return $"{_httpClient.BaseAddress}api/attachement/content/{attachmentId}";
    }
} 