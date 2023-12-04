﻿using Bit.TemplatePlayground.Server.Models.Categories;
using Bit.TemplatePlayground.Shared.Dtos.Categories;
using Riok.Mapperly.Abstractions;

namespace Bit.TemplatePlayground.Server.Mappers;

/// <summary>
/// More info at Server/Mappers/README.md
/// </summary>
[Mapper(UseDeepCloning = true)]
public static partial class CategoriesMapper
{
    public static partial IQueryable<CategoryDto> Project(this IQueryable<Category> query);
    public static partial CategoryDto Map(this Category source);
    public static partial Category Map(this CategoryDto source);
    public static partial void Patch(this CategoryDto source, Category destination);
    public static partial void Patch(this Category source, CategoryDto destination);
}
