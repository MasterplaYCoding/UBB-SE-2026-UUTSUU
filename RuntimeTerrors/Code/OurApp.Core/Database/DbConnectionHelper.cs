using Microsoft.Data.SqlClient;
using System;

namespace OurApp.Core.Database
{
    public static class DbConnectionHelper
    {
        // Derived from your provided Server instance name and the MyDb database
        //Cip DB
        //private const string ConnectionString = @"Server=(localdb)\ProjectModels;Database=MyDb;Trusted_Connection=True;TrustServerCertificate=True;";
        private const string ConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=iss_project;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
        //private const string ConnectionString = "Data Source=DESKTOP-D8Q1P5P\\SQLEXPRESS;Initial Catalog=iss_project;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
        //private const string ConnectionString =@"Server=(localdb)\ProjectModels;Database=MyDb;Trusted_Connection=True;TrustServerCertificate=True;";
        //private const string ConnectionString = @"Data Source=Aron\\SQLEXPRESS;Initial Catalog=iss_project;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
        //private const string ConnectionString = "Data Source=.;Initial Catalog=iss_project;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
