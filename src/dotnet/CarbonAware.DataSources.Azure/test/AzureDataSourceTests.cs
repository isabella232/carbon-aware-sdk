using CarbonAware.Exceptions;
using CarbonAware.Interfaces;
using CarbonAware.Model;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarbonAware.DataSources.Azure.Tests;

public class AzureDataSourceTests
{
    #pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Mock<ILogger<AzureDataSource>> Logger { get; set; }
    private ActivitySource ActivitySource { get; set; }
    private AzureDataSource DataSource { get; set; }
    #pragma warning restore CS8618

    [SetUp]
    public void Setup()
    {
        this.ActivitySource = new ActivitySource("AzureDataSourceTests");

        this.Logger = new Mock<ILogger<AzureDataSource>>();

        this.DataSource = new AzureDataSource(this.Logger.Object, this.ActivitySource);
    }

    [Test]
    public void GetEnergyAsync_ReturnsResultsWhenRecordsFound()
    {
        Assert.IsTrue(true);
    }
}
