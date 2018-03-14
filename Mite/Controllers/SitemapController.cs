using Mite.BLL.IdentityManagers;
using Mite.Core;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using Mite.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Mite.Controllers
{
    public class SitemapController : BaseController
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly AppDbContext dbContext;

        public SitemapController(IUnitOfWork unitOfWork, AppDbContext dbContext)
        {
            this.unitOfWork = unitOfWork;
            this.dbContext = dbContext;
        }
        public async Task<ContentResult> Index()
        {
            var sitemap = await GetSitemapDoc();
            return Content(sitemap, "text/xml", Encoding.UTF8);
        }
        private async Task<string> GetSitemapDoc()
        {
            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var root = new XElement(xmlns + "urlset");

            var nodes = await GetNodes();

            foreach(var node in nodes)
            {
                var urlElement = new XElement(
                    xmlns + "url",
                    new XElement(xmlns + "loc", Uri.EscapeUriString(node.Url)),
                    node.LastModified == null ? null : new XElement(
                        xmlns + "lastmod",
                        node.LastModified.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:sszzz")),
                    node.Frequency == null ? null : new XElement(
                        xmlns + "changefreq",
                        node.Frequency),
                    node.Priority == null ? null : new XElement(
                        xmlns + "priority",
                    node.Priority.Value.ToString("F1", CultureInfo.InvariantCulture)));
                root.Add(urlElement);
            }

            var doc = new XDocument(root);
            return doc.ToString();
        }
        private async Task<IEnumerable<SitemapNode>> GetNodes()
        {
            var postsRepo = unitOfWork.GetRepo<PostsRepository, Post>();
            var servicesRepo = unitOfWork.GetRepo<AuthorServiceRepository, AuthorService>();

            var nodes = new List<SitemapNode>
            {
                new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "Home", action = "Index" })
                },
                new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "Home", action = "Privacy" })
                },
                new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "Home", action = "Help" })
                },
                new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "Home", action = "Donate" })
                },
                new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "Posts", action = "Top" })
                }
            };

            var users = await dbContext.Users.ToListAsync();
            var posts = await postsRepo.GetAllAsync();
            var authorServices = await servicesRepo.GetAllAsync();

            foreach(var user in users)
            {
                nodes.Add(new SitemapNode
                {
                    Url = AbsoluteRouteUrl("UserProfile", new { name = user.UserName.ToLower() })
                });
            }
            foreach(var post in posts)
            {
                nodes.Add(new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "Posts", action = "ShowPost", id = post.Id.ToString("N") }),
                    LastModified = post.LastEdit
                });
            }
            foreach(var service in authorServices)
            {
                nodes.Add(new SitemapNode
                {
                    Url = AbsoluteRouteUrl("Default", new { controller = "AuthorServices", action = "Show", id = service.Id.ToString() }),
                    Frequency = "weekly"
                });
            }
            return nodes;
        }
        private string AbsoluteRouteUrl(string routeName, object routeValues = null)
        {
            string scheme = Url.RequestContext.HttpContext.Request.Url.Scheme;
            return Url.RouteUrl(routeName, routeValues, "https");
        }
    }
    
}