/*==============================================================================================================================
| Author        Ignia, LLC
| Client        Ignia, LLC
| Project       Sample OnTopic Site
\=============================================================================================================================*/
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using OnTopic.AspNetCore.Mvc;
using OnTopic.AspNetCore.Mvc.Host;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

/*==============================================================================================================================
| CONFIGURE SERVICES
\-----------------------------------------------------------------------------------------------------------------------------*/
var builder = WebApplication.CreateBuilder(args);

/*------------------------------------------------------------------------------------------------------------------------------
| Configure: Cookie Policy
\-----------------------------------------------------------------------------------------------------------------------------*/
builder.Services.Configure<CookiePolicyOptions>(options => {
  // This lambda determines whether user consent for non-essential cookies is needed for a given request.
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;
});

/*------------------------------------------------------------------------------------------------------------------------------
| Configure: MVC
\-----------------------------------------------------------------------------------------------------------------------------*/
builder.Services.AddControllersWithViews()

  //Add OnTopic support
  .AddTopicSupport();

/*------------------------------------------------------------------------------------------------------------------------------
| Register: Activators
\-----------------------------------------------------------------------------------------------------------------------------*/
var activator = new SampleActivator(builder.Configuration.GetConnectionString("OnTopic"));

builder.Services.AddSingleton<IControllerActivator>(activator);
builder.Services.AddSingleton<IViewComponentActivator>(activator);

/*==============================================================================================================================
| CONFIGURE APPLICATION
\-----------------------------------------------------------------------------------------------------------------------------*/
var app = builder.Build();

/*------------------------------------------------------------------------------------------------------------------------------
| Configure: Error Pages
\-----------------------------------------------------------------------------------------------------------------------------*/
if (app.Environment.IsDevelopment()) {
  app.UseDeveloperExceptionPage();
}
else {
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

/*------------------------------------------------------------------------------------------------------------------------------
| Configure: Server defaults
\-----------------------------------------------------------------------------------------------------------------------------*/
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();
app.UseCors("default");

/*------------------------------------------------------------------------------------------------------------------------------
| Configure: MVC
\-----------------------------------------------------------------------------------------------------------------------------*/
app.MapTopicRoute("Web");
app.MapTopicSitemap();
app.MapTopicRedirect();
app.MapControllers();

#pragma warning restore CA1812 // Avoid uninstantiated internal classes