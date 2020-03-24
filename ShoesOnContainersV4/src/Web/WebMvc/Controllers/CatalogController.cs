using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebMvc.Models;
using ShoesOnContainers.Web.WebMvc.Services;
using ShoesOnContainers.Web.WebMvc.ViewModels;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace WebMvc.Controllers
{
    public class CatalogController : Controller
    {
        private ICatalogService _catalogSvc;

        public CatalogController(ICatalogService catalogSvc) =>
            _catalogSvc = catalogSvc;

        public async Task<IActionResult> Index(int? BrandFilterApplied, int? TypesFilterApplied, int? page)
        {
           
            int itemsPage=10 ;
            var catalog = await _catalogSvc.GetCatalogItems(page ?? 0, itemsPage, BrandFilterApplied, TypesFilterApplied);
            var vm = new CatalogIndexViewModel()
            {
                CatalogItems = catalog.Data,
                Brands = await _catalogSvc.GetBrands(),
                Types = await _catalogSvc.GetTypes(),
                BrandFilterApplied = BrandFilterApplied ?? 0,
                TypesFilterApplied = TypesFilterApplied ?? 0,
                PaginationInfo = new PaginationInfo()
                {
                    ActualPage = page ?? 0,
                    ItemsPerPage = itemsPage , //catalog.Data.Count,
                    TotalItems = catalog.Count,
                    TotalPages = (int)Math.Ceiling(((decimal)catalog.Count / itemsPage))
                }
            };

            vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
            vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";

            return View(vm);
        }
        //[Authorize]
        //public async Task<IActionResult> About()
        //{
        //    ViewData["Message"] = "Your application description page.";
        //    var user = User as ClaimsPrincipal;

        //    var token = await HttpContext.GetTokenAsync("access_token");
        //    var idToken = await HttpContext.GetTokenAsync("id_token");
        //    foreach (var claim in user.Claims)
        //    {
        //        Debug.WriteLine($"Claim Type: {claim.Type} - Claim Value : {claim.Value}");
        //    }

        //    if (token != null)
        //    {
        //        ViewData["access_token"] = token;

        //    }
        //    if (idToken != null)
        //    {

        //        ViewData["id_token"] = idToken;
        //    }

        //    return View();
        //}

        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";


            return View();
        }




        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
    }
}
