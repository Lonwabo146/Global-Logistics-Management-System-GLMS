using System.Net.Http;
using System.Text;
using System.Text.Json;
using GLMS.Models;

namespace GLMS.Services
{
    public class GlmsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        
        public GlmsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new System.Text.Json.Serialization.JsonStringEnumConverter()
                }
            };
        }

        // ── CUSTOMERS ─────────────────────────────────────────────────

        public async Task<List<Customer>> GetCustomersAsync()
        {
            var response = await _httpClient.GetAsync("api/customers");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Customer>>(json, _jsonOptions)
                ?? new List<Customer>();
        }

        public async Task<Customer?> GetCustomerAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/customers/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Customer>(json, _jsonOptions);
        }

        public async Task<Customer?> CreateCustomerAsync(Customer customer)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(customer, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/customers", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Customer>(json, _jsonOptions);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(customer, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"api/customers/{customer.CustomerId}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/customers/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ── CONTRACTS ─────────────────────────────────────────────────

        public async Task<List<Contract>> GetContractsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            ContractStatus? status = null)
        {
            var query = new List<string>();
            if (startDate.HasValue)
                query.Add($"startDate={startDate.Value:yyyy-MM-dd}");
            if (endDate.HasValue)
                query.Add($"endDate={endDate.Value:yyyy-MM-dd}");
            if (status.HasValue)
                query.Add($"status={status.Value}");

            var url = "api/contracts";
            if (query.Any())
                url += "?" + string.Join("&", query);

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Contract>>(json, _jsonOptions)
                ?? new List<Contract>();
        }

        public async Task<Contract?> GetContractAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/contracts/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json, _jsonOptions);
        }

        public async Task<Contract?> CreateContractAsync(
            string serviceLevel, DateTime startDate,
            DateTime endDate, int customerId, bool isInternational)
        {
            var dto = new
            {
                serviceLevel,
                startDate,
                endDate,
                customerId,
                isInternational
            };

            var content = new StringContent(
                JsonSerializer.Serialize(dto, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/contracts", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Contract>(json, _jsonOptions);
        }

        public async Task UpdateContractStatusAsync(int id, ContractStatus status)
        {
            var dto = new { status };
            var content = new StringContent(
                JsonSerializer.Serialize(dto, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PatchAsync(
                $"api/contracts/{id}/status", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateContractAsync(Contract contract)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(contract, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"api/contracts/{contract.ContractId}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteContractAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/contracts/{id}");
            response.EnsureSuccessStatusCode();
        }

        // ── SERVICE REQUESTS ──────────────────────────────────────────

        public async Task<List<ServiceRequest>> GetServiceRequestsAsync()
        {
            var response = await _httpClient.GetAsync("api/servicerequests");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<ServiceRequest>>(json, _jsonOptions)
                ?? new List<ServiceRequest>();
        }

        public async Task<ServiceRequest?> GetServiceRequestAsync(int id)
        {
            var response = await _httpClient.GetAsync($"api/servicerequests/{id}");
            if (!response.IsSuccessStatusCode) return null;
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ServiceRequest>(json, _jsonOptions);
        }

        public async Task<(bool success, string error)> CreateServiceRequestAsync(
            ServiceRequest request)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(
                "api/servicerequests", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var errorDoc = JsonDocument.Parse(errorJson);
                var message = errorDoc.RootElement
                    .TryGetProperty("message", out var msg)
                    ? msg.GetString() ?? "Unknown error"
                    : "Unknown error";
                return (false, message);
            }

            return (true, string.Empty);
        }

        public async Task UpdateServiceRequestAsync(ServiceRequest request)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request, _jsonOptions),
                Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(
                $"api/servicerequests/{request.ServiceRequestId}", content);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteServiceRequestAsync(int id)
        {
            var response = await _httpClient
                .DeleteAsync($"api/servicerequests/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<decimal> GetExchangeRateAsync()
        {
            var response = await _httpClient
                .GetAsync("api/servicerequests/exchangerate");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("usdToZar").GetDecimal();
        }
    }
}