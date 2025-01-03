using AutoMapper;
using DayPlanner.Abstractions.Models.Backend;
using DayPlanner.Abstractions.Models.Backend.Extensions;
using DayPlanner.Abstractions.Models.DTO;
using DayPlanner.Abstractions.Stores;
using Google.Cloud.Firestore;

namespace DayPlanner.FireStore
{
    public class FireStoreAppointmentStore(FirestoreDb db, IMapper mapper) : IAppointmentStore
    {
        private readonly FirestoreDb _fireStoreDb = db;
        private readonly IMapper _mapper = mapper;

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

            ThrowIfUnauthorized(userId, appointment);
            await appointmentRef.DeleteAsync();
        }

        public async Task<Appointment?> GetAppointmentById(string userId,string appointmentId)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointmentId);   

            DocumentSnapshot snapshot = await appointmentRef.GetSnapshotAsync();

            if (snapshot.Exists)
            { 
                var appointment = snapshot.ConvertTo<Appointment>(); 
                ThrowIfUnauthorized(userId, appointment);
                return appointment;
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
            //TODO: Auth check
            return snapshot.Count; 
        }

        public async Task<List<Appointment>> GetUsersAppointments(string userId, DateTime start, DateTime end)
        {
            Query query = _fireStoreDb.Collection("appointments")
                .WhereEqualTo("userId", userId)
                .WhereGreaterThanOrEqualTo("startTime", start)
                .WhereLessThanOrEqualTo("endTime", end);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            var appointments = snapshot.Documents.Select(doc => doc.ConvertTo<Appointment>()).ToList(); 
            ThrowIfUnauthorized(userId, appointments);
            return appointments;
        }

        public async Task<List<Appointment>> GetUsersAppointments(string userId, int page, int pageSize)
        {
            if (page < 1) throw new ArgumentException("Page number must be greater than 0.");

            Query query = _fireStoreDb.Collection("appointments")
                .WhereEqualTo("userId", userId)
                .Limit(pageSize);

            DocumentSnapshot? lastDocument = null;
            if (page > 1)
            {
                QuerySnapshot previousSnapshot = await query.GetSnapshotAsync();
                int skip = (page - 1) * pageSize;

                if (skip >= previousSnapshot.Documents.Count)
                {
                    return [];
                }

                lastDocument = previousSnapshot.Documents.ElementAt(skip - 1);
                query = query.StartAfter(lastDocument);
            }

            QuerySnapshot currentSnapshot = await query.GetSnapshotAsync();

            var appointments = currentSnapshot.Documents.Select(doc => doc.ConvertTo<Appointment>()).ToList();
            ThrowIfUnauthorized(userId, appointments);
            return appointments;

        }

        public async Task<Appointment> UpdateAppointment(string appointmentId, string userId, AppointmentRequest request)
        {
            DocumentReference appointmentRef = _fireStoreDb.Collection("appointments").Document(appointmentId);

            var appointment = _mapper.Map<Appointment>(request);
            ThrowIfUnauthorized(userId, appointment);
            await appointmentRef.SetAsync(appointment);

            var updatedSnapshot = await appointmentRef.GetSnapshotAsync();
            if (!updatedSnapshot.Exists)
            {
                throw new Exception("Failed to retrieve the updated appointment.");
            }

            var updatedAppointment = updatedSnapshot.ConvertTo<Appointment>();
            
            return updatedAppointment;
        }

        private static void ThrowIfUnauthorized(string givenUserId, Appointment dbAppointment)
        {
            if(givenUserId != dbAppointment.UserId)
            {
                throw new UnauthorizedAccessException("User is not authorized to access this appointment.");
            }
        }

        private static void ThrowIfUnauthorized(string givenUserId, List<Appointment> dbAppointments)
        {
            if(dbAppointments.Any(c => c.UserId != givenUserId))
            {
                throw new UnauthorizedAccessException("User is not authorized to access this appointment.");
            }
        }
    }
}
