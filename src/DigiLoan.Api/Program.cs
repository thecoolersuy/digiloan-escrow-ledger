using System.Text;
using DigiLoan.Application.Common.Interfaces;
using DigiLoan.Infrastructure.Identity;
using DigiLoan.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using DigiLoan.Api.Extensions;
using DigiLoan.Infrastructure.Repositories;
using DigiLoan.Domain.Entities;
using DigiLoan.Application.Services;
using DigiLoan.Api.Middleware;




var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token below(no need to type Bearer)"
    });

    options.AddSecurityRequirement(document =>
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });

});
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    //password rules, making them explicit rather than trusting to the identity's defaults
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;

    ////prevents two users registering for the same email
    options.User.RequireUniqueEmail = true;

    ///lockout time after repeated failed ateempts, brute force protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidIssuer = builder.Configuration["Jwt:Issuer"],

         ValidateAudience = true,
         ValidAudience = builder.Configuration["Jwt:Audience"],

         ValidateIssuerSigningKey = true,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),

         ValidateLifetime = true,
         ClockSkew = TimeSpan.Zero

     };
 });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<ILedgerEntryRepository, LedgerEntryRepository>();
builder.Services.AddScoped<ILoanApplicationRepository, LoanApplicationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDataSeederService, DataSeederService>();
builder.Services.AddScoped<ICreditScoringService, CreditScoringService>();
builder.Services.AddScoped<ILoanDisbursementService, LoanDisbursementService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();



builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});



var app = builder.Build();
app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var systemAccounts = new[]
    {
        DigiLoan.Domain.Common.SystemAccounts.Employer,
        DigiLoan.Domain.Common.SystemAccounts.MerchantPool,
        DigiLoan.Domain.Common.SystemAccounts.UtilityProvider,
        DigiLoan.Domain.Common.SystemAccounts.EscrowAccount,
        DigiLoan.Domain.Common.SystemAccounts.OpeningBalance
    };

    foreach (var id in systemAccounts)
    {
        var exists = await db.UserAccounts.AnyAsync(e => e.Id == id);
        if (!exists)
        {
            db.UserAccounts.Add(new UserAccount
            {
                Id = id,
                UserId = Guid.Empty,
                AccountNumber = $"SYS-{id.ToString()[^4..]}",
                CurrentBalance = 0
            });
        }
    }
    await db.SaveChangesAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.Run();
