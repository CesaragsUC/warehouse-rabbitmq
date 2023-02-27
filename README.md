<h1>How to use MassTransit 8.0.13 , RabbitMQ with AspNetCore </h1>

<h2>Introduction </h2>
This project is a demonstration on how to use the latest version of MassTransit(8.0.13) with RabbitMQ and AspNetCore.

In this project we will use to warehouse two simple warehouse services both crated using AspNetCore WebApi
    <li>Warehouse.Catalog Service 
          Responsible for for the CRUD operation of the Product and related items . (only Post is implmented)
    <li>Warehouse.Stock Service
           Responsible for handling stock staus, reordering level ...etc and will be used as consumer
 
 <h3>Scenario </h3>
 
 1. Catalog service receives product create request
 2. Then pushes ProductCreated event message to RabbitMQ (catalog service is expected to persist .. not implmented here)
 3. Stock service subscribes and consumes ProductCreated event message
 4. Stock service logs received data (Stock service is expected to persist .. not implmented here)
 


Steps

1.  Create solution
```cli

    dotnet new sln -o Warehouse
    cd Warehouse
    
```
2. Create Catalog project
```

    dotnet new WebApi -o Warehouse.Catalog
    dotnet new sln add Warehouse.Catalog/Warehouse.Catalog.csproj
    cd Warehouse.Catalog
    
```
3. Add required packages
```
   
    dotnet add Warehouse.Catalog.csproj package MassTransit
    dotnet add Warehouse.Catalog.csproj package MassTransit.RabbitMQ
    dotnet add Warehouse.Catalog.csproj package MassTransit.AspNetCore

```
4. Verify if all package are added , open Warehouse.Catalog.csproj

```csproj
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MassTransit" Version="8.0.13" />
    <PackageReference Include="MassTransit.AspNetCore" Version="7.3.1" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="8.0.13" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.2.3" />
  </ItemGroup>

</Project>
```
5. Create folder Models and add ProductCreateDto class and replace with the following code
```c#
   namespace Warehouse.Catalog.Models;

   public class ProductCreateDto
   {
       public string? Code { get; set; }
       public string? Name { get; set; }
   }
 
```

6. Create ConfigurationServices class at the root of the project , we will use this class to register our service extension methods
   for now we will use it to register and configure MassTransit. Replace the content with the following code
   
```c#
     using MassTransit;

     namespace Warehouse.Catalog;

     public static class ConfigurationService 
     {
         public static IServiceCollection AddServices(this IServiceCollection services)
         {
               services.AddMassTransit(x=>{

                 x.UsingRabbitMq((ctx,cfg)=>{

                   cfg.Host("localhost","/" , c=>
                   {
                       c.Username("guest");
                       c.Password("guest");
                   });

                   cfg.ConfigureEndpoints(ctx);
                 });


               });

             return services;
         }
     }
```
   AddMassTransit method will register mastransit services including all interfaces we will be using for publishing and consuming the message
   IBus,IPublishEndPoint, IConsumer ...etc.  More detail information can be found [here](https://masstransit.io/documentation/configuration)
  
  7. Open your Program.cs file and add call the service extension
  
  ```c#
     using Warehouse.Catalog;

    var builder = WebApplication.CreateBuilder(args);

    // Add services 
    builder.Services.AddServices();
    
    ----the rest of the code goes here
  ```
  8. Defining the RabbitMQ message contracts
    
			It is recommended to use interface or record while creating contracts and must be in the same namespace both for the publisher and consumer even       				though they are at diffrent projects. 
			
			Because RabbitMQ will use fully qalified name while creating exchange and queue , if there is any diffrence in the name subscribe will not receive the    message. Therfore , It is better to create the contract and share the folder by copy and paste.
		
				
		8.1 Create folder name Contracts
		8.2 Inside the folder create an interface called ProductMessage (It is recommended to name it as past tense ProductCreated ... since we have one message we keep it like this) replace the content with following 
```c#
  namespace Warehouse.Contracts;

  public interface ProductMessage
  {
      public string Code { get; set; }
      public string Name { get; set; }
  }
```
9. Create ProductController on Controllers folder and replace with the following code

```c#
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
    [HttpPost]
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
```
<li> In the constructer we will inject IPublisherEndPoint which we will be using to publish 
<li> Define ActionMethod(EndPoint) for post
<li> Publish the recived product to rabbitMQ using _publish as shown

Now we have finished our Catalog project code.
