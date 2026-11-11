# خطوات بناء مشروع .NET (Onion Architecture)

Checklist بالترتيب. كل خطوة: بنعمل إيه، وليه، والأمر.
الأمثلة من SchoolSystem. في مشروع جديد غيّري الأسماء بس.

---

## 0. التصميم (قبل أي كود)

- [ ] **ERD**: الـ entities، والـ attributes، والعلاقات (1:1، 1:M، M:M).
- [ ] **Mapping**: كل entity تبقى جدول.
  - **1:M**: الـ FK بيتحط في جدول الـ "many".
  - **M:M**: بنعمل جدول في النص (Junction)، زي `Enrollment`.
  - **1:1**: الـ FK بيتحط في أي طرف، واحنا اللي بنحدده.

---

## 1. هيكل المشروع

- [ ] Solution + المشاريع:

```bash
dotnet new sln -n SchoolSystem
dotnet new classlib -o Core/DomainLayer
dotnet new classlib -o Core/ServiceAbstractionLayer
dotnet new classlib -o Core/ServiceLayer
dotnet new classlib -o Infrastructure/Persistence
dotnet new classlib -o Infrastructure/Presentation
dotnet new classlib -o Shared
dotnet new web -o SchoolSystem --use-program-main
dotnet sln add **/*.csproj
dotnet new gitignore
```

> `--use-program-main` بيخلي `Program.cs` يتعمل بـ `class Program` و `Main`، مش top-level statements.

- [ ] امسحي كل ملفات `Class1.cs`.
- [ ] اربطيه بـ GitHub: اعملي repo **فاضي** (من غير README)، وبعدين:

```bash
git remote add origin https://github.com/nermeennana/SchoolSystem.git
git push -u origin main
```

> لو طلع `403`: الـ token مش مسموحله على الـ repo ده. عدّليه من GitHub ← Settings ← Developer settings ← Fine-grained tokens.

## 2. الـ References (مين يشوف مين)

القاعدة: الاعتمادية بتشاور **لجوه**، يعني ناحية الـ Domain.

| المشروع | بيشاور على |
|---|---|
| SchoolSystem (startup) | ServiceLayer، Persistence، Presentation |
| ServiceLayer | DomainLayer، ServiceAbstractionLayer |
| Presentation | ServiceAbstractionLayer |
| Persistence | DomainLayer |
| DomainLayer | Shared |
| ServiceAbstractionLayer | Shared |
| Shared | ولا حاجة |

```bash
dotnet add SchoolSystem reference Core/ServiceLayer
# ... وهكذا لكل سطر في الجدول
```

---

## 3. الـ Domain (DomainLayer/Models)

- [ ] `BaseEntity<TKey>`، وفيه `Id`. كل الـ entities بتورث منه.
- [ ] الـ Entities: properties بس، من غير أي حاجة عن الـ database.
- [ ] الـ Enums بتتحط في `Shared/Enums`.
- [ ] **Navigation properties:**
  - طرف الـ "many": `Subject Subject = null!` (object واحد)
  - طرف الـ "one": `ICollection<Enrollment> Enrollments = new List<Enrollment>()`
  - بنحطها **بس** في الاتجاه اللي محتاجين نمشي فيه.

> `= null!`: "عارفة إنه null دلوقتي، بس EF هيملاه".
> `int?`: رقم يقبل null، زي `Grade`.

---

## 4. الـ DbContext (Persistence/Data)

- [ ] Package الـ SQL Server:

```bash
dotnet add Infrastructure/Persistence package Microsoft.EntityFrameworkCore.SqlServer
```

- [ ] `SchoolDbContext : DbContext`:
  - Constructor بياخد `DbContextOptions<SchoolDbContext>`.
  - `DbSet<T>` لكل entity، **public**.

## 5. الـ Connection String (User Secrets)

⚠️ الباسورد **مش** بيتحط في `appsettings.json`، لأن الملف ده بيروح GitHub.

```bash
dotnet user-secrets init --project SchoolSystem
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=SchoolSystemDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True" --project SchoolSystem
```

## 6. تسجيل الـ DbContext (Program.cs)

