/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Integration Tests Host
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace OnTopic.AspNetCore.Mvc.IntegrationTests.Host {

  /*============================================================================================================================
  | CLASS: STARTUP
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   Configures the application and sets up dependencies.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public class Startup {

    /*==========================================================================================================================
    | CONSTRUCTOR
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Constructs a new instances of the <see cref="Startup"/> class. Accepts an <see cref="IConfiguration"/>.
    /// </summary>
    /// <param name="configuration">
    ///   The shared <see cref="IConfiguration"/> dependency.
    /// </param>
    public Startup(IConfiguration configuration) {
      Configuration = configuration;
    }

    /*==========================================================================================================================
    | PROPERTY: CONFIGURATION
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides a (public) reference to the application's <see cref="IConfiguration"/> service.
    /// </summary>
    public IConfiguration Configuration { get; }

    /*==========================================================================================================================
    | METHOD: CONFIGURE SERVICES
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides configuration of services. This method is called by the runtime to bootstrap the server configuration.
    /// </summary>
    public void ConfigureServices(IServiceCollection services) {

      /*------------------------------------------------------------------------------------------------------------------------------
      | Configure: Output Caching
      \-----------------------------------------------------------------------------------------------------------------------------*/
      services.AddResponseCaching();

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure: MVC
      \-----------------------------------------------------------------------------------------------------------------------*/
      services.AddControllersWithViews()

        //Add OnTopic support
        .AddTopicSupport();

      /*------------------------------------------------------------------------------------------------------------------------
      | Register: Activators
      \-----------------------------------------------------------------------------------------------------------------------*/
      var activator = new SampleActivator();

      services.AddSingleton<IControllerActivator>(activator);
      services.AddSingleton<IViewComponentActivator>(activator);

    }

    /*==========================================================================================================================
    | METHOD: CONFIGURE (APPLICATION)
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Provides configuration the application. This method is called by the runtime to bootstrap the application
    ///   configuration, including the HTTP pipeline.
    /// </summary>
    public static void Configure(IApplicationBuilder app) {

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure: Error Pages
      \-----------------------------------------------------------------------------------------------------------------------*/
      app.UseDeveloperExceptionPage();
      app.UseStatusCodePagesWithReExecute("/Error/{0}/");

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure: Server defaults
      \-----------------------------------------------------------------------------------------------------------------------*/
      app.UseStaticFiles();
      app.UseRouting();
      app.UseResponseCaching();

      /*------------------------------------------------------------------------------------------------------------------------
      | Configure: MVC
      \-----------------------------------------------------------------------------------------------------------------------*/
      app.UseEndpoints(endpoints => {
        endpoints.MapTopicErrors(includeStaticFiles: false);
        endpoints.MapDefaultAreaControllerRoute();
        endpoints.MapDefaultControllerRoute();
        endpoints.MapImplicitAreaControllerRoute();
        endpoints.MapTopicAreaRoute();
        endpoints.MapTopicRoute("Web");
        endpoints.MapTopicRoute("Error");
        endpoints.MapTopicSitemap();
        endpoints.MapTopicRedirect();
        endpoints.MapControllers();
      });

    }

  } //Class
} //Namespace