namespace Domain.Exception;

public class NotFoundException(string message) : System.Exception(message);
