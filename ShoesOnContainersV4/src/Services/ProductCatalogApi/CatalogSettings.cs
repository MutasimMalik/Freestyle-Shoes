using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoesOnContainers.Services.ProductCatalogApi
{
    public class CatalogSettings
    {

        public string PicBaseUrl { get; set; }

        public string ExternalCatalogBaseUrl { get; set; }

        // public bool UseCustomizationData { get; set; }
        public Catalog Catalog { get; set; }

    }
    public class Catalog
    {
        public string ConnectionString { get; set; }
    }
}
