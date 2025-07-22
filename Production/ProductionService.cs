using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AasCore.Aas3_0;
using AasDemoapp.Database;
using AasDemoapp.Database.Model;
using AasDemoapp.Import;

namespace AasDemoapp.Production
{
    public class ProductionService
    {
        private readonly AasDemoappContext _context;
        private readonly ImportService _importService;

        public ProductionService(AasDemoappContext aasDemoappContext, ImportService importService)
        {
            _context = aasDemoappContext;
            _importService = importService;
        }

        public async Task<ProducedProduct> CreateProduct(ProducedProductRequest producedProductRequest)
        {

            // Produkt zusammenbauen

            var producedProduct = new ProducedProduct()
            {
                ConfiguredProductId = producedProductRequest.ConfiguredProductId,
                ProductionDate = DateTime.Now
            };

            var configuredProduct = _context.ConfiguredProducts.First(c => c.Id == producedProductRequest.ConfiguredProductId);

            producedProductRequest.BestandteilRequests.ForEach((bestandteil) =>
            {
                var katalogEintrag = _context.KatalogEintraege.First(b => b.GlobalAssetId == bestandteil.GlobalAssetId);
                producedProduct.Bestandteile.Add(new ProductPart()
                {
                    Amount = bestandteil.Amount,
                    Name = katalogEintrag.Name,
                    Price = katalogEintrag.Price,
                    UsageDate = bestandteil.UsageDate,
                    KatalogEintrag = katalogEintrag
                });
                katalogEintrag.Amount = katalogEintrag.Amount - bestandteil.Amount;
            });

            _context.ProducedProducts.Add(producedProduct);

            // TODO: jetzt die neue Verwaltungsschale bauen
            var assetInformation = new AssetInformation(AssetKind.Instance, Guid.NewGuid().ToString(), null, configuredProduct.GlobalAssetId);
            var aas = new AssetAdministrationShell(Guid.NewGuid().ToString(), assetInformation, null, null, configuredProduct.Name);

            var nameplate = new Submodel(Guid.NewGuid().ToString(), null, null, "Nameplate", null, null, null, ModellingKind.Instance, new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAO677#002")]));
            var smeManufacturer = new MultiLanguageProperty(null, null, "ManufacturerName", null, null, new Reference(ReferenceTypes.ExternalReference, [new Key(KeyTypes.GlobalReference, "0173-1#02-AAO677#002")]))
            {
                Value = [new LangStringTextType("de", "ML DEMO App")]
            };
            nameplate.SubmodelElements = [smeManufacturer];
            aas.Submodels = [new Reference(ReferenceTypes.ModelReference, [new Key(KeyTypes.Submodel, nameplate.Id)])];

            var handoverdoc = HandoverDocumentationCreator.CreateHandoverDocumentationFromJson();
            handoverdoc.Id = Guid.NewGuid().ToString();
            handoverdoc.IdShort = "HandoverDocumentation";
            if (handoverdoc.Administration != null)
            {

                handoverdoc.Administration.Version = "1.0";
                handoverdoc.Administration.Version = "1.0";
            }
            handoverdoc.Description = [new LangStringTextType("de", "Handover documentation for the produced product")];

            aas.Submodels.Add(new Reference(ReferenceTypes.ModelReference, [new Key(KeyTypes.Submodel, handoverdoc.Id)]));

            await _importService.PushNewToLocalRepositoryAsync(aas, [nameplate, handoverdoc], "http://localhost:9421");

            producedProduct.AasId = aas.Id;
            producedProduct.GlobalAssetId = assetInformation.GlobalAssetId;

            _context.SaveChanges();
            return producedProduct;
        }

    }
}