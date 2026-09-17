namespace RaceDay.Api.Services;

public class NotFoundException : Exception { public NotFoundException(string message) : base(message) { } }
public class ForbiddenException : Exception { public ForbiddenException(string message) : base(message) { } }
public class ConflictException : Exception { public ConflictException(string message) : base(message) { } }
public class ValidationAppException : Exception { public ValidationAppException(string message) : base(message) { } }
