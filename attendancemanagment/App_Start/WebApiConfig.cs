
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Owin.Security.OAuth;



namespace attendancemanagment
{
  public static class WebApiConfig
  {
    public static void Register(HttpConfiguration config)
    {

      //Enable Cors Global
      var enableCorsAttribute = new EnableCorsAttribute("*", "*", "*");
      config.EnableCors(enableCorsAttribute);


      // Web API configuration and services
      // Configure Web API to use only bearer token authentication.
      config.SuppressDefaultHostAuthentication();
      config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

      // Web API routes
      config.MapHttpAttributeRoutes();

      config.Routes.MapHttpRoute(
          name: "DefaultApi",
          routeTemplate: "api/{controller}/{id}",
          defaults: new { id = RouteParameter.Optional }
      );
    }
  }
}
