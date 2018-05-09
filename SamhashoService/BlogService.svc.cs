
namespace SamhashoService
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using HttpMultipartParser;

    using SamhashoService.Extensions;
    using SamhashoService.Model;

    public class BlogService : IBlogService
    {
        public BlogResponse AddBlog(Stream blogData)
        {
            MultipartFormDataParser dataParser = new MultipartFormDataParser(blogData);
            try
            {
                string name = dataParser.GetParameterValue("name");
                string description = dataParser.GetParameterValue("description");
                string titleDescription = dataParser.GetParameterValue("titleDescription");
                FilePart dataParserFile = dataParser.Files[0];
                string path = ServiceHelper.SaveImage(dataParserFile.Data, dataParserFile.FileName);
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    Blog blog = new Blog
                    {
                        Name = name,
                        IsDeleted = false,
                        Description = description,
                        DateCreated = DateTime.Now,
                        MediaUrl = path,
                        TitleDescription = titleDescription
                    };
                    entities.Blogs.Add(blog);
                    entities.SaveChanges();
                    return new BlogResponse
                               {
                                   Name = blog.Name,
                                   Description = blog.Description,
                                   TitleDescription = blog.TitleDescription,
                                   Id = blog.Id,
                                   DateCreated = blog.DateCreated.ToLongDateString()+" "+ blog.DateCreated.ToLongTimeString(),
                                   MediaUrl = blog.MediaUrl
                               };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string,string> dictionary = new Dictionary<string, string>
                                                           {
                    {"name",dataParser.GetParameterValue("name")},
                    {"description", dataParser.GetParameterValue("description")},
                    {"titleDescription", dataParser.GetParameterValue("titleDescription")}
                                                           };
                ServiceHelper.LogException(exception, dictionary,ErrorSource.Blog);
                return new BlogResponse ();
            }
        }

        public ActionResult DeleteBlog(string blogId)
        {
            try
            {
                if (!int.TryParse(blogId, out int id)) return new ActionResult { Message = "Blog is invalid" };
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    Blog blog = entities.Blogs.FirstOrDefault(a => a.Id == id);
                    if (blog == null) return new ActionResult { Message = "Blog does not exist" };
                    blog.IsDeleted = true;
                    entities.SaveChanges();
                    return new ActionResult { Success = true, Message = "Blog deleted successfully" };
                }
            }
            catch (Exception exception)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>
                                                            {
                                                                {"blogId",blogId}
                                                               
                                                            };
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Blog);
                return new ActionResult { Message = "Error, blog failed to delete, try again." };
            }
        }

        public List<BlogResponse> GetBlogs()
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    List<Blog> responses = entities.Blogs.Where(a => !a.IsDeleted).OrderByDescending(a=>a.DateCreated).ToList();
                    List<BlogResponse> blogs =  responses.Select(a => new BlogResponse
                      {
                        Name = a.Name,
                        DateCreated = a.DateCreated.ToLongDateString() + " " + a.DateCreated.ToLongTimeString(),
                        Description = a.Description,
                        Id = a.Id,
                        TitleDescription = a.TitleDescription,
                        MediaUrl = a.MediaUrl
                    }).ToList();
                    return blogs;
                }
            }
            catch (Exception exception)
            {
               ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Blog);
                return new List<BlogResponse>();
            }
        }

        public List<ViewerResponse> GetBlogViews()
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    List<BlogViewer> blogViews = entities.BlogViewers.ToList();
                    return (from blogViewer in blogViews let blog = entities.Blogs.FirstOrDefault(a => a.Id == blogViewer.BlogId) where blog != null select new ViewerResponse { ViewId = blogViewer.Id, Country = blogViewer.Country, Name = blog.Name, IpAddress = blogViewer.IpAddress, Town = blogViewer.Town, DateCreated = blogViewer.DateCreated }).ToList();
                    
                }
            }
            catch (Exception exception)
            {
                 ServiceHelper.LogException(exception, new Dictionary<string, string>(), ErrorSource.Blog);
                return new List<ViewerResponse> {new ViewerResponse()};
            }
        }

        public ActionResult AddBlogViewer(Viewer request)
        {
            try
            {
                using (ESamhashoEntities entities = new ESamhashoEntities())
                {
                    DateTime dateTime = DateTime.Now.AddDays(-1);
                    BlogViewer viewer = entities.BlogViewers.FirstOrDefault(
                        a => a.BlogId == request.DataId && a.IpAddress.Equals(request.IpAddress) && a.DateCreated >= dateTime && a.DateCreated <= DateTime.Now);
                    if (viewer!=null)
                    {
                        return new ActionResult
                                   {
                                       Success = true,
                                       Message = ""
                                   };
                    }
                    BlogViewer blogViewer = new BlogViewer
                    {
                        DateCreated = DateTime.Now,
                        BlogId = (int)request.DataId,
                        Country = request.Country,
                        IpAddress = request.IpAddress,
                        Town = request.Town
                    };
                    entities.BlogViewers.Add(blogViewer);
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
                ServiceHelper.LogException(exception, dictionary, ErrorSource.Blog);
                return new ActionResult { Message = "Error, blog failed to confirm viewed, error." };
            }
        }

    }
}
