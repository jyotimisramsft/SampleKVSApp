using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using KvsActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Runtime.Migration;
using System.Fabric;
using Microsoft.ServiceFabric.Actors.Client;

namespace KvsActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>

    [StateMigration(StateMigration.Source)]
    [StatePersistence(StatePersistence.Persisted)]
    //internal class KvsActor : Actor, IKvsActor, IRemindable
    internal class KvsActor : Actor, IKvsActor
    {
        /// <summary>
        /// Initializes a new instance of KvsActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /*public KvsActor(StatefulServiceContext context, ActorTypeInformation actorTypeInfo, Func<ActorBase> actorFactory = null, IActorStateProvider stateProvider = null, ActorServiceSettings settings = null)
            : base(context, actorTypeInfo, actorFactory, stateProvider, settings)
        {
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);

            //Activate your actors here:
            var proxy1 = ActorProxy.Create<IKvsActor>(new ActorId(0));
            var proxy2 = ActorProxy.Create<IKvsActor>(new ActorId(1));
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            string reminderName = "Increment count";

            IActorReminder reminderRegistration = await this.RegisterReminderAsync(
                reminderName,
                BitConverter.GetBytes(5),
                TimeSpan.FromSeconds(0),    //The amount of time to delay before firing the reminder
                TimeSpan.FromSeconds(5));    //The time interval between firing of reminders

            await this.StateManager.TryAddStateAsync("count", 0);

        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName.Equals("Increment count"))
            {
                var count = await this.StateManager.GetStateAsync<int>("count", new CancellationToken());
                count += 5;
                await this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, new CancellationToken());
            }
        }*/

        public KvsActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<int> IKvsActor.GetCountAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<int>("count", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<string> IKvsActor.GetNameAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<string>("name", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <returns></returns>
        Task<byte[]> IKvsActor.GetByteArrayAsync(CancellationToken cancellationToken)
        {
            return this.StateManager.GetStateAsync<byte[]>("array", cancellationToken);
        }

        /// <summary>
        /// TODO: Replace with your own actor method.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        Task IKvsActor.SetCountAsync(int count, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("count", count, (key, value) => count > value ? count : value, cancellationToken);
        }

        Task IKvsActor.SetNameAsync(string name, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("name", name, (key, value) => name, cancellationToken);
        }

        Task IKvsActor.SetByteArrayAsync(byte[] byteArray, CancellationToken cancellationToken)
        {
            // Requests are not guaranteed to be processed in order nor at most once.
            // The update function here verifies that the incoming count is greater than the current count to preserve order.
            return this.StateManager.AddOrUpdateStateAsync("array", byteArray, (key, value) => byteArray, cancellationToken);
        }
    }
}
