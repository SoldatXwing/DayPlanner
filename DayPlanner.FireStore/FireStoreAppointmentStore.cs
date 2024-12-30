using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.Backend.Extensions;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore
{
    public class FireStoreAppointmentStore : IAppointmentStore
    {
        private readonly FirestoreDb _fireStoreDb;
        private readonly IMapper _mapper;
        public FireStoreAppointmentStore(string projectId, IMapper mapper)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "serviceAccountKey.json");
            _fireStoreDb = FirestoreDb.Create(projectId);
            _mapper = mapper;
        }

        public async Task<Appointment> CreateAppointment(string userId, AppointmentRequest request)
        {
            var appointment = _mapper.Map<Appointment>(request);

            appointment.UserId = userId;

            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document();
            await appointmentRef.SetAsync(appointment.ToDictionary());

            appointment.Id = appointmentRef.Id;
            return appointment;
        }

        public async Task DeleteAppointment(string userId, string appointmentId)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments")
                .Document(appointmentId);

            DocumentSnapshot snapshot = await appointmentRef.GetSnapshotAsync();
            if (!snapshot.Exists)
            {
                throw new InvalidOperationException($"Appointment with id {appointmentId} not found.");               
            }
            var appointment = snapshot.ConvertTo<Appointment>();

            if (appointment.UserId != userId)
            {
                throw new UnauthorizedAccessException("User is not authorized to delete this appointment.");
            }
            await appointmentRef.DeleteAsync();
        }

        public async Task<Appointment?> GetAppointmentById(string appointmentId)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointmentId);   

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

        public async Task<long?> GetAppointmentsCount(string userId)
        {
            var query = _fireStoreDb.Collection("appointments")
             .WhereEqualTo("userId", userId)
             .Count();

            var snapshot = await query.GetSnapshotAsync();

            return snapshot.Count; 
        }

        public async Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
        {
            Query query = _fireStoreDb.Collection("appointments")
                .WhereEqualTo("userId", userId)
                .WhereGreaterThanOrEqualTo("startTime", start)
                .WhereLessThanOrEqualTo("endTime", end);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.Select(doc => doc.ConvertTo<Appointment>()).ToList();
        }
        public async Task<List<Appointment>> GetUsersAppointments(string userId, int page, int pageSize)
        {
            if (page < 1) throw new ArgumentException("Page number must be greater than 0.");

            Query query = _fireStoreDb.Collection("appointments")
                .WhereEqualTo("userId", userId)
                .Limit(pageSize);

            if (page > 1)
            {
                int skip = (page - 1) * pageSize;
                QuerySnapshot snapshot = await query.Limit(skip).GetSnapshotAsync();

                if (snapshot.Documents.Count > 0)
                {
                    var lastDocument = snapshot.Documents.Last();
                    query = query.StartAfter(lastDocument);
                }
                else
                {
                    return new();
                }
            }

            QuerySnapshot pageSnapshot = await query.GetSnapshotAsync();

            return pageSnapshot.Documents.Select(doc => doc.ConvertTo<Appointment>()).ToList();
        }


        public async Task<Appointment> UpdateAppointment(string appointmentId, AppointmentRequest request)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointmentId);
            var appointment = _mapper.Map<Appointment>(request);

            await appointmentRef.SetAsync(appointment);

            return appointment;
        }

    }

}
