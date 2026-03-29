# UBB-SE-2026-UUTSUU
This environment should take care of searching and renting items from the database as well as searching for users, the UI should enable the user to search boardgames using filters, interact with the results by seeing detail pages and going through the renting process.

## Database setup

1. Open Visual Studio

2. Open:
   View -> SQL Server Object Explorer

3. If LocalDB is NOT available:
   - Close Visual Studio
   - Open Visual Studio Installer
   - Click Modify on your installed version
   - Go to Individual components
   - Install:
     - SQL Server Express LocalDB
     - SQL Server Data Tools (recommended)
   - Click Modify / Install
   - Restart Visual Studio

4. In SQL Server Object Explorer:
   - Expand SQL Server
   - Right-click -> Add SQL Server
   - Server Name: (localdb)\MSSQLLocalDB
   - Authentication: Windows Authentication
   - Trust Server Certificate: True
   - Click Connect

5. Open:
   Database/generate_db.sql

6. Click Execute

---

## Expected result

After running the script, the database should contain:

- 8 Users
- 34 Games
- 2 Rentals

---

## Important

Running `generate_db.sql` will reset the database (drops and recreates tables).