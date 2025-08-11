using AutoMapper;
using AasDemoapp.Database.Model;
using AasDemoapp.Database.Model.DTOs;
using AasDemoapp.Controllers.Model;

namespace AasDemoapp.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ProducedProduct zu ProducedProductDto
            CreateMap<ProducedProduct, ProducedProductDto>()
                .ForMember(dest => dest.ConfiguredProductName,
                        opt => opt.MapFrom(src => src.ConfiguredProduct.Name))
                .ForMember(dest => dest.Bestandteile,
                        opt => opt.MapFrom(src => src.Bestandteile));

            // ProductPart zu ProductPartDto
            CreateMap<ProductPart, ProductPartDto>();

            // ConfiguredProduct zu ConfiguredProductDto
            CreateMap<ConfiguredProduct, ConfiguredProductDto>()
                .ForMember(dest => dest.ProducedProductsCount,
                        opt => opt.MapFrom(src => src.ProducedProducts != null ? src.ProducedProducts.Count : 0))
                .ForMember(dest => dest.Bestandteile,
                        opt => opt.MapFrom(src => src.Bestandteile));

            // KatalogEintrag zu KatalogEintragDto
            CreateMap<KatalogEintrag, KatalogEintragDto>()
                .ForMember(dest => dest.InventoryStatus,
                        opt => opt.MapFrom(src => src.InventoryStatus))
                .ForMember(dest => dest.ReferencedType,
                        opt => opt.MapFrom(src => src.ReferencedType));

            // Production Request Mappings
            CreateMap<ProducedProductRequestDto, ProducedProductRequest>();
            CreateMap<ProducedProductRequest, ProducedProductRequestDto>();
            CreateMap<BestandteilRequestDto, BestandteilRequest>();
            CreateMap<BestandteilRequest, BestandteilRequestDto>();

                        // Konfigurator mappings
            CreateMap<CreateConfiguredProductDto, ConfiguredProduct>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Bestandteile, opt => opt.MapFrom(src => src.Bestandteile));
            
            CreateMap<CreateProductPartDto, ProductPart>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ConfiguredProductId, opt => opt.Ignore())
                .ForMember(dest => dest.ConfiguredProduct, opt => opt.Ignore())
                .ForMember(dest => dest.KatalogEintrag, opt => opt.Ignore());

            // Supplier mappings
            CreateMap<Supplier, SupplierDto>().ReverseMap();
            CreateMap<CreateSupplierDto, Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
            CreateMap<UpdateSupplierDto, Supplier>();

            // ProductionOrder mappings
            CreateMap<ProductionOrder, ProductionOrderDto>()
                .ForMember(dest => dest.ConfiguredProductName,
                        opt => opt.MapFrom(src => src.ConfiguredProduct.Name));
            CreateMap<CreateProductionOrderDto, ProductionOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ConfiguredProduct, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.FertigstellungsDatum, opt => opt.Ignore())
                .ForMember(dest => dest.ProduktionAbgeschlossen, opt => opt.Ignore())
                .ForMember(dest => dest.Versandt, opt => opt.Ignore())
                .ForMember(dest => dest.VersandDatum, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            // Address mappings
            CreateMap<Address, AddressDto>().ReverseMap();

            // Reverse Mappings (falls benötigt)
            CreateMap<ProducedProductDto, ProducedProduct>()
                .ForMember(dest => dest.ConfiguredProduct, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            CreateMap<ProductPartDto, ProductPart>()
                .ForMember(dest => dest.KatalogEintrag, opt => opt.Ignore())
                .ForMember(dest => dest.ConfiguredProduct, opt => opt.Ignore())
                .ForMember(dest => dest.ProducedProduct, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            CreateMap<ConfiguredProductDto, ConfiguredProduct>()
                .ForMember(dest => dest.ProducedProducts, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());

            CreateMap<KatalogEintragDto, KatalogEintrag>()
                .ForMember(dest => dest.ReferencedType, opt => opt.Ignore())
                .ForMember(dest => dest.ConfiguredProducts, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore());
        }
    }
}
