namespace GymMate.Messages;

using GymMate.Models;

public record SessionUpdatedMessage(WorkoutSession Session);
public record SessionDeletedMessage(string SessionId);
public record SessionsReloadMessage();
