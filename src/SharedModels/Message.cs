namespace SharedModels;

public record Message(
    Guid PaymentId,
    string OperationName,
    DateTime OperationDate,
    string CustomerEmail,
    string CustomerName,
    decimal Amount);
