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

**🗺️ كل تسجيل فين؟** كل layer بيسجّل حاجاته في ملف **جوه نفسه**:

```
Persistence/
└── InfrastructureServicesRegistration.cs   ← DbContext, Identity, UnitOfWork, Seeding

ServiceLayer/
└── ApplicationServicesRegistration.cs      ← AutoMapper (وبعدين ServiceManager)

SchoolSystem/Extensions/
└── WebApplicationRegistration.cs           ← حاجات الـ web نفسه (Seeding call، وبعدين Swagger و JWT)
```

> و`Program.cs` **بينادي** عليهم بس.
> ليه؟ لأن **كل layer هو اللي عارف كلاساته**: Persistence عارف `UnitOfWork`، و ServiceLayer عارف `SubjectProfile`. كده `Program.cs` مش محتاج يعرف كل كلاس، وده فكرة الـ Onion.

**مشاكل مشهورة:**
> `address already in use`: البروجكت شغال في مكان تاني، فوقفيه بـ `Ctrl+C`.
> `Error Number:10061` أو `Connection refused`: Docker مقفول. افتحيه، وبعدين `docker start todo-sql`.

---

## 12. Generic Repository + Unit of Work ✅

**الفكرة:** بدل ما نكتب Add/Update/Delete لكل entity لوحده، نكتبهم **مرة واحدة** generic.

- [x] `IGenericRepository<TEntity, TKey>` في `DomainLayer/Contracts`:
  `GetAllAsync`, `GetByIdAsync`, `Add`, `Update`, `Delete`
  > الشرط: `where TEntity : BaseEntity<TKey>`
  > `Add`/`Update`/`Delete` مش async، لأنها بتسجل التغيير في الذاكرة بس. الحفظ الحقيقي في `SaveChangesAsync`.
  > قاعدة: أي method اسمها فيه `Async` لازم ترجع `Task`.
- [x] `GenericRepository` في `Persistence/Repositories`، وبيستخدم `_context.Set<TEntity>()`
  > لو `ToListAsync` عملت error ← ناقص `using Microsoft.EntityFrameworkCore;`
- [x] `IUnitOfWork` في `DomainLayer/Contracts`: `GetRepository<TEntity, TKey>()` و `SaveChangesAsync()`
- [x] `UnitOfWork` في `Persistence/Repositories`:
  - `Dictionary<string, object>` = دولاب، الـ key اسم الـ entity (`typeof(TEntity).Name`)
  - لو الدرج مش موجود ← `new GenericRepository` ونحطه فيه
  - نرجّع اللي في الدرج مع **cast** لـ `IGenericRepository<TEntity, TKey>` (لأنه متخزن `object`)
- [x] التسجيل: `services.AddScoped<IUnitOfWork, UnitOfWork>();` في `AddInfrastructureServices`

**الاستخدام (في السيرفيس):**
```csharp
var repo = _unitOfWork.GetRepository<Student, int>();
repo.Add(student);                    // لسه في الذاكرة
await _unitOfWork.SaveChangesAsync(); // دلوقتي اتحفظ في الداتابيز
```
> الـ UnitOfWork = **مدير المخزن**: يديكي الـ repository، ولما تخلصي تقوليله "احفظ" فيحفظ كله مرة واحدة.

---

## 13. Services (Subject) ⏳

**الترتيب:** DTO ← IService ← AutoMapper (Profile + تسجيل) ← Service ← Service Manager ← Controller

### الـ DTOs (`Shared/DataTransferObjects/Subjects/`)
- [x] `SubjectDto` (`Id` + `Name`): اللي بيطلع للـ frontend.
- [x] `CreateAndUpdateSubjectDto` (`Name` بس): اللي بييجي من الـ frontend.
  > **DTO** = الشكل اللي البيانات بتطلع بيه لبرّه. مش بنبعت الـ entity نفسه، عشان منطلّعش حاجات زي `PasswordHash`.
  > الـ Create مفيهوش `Id`، لأن الداتابيز هي اللي بتعمله.
  > ⚠️ نوع الـ `Id` في الـ DTO لازم يبقى زي الـ entity (`int` هنا، مش `Guid` زي ToDoApp).

