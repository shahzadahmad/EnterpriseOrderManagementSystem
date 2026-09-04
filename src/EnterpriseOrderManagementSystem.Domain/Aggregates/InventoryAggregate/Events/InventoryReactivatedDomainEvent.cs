using EnterpriseOrderManagementSystem.Domain.Common.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseOrderManagementSystem.Domain.Aggregates.InventoryAggregate.Events
{
    public sealed record InventoryReactivatedDomainEvent(
            Guid InventoryId,
            Guid ProductId,
            Guid WarehouseId) : DomainEvent;
}
