namespace GymMate.Messages;

using GymMate.Models;

public record RoutineUpdatedMessage(WorkoutRoutine Routine);
public record RoutineDeletedMessage(string RoutineId);
public record RoutinesReloadMessage();
