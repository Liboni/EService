using System;
using System.Collections.Generic;
using System.Linq;

namespace SamhashoService
{
    using SamhashoService.Model;

    public class CatergoryService : ICatergoryService
    {
        public ActionResult AddCatergory(string catergoryName)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    Catergory catergory = new Catergory
                    {
                        IsDeleted = false,
                        Name = catergoryName,
                        DateCreated = DateTime.Now,
                        DateModified = DateTime.Now
                    };
                    entities.Catergories.Add(catergory);
                    entities.SaveChanges();
                    return new ActionResult { Success = true, Message = catergory.Id.ToString() };
                }
             
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"catergoryName",catergoryName}

                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Catergory);
                return new ActionResult { Message = "Error, catergory failed to save, try again." };
            }
        }

        public List<CatergoryResponse> GetAllCatergories()
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    List<CatergoryResponse> responses = entities.Catergories.Where(a=>!a.IsDeleted).OrderBy(a=>a.Name).Select(a => new CatergoryResponse
                    {
                        Name = a.Name,
                        Id = a.Id
                    }).ToList();
                    return responses;
                }
            }
            catch (Exception exception)
            {
                ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Catergory);
                return new List<CatergoryResponse>();
            }
        }

        public ActionResult DeleteCatergory(string productId)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    if (!long.TryParse(productId, out long id))
                    {
                        return new ActionResult { Message = "Catergory is invalid." };
                    }
                    Catergory catergory = entities.Catergories.FirstOrDefault(a => a.Id == id);
                    if (catergory == null) return new ActionResult { Message = "Catergory does not exist." };
                    catergory.IsDeleted = true;
                    entities.SaveChanges();
                    return new ActionResult { Success = true, Message = "Catergory saved successfully." };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"productId",productId}

                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Catergory);
                return new ActionResult { Message = "Error, catergory failed to delete, try again." };
            }
        }
    }
}
