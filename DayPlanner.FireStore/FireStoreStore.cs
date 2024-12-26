using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.Backend.Extensions;
using DayPlanner.Abstractions.Stores;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore
{
    public class FireStoreStore : IAppointmentStore
    {

        private readonly FirestoreDb _db;
        public FireStoreStore(string projectId)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "serviceAccountKey.json");
            _db = FirestoreDb.Create(projectId);
        }
        public async Task<Appointment> CreateAppointmentAsync(Appointment appointment)
        {
            DocumentReference appointmentRef = _db.Collection("appointments").Document();

            await appointmentRef.SetAsync(appointment.ToDictionary());

            appointment.Id = appointmentRef.Id;
            return appointment;
        }
        public async Task DeleteUsersAppointmentsAsync(string appointmentId)
        {
            DocumentReference appointmentRef = _db.Collection("appointments").Document(appointmentId);
            await appointmentRef.DeleteAsync();
        }

        public async Task<Appointment> GetSingleAppointmentAsync(string appointmentId)

        {
            DocumentReference appointmentRef = _db.Collection("appointments").Document(appointmentId);            // Retrieve the document

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


        public async Task<List<Appointment>> GetUsersAppointmentsAsync(string userId, DateTime start, DateTime end)
        {
            Query query = _db.Collection("appointments")
                .WhereEqualTo("userId", userId)
                .WhereGreaterThanOrEqualTo("startTime", start)
                .WhereLessThanOrEqualTo("endTime", end);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(doc => doc.ConvertTo<Appointment>()).ToList();
        }

        public async Task<Appointment> UpdateAppointmentAsync(Appointment appointment)

        {

            // Get a reference to the appointment document

            DocumentReference appointmentRef = _db.Collection("appointments").Document(appointment.Id);


            // Update the appointment data

            await appointmentRef.SetAsync(appointment);


            return appointment;

        }

    }

}