### `ISubjectService` (`Core/ServiceAbstractionLayer`)
- [x] `GetAll`, `GetById`, `Create`, `Update`, `Delete` (كلهم `Async`)
  > **Create بترجع `SubjectDto`**: عشان نرجّع الـ **Id** الجديد اللي اتعمل.
  > **Update بتاخد `id`**: عشان نعرف **أنهي** مادة هتتعدل (الـ id من الـ URL، والبيانات من الـ DTO).
  > **Update و Delete بيرجعوا `Task` بس**: مفيش معلومة جديدة نرجعها.
  > **مين يقدر يضيف/يعدّل؟** ده مش شغل السيرفيس. ده في الـ Controller بـ `[Authorize(Roles = Roles.Admin)]` بعد الـ JWT.

### AutoMapper
- [x] التسطيب في `ServiceLayer`:
  ```bash
  dotnet add Core/ServiceLayer package AutoMapper --version 16.3.0
  ```
  > ⚠️ 14.0.0 و 16.0.0 (اللي في الكورس) فيهم ثغرة DoS. الصلّحة من 16.1.1. فحص الثغرات:
  > `dotnet list package --vulnerable --include-transitive`
  > مش محتاجين `AutoMapper.Extensions.Microsoft.DependencyInjection`، لأنها بقت جوه AutoMapper نفسها.
- [x] `ServiceLayer/MappingProfiles/SubjectProfile.cs`:
  ```csharp
  public class SubjectProfile : Profile
  {
      public SubjectProfile()
      {
          CreateMap<Subject, SubjectDto>();                    // داتابيز ← frontend
          CreateMap<CreateAndUpdateSubjectDto, Subject>();     // frontend ← داتابيز
      }
  }
  ```
  > **Mapping** = ننقل البيانات من نوع لنوع: `new SubjectDto { Id = s.Id, Name = s.Name }`. AutoMapper بيعمل ده لوحده لما الأسماء متطابقة.
  > `ForMember`: لما الاسم **مختلف**، مثلاً `BrandName` ← `ProductBrand.Name`.
- [ ] `ServiceLayer/ApplicationServicesRegistration.cs` ← `AddApplicationServices`:
  ```csharp
  services.AddAutoMapper(cfg => { }, typeof(SubjectProfile).Assembly);
  ```
  وفي `Program.cs`: `builder.Services.AddApplicationServices();`
  > ⚠️ في AutoMapper 16 لازم `cfg => { }`، حتى لو فاضية. وده مكان الـ license key لو احتجناه.

  > **`typeof(SubjectProfile).Assembly` يعني إيه؟**
  > **Assembly** = المشروع بعد الـ build (`ServiceLayer.dll`).
  > 1. `typeof(SubjectProfile)` ← الكلاس `SubjectProfile`
  > 2. `.Assembly` ← المشروع اللي هو فيه (ServiceLayer)
  > 3. AutoMapper بيدوّر جوه ServiceLayer على **أي** كلاس بيورث من `Profile` ويسجّله
  >
  > `SubjectProfile` مجرد **علامة** بنشاور بيها على المشروع، كأنك بتقولي: "روح المدرسة اللي فيها فصل 1/أ، وهات كل الفصول اللي فيها".
  > **بدل** ما نكتب `cfg.AddProfile<SubjectProfile>()` لكل profile، ونفتكر نضيف كل واحد جديد.
  >
  > **نفس فكرة `ApplyConfigurationsFromAssembly`:**
  >
  > | | بيدوّر على | جوه |
  > |---|---|---|
  > | EF Core | أي `IEntityTypeConfiguration` | Persistence |
  > | AutoMapper | أي `Profile` | ServiceLayer |

