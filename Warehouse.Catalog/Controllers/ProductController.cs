using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Catalog.Models;
using Warehouse.Contracts;

namespace Warehouse.Catalog.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductController: ControllerBase
{
    private readonly IPublishEndpoint _publish;
  public ProductController(IPublishEndpoint publish)
  {
    _publish=publish;
  }
    public  Task Post([FromBody]ProductCreateDto product )
    {
        _publish.Publish<ProductMessage>(new 
        {
            Code=product.Code,
            Name=product.Name
        });
        return Task.CompletedTask;
    }
}