using MediatR;

namespace MyServe.Backend.App.Application.Abstract;

public interface IAppRequest : IRequest, IRequestBase { }
public interface IAppRequest<out TResponse> : IRequest<TResponse>, IRequestBase { }