# Address Management Module

This module handles Vietnamese administrative divisions (Tỉnh/Thành phố, Quận/Huyện, Phường/Xã) for the RentMaster application.

## Features

- Import Vietnamese administrative divisions from CSV
- Hierarchical data structure (Country > Province > District > Ward)
- Support for historical/previous unit codes
- Batch processing for better performance

## Prerequisites

- .NET 6.0 or later
- PostgreSQL database
- CSV file with address data

## Installation

1. Make sure you have the required NuGet packages:
   ```bash
   dotnet add package CsvHelper
   dotnet add package Microsoft.EntityFrameworkCore.Design
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. Ensure your database connection string is properly configured in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=your_database;Username=your_username;Password=your_password"
     }
   }
   ```

## Usage

### Importing Address Data

1. Place your CSV file at `Addresses/Data/address_data.csv` with the following format:
   ```
   code,name,parent_code,type,old_code
   01,Thành phố Hà Nội,VN,2,
   79,Thành phố Hồ Chí Minh,VN,2,
   ...
   ```

2. Run the import command:
   ```bash
   dotnet run --project RentMaster.csproj -- seed-addresses --file Addresses/Data/address_data.csv
   ```

   Or if you're already in the project directory:
   ```bash
   dotnet run -- seed-addresses --file Addresses/Data/address_data.csv
   ```

### Available Commands

- `seed-addresses`: Import address data from CSV file
  - `--file`: Path to the CSV file (default: `Addresses/Data/address_data.csv`)

## Data Model

### AddressDivision
- `Id`: Unique identifier (string)
- `Name`: Division name (e.g., "Hà Nội", "Quận 1")
- `Code`: Administrative code (e.g., "01", "79")
- `Type`: Division type (Country=1, Province=2, District=3, Ward=4)
- `ParentId`: Reference to parent division
- `IsDeprecated`: Whether the division is no longer in use
- `DeprecatedAt`: When the division was deprecated (if applicable)
- `PreviousUnitCodes`: List of previous administrative codes

## Troubleshooting

### Common Issues

1. **File not found**
   - Ensure the CSV file exists at the specified path
   - Use absolute path if relative path doesn't work

2. **Database connection issues**
   - Verify your connection string in `appsettings.json`
   - Ensure PostgreSQL is running

3. **Permission denied**
   - Make sure the application has read access to the CSV file
   - Check database user permissions

4. **CSV format errors**
   - Ensure the CSV has the correct headers
   - Check for empty or malformed rows

## Development

### Adding New Administrative Divisions

1. Update the CSV file with new divisions
2. Run the import command again
3. The importer will skip existing records and only add new ones

### Testing

To run tests:
```bash
dotnet test
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
