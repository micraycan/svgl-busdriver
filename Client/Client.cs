using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Client : BaseScript
    {
        private enum BusDriverState
        {
            Idle,
            DrivingToStop,
            WaitingForBoarding,
            BoardingInProgress,
            WaitingForDeparture,
            RouteComplete
        }

        private BusDriverState jobState = BusDriverState.Idle;

        private bool jobStarted;
        private bool enteredBus;
        private bool atBusStop;
        private bool boardingInProgress;
        private bool isUnloadingComplete;

        private Vehicle assignedBus;
        private Route currentRoute;
        private Stack<Route> fullRoute = new Stack<Route>();

        private Blip routeBlip;

        private int currentStop = 0;
        private int busFare = 50;

        private List<int> passengersWaiting = new List<int>();
        private List<int> currentPassengers = new List<int>();

        private static readonly Random rnd = new Random();

        public Client()
        {
            Tick += OnTick;

            Blip mapBlip = World.CreateBlip(new Vector3(453.7f, -600.52f, 27.59f));
            API.SetBlipSprite(mapBlip.Handle, 513);
            mapBlip.Name = "Dashound";
            mapBlip.Scale = 0.5f;
            mapBlip.Color = BlipColor.Blue;
            API.SetBlipAsShortRange(mapBlip.Handle, true);
        }

        private async Task OnTick()
        {
            if (jobStarted)
            {
                if (!enteredBus)
                {
                    WaitForPlayerToEnterBus();
                }

                switch (jobState)
                {
                    case BusDriverState.DrivingToStop:
                        CheckArrivalAtBusStop();
                        break;
                    case BusDriverState.WaitingForBoarding:
                        if (API.IsControlPressed(0, 38))
                        {
                            await StartBoarding();
                        }
                        break;
                    case BusDriverState.BoardingInProgress:
                        if (BoardingComplete())
                        {
                            jobState = BusDriverState.WaitingForDeparture;
                            TriggerServerEvent("svgl:Notification", "Boarding complete, press 'E' to proceed to next stop", "primary", 5000);
                        }
                        break;
                    case BusDriverState.WaitingForDeparture:
                        if (API.IsControlPressed(0, 38))
                        {
                            TriggerEvent("svgl-busdriver:client:SetupRoute");
                            CloseVehicleDoors();
                        }
                        break;
                }
            }
        }

        [EventHandler("svgl-busdriver:client:StartJob")]
        private void StartBusDriverJob()
        {
            // initialize
            jobStarted = true;

            TriggerServerEvent("svgl:server:RemoveMoney", "bank", 500, "Bus deposit");
            TriggerEvent("svgl-busdriver:client:SpawnBus");

            // determine route initially
            fullRoute = Routes.GetRoute();
        }

        [EventHandler("svgl-busdriver:client:CompleteJob")]
        private void CompleteBusDriverJob()
        {
            if (assignedBus != null && assignedBus.Exists())
            {
                float distanceToDepot = Vector3.Distance(assignedBus.Position, Game.PlayerPed.Position);

                if (distanceToDepot <= 100.0f)
                {
                    TriggerServerEvent("svgl:server:AddMoney", "bank", 500, "Bus deposit refund");
                    TriggerServerEvent("svgl:Notification", "Your bus deposit has been refunded", "primary", 5000);
                }
                else
                {
                    TriggerServerEvent("svgl:Notification", "Next time, try returning the bus", "error", 5000);
                }
            }

            ResetJob();
        }

        [EventHandler("svgl-busdriver:client:SpawnBus")]
        private void OnBusSpawn()
        {
            TriggerServerEvent("svgl:Notification", "Your bus is ready around the building", "primary", 5000);
            LoadBus();
        }

        [EventHandler("svgl-busdriver:client:TargetSelected")]
        private void OnTargetSelected()
        {
            TriggerEvent("svgl-busdriver:client:OpenMenu", jobStarted);
        }

        [EventHandler("svgl-busdriver:client:SetupRoute")]
        private void OnSetupRoute()
        {
            if (fullRoute.Any())
            {
                bool lastStop = fullRoute.Count == 1;
                currentRoute = fullRoute.Pop();
                jobState = BusDriverState.DrivingToStop;
                SetWaypoint(currentRoute.Location);

                // do not spawn peds at last stop
                if (!lastStop)
                {
                    foreach (var spawnLocation in currentRoute.NPCSpawns)
                    {
                        //Debug.WriteLine("Spawning ped at route location");
                        string randomModel = Pedestrians.GetRandomNpcModel();
                        SpawnPedAtLocation(spawnLocation, currentRoute.Heading, randomModel);
                    }
                }
            }
            else
            {
                TriggerServerEvent("svgl:Notification", "Route complete, head back to the depot", "success", 5000);
                jobState = BusDriverState.RouteComplete;
                Vector3 busDepot = new Vector3(455.09f, -600.93f, 28.54f);
                SetWaypoint(busDepot);
            }
        }

        [EventHandler("svgl-busdriver:client:ArriveAtBusStop")]
        private void OnArrivalAtBusStop()
        {
            TriggerEvent("svgl:Notification", "Press 'E' to allow passengers to board", "primary", 5000);
        }

        [EventHandler("svgl-busdriver:client:SetNextStop")]
        private void OnSetNextStop()
        {
            TriggerEvent("svgl:Notification", "Press 'E' to go to next stop", "primary", 5000);
        }

        [EventHandler("svgl-busdriver:client:FinishRoute")]
        private void OnFinishRoute()
        {
            //new Vector3(455.09f, -600.93f, 28.54f)
        }

        private async void LoadBus()
        {
            uint busModel = (uint)API.GetHashKey("bus");

            bool busModelLoaded = await LoadModel(busModel);
            if (!busModelLoaded)
            {
                TriggerServerEvent("svgl:Notification", "All buses are out at the moment", "error", 5000);
                return;
            }

            assignedBus = new Vehicle(API.CreateVehicle(busModel, 446.18f, -591.06f, 28.5f, 269.18f, true, false));

            string plateNumber = API.GetVehicleNumberPlateText(assignedBus.Handle);
            TriggerEvent("qb-vehiclekeys:client:AddKeys", plateNumber);
        }

        private async Task<bool> LoadModel(uint model)
        {
            API.RequestModel(model);
            int attempts = 0;

            while (!API.HasModelLoaded(model) && attempts++ < 10)
            {
                await BaseScript.Delay(100);
                attempts++;
            }

            return API.HasModelLoaded(model);
        }

        private void ResetJob()
        {
            jobState = BusDriverState.Idle;

            jobStarted = false;
            enteredBus = false;
            atBusStop = false;
            boardingInProgress = false;
            isUnloadingComplete = false;

            currentStop = 0;

            passengersWaiting.Clear();
            currentPassengers.Clear();

            fullRoute.Clear();

            routeBlip?.Delete();
            if (assignedBus?.Exists() == true) { assignedBus?.Delete(); }
        }

        private void SetWaypoint(Vector3 destination)
        {
            routeBlip?.Delete();
            routeBlip = World.CreateBlip(destination);
            routeBlip.Sprite = BlipSprite.Standard;
            routeBlip.Color = BlipColor.Yellow;
            routeBlip.ShowRoute = true;
        }

        private void WaitForPlayerToEnterBus()
        {
            if (assignedBus == null || !assignedBus.Exists()) { return; }

            var player = Game.PlayerPed;
            if (player.IsInVehicle(assignedBus))
            {
                enteredBus = true;
                TriggerServerEvent("svgl:Notification", "Head to first stop", "primary", 5000);
                TriggerEvent("svgl-busdriver:client:SetupRoute");
            }
        }

        private void CheckArrivalAtBusStop()
        {
            if (Vector3.Distance(Game.PlayerPed.Position, currentRoute.Location) < 10f)
            {
                jobState = BusDriverState.WaitingForBoarding;
                TriggerServerEvent("svgl:Notification", "You have arrived at the stop, press 'E' to start boarding", "primary", 5000);
            }
        }

        private async Task StartBoarding()
        {
            jobState = BusDriverState.BoardingInProgress;
            isUnloadingComplete = false;
            int count = 0;
            foreach (var pedhandle in currentPassengers)
            {
                OpenDoors();
                API.TaskLeaveVehicle(pedhandle, assignedBus.Handle, 256);
                API.TaskWanderStandard(pedhandle, 10.0f, 10);
                Debug.WriteLine($"current passenger: {pedhandle} leaving vehicle");

                await BaseScript.Delay(100);
            }

            // wait for all passengers to get off before loading new passengers
            while (!isUnloadingComplete)
            {
                await BaseScript.Delay(500);
                isUnloadingComplete = CheckUnloadingComplete();
            }

            var seatAssignments = AssignSeatToPassengers(passengersWaiting);
            foreach (var pedHandle in passengersWaiting)
            {
                int assignedSeat = seatAssignments[pedHandle];
                TriggerServerEvent("svgl:server:AddMoney", "cash", busFare, "Bus Fare");
                OpenDoors();

                API.ClearPedTasksImmediately(pedHandle);
                Debug.WriteLine($"passenger waiting: {pedHandle} added to current passengers");
                API.TaskEnterVehicle(pedHandle, assignedBus.Handle, -1, assignedSeat, 1.0f, 1, 0);
                currentPassengers.Add(pedHandle);
                count++;

                await BaseScript.Delay(100);
            }

            if (count > 0)
            {
                TriggerServerEvent("svgl:Notification", $"You earned ${busFare * count} from bus fare", "success", 5000);
            }

            passengersWaiting.Clear();
        }

        private bool CheckUnloadingComplete()
        {
            foreach (var pedHandle in currentPassengers)
            {
                if (API.IsPedInVehicle(pedHandle, assignedBus.Handle, false))
                {
                    return false;
                }
            }

            currentPassengers.Clear();
            return true;
        }

        private void CloseVehicleDoors()
        {
            API.SetVehicleDoorsShut(assignedBus.Handle, false);
        }

        private void OpenDoors()
        {
            API.SetVehicleDoorOpen(assignedBus.Handle, 0, false, false);
        }

        private List<int> GetAvailableSeats()
        {
            List<int> availableSeats = new List<int>();
            int maxSeats = API.GetVehicleMaxNumberOfPassengers(assignedBus.Handle);

            for (int i = 1; i <= maxSeats; i++)
            {
                if (API.IsVehicleSeatFree(assignedBus.Handle, i))
                {
                    availableSeats.Add(i);
                }
            }

            return availableSeats;
        }

        private Dictionary<int, int> AssignSeatToPassengers(List<int> passengers)
        {
            var seatAssignments = new Dictionary<int, int>();
            var availableSeats = GetAvailableSeats();

            foreach (var pedHandle in passengers)
            {
                if (availableSeats.Count > 0)
                {
                    int seat = availableSeats[0];
                    availableSeats.RemoveAt(0);
                    seatAssignments[pedHandle] = seat;
                }
                else
                {
                    Debug.WriteLine("Not enough seats for all passengers");
                    break;
                }
            }

            return seatAssignments;
        }

        private bool BoardingComplete()
        {
            if (!isUnloadingComplete)
            {
                return false;
            }

            foreach (var pedHandle in currentPassengers)
            {
                if (!API.IsPedSittingInVehicle(pedHandle, assignedBus.Handle))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task SpawnPedAtLocation(Vector3 location, int heading, string npcModel)
        {
            uint pedModel = (uint)API.GetHashKey(npcModel);

            bool pedLoaded = await LoadModel(pedModel);
            if (!pedLoaded)
            {
                TriggerServerEvent("svgl:Notification", "Dispatch says there aren't any passengers to pick up", "error", 5000);
                return;
            }

            int variation = rnd.Next(-10, 11);
            int variedHeading = (heading + variation) % 360;

            if (variedHeading < 0) { variedHeading += 360; }

            int pedHandle = API.CreatePed(4, pedModel, location.X, location.Y, location.Z, variedHeading, true, true);
            passengersWaiting.Add(pedHandle);

            API.TaskStartScenarioInPlace(pedHandle, "WORLD_HUMAN_STAND_IMPATIENT", 0, true);
            API.SetPedCanRagdoll(pedHandle, false);
            API.SetEntityInvincible(pedHandle, true);
            API.SetBlockingOfNonTemporaryEvents(pedHandle, true);
        }
    }
}
