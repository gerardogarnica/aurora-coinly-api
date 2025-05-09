﻿namespace Aurora.Coinly.Application.Wallets.Update;

public sealed record UpdateWalletCommand(
    Guid Id,
    string Name,
    string? Notes) : ICommand;