using Firebase.Database;
using Firebase.Database.Query;
using PoputkaKGAMT.Models;
using PoputkaKGAMT.Services;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace PoputkaKGAMT.Services
{
    public class BackgroundUpdateService : IBackgroundUpdateService
    {
        // Подключение к БД
        private FirebaseClient firebase = new FirebaseClient("https://poputka-datebase-default-rtdb.europe-west1.firebasedatabase.app/");
        private readonly TripService tripsService;

        private Timer? updateTimer; // таймер, который будет раз в N секунд запускать фоновую задачу

        public event Action OnTripsUpdated; 

        public BackgroundUpdateService(TripService tripService)
        {
            tripsService = tripService;
        }

        public void Start(int intervalSeconds = 10)
        {
            if (updateTimer != null) // нельзя запустить два таймера сразу
                return;

            updateTimer = new Timer(async _ => await UpdateTripsAndStatusesAsync(),null,TimeSpan.Zero,TimeSpan.FromSeconds(intervalSeconds));
            // new Timer( действие, состояние, первыйзапуск, интервал ) — стандартный System.Threading.Timer
        }

        public void Stop()
        {
            updateTimer?.Dispose();
            updateTimer = null;
        }


        // Прверяем и берем новые данные из таблицы trip
        private async Task UpdateTripsAndStatusesAsync()
        {
            try
            {
                var allTrips = await tripsService.GetTrips();
                var now = DateTime.Now;

                foreach (var trip in allTrips)
                {
                    if (!trip.StatusId.Equals("3")) continue; // только планируемые

                    string dateTimeStr = $"{trip.Date} {trip.Time}";
                    if (!DateTime.TryParseExact(dateTimeStr, "dd.MM.yyyy HH:mm", null, DateTimeStyles.None, out DateTime tripDateTime))
                        continue;

                    // Если поездка стала "активной" (только в момент времени)
                    if (now >= tripDateTime)
                    {
                        trip.StatusId = "2";
                        await firebase.Child("trips").Child(trip.Id).PatchAsync(new { status_id = "2" }); // обновить в Firebase



                        // После того как статус поездки = "2" — удаляем все заявки со status_id = "5"
                        var allFellowTravelers = await firebase.Child("fellow_travelers").OnceAsync<FellowTravelerModel>();

                        foreach (var fellowSnap in allFellowTravelers)
                        {
                            var fellow = fellowSnap.Object;
                            if (fellow.TripId == trip.Id && fellow.StatusId == "5")
                            {
                                await firebase.Child("fellow_travelers").Child(fellowSnap.Key).DeleteAsync();
                            }
                        }


                            // После обновления — триггерим обновление для всех подписчиков
                            OnTripsUpdated?.Invoke();
                    }
                }
            }
            catch (Exception ex)
            {
                // логировать, если нужно, но не ронять таймер
                Console.WriteLine("Error in BackgroundUpdateService: " + ex.Message);
            }
        }
    }
}
