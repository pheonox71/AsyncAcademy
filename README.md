# AsyncAcademy

A team-built learning management system for students and instructors: course catalogs, enrollment, assignments, grades, and tuition payments.

This is my portfolio copy of a Weber State University group project. The [original repository](https://github.com/chr-lucas/AsyncAcademy) is public and licensed under AGPL-3.0.

<!-- TODO: add a short demo GIF or screenshots of the welcome dashboard, course enrollment, and assignment creation once they are captured. -->

**Demo:** <!-- TODO: add a live demo URL if this copy is redeployed. A historical Azure App Service URL is commented in the original README, but that site did not respond when checked in November 2026. Do not treat it as a working demo. -->

## Features

- **Accounts and roles.** Students and instructors can sign up and log in. Role (`IsProfessor`) changes what you can see and do. Passwords are hashed with ASP.NET Core Identity's `PasswordHasher`; sessions (not cookie Identity auth) keep you signed in.
- **Courses.** Instructors can create and manage courses. Students can browse available courses, enroll, and drop. Instructor lists are limited to courses they teach; students cannot delete courses.
- **Assignments and submissions.** Instructors can create text, file, and URL assignments. Students can submit work; instructors can review and grade submissions.
- **Calendar and to-do list.** Upcoming work shows up on a calendar and a to-do list on the welcome page.
- **Grades and class overview.** Course grades, class overview, and charts for instructors.
- **Tuition payments.** Students can pay tuition through a Stripe Checkout session; payment records are stored in the database.
- **Seeded demo data.** On first run against an empty database, the app seeds instructors, students, courses, enrollments, assignments, and submissions.

## Tech stack

| Layer | Choice |
| --- | --- |
| App | .NET 8, ASP.NET Core Razor Pages, a few API controllers |
| Data | Entity Framework Core 8, SQL Server |
| Auth | Session state plus Identity `PasswordHasher` (Identity EF packages are referenced, but the app does not use full Identity cookie authentication) |
| Payments | Stripe.net (Checkout sessions) |
| Hosting (original) | Azure App Service (Windows / .NET) |
| Unit tests | MSTest, Moq, EF Core InMemory |
| UI tests | Python + Selenium (Microsoft Edge), in `SeleniumUItests/` |

## Architecture

```
AsyncAcademy.sln          (lives under AsyncAcademy/, not the repo root)
├── AsyncAcademy/         # web app
│   ├── Program.cs        # service setup, session, SQL Server, Stripe, seed
│   ├── Data/             # EF Core context (Users, Course, Enrollments, …)
│   ├── Models/           # entities + SeedData
│   ├── Pages/            # Razor UI (Account, Course Pages, Assignments, …)
│   ├── Controllers/      # Course enroll/drop API, Stripe Checkout
│   ├── Utils/            # helpers (birthday validation, notifications)
│   └── wwwroot/          # static files, profile images, submissions
├── AsyncAcademyTest/     # MSTest unit tests
└── SeleniumUItests/      # browser tests against a running instance
```

`Program.cs` registers Razor Pages, MVC controllers, EF Core with the `TITAN-CONNECTION-STRING` connection string, session state, and Stripe, then seeds an empty database before the HTTP pipeline starts. It does **not** automatically apply EF migrations, so you need to update the database before the first run.

Role-specific course UI lives under `Pages/Course Pages/` (`InstructorIndex`, `StudentIndex`, `Create`, `Enroll`, and the usual Details/Edit/Delete pages). Assignment flows live under `Pages/Assignments/`.

## My contributions

I contributed as [chancehardmanweber](https://github.com/chancehardmanweber) (40 commits, September–December 2024). The work below is limited to what those commits show. Teammates built substantial parts of the app as well, including payments, grading, calendar, profile, Azure hosting, and much of the later test suite.

**Course domain and student/instructor course flows.** I added the `Course` model, then the Razor course pages, and split the catalog into instructor and student views. I wired those pages to session user data, auto-assigned the instructor ID on course creation, limited the instructor list to courses that person teaches, hid course deletion from students, and added the student enrollment page and enroll flow.

- [created course model](https://github.com/chr-lucas/AsyncAcademy/commit/0d548b0b685a67ae331b5e234dccc8a906d9648e)
- [added template course pages](https://github.com/chr-lucas/AsyncAcademy/commit/2e7968c81854ed6d4a7a044c4b8cdaebd04abf4a)
- [course views can now be linked to](https://github.com/chr-lucas/AsyncAcademy/commit/cef70d08bae75ca1ec857beb365fddb313136ef9)
- [instructor Id is now automatically added](https://github.com/chr-lucas/AsyncAcademy/commit/db91056545a584b18ef9439638e1ad21f9330d29)
- [students can now enroll](https://github.com/chr-lucas/AsyncAcademy/commit/c4348197f74f6d40c8d001f6582bbc00dd8b71ee)

**Assignments.** I changed assignment creation so `CourseId` is taken from session/context instead of being typed in by hand, and later cleaned up redundant submission code.

- [courseId autoadded](https://github.com/chr-lucas/AsyncAcademy/commit/8e6399d887e2a8d84547752a231b1c328c0abff2)

**Testing.** I stood up early unit tests, moved them onto EF Core's in-memory provider, added `UnitTestAssignments` (text, file, and URL assignment creation), added signup and course-creation tests, and split the original catch-all test file into focused classes. I also fixed a shared in-memory database name issue across the test project and added a Selenium test that walks through instructor assignment creation.

- [in memory database is working now](https://github.com/chr-lucas/AsyncAcademy/commit/ec9ec2d927f754da40946d008d687ce3686e20b9)
- [Create UnitTestAssignments.cs](https://github.com/chr-lucas/AsyncAcademy/commit/76131a2de8c85dfe58968ad7333b3ecc204c2c10)
- [Signup tests done with test refactoring](https://github.com/chr-lucas/AsyncAcademy/commit/f5730ca2477d63c46c55d35f918c952d30993af6)
- [fixed database name issue](https://github.com/chr-lucas/AsyncAcademy/commit/66f02b6230242245120ac522b70317535f071e2d)
- [added in my selenium test](https://github.com/chr-lucas/AsyncAcademy/commit/0cc7e3f4a3152c37f550cb22d1000ef0fa6a1c06)

**Other.** An early commit tightened email validation on the user model. I also prototyped section signup pages, then removed that Section model and those pages in favor of the Course pages above. Several commits updated `.gitignore`; build output was still committed in the original repository.

## Team

All six of us committed to the original repo. We worked in an Agile/Scrum-style process and attended weekly sprint meetings throughout the project.

- [chr-lucas](https://github.com/chr-lucas) — repository owner
- [bell-kevin](https://github.com/bell-kevin)
- [Alexmar97](https://github.com/Alexmar97)
- [sebastian-sato](https://github.com/sebastian-sato)
- [chancehardmanweber](https://github.com/chancehardmanweber) — me
- [hannawhitney](https://github.com/hannawhitney)

## Running locally

**Requirements:** .NET 8 SDK, and a SQL Server instance you can create a database on.

1. Clone this repository.
2. Set your own SQL Server connection string for `ConnectionStrings:TITAN-CONNECTION-STRING`. `appsettings.json` ships with a placeholder, and `Program.cs` throws on startup if that placeholder is still in place. Use [user secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) in Development, or the environment variable `ConnectionStrings__TITAN-CONNECTION-STRING` otherwise. The configuration key is what `Program.cs` reads.

```bash
dotnet user-secrets set "ConnectionStrings:TITAN-CONNECTION-STRING" "Server=localhost;Database=AsyncAcademy;User Id=YOUR_SQL_USER;Password=YOUR_SQL_PASSWORD;TrustServerCertificate=True;" --project AsyncAcademy/AsyncAcademy.csproj
```

3. Set `Stripe:SecretKey` to a Stripe test secret key the same way (environment variable `Stripe__SecretKey`, or user secrets). `Program.cs` reads that key and throws on startup if it is missing or still the placeholder. Do not commit the key.

```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_key_here" --project AsyncAcademy/AsyncAcademy.csproj
```

4. Apply migrations, then run the web app. `dotnet ef` is not part of the SDK. This repo references `Microsoft.EntityFrameworkCore.Tools` 8.0.10, so install the matching global tool once if `dotnet ef` is missing:

```bash
dotnet tool install --global dotnet-ef --version 8.0.10
dotnet ef database update --project AsyncAcademy/AsyncAcademy.csproj --startup-project AsyncAcademy/AsyncAcademy.csproj
dotnet run --project AsyncAcademy/AsyncAcademy.csproj
```

The HTTPS launch profile is `https://localhost:7082`.

The first successful start against an empty database seeds demo users, courses, and assignments. Official demo usernames (including `instructortest` and `studenttest`) and their password are documented in `AsyncAcademy/Models/SeedData.cs`.

## Testing

**Unit tests** (MSTest, in-memory EF Core; no SQL Server required):

```bash
dotnet test AsyncAcademy/AsyncAcademy.sln
```

The suite covers signup, course creation, enrollment, assignments, submissions, grades, and tuition updates, among other behaviors. Tests use a unique in-memory database name per test (`[TestInitialize]` creates a new GUID) so they do not collide.

**Selenium UI tests:** start the web app at `https://localhost:7082`, install Python dependencies (`selenium`), have Microsoft Edge and its WebDriver available, then from `SeleniumUItests/` run:

```bash
python main.py
```

These scripts log in as the seeded demo accounts and exercise login, signup, profile updates, course registration, the to-do list, and assignment creation. They currently hardcode local and historical Azure URLs.

## License

This project is licensed under the [GNU Affero General Public License v3.0](LICENSE), the same license as the [original repository](https://github.com/chr-lucas/AsyncAcademy).
