using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using CarbonAware.Aggregators.CarbonAware;
using CarbonAware.Aggregators.SciScore;

namespace CarbonAware.CLI.Tests;

public class CarbonAwareCLITests
{
    private Mock<ILogger<CarbonAwareCLI>> Log = new Mock<ILogger<CarbonAwareCLI>>();
    
    [Test]
    public void ParseCommandLineArguments_SetsLocationWhenProvided()
    {
        string[] args = new string[] {"-l", "test"};
        
        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), It.IsAny<ISciScoreAggregator>(), Log.Object);
        
        Assert.AreEqual("test", cli._state.Locations[0]); 
    }

    [Test]
    public void ParseCommandLineArguments_SetsStartTimeWhenProvided()
    {
        string[] args = new string[] {"-l", "test", "-t", "2021-11-11"};
        
        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), It.IsAny<ISciScoreAggregator>(), Log.Object);
        
        Assert.AreEqual("test", cli._state.Locations[0]); 
        Assert.AreEqual(DateTime.Parse("2021-11-11"), cli._state.Time); 
    }

    [Test]
    public void ParseCommandLineArguments_SetsStartTimeAndEndTimeWhenProvided()
    {
        string[] args = new string[] {"-l", "test", "-t", "2021-11-11", "--toTime", "2021-12-12"};
        
        var cli = new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), It.IsAny<ISciScoreAggregator>(), Log.Object);

        Assert.AreEqual("test", cli._state.Locations[0]); 
        Assert.AreEqual(DateTime.Parse("2021-11-11"), cli._state.Time); 
        Assert.AreEqual(DateTime.Parse("2021-12-12"), cli._state.ToTime); 
    }

    [Test]
    public void ParseCommandLineArguments_ThrowsErrorWhenLocationNotProvided()
    {
        string[] args = new string[] {"--route", "SciScore"};
        try {
            new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), It.IsAny<ISciScoreAggregator>(), Log.Object);
        } catch (AggregateException e)  {
            StringAssert.Contains("Required option 'l, location' is missing", e.ToString());
        }
    }

  [Test]
    public void ParseCommandLineArguments_ThrowsErrorWhenRouteNotProvided()
    {
        string[] args = new string[] {"-l", "test"};
        try {
            new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), It.IsAny<ISciScoreAggregator>(), Log.Object);
        } catch (AggregateException e)  {
            StringAssert.Contains("Required option 'route' is missing", e.ToString());
        }
    }
    
    [Test]
    public void ParseCommandLineArguments_ThrowsErrorWhenInvalidDateProvided()
    {
        string[] args = new string[] {"-l","test", "-t", "invalid", "--route", "SciScore"};
        
        var ex = Assert.Throws<ArgumentException>(() => new CarbonAwareCLI(args, It.IsAny<ICarbonAwareAggregator>(), It.IsAny<ISciScoreAggregator>(), Log.Object));

        StringAssert.Contains("Date and time needs to be in the format", ex?.Message);
    }
}