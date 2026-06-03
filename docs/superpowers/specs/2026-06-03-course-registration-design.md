# Course Registration Design

**Date:** 2026-06-03

**Goal**

Build an ASP.NET Core MVC application for course registration that satisfies the exam requirements in `exam.md`, using MySQL in Laragon, Entity Framework Core, and ASP.NET Core Identity.

## Scope

This design covers one server-rendered MVC application with:

- public course listing on the home page
- admin CRUD for courses
- account registration and login with Identity
- role-based authorization for `Admin` and `Student`
- student enrollment and cancellation
- student `My Courses` page
- course search by name
- Google external login
- responsive Bootstrap-based UI

The implementation will stay inside the current repo and extend the default MVC starter app already present.

## Architecture

The application will remain a standard ASP.NET Core MVC app with Razor views and EF Core-backed persistence.

Main layers:

- `Controllers`: request handling and authorization boundaries
- `Models`: entity models, view models, and Identity user type
- `Data`: EF Core `DbContext`, seed logic, and migrations support
- `Views`: Razor UI for public pages, admin pages, account pages, and student pages

The app will not introduce a separate SPA frontend or REST API because the exam explicitly asks for MVC and Views.

## Technology Decisions

- Framework: `ASP.NET Core MVC`
- ORM: `Entity Framework Core`
- Auth: `ASP.NET Core Identity`
- Database: `MySQL` running in `Laragon`
- MySQL provider: `Pomelo.EntityFrameworkCore.MySql`
- UI: Bootstrap with server-rendered Razor views

## Data Model

### ApplicationUser

Extend Identity user with the default Identity fields only unless a later requirement forces extra profile fields.

Reasoning: the exam only requires username, password, and email.

### Category

- `Id`
- `Name`

Purpose: group courses and satisfy the suggested database structure.

### Course

- `Id`
- `Name`
- `Image`
- `Credits`
- `Lecturer`
- `CategoryId`
- navigation to `Category`
- navigation collection to `Enrollment`

### Enrollment

- `Id`
- `UserId`
- `CourseId`
- `EnrollDate`
- navigation to `ApplicationUser`
- navigation to `Course`

Constraints:

- one student can enroll in many courses
- one course can have many students
- a student cannot enroll in the same course more than once

This duplicate-enrollment rule should be enforced both in application logic and with a unique index on `(UserId, CourseId)`.

## Database and Configuration

The application will use a new MySQL database named `giuakyweb_db`.

Configuration approach:

- connection string stored in `appsettings.json`
- MySQL provider configured in `Program.cs`
- `ApplicationDbContext` inherits from `IdentityDbContext<ApplicationUser>`

The app will use EF Core migrations for schema creation and updates.

## Roles and Authorization

Two roles will be present:

- `Admin`
- `Student`

Startup seed behavior:

- ensure both roles exist
- ensure an admin account can be seeded from configuration if credentials are provided

Registration behavior:

- new users are created through Identity
- every new registered user is automatically assigned to `Student`

Authorization rules:

- course listing pages are public
- admin course management actions require `Admin`
- enrollment actions and `My Courses` require `Student`

## Routing and Controllers

### HomeController

Responsibilities:

- show all courses on the home page
- support pagination with page size `5`
- support search by course name

Primary view:

- `Views/Home/Index.cshtml`

### CoursesController

Responsibilities:

- create, edit, and delete courses
- optionally include an admin list/details surface if useful for management

Authorization:

- controller or actions protected by `[Authorize(Roles = "Admin")]`

### EnrollmentsController

Responsibilities:

- enroll the current student in a course
- cancel an enrollment
- show `My Courses`

Authorization:

- protected by `[Authorize(Roles = "Student")]`

### Account/Auth Surface

Responsibilities:

- register
- login
- logout
- external login with Google

Implementation choice:

- use ASP.NET Core Identity with minimal custom MVC pages instead of full custom auth logic

## User Flows

### Public visitor

- open home page
- browse paginated course list
- search by course name
- view login and register links

### Student

- register account
- receive `Student` role automatically
- log in
- see course list
- enroll in a course
- cancel an enrollment
- open `My Courses` to view enrolled courses

### Admin

- log in
- access course management pages
- create, edit, and delete courses

## UI Design

The UI should remain practical and exam-oriented rather than decorative.

Guidelines:

- keep the default shared layout and adapt it
- use Bootstrap components for navbar, forms, tables, buttons, cards, and pagination
- make navigation reflect authentication state and role
- ensure mobile-friendly spacing and wrapping
- show course images in the list if available

The first screen should be the usable course list, not a marketing page.

## Error Handling and Validation

Validation rules:

- required fields for course creation and editing
- positive numeric validation for credits
- duplicate enrollment blocked with a user-facing message
- unauthorized access redirected through the normal auth pipeline

Failure handling:

- invalid form submissions return the same view with validation messages
- deleting a missing course returns `NotFound`
- enrollment of a missing course returns `NotFound`

## Search and Pagination Behavior

Home page behavior:

- query parameter for page number
- query parameter for search term
- page size fixed at `5`
- search filters by partial course name match
- pagination preserves search term

## Google Login

The app will support Google external login through ASP.NET Core authentication middleware.

Configuration:

- client id and client secret from configuration
- feature should fail gracefully when credentials are absent, meaning the normal login flow still works and the Google button can be hidden or disabled

## Testing Strategy

Tests will focus on the logic that has the most regression risk and the clearest business rules:

- pagination returns the correct slice of courses
- search returns matching courses
- duplicate enrollment is rejected
- new registration assigns `Student` role

Lower-level CRUD screen rendering will be validated through manual app verification because this repo currently has no existing UI test harness.

## Files and Boundaries

Expected additions:

- `Data/` for `ApplicationDbContext` and seed helpers
- new entity models for `Category`, `Course`, `Enrollment`, `ApplicationUser`
- view models for pagination, search, and auth forms as needed
- controllers for courses and enrollments
- Razor views for public, admin, and student workflows

The design keeps responsibilities narrow:

- persistence in `Data`
- auth and role concerns in Identity configuration and seed logic
- business actions in controllers with EF Core-backed queries
- presentation in Razor views

## Out of Scope

These items are intentionally excluded unless the implementation needs a tiny supporting piece:

- payment
- notifications
- schedule conflict detection
- advanced admin analytics
- file upload service for course images
- REST API layer

## Success Criteria

The implementation is complete when:

- the app runs against `giuakyweb_db` on MySQL in Laragon
- roles and Identity-based authentication work
- home page lists courses with search and pagination
- admin can create, edit, and delete courses
- student can register, log in, enroll, cancel enrollment, and view `My Courses`
- Google login is wired for environments with credentials
- the UI is responsive and coherent
