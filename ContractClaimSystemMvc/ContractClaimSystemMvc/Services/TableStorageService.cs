using Azure.Data.Tables;
using ContractClaimSystemMvc.Models;
using Azure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ContractClaimSystemMvc.Services
{
    public class TableStorageService
    {
        private readonly TableClient _tableClaimsClient;

        public TableStorageService(string connectionString)
        {
            _tableClaimsClient = new TableClient(connectionString, "Claims");
            _tableClaimsClient.CreateIfNotExists();
        }

        public async Task<List<Claims>> GetAllClaimsAsync()
        {
            var claims = new List<Claims>();
            await foreach (var claim in _tableClaimsClient.QueryAsync<Claims>())
            {
                claims.Add(claim);
            }
            return claims;
        }

        public async Task AddClaimAsync(Claims claim)
        {
            if (string.IsNullOrEmpty(claim.PartitionKey) || string.IsNullOrEmpty(claim.RowKey))
            {
                throw new ArgumentException("PartitionKey and RowKey must be set");
            }

            try
            {
                await _tableClaimsClient.AddEntityAsync(claim);
            }
            catch (RequestFailedException ex)
            {
                throw new ArgumentException("Error adding entity to Azure Table Storage", ex);
            }
        }

        public async Task DeleteClaimAsync(string partitionKey, string rowKey)
        {
            await _tableClaimsClient.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task<Claims?> GetClaimAsync(string partitionKey, string rowKey)
        {
            try
            {
                var response = await _tableClaimsClient.GetEntityAsync<Claims>(partitionKey, rowKey);
                return response.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }
    }
}
