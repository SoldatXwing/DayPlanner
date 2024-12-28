using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.Backend.Extensions;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore
{
    public class FireStoreAppointmentStore : IAppointmentStore
    {
        //TODO: Implement the IAppointmentStore interface
        private readonly FirestoreDb _fireStoreDb;
        public FireStoreAppointmentStore(string projectId)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "serviceAccountKey.json");
            _fireStoreDb = FirestoreDb.Create(projectId);
        }

        public Task<Appointment> CreateAppointment(AppointmentRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document();

            await appointmentRef.SetAsync(appointment.ToDictionary());

            appointment.Id = appointmentRef.Id;
            return appointment;
        }

        public Task DeleteAppointment(string appointmentId)
        {
            throw new NotImplementedException();
        }

        public async Task DeleteUsersAppointmentsAsync(string appointmentId)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointmentId);
            await appointmentRef.DeleteAsync();
        }

        public Task<Appointment> GetAppointmentById(string appointmendId)
        {
            throw new NotImplementedException();
        }

        public async Task<Appointment> GetSingleAppointmentAsync(string appointmentId)

        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointmentId);            // Retrieve the document

            DocumentSnapshot snapshot = await appointmentRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<Appointment>();
            }
            else
            {
                return null;
            }
        }

        public Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Appointment>> GetUsersAppointmentsAsync(string userId, DateTime start, DateTime end)
        {
            Query query = _fireStoreDb.Collection("appointments")
                .WhereEqualTo("userId", userId)
                .WhereGreaterThanOrEqualTo("startTime", start)
                .WhereLessThanOrEqualTo("endTime", end);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(doc => doc.ConvertTo<Appointment>()).ToList();
        }

        public Task<Appointment> UpdateAppointment(string appointmentId, AppointmentRequest request)
        {
            throw new NotImplementedException();
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)

        {

            // Get a reference to the appointment document

            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointment.Id);


            // Update the appointment data

            await appointmentRef.SetAsync(appointment);


            return appointment;

        }

    }

}
