namespace API.Dto;

public record UpdatedProductDto(string Name, string Description, decimal Price, int CategoryId, int Quantity, IFormFile photo);
