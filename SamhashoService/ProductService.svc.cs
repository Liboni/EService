
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using HttpMultipartParser;
    using SamhashoService.Extensions;
    using SamhashoService.Model;

    public class ProductService : IProductService
    {
        public CreateProductResponse CreateProduct(Stream data)
        {
            MultipartFormDataParser dataParser = new MultipartFormDataParser(data);
            string name = dataParser.GetParameterValue("name");
            string description = dataParser.GetParameterValue("description");
            string catergoryId = dataParser.GetParameterValue("catergory");
            string price = dataParser.GetParameterValue("price");
            string per = dataParser.GetParameterValue("per");
            string shortDescription = dataParser.GetParameterValue("shortDescription");
            string manufacturer = dataParser.GetParameterValue("manufacturer");
            string code = dataParser.GetParameterValue("code");
            string userId = dataParser.GetParameterValue("userId");
            string isMain = dataParser.GetParameterValue("IsMain");
            string isActive = dataParser.GetParameterValue("isActive");
            try
            {
                bool.TryParse(isMain, out bool checkMain);
                bool.TryParse(isActive, out bool checkActive);
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (checkActive)
                    {
                        List<Product> products = entities.Products.ToList();
                        products.ForEach(a=>a.IsActive = false);
                        entities.SaveChanges();
                    }
                    if (checkMain)
                    {
                        List<Product> products = entities.Products.Where(a=>a.IsMain).ToList();
                        if (products.Count > 3)
                        {
                            Product productMain= products.FirstOrDefault();
                            if (productMain != null) productMain.IsMain = false;
                            entities.SaveChanges();
                        }
                    }
                    Product product = new Product
                    {
                        CatergoryId = int.Parse(catergoryId),
                        Code = code,
                        CreatedDate = DateTime.Now,
                        Description = description,
                        IsDeleted = false,
                        Manufacturer = manufacturer,
                        ModifiedDate = DateTime.Now,
                        Name = name,
                        Per = per,
                        Price = decimal.Parse(price),
                        ShortDescription = shortDescription,
                        UserId = userId,
                        IsMain = checkMain,
                        IsActive = checkActive
                    };
                    entities.Products.Add(product);
                    entities.SaveChanges();
                    string path = ServiceHelper.SaveImage(dataParser.Files[0].Data, dataParser.Files[0].FileName);
                    ProductMedia productMedia = new ProductMedia
                    {
                        MediaSource = path,
                        ProductId = product.Id,
                        DateCreate = DateTime.Now,
                        IsDeleted = false,
                        IsMain = true
                    };
                    entities.ProductMedias.Add(productMedia);
                    entities.SaveChanges();
                    Catergory catergory= entities.Catergories.FirstOrDefault(a => a.Id == product.CatergoryId);
                    return new CreateProductResponse
                               {
                                   DateCreated = product.CreatedDate.ToLongDateString()+" "+product.CreatedDate.ToLongTimeString(),
                                   ProductId = product.Id ,
                                    MediaSource = path,
                                    Catergory = catergory==null? "":catergory.Name
                               };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"name",name},
                                                                {"description",description},
                                                                {"catergoryId",catergoryId},
                                                                {"price",price},
                                                                {"per",per},
                                                                {"shortDescription",shortDescription},
                                                                {"manufacturer",manufacturer},
                                                                {"code",code},
                                                                {"userId",userId},
                                                                {"mainImage",true.ToString()},
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new CreateProductResponse();
            }
        }
        
        public List<ProductResponse> SearchProduct(SearchProductRequest searchProductRequest)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    List<ProductResponse> productResponses = entities.SearchProducts(searchProductRequest.Search, searchProductRequest.Catergory)
                                                                                .Select(a => new ProductResponse
                                                                                {
                                                                                    IsDeleted = a.IsDeleted,
                                                                                    CatergoryId = a.CatergoryId,
                                                                                    Description = a.Description,
                                                                                    Code = a.Code,
                                                                                    ModifiedDate = a.ModifiedDate.ToLongDateString(),
                                                                                    Name = a.Name,
                                                                                    Manufacturer = a.Manufacturer,
                                                                                    UserId = a.UserId,
                                                                                    MediaSource = a.MediaSource,
                                                                                    ProductId = a.ProductId,
                                                                                    Price = a.Price,
                                                                                    Per = a.Per,
                                                                                    ShortDescription = a.ShortDescription,
                                                                                    CreatedDate = a.CreatedDate.ToLongDateString(),
                                                                                    IsMain = a.IsMain,
                                                                                    IsActive = a.IsActive
                                                                                }).ToList();
                    return productResponses;
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = searchProductRequest.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new List<ProductResponse>();
            }
        }

        public ActionResult DeleteProduct(string productId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (!long.TryParse(productId, out long id)) return new ActionResult { Message = "Deleted failed" };
                    entities.DeleteProduct(id);
                    return new ActionResult { Message = "Deleted successfully", Success = true };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"productId",productId}
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new ActionResult { Message = "Error, failed to delete product, try again." };
            }
        }

        public ActionResult DeleteProductMedia(string mediaId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (!long.TryParse(mediaId, out long id)) return new ActionResult { Message = "Deleted failed" };
                    entities.DeleteProductMedia(id);
                    return new ActionResult { Message = "Deleted successfully", Success = true };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"mediaId",mediaId}
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new ActionResult { Message = "Error, failed to delete product media, try again." };
            }
        }

        public ProductMediaResponse AddProductMedia(Stream media)
        {
            MultipartFormDataParser dataParser = new MultipartFormDataParser(media);
            string productId = dataParser.GetParameterValue("productId");
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                        string path = ServiceHelper.SaveImage(dataParser.Files[0].Data, dataParser.Files[0].FileName);
                        if (long.TryParse(productId, out long id))
                        {
                            ProductMedia productMedia = new ProductMedia
                            {
                                MediaSource = path,
                                ProductId = id,
                                DateCreate = DateTime.Now,
                                IsDeleted = false,
                                IsMain = false
                            };
                            entities.ProductMedias.Add(productMedia);
                            entities.SaveChanges();
                            return new ProductMediaResponse { MediaSource = path, Id = productMedia.Id};
                      }
                    return new ProductMediaResponse();
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"productId",productId}
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new ProductMediaResponse ();
            }
        }

        public List<ProductResponse> GetAllProducts()
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    List<ProductResponse> productResponses = entities.GetAllProducts().Select(a => new ProductResponse
                    {
                        IsDeleted = a.IsDeleted,
                        Description = a.Description,
                        Code = a.Code,
                        ModifiedDate = a.ModifiedDate.ToLongDateString(),
                        Name = a.Name,
                        Manufacturer = a.Manufacturer,
                        UserId = a.UserId,
                        MediaSource = a.MediaSource,
                        ProductId = a.ProductId,
                        Price = a.Price,
                        Per = a.Per,
                        ShortDescription = a.ShortDescription,
                        CreatedDate = a.CreatedDate.ToLongDateString()+" " + a.CreatedDate.ToLongTimeString(),
                        CatergoryId = a.CatergoryId,
                        Catergory = a.Catergory,
                        IsMain = a.IsMain,
                        IsActive = a.IsActive
                    }).ToList();
                    return productResponses;
                }
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Product);
                return new List<ProductResponse>();
            }
        }

        public ProductResponse GetProduct(string productId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (long.TryParse(productId, out long id))
                    {
                        ProductResponse productResponses = entities.GetProduct(id)
                            .Select(
                                a => new ProductResponse
                                         {
                                             IsDeleted = a.IsDeleted,
                                             Description = a.Description,
                                             Code = a.Code,
                                             ModifiedDate = a.ModifiedDate.ToLongDateString(),
                                             Name = a.Name,
                                             Manufacturer = a.Manufacturer,
                                             UserId = a.UserId,
                                             MediaSource = a.MediaSource,
                                             ProductId = a.ProductId,
                                             Price = a.Price,
                                             Per = a.Per,
                                             ShortDescription = a.ShortDescription,
                                             CreatedDate = a.CreatedDate.ToLongDateString() + " " + a.CreatedDate.ToLongTimeString(),
                                             CatergoryId = a.CatergoryId,
                                             Catergory = a.Catergory,
                                             IsMain = a.IsMain,
                                             IsActive = a.IsActive
                                })
                            .FirstOrDefault();
                        return productResponses;
                    }
                    return new ProductResponse();
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"productId",productId}
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new ProductResponse();
            }
        }

        public List<ProductMediaResponse> GetProductMedia(string productId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (long.TryParse(productId, out long id))
                    {
                        List<ProductMediaResponse> productResponses = entities.ProductMedias.Where(a=>a.ProductId==id && !a.IsDeleted)
                            .Select(a => new ProductMediaResponse{
                                            MediaSource = a.MediaSource,
                                            Id = a.Id
                                         }).ToList();
                        return productResponses;
                    }
                    return new List<ProductMediaResponse>();
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"productId",productId}
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new List<ProductMediaResponse>();
            }
        }

        public ActionResult UpdateProduct(ProductRequest product)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (product.IsActive)
                    {
                        List<Product> products = entities.Products.ToList();
                        products.ForEach(a => a.IsActive = false);
                        entities.SaveChanges();
                    }
                    if (product.IsMain)
                    {
                        List<Product> products = entities.Products.Where(a => a.IsMain).ToList();
                        if (products.Count > 3)
                        {
                            Product productMain = products.FirstOrDefault();
                            if (productMain != null) productMain.IsMain = false;
                            entities.SaveChanges();
                        }
                    }
                  Product updateProduct=  entities.Products.FirstOrDefault(a => a.Id == product.ProductId);
                    if (updateProduct != null) {
                        updateProduct.Description = product.Description;
                        updateProduct.ShortDescription = product.ShortDescription;
                        updateProduct.CatergoryId = product.Catergory;
                        updateProduct.Price = product.Price;
                        updateProduct.Per = product.Per;
                        updateProduct.Manufacturer = product.Manufacturer;
                        updateProduct.IsActive = product.IsActive;
                        updateProduct.IsMain = product.IsMain;
                        updateProduct.Name = product.Name;
                    }
                    entities.SaveChanges();
                    return new ActionResult { Message = "Product updated successfully.", Success = true };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = product.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new ActionResult { Message = "Error, failed to update product, try again." };
            }
        }

        public string CreateGoogleDriveFolder(string folderName)
        {
            return ServiceHelper.CreateFolder(folderName);
        }

        public ActionResult AddProductViewer(Viewer request)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    DateTime dateTime = DateTime.Now.AddDays(-1);
                    ProductViewer viewer = entities.ProductViewers.FirstOrDefault(
                        a => a.ProductId == request.DataId && a.IpAddress.Equals(request.IpAddress) && a.DateCreated >= dateTime &&a.DateCreated<=DateTime.Now);
                    if (viewer != null)
                    {
                        return new ActionResult
                                   {
                                       Success = true,
                                       Message = ""
                                   };
                    }
                    ProductViewer productViewer = new ProductViewer
                    {
                                                    DateCreated = DateTime.Now,
                                                    ProductId = request.DataId,
                                                    Country = request.Country,
                                                    IpAddress = request.IpAddress,
                                                    Town = request.Town
                                                };
                    entities.ProductViewers.Add(productViewer);
                    entities.SaveChanges();
                    return new ActionResult
                               {
                                   Success = true,
                                   Message = ""
                               };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = request.ToDictionary();
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Product);
                return new ActionResult { Message = "Error, product failed to confirm viewed." };
            }
        }

        public List<ViewerResponse> GetProductViews()
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                   List<ProductViewer> productViews = entities.ProductViewers.ToList();
                   return (from productViewer in productViews let product = entities.Products.FirstOrDefault(a => a.Id == productViewer.ProductId) where product != null select new ViewerResponse { ViewId = productViewer.Id, Country = productViewer.Country, Name = product.Name, IpAddress = productViewer.IpAddress, Town = productViewer.Town, DateCreated = productViewer.DateCreated }).ToList();
                }
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Product);
                return new List<ViewerResponse> { new ViewerResponse() };
            }
        }
    }
}
