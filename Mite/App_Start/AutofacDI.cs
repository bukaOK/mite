using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataProtection;
using Mite.BLL.IdentityManagers;
using Mite.BLL.Services;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Owin;
using System.Reflection;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using Autofac;
using Autofac.Integration.WebApi;
using Autofac.Integration.Mvc;
using Mite.Infrastructure;

namespace Mite
{
    public static class AutofacDI
    {
        public static void Initialize(IAppBuilder app, HttpConfiguration apiConfiguration)
        {
            var builder = new ContainerBuilder();
            var executingAssembly = Assembly.GetExecutingAssembly();

            builder.RegisterControllers(executingAssembly);
            builder.RegisterApiControllers(executingAssembly);
            RegisterComponents(builder, app);

            var container = builder.Build();
            DependencyResolver.SetResolver(new Autofac.Integration.Mvc.AutofacDependencyResolver(container));
            apiConfiguration.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(apiConfiguration);
            app.UseAutofacMvc();
        }

        private static void RegisterComponents(ContainerBuilder builder, IAppBuilder app)
        {
            var dataProtectionProvider = app.GetDataProtectionProvider();
            builder.RegisterType<AppDbContext>().InstancePerRequest();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
            builder.Register(c => c.Resolve<IOwinContext>().Authentication).As<IAuthenticationManager>().InstancePerRequest();
            builder.Register(c => new AppUserManager(new UserStore<User>(c.Resolve<AppDbContext>()),
                dataProtectionProvider));
            builder.RegisterType<AppSignInManager>();
            builder.RegisterType<UserService>().As<IUserService>();
            builder.RegisterType<PostsService>().As<IPostsService>();
            builder.RegisterType<TagsService>().As<ITagsService>();
            builder.RegisterType<CommentsService>().As<ICommentsService>();
            builder.RegisterType<RatingService>().As<IRatingService>();
            builder.RegisterType<FollowersService>().As<IFollowersService>();
            builder.RegisterType<NotificationService>().As<INotificationService>();
            builder.RegisterType<ArticleImagesContainer>().As<IImagesContainer>().SingleInstance();
            builder.RegisterType<HelpersService>().As<IHelpersService>();
        }
    }
}