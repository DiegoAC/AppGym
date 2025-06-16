namespace GymMate.ViewModels;

using GymMate.Models;

public record RoutineCardVm(string Name, string Focus, string? Difficulty, DateTime CreatedUtc, WorkoutRoutine CommandParameter);

