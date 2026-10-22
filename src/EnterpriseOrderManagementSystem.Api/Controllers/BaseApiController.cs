
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseOrderManagementSystem.API.Controllers;

#region Base API Controller

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    #region Dependencies

    protected ISender Sender { get; }

    #endregion

    #region Constructor

    protected BaseApiController(ISender sender)
    {
        Sender = sender;
    }

    #endregion
}

#endregion
