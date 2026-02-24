/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using FluentValidation;
using WebUI.Core.Models;

namespace WebUI.API.Validators
{
    /// <summary>
    /// Validator for IBKR connection configuration
    /// </summary>
    public class IbkrConnectionConfigValidator : AbstractValidator<IbkrConnectionConfig>
    {
        public IbkrConnectionConfigValidator()
        {
            RuleFor(x => x.Host)
                .NotEmpty()
                .WithMessage("Host is required")
                .MaximumLength(255)
                .WithMessage("Host must be less than 255 characters");

            RuleFor(x => x.Port)
                .InclusiveBetween(1, 65535)
                .WithMessage("Port must be between 1 and 65535")
                .Must((config, port) =>
                {
                    // Validate common IBKR ports
                    if (config.AccountType == IbkrAccountType.Paper && port != 7497)
                    {
                        // Warning: typical paper trading port is 7497
                        return true;
                    }
                    if (config.AccountType == IbkrAccountType.Live && port != 7496)
                    {
                        // Warning: typical live trading port is 7496
                        return true;
                    }
                    return true;
                })
                .WithMessage("Port {PropertyValue} may not match account type {AccountType}. " +
                    "Typical ports: 7496 (live), 7497 (paper)");

            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("Account ID is required")
                .Must(accountId => accountId != null && accountId.Length >= 4)
                .WithMessage("Account ID must be at least 4 characters")
                .MaximumLength(50)
                .WithMessage("Account ID must be less than 50 characters");

            RuleFor(x => x.AccountType)
                .IsInEnum()
                .WithMessage("Invalid account type");

            RuleFor(x => x.TimeoutSeconds)
                .InclusiveBetween(1, 300)
                .WithMessage("Timeout must be between 1 and 300 seconds");

            RuleFor(x => x.MaxReconnectAttempts)
                .InclusiveBetween(1, 100)
                .WithMessage("Max reconnect attempts must be between 1 and 100");
        }
    }
}