### `SubjectService` (`Core/ServiceLayer`)
- [x] Primary Constructor: `public class SubjectService(IUnitOfWork _unitOfWork, IMapper _mapper) : ISubjectService`
  > الـ parameters جنب اسم الكلاس، وبتتشاف في كل الـ methods. مش محتاجين field ولا constructor (C# 12).
- [x] **Get:** `GetRepository` ← `GetAllAsync` / `GetByIdAsync` ← `_mapper.Map<...>`
  > `_mapper.Map<IEnumerable<SubjectDto>>(subjects)`: اللي بين `< >` هو اللي **عايزاه**، واللي بين `( )` هو اللي **عندك**.
- [x] **Create:** Map ← `repo.Add` ← `SaveChangesAsync` ← رجّعي `Map<SubjectDto>`
  > بعد الـ Save، EF بيحط الـ **Id الجديد** جوه نفس الـ object لوحده.
- [x] **Update:** `GetByIdAsync(id)` ← لو null `throw` ← `_mapper.Map(dto, subject)` ← `repo.Update` ← Save
- [x] **Delete:** `GetByIdAsync(id)` ← لو null `throw` ← `repo.Delete(subject)` ← Save
  > **Update و Delete بيبدأوا بـ "هات الحاجة واتأكد إنها موجودة"**، لأنك مينفعش تعدّلي أو تمسحي حاجة مش موجودة.
  > `_mapper.Map<Subject>(dto)` ← يعمل object **جديد** (Id = 0) ❌ في الـ Update
  > `_mapper.Map(dto, subject)` ← ينقل البيانات **جوه الموجود** ✅
  > الـ `Delete` في الـ repository بتاخد **entity**، مش id.
  > ⚠️ الـ `throw new Exception(...)` مؤقت، وهيتبدل بـ `NotFoundException` في الـ error handling.

  > **إمتى الـ method تبقى `async`؟** لو جواها حاجة **بتستنى الداتابيز**.
  > `repo.Add/Update/Delete` ← في الذاكرة بس (تحطي في العربية 🛒) ← `void`
  > `SaveChangesAsync` ← بيروح للداتابيز (الكاشير ⏳) ← `await`
  > السيرفيس بيعمل الاتنين، فبيبقى `async`.

  > ⚠️ **خدي بالك من اقتراحات الـ AI (Copilot):** كتب `AddAsync` و `DeleteAsync(id)`، وهما مش موجودين عندنا. دايماً قارني باللي في الـ interface.

### Service Manager
**ليه؟** مدخل واحد لكل السيرفيسات، بدل ما الـ Controller يطلب كل سيرفيس لوحده:
```
Controller ← ServiceManager ← (SubjectService, StudentService, ...)
Service    ← UnitOfWork     ← (Subject repo, Student repo, ...)
```
- [x] `IServiceManager` (`ServiceAbstractionLayer`): `ISubjectService SubjectService { get; }`
- [x] `ServiceManager` (`ServiceLayer`):
  ```csharp
  public class ServiceManager(IMapper _mapper, IUnitOfWork _unitOfWork) : IServiceManager
  {
      private readonly Lazy<ISubjectService> _lazySubjectService = new(() => new SubjectService(_unitOfWork, _mapper));
      public ISubjectService SubjectService => _lazySubjectService.Value;
  }
  ```
  > **Lazy** = "متعملوش غير لما حد يطلبه" (مطعم بيطبخ لما الزبون يطلب، مش كل المنيو الصبح).
  > `Lazy<ISubjectService>` = صندوق فاضي 📦. و `() => new SubjectService(...)` = **الوصفة** جواه، ولسه متنفذتش.
  > `.Value` = افتح الصندوق: أول مرة يعمل السيرفيس، وبعد كده يرجّع نفس النسخة.
  >
  > **ليه الـ ServiceManager بياخد UnitOfWork و Mapper؟** لأنه بيعمل `new SubjectService(...)` **بإيده**، فالـ DI مش هيساعد. فلازم هو اللي يسلّم السيرفيس المكونات اللي محتاجها 🥕
  > **Lazy** = **إمتى** نعمله. و **UnitOfWork/Mapper** = **محتاج إيه** عشان يتعمل. حاجتين مختلفين.
  >
  > **ليه الـ UnitOfWork مستخدمش Lazy؟** هو عمل نفس الفكرة بـ **Dictionary**، لأن الـ entities **مش معروفة مسبقاً** (generic). أما السيرفيسات **معروفة بالاسم**، فـ Lazy field لكل واحد.
- [ ] التسجيل: `services.AddScoped<IServiceManager, ServiceManager>();` في `AddApplicationServices`

### الجاي في القسم ده
- [ ] Controller

---

## الجاي ⏳

- [ ] Services + Service Manager
- [ ] Controllers (Presentation) و Swagger
- [ ] JWT (الـ login)
- [ ] DTOs (في Shared)
- [ ] Frontend

---

## نصايح

- **Commit بعد كل خطوة** شغالة، وبعدها `git push` عشان تترفع على GitHub.
- **`dotnet build` بعد كل تعديل**، عشان تمسكي الـ errors بدري.
- **راجعي ملف الـ migration** قبل `database update`.
- مش لازم تحفظي الكود. المهم تعرفي **كل حتة بتعمل إيه**، و**الترتيب**.
