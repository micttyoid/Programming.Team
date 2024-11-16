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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Programming.Team.ViewModels.Resume;

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
builder.Services.AddMemoryCache();
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
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRepository<Company, Guid>, Repository<Company, Guid>>();
builder.Services.AddScoped<IRepository<Reccomendation, Guid>, Repository<Reccomendation, Guid>>();
builder.Services.AddScoped<IRepository<Position, Guid>, Repository<Position, Guid>>();
builder.Services.AddScoped<IRepository<PositionSkill, Guid>, Repository<PositionSkill, Guid>>();
builder.Services.AddScoped<IRepository<Skill, Guid>, Repository<Skill, Guid>>();
builder.Services.AddScoped<IRepository<DocumentType, int>, Repository<DocumentType, int>>();
builder.Services.AddScoped<IRepository<Education, Guid>, Repository<Education, Guid>>();
builder.Services.AddScoped<IRepository<Institution, Guid>, Repository<Institution, Guid>>();
builder.Services.AddScoped<IRepository<Certificate, Guid>, Repository<Certificate, Guid>>();
builder.Services.AddScoped<IRepository<CertificateIssuer, Guid>, Repository<CertificateIssuer, Guid>>();
builder.Services.AddScoped<IRepository<Publication, Guid>, Repository<Publication, Guid>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Role, Guid>, BusinessRepositoryFacade<Role, Guid, IRepository<Role, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Company, Guid>, BusinessRepositoryFacade<Company, Guid, IRepository<Company, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Position, Guid>, BusinessRepositoryFacade<Position, Guid, IRepository<Position, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Reccomendation, Guid>, BusinessRepositoryFacade<Reccomendation, Guid, IRepository<Reccomendation, Guid>>>();
builder.Services.AddScoped<IUserBusinessFacade, UserBusinessFacade>();
builder.Services.AddScoped<IRoleBusinessFacade, RoleBusinessFacade>();
builder.Services.AddScoped<IBusinessRepositoryFacade<PositionSkill, Guid>, BusinessRepositoryFacade<PositionSkill, Guid, IRepository<PositionSkill, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Skill, Guid>, BusinessRepositoryFacade<Skill, Guid, IRepository<Skill, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Education, Guid>, BusinessRepositoryFacade<Education, Guid, IRepository<Education, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Institution, Guid>, BusinessRepositoryFacade<Institution, Guid, IRepository<Institution, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Certificate, Guid>, BusinessRepositoryFacade<Certificate, Guid, IRepository<Certificate, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<CertificateIssuer, Guid>, BusinessRepositoryFacade<CertificateIssuer, Guid, IRepository<CertificateIssuer, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<Publication, Guid>, BusinessRepositoryFacade<Publication, Guid, IRepository<Publication, Guid>>>();
builder.Services.AddScoped<IBusinessRepositoryFacade<DocumentType, int>, BusinessRepositoryFacade<DocumentType, int, IRepository<DocumentType, int>>>();
builder.Services.AddTransient<AlertView.AlertViewModel>();
builder.Services.AddTransient<AddRoleViewModel>();
builder.Services.AddTransient<ManageRolesViewModel>();
builder.Services.AddTransient<UsersViewModel>();
builder.Services.AddTransient<SelectUsersViewModel>();
builder.Services.AddTransient<RoleLoaderViewModel>();
builder.Services.AddTransient<AddCompanyViewModel>();
builder.Services.AddTransient<SearchSelectCompanyViewModel>();
builder.Services.AddTransient<AddPositionViewModel>();
builder.Services.AddTransient<PositionsViewModel>();
builder.Services.AddTransient<AddSkillViewModel>();
builder.Services.AddTransient<SearchSelectSkillViewModel>();
builder.Services.AddTransient<AddPositionSkillViewModel>();
builder.Services.AddTransient<PositionSkillsViewModel>();
builder.Services.AddTransient<AddInstitutionViewModel>();
builder.Services.AddTransient<SearchSelectInstiutionViewModel>();
builder.Services.AddTransient<AddEducationViewModel>();
builder.Services.AddTransient<EducationsViewModel>();
builder.Services.AddTransient<SearchSelectPositionViewModel>();
builder.Services.AddTransient<AddReccomendationViewModel>();
builder.Services.AddTransient<ReccomendationsViewModel>();
builder.Services.AddTransient<SearchSelectCertificateIssuerViewModel>();
builder.Services.AddTransient<AddCertificateIssuerViewModel>();
builder.Services.AddTransient<AddCertificateViewModel>();
builder.Services.AddTransient<CertificatesViewModel>();
builder.Services.AddTransient<AddPublicationViewModel>();
builder.Services.AddTransient<PublicationsViewModel>();
builder.Services.AddTransient<AddDocumentTypeViewModel>();
builder.Services.AddTransient<DocumentTypesViewModel>();
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
