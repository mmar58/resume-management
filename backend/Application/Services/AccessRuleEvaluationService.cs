using backend.Domain.Entities;
using backend.Domain.Enums;

namespace backend.Application.Services;

public interface IAccessRuleEvaluationService
{
    bool Evaluate(List<PositionAccessRule> rules, List<CandidateAttributeValue> candidateValues);
}

public class AccessRuleEvaluationService : IAccessRuleEvaluationService
{
    public bool Evaluate(List<PositionAccessRule> rules, List<CandidateAttributeValue> candidateValues)
    {
        if (rules == null || !rules.Any())
            return true; // No rules = access granted

        // A candidate meets the requirements only if ALL rules evaluate to true (AND logic).
        foreach (var rule in rules)
        {
            var candidateValue = candidateValues.FirstOrDefault(v => v.AttributeDefinitionId == rule.AttributeDefinitionId);
            
            if (candidateValue == null)
            {
                // If candidate is missing a required attribute value, evaluation fails
                return false;
            }

            if (!EvaluateRule(rule, candidateValue))
            {
                return false;
            }
        }

        return true;
    }

    private bool EvaluateRule(PositionAccessRule rule, CandidateAttributeValue candidateValue)
    {
        // Based on DataType, we need to compare the typed value against rule.Value (which is a string)
        var dataType = rule.AttributeDefinition?.DataType ?? candidateValue.AttributeDefinition?.DataType;

        if (dataType == null)
        {
            throw new InvalidOperationException("Attribute DataType must be included for rule evaluation.");
        }

        switch (dataType.Value)
        {
            case AttributeDataType.String:
            case AttributeDataType.Text:
                return EvaluateString(rule.Value, candidateValue.StringValue ?? candidateValue.TextValue ?? "", rule.Operator);

            case AttributeDataType.Numeric:
                if (!decimal.TryParse(rule.Value, out var expectedNum))
                    return false;
                return EvaluateNumeric(expectedNum, candidateValue.NumericValue, rule.Operator);

            case AttributeDataType.Date:
            case AttributeDataType.Period:
                if (!DateTime.TryParse(rule.Value, out var expectedDate))
                    return false;
                // For periods, we might check DateValue (start) or DateEndValue depending on complex business rules. 
                // We'll use DateValue for now.
                return EvaluateDate(expectedDate, candidateValue.DateValue, rule.Operator);

            case AttributeDataType.Boolean:
                if (!bool.TryParse(rule.Value, out var expectedBool))
                    return false;
                return EvaluateBoolean(expectedBool, candidateValue.BoolValue, rule.Operator);
                
            case AttributeDataType.OneOfMany:
                return EvaluateString(rule.Value, candidateValue.OptionValue ?? "", rule.Operator);

            case AttributeDataType.Image:
                // Typically you wouldn't have an access rule on an image, but just in case:
                return EvaluateString(rule.Value, candidateValue.ImageUrl ?? "", rule.Operator);

            default:
                throw new NotSupportedException($"Data type {dataType} is not supported for rule evaluation.");
        }
    }

    private bool EvaluateString(string expected, string actual, AccessRuleOperator op)
    {
        var expectedLower = expected.ToLowerInvariant();
        var actualLower = actual.ToLowerInvariant();

        return op switch
        {
            AccessRuleOperator.Equals => actualLower == expectedLower,
            AccessRuleOperator.NotEquals => actualLower != expectedLower,
            AccessRuleOperator.Contains => actualLower.Contains(expectedLower),
            AccessRuleOperator.StartsWith => actualLower.StartsWith(expectedLower),
            AccessRuleOperator.EndsWith => actualLower.EndsWith(expectedLower),
            _ => false // Other operators (GreaterThan, etc.) don't make sense for generic strings
        };
    }

    private bool EvaluateNumeric(decimal expected, decimal? actual, AccessRuleOperator op)
    {
        if (actual == null) return false;

        return op switch
        {
            AccessRuleOperator.Equals => actual == expected,
            AccessRuleOperator.NotEquals => actual != expected,
            AccessRuleOperator.GreaterThan => actual > expected,
            AccessRuleOperator.GreaterThanOrEqual => actual >= expected,
            AccessRuleOperator.LessThan => actual < expected,
            AccessRuleOperator.LessThanOrEqual => actual <= expected,
            _ => false
        };
    }

    private bool EvaluateDate(DateTime expected, DateTime? actual, AccessRuleOperator op)
    {
        if (actual == null) return false;

        return op switch
        {
            AccessRuleOperator.Equals => actual.Value.Date == expected.Date,
            AccessRuleOperator.NotEquals => actual.Value.Date != expected.Date,
            AccessRuleOperator.GreaterThan => actual.Value.Date > expected.Date,
            AccessRuleOperator.GreaterThanOrEqual => actual.Value.Date >= expected.Date,
            AccessRuleOperator.LessThan => actual.Value.Date < expected.Date,
            AccessRuleOperator.LessThanOrEqual => actual.Value.Date <= expected.Date,
            _ => false
        };
    }

    private bool EvaluateBoolean(bool expected, bool? actual, AccessRuleOperator op)
    {
        if (actual == null) return false;

        return op switch
        {
            AccessRuleOperator.Equals => actual == expected,
            AccessRuleOperator.NotEquals => actual != expected,
            AccessRuleOperator.IsTrue => actual == true,
            AccessRuleOperator.IsFalse => actual == false,
            _ => false
        };
    }
}
