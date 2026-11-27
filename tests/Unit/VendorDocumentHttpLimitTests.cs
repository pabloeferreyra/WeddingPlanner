using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeddingPlanner.Application.Abstractions.Persistence;
using WeddingPlanner.Application.Abstractions.Services;
using WeddingPlanner.Application.UseCases.VendorDocuments;
using WeddingPlanner.Application.UseCases.Vendors;
using WeddingPlanner.Core.Domain.Entities;
using WeddingPlanner.Web.Controllers;

namespace WeddingPlanner.Tests.Unit;

public class VendorDocumentHttpLimitTests
{
    [Fact]
    public async Task Multipart_limits_allow_one_file_and_reject_oversized_or_multiple_files()
    {
        var vendor = new Vendor
        {
            BusinessName = "Test", Email = "test@example.invalid",
            Address = new Address { Street = "Test", City = "Test", State = "Test", ZipCode = "12345" }
        };
        var assignmentId = Guid.NewGuid();
        var documents = new FakeDocuments();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(VendorDocumentsController).Assembly.FullName,
            EnvironmentName = "Development"
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddControllersWithViews().AddApplicationPart(typeof(VendorDocumentsController).Assembly);
        builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");
        builder.Services.AddScoped<GetVendorByIdHandler>();
        builder.Services.AddScoped<VendorDocumentQueriesHandler>();
        builder.Services.AddScoped<GetVendorDocumentByIdHandler>();
        builder.Services.AddScoped<AddVendorDocumentHandler>();
        builder.Services.AddScoped<UpdateVendorDocumentHandler>();
        builder.Services.AddScoped<DeleteVendorDocumentHandler>();
        builder.Services.AddScoped<StoredVendorDocumentHandler>();
        builder.Services.AddSingleton<IVendorService>(new FakeVendor(vendor));
        builder.Services.AddSingleton<IVendorDocumentService>(documents);
        builder.Services.AddSingleton<IVendorDocumentQueries>(new FakeQueries(vendor.Id, assignmentId));
        await using var app = builder.Build();
        app.MapGet("/test-token", (HttpContext context, IAntiforgery antiforgery) =>
            antiforgery.GetAndStoreTokens(context).RequestToken);
        app.MapControllers();
        await app.StartAsync();
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single();
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(address) };
        var token = await client.GetStringAsync("/test-token");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token);
        var path = $"Vendors/{vendor.Id}/Documents/Create";

        using (var form = Form(assignmentId, (int)FileStorageOptions.MaximumFileSizeBytes))
        using (var response = await client.PostAsync(path, form))
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal(1, documents.Uploads);

        using (var form = Form(assignmentId, (int)FileStorageOptions.MaximumFileSizeBytes + 1))
        using (var response = await client.PostAsync(path, form))
            Assert.True(response.StatusCode is HttpStatusCode.RequestEntityTooLarge or HttpStatusCode.BadRequest,
                $"Status: {response.StatusCode}; body: {await response.Content.ReadAsStringAsync()}");
        Assert.Equal(1, documents.Uploads);

        using (var form = Form(assignmentId, (int)FileStorageOptions.MaximumRequestSizeBytes))
        using (var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = form })
        {
            request.Headers.ExpectContinue = true;
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        }
        Assert.Equal(1, documents.Uploads);

        using (var form = Form(assignmentId, 20, 20))
        using (var response = await client.PostAsync(path, form))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Solo se permite un archivo", await response.Content.ReadAsStringAsync());
        }
        Assert.Equal(1, documents.Uploads);

        using (var form = Form(assignmentId, 6 * 1024 * 1024, 6 * 1024 * 1024))
        using (var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = form })
        {
            request.Headers.ExpectContinue = true;
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        }
        Assert.Equal(1, documents.Uploads);

        using (var form = Form(assignmentId, 6 * 1024 * 1024, 6 * 1024 * 1024))
        using (var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = form })
        {
            request.Headers.TransferEncodingChunked = true;
            request.Headers.ExpectContinue = true;
            using var response = await client.SendAsync(request);
            Assert.True(response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.RequestEntityTooLarge);
        }
        Assert.Equal(1, documents.Uploads);

        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        using (var form = Form(assignmentId, (int)FileStorageOptions.MaximumFileSizeBytes + 1))
        {
            form.Add(new StringContent(token), "__RequestVerificationToken");
            using var response = await client.PostAsync(path, form);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        Assert.Equal(1, documents.Uploads);
        using (var form = Form(assignmentId, 20))
        {
            form.Add(new StringContent(token), "__RequestVerificationToken");
            using var response = await client.PostAsync(path, form);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
        Assert.Equal(2, documents.Uploads);
        await app.StopAsync();
    }

    private static MultipartFormDataContent Form(Guid assignmentId, params int[] sizes)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent("Contract"), "DocumentName");
        form.Add(new StringContent("0"), "DocumentType");
        form.Add(new StringContent(assignmentId.ToString()), "VendorAssignmentId");
        foreach (var size in sizes)
        {
            var file = new ByteArrayContent(new byte[size]);
            file.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            form.Add(file, "File", "contract.pdf");
        }
        return form;
    }

    private sealed class FakeVendor(Vendor vendor) : IVendorService
    {
        public Task<Vendor> GetVendorByIdAsync(Guid id) => Task.FromResult(vendor);
        public Task<IEnumerable<Vendor>> GetAllVendorsAsync() => throw new NotSupportedException();
        public Task<Vendor> AddVendorAsync(Vendor entity) => throw new NotSupportedException();
        public Task<Vendor> UpdateVendorAsync(Vendor entity) => throw new NotSupportedException();
        public Task DeleteVendorAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> VendorExistsAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class FakeQueries(Guid vendorId, Guid assignmentId) : IVendorDocumentQueries
    {
        public Task<IReadOnlyList<VendorDocumentAssignment>> GetAssignmentsByVendorAsync(Guid id, CancellationToken token) =>
            Task.FromResult<IReadOnlyList<VendorDocumentAssignment>>(id == vendorId
                ? [new VendorDocumentAssignment(assignmentId, Guid.NewGuid(), "Wedding", DateTime.Today, 1)] : []);
        public Task<IReadOnlyList<VendorDocument>> GetVisibleByVendorAsync(Guid id, string? userId, CancellationToken token) => throw new NotSupportedException();
        public Task<bool> HasDocumentsForVendorAsync(Guid id, CancellationToken token) => throw new NotSupportedException();
        public Task<bool> HasDocumentsForAssignmentAsync(Guid id, CancellationToken token) => throw new NotSupportedException();
    }

    private sealed class FakeDocuments : IVendorDocumentService
    {
        public int Uploads { get; private set; }
        public Task<VendorDocument> UploadAsync(VendorDocument document, Stream content, string fileName, string contentType, string? userId, CancellationToken token)
        {
            Uploads++;
            return Task.FromResult(document);
        }
        public Task<IEnumerable<VendorDocument>> GetAllVendorDocumentsAsync() => throw new NotSupportedException();
        public Task<VendorDocument> GetVendorDocumentByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<VendorDocument> AddVendorDocumentAsync(VendorDocument document) => throw new NotSupportedException();
        public Task<VendorDocument> UpdateVendorDocumentAsync(VendorDocument document) => throw new NotSupportedException();
        public Task DeleteVendorDocumentAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> VendorDocumentExistsAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> CanAccessStoredDocumentAsync(Guid vendorId, Guid assignmentId, string? userId, CancellationToken token) => throw new NotSupportedException();
        public Task<Stream> DownloadAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken token) => throw new NotSupportedException();
        public Task DeleteStoredAsync(Guid vendorId, Guid documentId, string? userId, CancellationToken token) => throw new NotSupportedException();
    }
}
