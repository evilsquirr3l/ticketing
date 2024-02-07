namespace SharedModels;

public record Message(
    Guid PaymentId,
    string OperationName,
    DateTimeOffset OperationDate,
    string CustomerEmail,
    string CustomerName,
    decimal Amount);
