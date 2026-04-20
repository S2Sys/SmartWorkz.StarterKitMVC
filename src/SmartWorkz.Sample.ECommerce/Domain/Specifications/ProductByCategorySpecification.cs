using SmartWorkz.Core.Shared.Specifications;
using SmartWorkz.Sample.ECommerce.Domain.Entities;

namespace SmartWorkz.Sample.ECommerce.Domain.Specifications;

public class ProductByCategorySpecification : Specification<Product>
{
    public ProductByCategorySpecification(int categoryId)
    {
        AddCriteria(p => p.CategoryId == categoryId && p.IsActive);
    }
}
