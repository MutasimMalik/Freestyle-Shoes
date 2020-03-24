using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShoesOnContainers.Services.ProductCatalogApi.Data;
using ShoesOnContainers.Services.ProductCatalogApi;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using ShoesOnContainers.Services.ProductCatalogApi.ViewModel;
using ShoesOnContainers.Services.ProductCatalogApi.Domain;

namespace ProductCatalogApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Catalog")]
    public class CatalogController : Controller
    {

        private readonly CatalogDbContext _catalogContext;
        private readonly CatalogSettings _settings;


        public CatalogController(CatalogDbContext context, IOptionsSnapshot<CatalogSettings> settings)
        {
            _catalogContext = context ?? throw new ArgumentNullException(nameof(context));
            _settings = settings.Value;
            ((DbContext)context).ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        // GET api/[controller]/items[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Items([FromQuery]int pageSize = 6, [FromQuery]int pageIndex = 0)

        {
            var totalItems = await _catalogContext.CatalogItems
                .LongCountAsync();

            var itemsOnPage = await _catalogContext.CatalogItems
                .OrderBy(c => c.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        // GET api/[controller]/items/withname/samplename[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("[action]/withname/{name:minlength(1)}")]
        public async Task<IActionResult> Items(string name, [FromQuery]int pageSize = 6, [FromQuery]int pageIndex = 0)
        {

            var totalItems = await _catalogContext.CatalogItems
                .Where(c => c.Name.StartsWith(name))
                .LongCountAsync();

            var itemsOnPage = await _catalogContext.CatalogItems
                .Where(c => c.Name.StartsWith(name))
                .OrderBy(c=>c.Name)
                .Skip(pageSize * pageIndex)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }

        // GET api/[controller]/items/type/1/brand/null[?pageSize=3&pageIndex=10]
        [HttpGet]
        [Route("[action]/type/{catalogTypeId}/brand/{catalogBrandId}")]
        public async Task<IActionResult> Items(int? catalogTypeId, int? catalogBrandId, [FromQuery]int pageSize = 6, [FromQuery]int pageIndex = 0)
        {
            var root = (IQueryable<CatalogItem>)_catalogContext.CatalogItems;

            if (catalogTypeId.HasValue)
            {
                root = root.Where(ci => ci.CatalogTypeId == catalogTypeId);
            }

            if (catalogBrandId.HasValue)
            {
                root = root.Where(ci => ci.CatalogBrandId == catalogBrandId);
            }

            var totalItems = await root
                .LongCountAsync();

            var itemsOnPage = await root
                .Skip(pageSize * pageIndex)
                .OrderBy(c=>c.Name)
                .Take(pageSize)
                .ToListAsync();

            itemsOnPage = ChangeUriPlaceholder(itemsOnPage);

            var model = new PaginatedItemsViewModel<CatalogItem>(
                pageIndex, pageSize, totalItems, itemsOnPage);

            return Ok(model);
        }






        [HttpGet]
        [Route("items/{id:int}")]
        public async Task<IActionResult> GetItemById(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var item = await _catalogContext.CatalogItems.SingleOrDefaultAsync(ci => ci.Id == id);
            if (item != null)
            {
                return Ok(item);
            }

            return NotFound();
        }

     
        // GET api/[controller]/CatalogTypes
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CatalogTypes()
        {
            var items = await _catalogContext.CatalogTypes
                .ToListAsync();

            return Ok(items);
        }

        // GET api/[controller]/CatalogBrands
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> CatalogBrands()
        {
            var items = await _catalogContext.CatalogBrands
                .ToListAsync();

            return Ok(items);
        }

        //PUT api/v1/[controller]/items
        [Route("items")]
        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody]CatalogItem productToUpdate)
        {
            var catalogItem = await _catalogContext.CatalogItems
                .SingleOrDefaultAsync(i => i.Id == productToUpdate.Id);

            if (catalogItem == null)
            {
                return NotFound(new { Message = $"Item with id {productToUpdate.Id} not found." });
            }

            //var oldPrice = catalogItem.Price;
            //var raiseProductPriceChangedEvent = oldPrice != productToUpdate.Price;


            // Update current product
            catalogItem = productToUpdate;
            _catalogContext.CatalogItems.Update(catalogItem);


            await _catalogContext.SaveChangesAsync();


            return CreatedAtAction(nameof(GetItemById), new { id = productToUpdate.Id }, null);
        }

        //POST api/[controller]/items
        [Route("items")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody]CatalogItem product)
        {
            var item = new CatalogItem
            {
                CatalogBrandId = product.CatalogBrandId,
                CatalogTypeId = product.CatalogTypeId,
                Description = product.Description,
                Name = product.Name,
                PictureFileName = product.PictureFileName,
                Price = product.Price
            };
            _catalogContext.CatalogItems.Add(item);

            await _catalogContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItemById), new { id = item.Id }, null);
        }

        //DELETE api/[controller]/id
        [Route("{id}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = _catalogContext.CatalogItems.SingleOrDefault(x => x.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _catalogContext.CatalogItems.Remove(product);

            await _catalogContext.SaveChangesAsync();

            return NoContent();
        }





        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
        {
            var baseUri = _settings.ExternalCatalogBaseUrl;

            items.ForEach(x =>
            {
                x.PictureUri = x.PictureUri.Replace("http://externalcatalogbaseurltobereplaced", baseUri);
            });
            return items;
        }
    }
}