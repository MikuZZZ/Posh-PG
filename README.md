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

## Save a query in powershell session

### Create a Query with parameter

```
New-PgQuery `
    -Name GetSchemas `
    -Query " `
        select *
        from information_schema.tables
        where (
            @Schema is null or table_schema = @Schema
        )
    "
```

### Execute saved query with parameter

```
Invoke-PgQuery `
    -Name GetSchemas ` // The name of a saved query
    -Parameters @{ Schema = "public" } // Parameters as a dictionary
```

## Persistent you session and saved query in Powershell profile

### Create a powershell script to define sessions and saved query and save as a ps1 script

```
Import-Module "./PoshPG/bin/Debug/netcoreapp3.1/PoshPG.dll" // Path of the module dll


// Create session
New-PgSession `
    -Endpoint "localhost" `
    -Username "postgres" `
    -Password "" `
    -Database "postgres" `
    -Name "dev" `
    -Quiet

// Set default session so you don't need to specify Session later
Set-PgDefaultSession -Session dev -Quiet

// Create saved query
New-PgQuery `
    -Name GetSchemas `
    -Query " `
        select *
        from information_schema.tables
        where (
            @Schema is null or table_schema = @Schema
        )
    "
```

### Start powershell with the profile

```
pwsh -NoExit -NoProfile <Path of the profile you created>
```

Then you can Use your pg connection and query in this powershell session

## Setting with Windows Terminal

Add the following code in your Windows Terminal setting

```
{
    "guid": "<GENERATE A UUID HERE>",
    "hidden": false,
    "name": "PowerShell Posh-PG",
    "commandline": "pwsh.exe -NoExit -NoProfile <PATH OF YOUR PROFILE>",
    "startingDirectory": "~"
}
```
