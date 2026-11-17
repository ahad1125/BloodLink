# BloodLink MVC

## Run locally

From this directory, run `dotnet restore` and `dotnet run`. The app applies EF Core migrations on startup and creates `bloodlink.db` beside the project. The database is excluded from Git.

## Administrator account

Public registration supports Donor, Requester, and Hospital accounts. Administrator is intentionally not a public registration choice. To bootstrap an administrator, configure these environment variables before starting the app:

- `BLOODLINK_ADMIN_EMAIL`
- `BLOODLINK_ADMIN_PASSWORD`

The password must contain at least 8 characters, with an uppercase letter, lowercase letter, and digit. The bootstrap creates that account if it does not exist and assigns the Administrator role. Do not commit administrator credentials to source control.

## Identity flow

Registration stores users through ASP.NET Core Identity, assigns the selected role, and signs the new account in. Login uses Identity’s password verification and lockout. Dashboard access is based on the signed-in account’s role. Requesters can create and view their own saved blood requests.
