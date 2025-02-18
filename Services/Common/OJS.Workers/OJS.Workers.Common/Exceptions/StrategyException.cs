namespace OJS.Workers.Common.Exceptions;

public class StrategyException(Exception innerException) : Exception("", innerException);