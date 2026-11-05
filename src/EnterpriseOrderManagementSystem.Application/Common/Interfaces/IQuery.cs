using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOrderManagementSystem.Application.Common.Interfaces
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
    {
    }
}
