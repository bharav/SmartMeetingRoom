using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using SmartMeetingRoom.Common.DTO;

namespace SmartMeetingRoom.Common.Services
{
    public class DocumentDbService
    {
        public DocumentClient DocumentClient { get; }

        internal DocumentCollection DevicesCollection { get; }

        internal DocumentCollection EmployeesCollection { get; }

        public DocumentCollection CalendersCollection { get; }

        public DocumentDbService()
        {

            DocumentClient = new DocumentClient(new Uri("https://smrtmeetdevi7v3j2pn2fltq.documents.azure.com:443/"), "ddPYVgETkFFkeDtnUdNBYxPSnE5jWkehe1h0baWmmbFrP0svxG5DqtRS0VNadsgYccfvuIPtewO5Aiw3oIwWqQ==");
            Database database = DocumentClient.CreateDatabaseQuery().Where(db => db.Id == "smartmeeting").AsEnumerable().First();
            DevicesCollection =
                DocumentClient.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(c => c.Id == "devices")
                    .AsEnumerable()
                    .First();
            EmployeesCollection =
                DocumentClient.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(c => c.Id == "employees")
                    .AsEnumerable()
                    .First();
            CalendersCollection =
                DocumentClient.CreateDocumentCollectionQuery(database.SelfLink)
                    .Where(c => c.Id == "calenders")
                    .AsEnumerable()
                    .First();
        }

        public Device GetDevice(string deviceId)
        {
            IEnumerable<Device> deviceQuery = DocumentClient
                .CreateDocumentQuery<Device>(DevicesCollection.DocumentsLink)
                .Where(d => d.id == deviceId)
                .AsEnumerable();

            Device selectedDevice = deviceQuery.FirstOrDefault();

            return selectedDevice;
        }

        public Calender GetMeetingDetails(string calenderId)
        {
            IEnumerable<Calender> calenderQuery = DocumentClient
                 .CreateDocumentQuery<Calender>(CalendersCollection.DocumentsLink)
                 .Where(d => d.Id == calenderId)
                 .AsEnumerable();

            Calender selectedCalender = calenderQuery.FirstOrDefault();
            return selectedCalender;
        }

        public async Task SaveAsync(Calender _calender)
        {
            await DocumentClient.CreateDocumentAsync(CalendersCollection.DocumentsLink, _calender);
        }

        public async Task SaveAsync(Employee employee)
        {
            await DocumentClient.CreateDocumentAsync(EmployeesCollection.DocumentsLink, employee);
        }

        public async Task SaveAsync(Device device)
        {
            await DocumentClient.UpsertDocumentAsync(DevicesCollection.DocumentsLink, device);
        }
    }
}