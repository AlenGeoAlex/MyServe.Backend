using System.Net;

namespace MyServe.Backend.App.Domain.Exceptions;

public class NotFoundException(Guid id, Type entityType) : AppException($"No entity [{entityType.Namespace}] exists for id: {id}]", 000, HttpStatusCode.NotFound, null) { }