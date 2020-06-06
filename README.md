# Posh-PG

Access PostgreSQL Database from PowerShell.

**Warning: This project is in early development stage, and everything could be changed. Do not use it in production environment.**

## Key Features

- Multiple connection session
- Saved query with parameter support
- Customizable table formatter
- Multiple environment by powershell profile
- Session and Saved Query autocompletion

## Build

```
dotnet build
```

## Load as a powershell module

```
Import-Module './PoshPG/bin/Debug/netcoreapp3.1/PoshPG.dll'
```

## Quick Start

### Connect to PostgreSQL Database

```
New-PgSession `
    -Name "dev" ` // The name of this session
    -Endpoint "localhost" ` // Database endpoint
    -Username "postgres" ` // Database username
    -Password "postgres" ` // Database password
    -Database "database" ` // Database to be connected
```

The connection will be saved in your powershell session and you can use `Get-PgSession` cmdlet to show all your connection

### Run Query

```
Invoke-PgQuery `
    -Session dev ` // Name of your session
    -Text "select * from information_schema.schemata" // Sql statement
```

## Create a saved query with parameter

```
New-PgQuery `
    -Name GetUserInfo `
    -Query "
        select user_id, active, email, nickname, username, phone_number, stripe_customer_id
        from _user.user_info
        where (
            (@Username is null or username = @Username) and
            (@Phone is null or phone_number = @Phone) and
            (@Nickname is null or nickname = @Nickname) and
            (@Email is null or email = @Email)
        )
        limit (case when @Limit is null then 100 else @Limit::integer end)
    "
```
