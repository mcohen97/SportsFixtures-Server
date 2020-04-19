using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Services.Logging;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.WebAPI.Test
{
    [ExcludeFromCodeCoverage]
    [Route("api/[controller]")]
    public class TestController : Controller
    {
        private DatabaseConnection context;
        private LoggingContext logging;
        public TestController(DatabaseConnection connection, LoggingContext logsConnetion) {
            context = connection;
            logging = logsConnetion;
        }
  
        [HttpPost]
        public void ResetDatabase()
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            logging.Database.EnsureDeleted();
            logging.Database.EnsureCreated();
            context.Database.ExecuteSqlCommand("INSERT INTO Users VALUES('admin','admin','admin','admin','admin@admin.com',1);");
            context.Database.ExecuteSqlCommand("INSERT INTO Users VALUES('name','surname','username','password','mail@domain.com',0);");
            logging.Database.ExecuteSqlCommand("INSERT INTO Logs VALUES('Login failed. Wrong password.','Login action','2001/09/11','admin');");
            logging.Database.ExecuteSqlCommand("INSERT INTO Logs VALUES('Login failed. Wrong password.','Login action','2010/06/27','admin');");
            logging.Database.ExecuteSqlCommand("INSERT INTO Logs VALUES('Login failed. Wrong password.','Login action','2017/07/04','admin');");
        }

    }
}
