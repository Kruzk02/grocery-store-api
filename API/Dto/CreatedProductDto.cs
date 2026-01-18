namespace API.Dto;

public record CreatedProductDto(string Name, string Description, decimal Price, int CategoryId, int Quantity, IFormFile photo);
