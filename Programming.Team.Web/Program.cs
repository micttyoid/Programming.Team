using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Invio.Extensions.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Programming.Team.Data.Core;
using Programming.Team.Data;
using Programming.Team.Core;
using Programming.Team.Business.Core;
using Programming.Team.Business;
using Microsoft.EntityFrameworkCore;
using Programming.Team.Web.Authorization;
using MudBlazor.Services;
using Programming.Team.Web.Shared;
using Programming.Team.ViewModels.Admin;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearerQueryStringAuthentication()
    .AddMicrosoftIdentityWebApi(builder.Configuration)
                    .EnableTokenAcquisitionToCallDownstreamApi()
                        .AddSessionTokenCaches();
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration).EnableTokenAcquisitionToCallDownstreamApi();

builder.Services.AddControllersWithViews()
    .AddMicrosoftIdentityUI();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddAuthorization();
builder.Services.AddTokenAcquisition();
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddMvc().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddMudServices();
var connectionString = builder.Configuration.GetConnectionString("Resumes");
builder.Services.AddDbContext<ResumesContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
builder.Services.AddScoped<IContextFactory, ContextFactory>();
builder.Services.AddScoped<IRepository<Role, Guid>, Repository<Role, Guid>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Role, Guid>, BusinessRepositoryFacade<Role, Guid, IRepository<Role, Guid>>>();
builder.Services.AddScoped<IUserBusinessFacade, UserBusinessFacade>();
builder.Services.AddTransient<AlertView.AlertViewModel>();
builder.Services.AddTransient<AddRoleViewModel>();
builder.Services.AddTransient<ManageRolesViewModel>();
builder.Services.AddTransient<UsersViewModel>();
builder.Services.AddSession();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseMiddleware<RolePopulationMiddleware>();
app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
