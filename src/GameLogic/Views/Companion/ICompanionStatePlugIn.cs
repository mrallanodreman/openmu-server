namespace MUnique.OpenMU.GameLogic.Views.Companion;

using MUnique.OpenMU.GameLogic.Bots;

/// <summary>View contract for sending the leader the current companion snapshot.</summary>
public interface ICompanionStatePlugIn : IViewPlugIn
{
    ValueTask SendCompanionStateAsync(BotPlayer companion);
}