```csharp
builder.Services.AddDbContext<SchoolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

> أي تسجيل لازم يبقى **قبل** `builder.Build()`.

---

## 7. الـ Configurations (Persistence/Data/Configurations)

ملف لكل entity، بيقول شكلها في الـ database.

- [ ] الكلاس **لازم** يعمل `IEntityTypeConfiguration<T>`، وإلا هيتجاهله من غير error.
- [ ] Strings: `.IsRequired().HasMaxLength(100)`، بدل `nvarchar(max)`.
- [ ] العلاقات:

```csharp
// 1:M
builder.HasOne(t => t.Subject)
       .WithMany()                     // أو .WithMany(s => s.Teachers) لو فيه collection
       .HasForeignKey(t => t.SubjectId)
       .OnDelete(DeleteBehavior.Restrict);

// 1:1
builder.HasOne(s => s.User)
       .WithOne()
       .HasForeignKey<Student>(s => s.UserId)   // لازم نحدد الـ FK في أنهي جدول
       .OnDelete(DeleteBehavior.Restrict);
```

- [ ] **OnDelete:**
  - `Restrict`: مينفعش تمسحي الأب لو ليه أولاد. ده **الأأمن**.
  - `Cascade`: امسح الأولاد مع الأب، بس لو ملهمش معنى من غيره، زي `Attendance`.
- [ ] في `OnModelCreating`:

```csharp
base.OnModelCreating(modelBuilder);
modelBuilder.ApplyConfigurationsFromAssembly(typeof(SchoolDbContext).Assembly);
```

> مش محتاجين `HasKey` لو الـ PK اسمه `Id`، لأن EF بيعرفه لوحده (Convention).

---

## 8. أول Migration

- [ ] Docker شغال: `docker ps -a`
- [ ] أداة EF (مرة واحدة على الجهاز): `dotnet tool install --global dotnet-ef`
- [ ] Package الـ Design في الـ startup:

```bash
dotnet add SchoolSystem package Microsoft.EntityFrameworkCore.Design
```

- [ ] اعملي الـ migration، و**راجعي الملف**:

```bash
dotnet ef migrations add InitialCreate --project Infrastructure/Persistence --startup-project SchoolSystem --output-dir Data/Migrations
```

- [ ] نفّذيها على الـ database:

```bash
dotnet ef database update --project Infrastructure/Persistence --startup-project SchoolSystem
```

> **لو لسه متنفذتش**: ينفع تمسحيها بـ `dotnet ef migrations remove ...` وتعيديها.
> **لو اتنفذت**: اعملي migration **جديدة** فوقها.

---

## 9. Identity (الـ login والـ roles)

- [ ] Packages:

```bash
dotnet add Core/DomainLayer package Microsoft.Extensions.Identity.Stores
dotnet add Infrastructure/Persistence package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

- [ ] `AppUser : IdentityUser` في `DomainLayer/Models`.
- [ ] `SchoolDbContext : IdentityDbContext<AppUser>`. لازم `base.OnModelCreating` يفضل **أول سطر**.
- [ ] اربطي `UserId` بالـ `AppUser` (علاقة 1:1، في الـ Configuration).
- [ ] Migration جديدة:

```bash
dotnet ef migrations add AddIdentity --project Infrastructure/Persistence --startup-project SchoolSystem --output-dir Data/Migrations
dotnet ef database update --project Infrastructure/Persistence --startup-project SchoolSystem
```

- [ ] سجّلي Identity في `Program.cs`:

```csharp
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<SchoolDbContext>();
```

> `AddIdentityCore` مش `AddIdentity`، لأن `AddIdentity` بيفعّل Cookies، واحنا هنستخدم JWT في الـ API.

---

## 10. Seeding (بيانات أولية) ✅

- [x] أسماء الـ roles كـ constants في `Shared/Constants/Roles.cs` (`public static class`).
- [x] بيانات الـ Admin في User Secrets، **مش في الكود**:

```bash
dotnet user-secrets set "AdminUser:Email" "admin@school.com" --project SchoolSystem
dotnet user-secrets set "AdminUser:Password" "YOUR_PASSWORD" --project SchoolSystem
```

- [x] `IDataSeeding` في `DomainLayer/Contracts`، وفيه method واحدة: `Task DataSeedAsync();`
  > الـ interface بيقول **إيه** اللي بيتعمل، مش **إزاي**. عشان كده method واحدة بس.
