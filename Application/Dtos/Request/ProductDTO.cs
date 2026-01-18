namespace Application.Dtos.Request;

public record ProductDto(string Name, string Description, decimal Price, int CategoryId, int Quantity, string filename) { }
