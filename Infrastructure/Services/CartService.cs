using Core.Entities;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Services
{
    public class CartService(IConnectionMultiplexer redis) : ICartService
    {
        private readonly IDatabase _redisdatabase = redis.GetDatabase();

        public async Task<bool> DeleteCartAsync(string cartId)
        {
            return await _redisdatabase.KeyDeleteAsync(cartId);
        }

        public async Task<ShoppingCart?> GetCartAsync(string cartId)
        {
            var data = await _redisdatabase.StringGetAsync(cartId);

            return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize < ShoppingCart>(data);
        }

        public async Task<ShoppingCart?> SetCartAsync(ShoppingCart cart)
        {
            var created = await _redisdatabase.StringSetAsync(cart.Id, JsonSerializer.Serialize(cart), TimeSpan.FromDays(30));

            if (!created) return null; 

            return await GetCartAsync(cart.Id);
        }
    }
}
