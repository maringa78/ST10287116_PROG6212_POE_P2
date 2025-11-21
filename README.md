# ST10287116_PROG6212_POE_P3
ClaimIt is a web-based application designed to simplify the process of submitting, managing, and tracking claims within an organization. The system follows a clear MVC (Model–View–Controller) architecture, ensuring a clean separation of logic and making the project easy to maintain and extend.

Below is a brief explanation of how the key components and code within ClaimIt work.

# 1. Models (Data Layer)

The Models represent the structure of the data stored in the system.
Typical models include:

Claim – stores claim details such as description, category, amount, status, date submitted, and user information.

User / Employee – represents the person submitting or approving claims.

SupportingDocument (optional) – stores file metadata if the user attaches documents.

Each model includes:

Properties used for database mapping

Data annotations for validation

Navigation properties for relationships (e.g., one User → many Claims)

# 2. Controllers (Application Logic Layer)

Controllers act as the brain of the system. They handle requests, run business logic, and choose which view to display.

# 2.1 ClaimController

Handles all claim-related operations:

Create() – renders and processes the form for submitting a new claim

Edit() – updates claim details

Delete() – removes a claim

Details() – displays all information about a specific claim

Index() – lists all claims submitted by a user

AdminIndex() – allows managers/admins to view and approve multiple claims

This controller also handles validation, document uploads (if enabled), and status changes (Submitted → Under Review → Approved/Rejected).

# 2.2 Account / Auth Controller

Handles authentication and session management depending on your setup (ASP.NET Identity or custom auth). This ensures users see only their claims.

# 3. Views (User Interface Layer)

Views are Razor pages (.cshtml) that display the UI.

# Examples include:

Claim Submission View – form for entering claim data

Claim List View – table showing all claims

Claim Details View – full breakdown of each claim

Admin Dashboard View – allows reviewing and approving claims

Views use model binding to display dynamic data and forms.

# 4. Database Layer

ClaimIt uses Entity Framework Core with a SQL database.

DbContext contains DbSets for Claims, Users, and Documents.

EF Core handles:

Migrations

Database updates

Data retrieval (LINQ queries)

Change tracking

# 5. File Handling (If Applicable)

If ClaimIt supports document uploads (receipts, PDFs, images):

Files are uploaded through the View

The controller stores metadata in the database

Files are stored locally or in a cloud bucket (Azure Blob Storage)

Validation ensures only acceptable file types and sizes are allowed

# 6. Business Rules & Validation

The system enforces important logic such as:

Required fields (amount, description, category)

Claim amount limits

Only managers/admins can approve or reject

Users can only view and edit their own claims

Status cannot be changed once finalised

Validation occurs at both:

Model level (Data annotations)

Controller level (server-side checks)

# 7. Sessions / Authentication

ClaimIt uses sessions or Identity-based authentication to:

Track logged-in users

Restrict access

Display user-specific claims

Apply role-based permissions (User vs Admin)

# 8. System Flow Summary

User logs in

Submits a claim using the Create form

Claim is stored in the database with status “Submitted”

Admin logs in and views pending claims

Admin approves or rejects

User receives an updated view of their claim status

(Optional) Documents are uploaded, stored, and linked to the claim

The system centralizes the claim process and ensures full transparency from submission to approval

# Link to video demonstration:
https://youtu.be/vP5etllFhsA


Refrences:
ASP.NET Core MVC Views. Available at: https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview?view=aspnetcore-8.0.
Microsoft (2025) ASP.NET Core Project Templates. Available at: https://learn.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/?view=aspnetcore-8.0).
Microsoft (2025) ASP.NET Core. https://learn.microsoft.com/en-us/aspnet/core/.
