using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SmartMeetingRoom.WebApi.Controllers
{
    [Route("api/[controller]")]
    public class EmployeeController : Controller
    {
        private DocumentDbService _ddbService;

        public EmployeeController()
        {
            _ddbService = new DocumentDbService();
        }
        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Employee emp = await _ddbService.GetEmployeeById(id);
            HttpContext.Response.StatusCode = 201;
            return Json(emp);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Register([FromBody]Employee value)
        {
            await _ddbService.SaveAsync(value);
            HttpContext.Response.StatusCode = 201;
            return Json(new
            {
            });
        }
    }
    public class Employee
    {
        public ObjectId Id { get; set; }
        [BsonElement("EmpName")]
        public string EmpName { get; set; }
        [BsonElement("PersonId")]
        public string PersonId { get; set; }
        [BsonElement("BlobName")]
        public string BlobName { get; set; }
        [BsonElement("Department")]
        public string Department { get; set; }

    }
    public class DocumentDbService
    {
        private IMongoDatabase _database { get; }
        internal IMongoCollection<Employee> EmployeeCollection { get; }

        public DocumentDbService()
        {
            try
            {
                MongoClientSettings settings = MongoClientSettings.FromUrl(new MongoUrl("mongodb://smartmeet:password123@ds034807.mlab.com:34807/smartmeeting"));
                if (true)
                {
                    settings.SslSettings = new SslSettings { EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 };
                }
                var mongoClient = new MongoClient(settings);
                _database = mongoClient.GetDatabase("smartmeeting");
                EmployeeCollection = _database.GetCollection<Employee>("employees");
            }
            catch (Exception ex)
            {
                throw new Exception("Can not access to db server.", ex);
            }
        }


        public async Task<Employee> GetEmployeeById(string Id)
        {

            var id = new ObjectId(Id);
            IEnumerable<Employee> employeeQuery = await EmployeeCollection.Find(d => d.Id == id).ToListAsync();
            Employee selectedEmployee = employeeQuery.FirstOrDefault();
            return selectedEmployee;
        }
        public Employee GetEmployeeByPersonId(string PersonId)
        {
            IEnumerable<Employee> employeeQuery = EmployeeCollection.Find(d => d.PersonId == PersonId).ToEnumerable();
            Employee selectedEmployee = employeeQuery.FirstOrDefault();
            return selectedEmployee;
        }


        public async Task<bool> SaveAsync(Employee employee)
        {
            if (GetEmployeeByPersonId(employee.PersonId) == null)
            {
                await EmployeeCollection.InsertOneAsync(employee);
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}