- [x] `DataSeeding : IDataSeeding` في `Persistence/Data/Seeding`:
  - الـ constructor بياخد `SchoolDbContext` و `UserManager<AppUser>` و `RoleManager<IdentityRole>` و `IConfiguration` (DI).
  - `DataSeedAsync()`: لو فيه pending migrations ← `MigrateAsync()`، وبعدين `SeedRolesAsync()`، وبعدين `SeedAdminAsync()`.
  - `private SeedRolesAsync()`: لو الـ role مش موجود (`RoleExistsAsync`)، اعمله.
  - `private SeedAdminAsync()`: اقري الإيميل والباسورد من `_configuration`، وبعدين `FindByEmailAsync`، و `CreateAsync`، و `AddToRoleAsync(user, Roles.Admin)`.

> ⚠️ الـ roles **قبل** الـ Admin، لأن `AddToRoleAsync` محتاج الـ role يكون موجود.

- [x] بيانات ثابتة (المواد) من JSON:
  - `Persistence/Data/Seeding/subjects.json`: **array** على طول `[ { "Name": "..." } ]`، من غير `Id`. وأسماء الـ properties لازم نفس أسماءها في الكلاس.
  - في `Persistence.csproj`، عشان الملف يتنسخ لـ `bin`:

```xml
<ItemGroup>
  <None Update="Data/Seeding/subjects.json" CopyToOutputDirectory="PreserveNewest" />
</ItemGroup>
```

  - `private SeedSubjectsAsync()`: لو `!Subjects.AnyAsync()`:
    - `Path.Combine(AppContext.BaseDirectory, "Data", "Seeding", "subjects.json")`
    - `File.OpenRead` ← `JsonSerializer.DeserializeAsync<List<Subject>>` ← `AddRangeAsync`
    - و `SaveChangesAsync()` في آخر `DataSeedAsync`.

> `AppContext.BaseDirectory` أحسن من `".."`، لأنه بيشتغل مهما كان البروجكت اتشغل منين.
> لو المسار في الـ csproj غلط، الملف مش هيتنسخ **من غير error**.

## 11. تنضيف Program.cs (Extension Methods) ✅

- [x] `Persistence/InfrastructureServicesRegistration.cs`:

```csharp
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<SchoolDbContext>(...);
    services.AddIdentityCore<AppUser>(...).AddRoles<IdentityRole>().AddEntityFrameworkStores<SchoolDbContext>();
    services.AddScoped<IDataSeeding, DataSeeding>();
    return services;
}
```

- [x] `SchoolSystem/Extensions/WebApplicationRegistration.cs`:

```csharp
public static async Task SeedDataBaseAsync(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var dataSeeding = scope.ServiceProvider.GetRequiredService<IDataSeeding>();
    await dataSeeding.DataSeedAsync();
}
```

- [x] `Program.cs` بقى:

```csharp
builder.Services.AddInfrastructureServices(builder.Configuration);
var app = builder.Build();
await app.SeedDataBaseAsync();
```

> **Extension method:** `static` class، و `static` method، و `this` قبل أول parameter. بنناديها بعد النقطة: `builder.Services.AddInfrastructureServices(...)`، من غير اسم الكلاس.
> لو قالك "does not contain a definition": ناقص `using` للـ namespace بتاع الكلاس (`Cmd+.`).
> **DI:** الكلاس بيطلب اللي محتاجه في الـ constructor، والبروجكت بيعمله ويديهوله.
> `CreateScope()`: الحاجات الـ `Scoped` (زي الـ `DbContext`) بتعيش جوه request واحد، ولسه مفيش request، فبنعمل scope مؤقت.

**مشاكل مشهورة:**
> `address already in use`: البروجكت شغال في مكان تاني، فوقفيه بـ `Ctrl+C`.
> `Error Number:10061` أو `Connection refused`: Docker مقفول. افتحيه، وبعدين `docker start todo-sql`.

---

## الجاي ⏳

- [ ] Controllers (Presentation) و Swagger
- [ ] JWT (الـ login)
- [ ] DTOs (في Shared)
- [ ] Service interfaces (ServiceAbstractionLayer) والـ implementations (ServiceLayer)
- [ ] Frontend

---

## نصايح

- **Commit بعد كل خطوة** شغالة، وبعدها `git push` عشان تترفع على GitHub.
- **`dotnet build` بعد كل تعديل**، عشان تمسكي الـ errors بدري.
- **راجعي ملف الـ migration** قبل `database update`.
- مش لازم تحفظي الكود. المهم تعرفي **كل حتة بتعمل إيه**، و**الترتيب**.
