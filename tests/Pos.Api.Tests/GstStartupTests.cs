using Microsoft.AspNetCore.Mvc.Testing;

namespace Pos.Api.Tests;

// Requirement 58a. Each test starts the API with a bad rate and expects startup itself to fail. The rate is an environment
// variable, like every other setting PosApiFactory uses, so it is put back to 18 afterwards; test parallelisation is off
// for the whole assembly, so no other test can start in between.
public class GstStartupTests
{
    [Theory]
    [InlineData("180")]
    [InlineData("-5")]
    [InlineData("17.125")]
    public void The_api_refuses_to_start_with_a_gst_rate_it_must_not_charge(string badRate)
    {
        try
        {
            using PosApiFactory factory = new PosApiFactory();
            Environment.SetEnvironmentVariable("Tax__GstPercentage", badRate);

            Exception? startupFailure = Record.Exception(() => factory.CreateClient());

            Assert.NotNull(startupFailure);
            Assert.Contains("Tax:GstPercentage", DescribeWithInnerExceptions(startupFailure));
        }
        finally
        {
            Environment.SetEnvironmentVariable("Tax__GstPercentage", "18");
        }
    }

    // The host can wrap the exception thrown in Program.cs, so every message in the chain is searched.
    private static string DescribeWithInnerExceptions(Exception exception)
    {
        string description = string.Empty;
        Exception? current = exception;
        while (current != null)
        {
            description = description + current.Message + " | ";
            current = current.InnerException;
        }

        return description;
    }
}