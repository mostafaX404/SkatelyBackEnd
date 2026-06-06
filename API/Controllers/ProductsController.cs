using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    public class ProductsController(IUnitOfWork uow) : BaseApiController
    {


        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(
            [FromQuery]ProductSpecParams specParams)

        {
            var spec = new ProductSpecifications(specParams);

            return await CreatePagedResult<Product>(uow.Repository<Product>(), spec, specParams.PageIndex, specParams.PageSize);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await uow.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


         [Authorize("Admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
           uow.Repository<Product>().Add(product);

            if(await uow.Complete())
            {
                return CreatedAtAction("GetProduct", new { id = product.Id, product });
            }
            return BadRequest("Problem in creating the product");
        }


         [Authorize("Admin")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if (product.Id != id || !ProductExists(id)) 
            { return BadRequest("Cannot update this product"); }

            uow.Repository<Product>().Update(product);

            if (await uow.Complete())
            {
                return NoContent();

            }
            return BadRequest("Problem updating the product");
        }


         [Authorize("Admin")]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await uow.Repository<Product>().GetByIdAsync(id);

            if (product == null)
            { return NotFound(); }

            uow.Repository<Product>().Delete(product);


            if (await uow.Complete())
            {
                return NoContent();

            }
            return BadRequest("Problem deleting the product");
        }

        private bool ProductExists(int id)
        {
            return uow.Repository<Product>().Exists(id);
        }


        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            var spec = new BrandListSpecification();


            return Ok(await uow.Repository<Product>().ListWithSpecAsync(spec));
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            //implement method

            var spec = new TypeListSpecification();

            return Ok(await uow.Repository<Product>().ListWithSpecAsync(spec));
        }
    }
    }

