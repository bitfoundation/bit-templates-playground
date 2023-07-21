using Bit.AdminPanel.Server.Api.Models.Products;
using Bit.AdminPanel.Shared.Dtos.Products;

namespace Bit.AdminPanel.Server.Api.Mappers;

public class ProductMapperConfiguration : Profile
{
    public ProductMapperConfiguration()
    {
        CreateMap<Product, ProductDto>()
            .ReverseMap()
            .ForMember(p => p.Category, config => config.Ignore());
    }
}
