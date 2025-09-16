using Shared.Core.Enum;

namespace Shared.Core.Dto;

public record TransactionUpdatesStatusDto
(
    string Code,
    TransactionStatus Status
);