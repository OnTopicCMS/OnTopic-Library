/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Integration Tests Host
\=============================================================================================================================*/

namespace OnTopic.AspNetCore.Mvc.IntegrationTests.Host {

  /*============================================================================================================================
  | CLASS: PROGRAM
  \---------------------------------------------------------------------------------------------------------------------------*/
  /// <summary>
  ///   The <see cref="Program"/> class�and it's <see cref="Program.Main(String[])"/> method�represent the entry point into the
  ///   ASP.NET Core web application.
  /// </summary>
  [ExcludeFromCodeCoverage]
  public static class Program {

    /*==========================================================================================================================
    | METHOD: MAIN
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Responsible for bootstrapping the web application.
    /// </summary>
    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

    /*==========================================================================================================================
    | METHOD: CREATE WEB HOST BUILDER
    \-------------------------------------------------------------------------------------------------------------------------*/
    /// <summary>
    ///   Configures a new <see cref="IWebHostBuilder"/> with the default options.
    /// </summary>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder => {
          webBuilder.UseStartup<Startup>();
        });

  } //Class
} //Namespace