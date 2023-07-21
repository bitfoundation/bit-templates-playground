using Bit.AdminPanel.Server.Api.Models.Categories;
using Bit.AdminPanel.Shared.Dtos.Categories;

namespace Bit.AdminPanel.Server.Api.Mappers;

public class CategoryMapperConfiguration : Profile
{
    public CategoryMapperConfiguration()
    {
        CreateMap<Category, CategoryDto>().ReverseMap();
    }
}
