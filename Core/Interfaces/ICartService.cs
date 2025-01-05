using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface ICartService
    {

        public Task<bool> DeleteCartAsync(string cartId);

        public Task<ShoppingCart?> GetCartAsync(string cartId);

        public Task<ShoppingCart?> SetCartAsync(ShoppingCart cart);

    }
}
