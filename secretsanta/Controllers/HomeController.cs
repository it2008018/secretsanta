using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using secretsanta.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace secretsanta.Controllers
{
    public class HomeController : Controller
    {
        private Boolean isPresent = false;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpPost]
        public IActionResult Generate([Bind] Models.pageValue pagevalues,
            string save)
        {
            var loginusername = pagevalues.Name.ToLower();
            var loginpassword = pagevalues.Password;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = "secretserverwk.database.windows.net";
                builder.UserID = "***";
                builder.Password = "***";
                builder.InitialCatalog = "secretdb";
                builder.MultipleActiveResultSets = true;
                SqlConnection connection = new SqlConnection(builder.ConnectionString);

                SqlCommand command = new SqlCommand("select count(*) from dbo.loginusers where address is null or address=''", connection);
                connection.Open();
                Int32 countOfNotNullAddress = (Int32)command.ExecuteScalar();
                
                if (countOfNotNullAddress == 0)
                {
                    command = new SqlCommand("select * from dbo.loginusers", connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        String name="";
                        String assignedto = "";
                        name = reader.GetString(6).ToLower();
                        

                        var password = reader.GetString(1);
                        if (name == loginusername && password == loginpassword)
                        {
                            String assignedName = "";
                            String assignedNameAddress = "";
                            String intrest = "";

                            assignedto = reader.GetString(3);

                            if (assignedto == "")
                            {
                                assignedName = getNameNotSame(reader.GetString(0), connection, command);
                                command = new SqlCommand("Update dbo.loginusers set nameAssigned='" + assignedName + "' where displayname='" + name + "'", connection);
                                command.ExecuteNonQuery();

                                SqlDataReader nameAlreadyAssignedReader = command.ExecuteReader();
                                command = new SqlCommand("select address,interest from dbo.loginusers where name='" + assignedName + "'", connection);
                                nameAlreadyAssignedReader = command.ExecuteReader();
                                if (nameAlreadyAssignedReader.Read())
                                    assignedNameAddress = nameAlreadyAssignedReader.GetString(0);
                                    intrest = nameAlreadyAssignedReader.GetString(1);
                            }
                            else
                            {
                                command = new SqlCommand("select nameAssigned from dbo.loginusers where displayname='" + name + "'", connection);
                                SqlDataReader nameAlreadyAssignedReader = command.ExecuteReader();

                                if (nameAlreadyAssignedReader.Read())
                                    assignedName = nameAlreadyAssignedReader.GetString(0);

                                command = new SqlCommand("select address,interest from dbo.loginusers where name='" + assignedName + "'", connection);
                                nameAlreadyAssignedReader = command.ExecuteReader();
                                if (nameAlreadyAssignedReader.Read())
                                    assignedNameAddress = nameAlreadyAssignedReader.GetString(0);

                                    intrest = nameAlreadyAssignedReader.GetString(1);
                            }
                            if(intrest=="NA")
                            {
                                ViewBag.Message = "You are secret santa for : " + assignedName + ". Send a surprise gift at the address : " + assignedNameAddress + ".";
                            }
                            else
                            {
                                ViewBag.Message = "You are secret santa for : " + assignedName + ". Send a surprise gift at the address : " + assignedNameAddress + ". For your help, " + assignedName + "'s personal interest are " + intrest;
                            }
                          
                            return View("Click");
                        }
                        else isPresent = false;
                    }
                    if (isPresent == false)
                        ViewBag.Message = "Credetials are not correct. Please try again!";
                }else
                    ViewBag.Message = "Please fill your Address & phone. If already done, please wait for sometime...Address & phone is not filled by everyone. Please try after some time.";
            }
            catch (SqlException e)
            {
                //Console.WriteLine(e.ToString());
                ViewBag.Message = "Looks like some issue happened. Please contact the organisers.";
            }
            return View("Click");
        }

        private string getNameNotSame(string name, SqlConnection connection, SqlCommand command)
        {

            var names = new string[] { "name1","name2","name3","name4"};
            while (true)
            {
                var random = new Random();
                var index = random.Next(0, 16);
                if (name.ToLower() != names[index].ToLower())
                {
                    command = new SqlCommand("select nameAssigned from dbo.loginusers", connection);
                    SqlDataReader nameAssignedColumnReader = command.ExecuteReader();
                    Boolean isNew = true;
                    while (nameAssignedColumnReader.Read())
                    {
                        if(names[index] == nameAssignedColumnReader.GetString(0))
                        {
                            isNew = false;
                            break;
                        }
                    }
                    if(isNew)
                        return names[index];
                }
            }
            
        }

        public IActionResult UpdateAddress([Bind] Models.pageValue pagevalues,
            string updateAddress)
        {
            if (updateAddress==null)
            {
                return View("UpdateAddress");
            }
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = "secretsan.database.windows.net";
            builder.UserID = "username";
            builder.Password = "password@123";
            builder.InitialCatalog = "secretsan";
            builder.MultipleActiveResultSets = true;
            SqlConnection connection = new SqlConnection(builder.ConnectionString);

            SqlCommand command = new SqlCommand("select count(*) from dbo.loginusers where name='"+ pagevalues.Name + "' and password='" + pagevalues.Password + "'", connection);
            connection.Open();
            Int32 matchingIdPwd = (Int32)command.ExecuteScalar();

            if (matchingIdPwd == 1)
            {
                command = new SqlCommand("Update dbo.loginusers set address='" + pagevalues.Address + "' where name='" + pagevalues.Name + "'", connection);
                command.ExecuteNonQuery();

                command = new SqlCommand("select address from dbo.loginusers where name='" + pagevalues.Name + "'", connection);
                SqlDataReader nameAlreadyAssignedReader = command.ExecuteReader();

                if (nameAlreadyAssignedReader.Read())
                {
                    ViewBag.Message = "You present address is updated to : " + nameAlreadyAssignedReader.GetString(0);
                }
            }else
                ViewBag.Message = "Credetials are not correct. Please try again!";

            return View("UpdateAddress");
        }
    }
}