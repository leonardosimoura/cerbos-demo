using Cerbos.Sdk.Builders;
using Cerbos.Sdk;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<CerbosBlockingClient>((svc) =>
{
    return new CerbosClientBuilder("http://localhost:3593").WithPlaintext().BuildBlockingClient();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var agreements = new List<Agreement>();

app.MapGet("/agreement", () =>
{
    return agreements;
})
.WithName("GetAgreement")
.WithOpenApi();

app.MapPost("/agreement", (AgreementViewModel model, CerbosBlockingClient client) =>
{
    CheckResult result = client
    .CheckResources(
        Principal.NewInstance(model.User, model.Roles)
        .WithPolicyVersion("default")
        .WithAttribute("geography", AttributeValue.StringValue("SP")),

        Resource.NewInstance("agreement", model.Id.ToString())
            .WithPolicyVersion("default")
            .WithAttribute("owner", AttributeValue.StringValue(model.User)),

        "create"
    );

    if (!result.IsAllowed("create"))
        return Results.Unauthorized();

    agreements.Add(new Agreement(model.Id, model.Identifier, false, model.User));
    return Results.Created($"/agreement/{model.Id}", model);
})
.WithName("PostAgreement")
.WithOpenApi();

app.MapPut("/agreement", (AgreementViewModel model, CerbosBlockingClient client) =>
{

    var agreement = agreements.SingleOrDefault(f => f.Id == model.Id);

    CheckResult result = client
    .CheckResources(
        Principal.NewInstance(model.User, model.Roles)
        .WithPolicyVersion("default")
        .WithAttribute("geography", AttributeValue.StringValue("SP")),

        Resource.NewInstance("agreement", agreement.Id.ToString())
            .WithPolicyVersion("default")
            .WithAttribute("owner", AttributeValue.StringValue(agreement.User)),

        "update"
    );

    if (!result.IsAllowed("update"))
        return Results.Unauthorized();

    return Results.Ok();
})
.WithName("PutAgreement")
.WithOpenApi();
app.Run();

public class Agreement
{
    public Agreement(int id, string identifier, bool signed, string user)
    {
        Id = id;
        Identifier = identifier;
        Signed = signed;
        User = user;
    }

    public Agreement()
    {

    }

    public int Id { get; set; }

    public string Identifier { get; set; }

    public bool Signed { get; set; }

    public string User { get; set; }
}


public class AgreementViewModel
{

    public int Id { get; set; }

    public string Identifier { get; set; }

    public bool Signed { get; set; }

    public string User { get; set; }

    public string[] Roles { get; set; }
}
