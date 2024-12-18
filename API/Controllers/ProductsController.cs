using API.RequestHelpers;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    public class ProductsController(IGenericRepository<Product> _productRepo) : BaseApiController
    {


        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(
            [FromQuery]ProductSpecParams specParams)

        {
            var spec = new ProductSpecifications(specParams);

            var pagination = await CreatePagedResult<Product>(_productRepo, spec,specParams.PageIndex,specParams.PageSize);

            return Ok(pagination);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
           _productRepo.Add(product);

            if(await _productRepo.SaveAllAsync())
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

            _productRepo.Update(product);

            if (await _productRepo.SaveAllAsync())
            {
                return NoContent();

            }
            return BadRequest("Problem updating the product");
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var product = await _productRepo.GetByIdAsync(id);

            if (product == null)
            { return NotFound(); }

            _productRepo.Delete(product);


            if (await _productRepo.SaveAllAsync())
            {
                return NoContent();

            }
            return BadRequest("Problem deleting the product");
        }

        private bool ProductExists(int id)
        {
            return _productRepo.Exists(id);
        }


        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            var spec = new BrandListSpecification();


            return Ok(await _productRepo.ListWithSpecAsync(spec));
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            //implement method

            var spec = new TypeListSpecification();

            return Ok(await _productRepo.ListWithSpecAsync(spec));
        }
    }
    }

