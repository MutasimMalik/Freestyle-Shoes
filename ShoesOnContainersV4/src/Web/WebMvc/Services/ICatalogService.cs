using Microsoft.AspNetCore.Mvc.Rendering;
using ShoesOnContainers.Web.WebMvc.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoesOnContainers.Web.WebMvc.Services
{
    public interface ICatalogService
    {
        Task<Catalog> GetCatalogItems(int page, int take, int? brand, int? type);
        Task<IEnumerable<SelectListItem>> GetBrands();
        Task<IEnumerable<SelectListItem>> GetTypes();
    }
}
