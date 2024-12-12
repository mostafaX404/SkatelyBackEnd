using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductRepository _productRepo) : ControllerBase
    {


        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand , string? type , string? sort)
        {
           return Ok(await _productRepo.GetProductsAsync(brand , type ,sort));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productRepo.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
           _productRepo.AddProduct(product);

            if(await _productRepo.SaveChangesAsync())
            {
                return CreatedAtAction("GetProduct", new { id = product.Id, product });
            }
            return BadRequest("Problem in creating the product");
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if (product.Id != id || !ProductExists(id)) 
            { return BadRequest("Cannot update this product"); }

            _productRepo.UpdateProduct(product);

            if (await _productRepo.SaveChangesAsync())
            {
                return NoContent();

            }
            return BadRequest("Problem updating the product");
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _productRepo.GetProductByIdAsync(id);

            if (product == null)
            { return NotFound(); }

            _productRepo.DeleteProduct(product);


            if (await _productRepo.SaveChangesAsync())
            {
                return NoContent();

            }
            return BadRequest("Problem deleting the product");
        }

        private bool ProductExists(int id)
        {
            return _productRepo.ProductExists(id);
        }


        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            return Ok(await _productRepo.GetBrandsAsync());
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            return Ok(await _productRepo.GetTypesAsync());
        }
    }
}
