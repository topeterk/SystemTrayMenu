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
    using System.Globalization;
    using Avalonia;
    using Avalonia.Data.Converters;

    internal class ArithmeticConverter : IMultiValueConverter
    {
        private static readonly IDictionary<string, Func<double, double, double>> Operations = new Dictionary<string, Func<double, double, double>>
            {
                { "+", (x, y) => x + y },
                { "-", (x, y) => x - y },
                { "*", (x, y) => x * y },
                { "/", (x, y) => x / y },
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
                if (!double.TryParse(values[i].ToString(), out var parsedNumber))
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

        private static bool TryGetOperations(object? parameter, int operationIndex, out Func<double, double, double>? operation)
        {
            operation = null;
            var operations = parameter?.ToString().Split(',');

            if (operations == null || operations.Length == 0)
            {
                return false;
            }

            if (operations.Length <= operationIndex)
            {
                operationIndex = operations.Length - 1;
            }

            return Operations.TryGetValue(operations[operationIndex].ToString(), out operation);
        }
    }
}
#endif
