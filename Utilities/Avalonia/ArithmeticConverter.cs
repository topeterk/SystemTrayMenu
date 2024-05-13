// <copyright file="ArithmeticConverter.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
//
// Copyright (c) 2024-2024 Peter Kirmeier

#if AVALONIA
namespace SystemTrayMenu.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Avalonia;
    using Avalonia.Data.Converters;

    internal class ArithmeticConverter : IMultiValueConverter
    {
        private static readonly Dictionary<string, Func<double, double, double>> Operations = new()
            {
                { "+", (x, y) => x + y },
                { "-", (x, y) => x - y },
                { "*", (x, y) => x * y },
                { "/", (x, y) => x / y },
                { "%", (x, y) => x % y },
                { "floor-to-lcm", (x, y) => x - (x % y) }, // floor to the next multiple with the least common multiple
            };

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            double result = 0;

            if (values is null)
            {
                return AvaloniaProperty.UnsetValue;
            }

            for (int i = 0; i < values.Count; i++)
            {
                if (!double.TryParse(values[i]?.ToString(), out var parsedNumber))
                {
                    return AvaloniaProperty.UnsetValue;
                }

                if (!TryGetOperations(parameter, i, out var operation))
                {
                    return AvaloniaProperty.UnsetValue;
                }

                result = operation(result, parsedNumber);
            }

            return result;
        }

        private static bool TryGetOperations(object? parameter, int operationIndex, [NotNullWhen(true)] out Func<double, double, double>? operation)
        {
            operation = null;
            string[]? operations = parameter?.ToString()?.Split(',');

            if (operations is null || operations.Length == 0)
            {
                return false;
            }

            if (operations.Length <= operationIndex)
            {
                // When no further operations are give, re-run the last operation again
                operationIndex = operations.Length - 1;
            }

            string operationName = operations[operationIndex];
            return Operations.TryGetValue(operationName, out operation);
        }
    }
}
#endif
