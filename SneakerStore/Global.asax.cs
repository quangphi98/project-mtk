using SneakerStore.Service.Send_Email;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Services.Description;
using Unity;
using Unity.Mvc5;


namespace SneakerStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // Tạo và cấu hình Unity Container
            var container = new UnityContainer();
            container.RegisterType<IEmail, Email>();

            // Thiết lập DependencyResolver để sử dụng Unity Container
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
