# Multiple Historical Forecasts

## Design Notes

- John agreed that `GeneratedAt` array makes sense for describing which forecasts to pull back.
- RelativeStart - Use a Timespan to start the forecast period some duration after the generateAt time
- RelativeEnd - same as above, for the end of the period relative to the GeneratedAt time.
- WindowSize - currently int (minutes).  Could be a TimeSpan.
- Location array,  keep consistent with existing endpoints.  Consider only allowing a single location if we uncover real throttling constraints.
- Default to not include raw `forecastData`, and only include optionally with a queryString flag

## Differences with current forecast endpoint

- Use of relative start/end times
- use of TimeSpan durations rather than integers (minutes) for window size





## GET /emissions/forecasts/current?locations=9,8,7

- Return JSON array of EmissionsForecastDTO

## GET /emissions/forecasts/?GeneratedAt=1,2,3,4&locations=9,8,7

- Return JSON array of EmissionsForecastDTO

`/emissions/forecasts?GeneratedAt=2022-07-04T12:00:00Z&GeneratedAt=2022-07-03T12:00:00Z&GeneratedAt=2022-07-02T12:00:00Z&GeneratedAt=2022-07-01T12:00:00Z&GeneratedAt=2022-06-30T12:00:00Z&location=europenorth&WindowSize=00:30:00&RelativeStart=06:00&RelativeEnd=12:00`

### How does this design block future usage

- Works well for answering questions when the business time constraints are relative the generatedat time or when they are for daily results for at the same time of day
- Does not work to describe an absolute time-of-day across multiple different generation times.


# POST /emissions/forecasts/

```
  [
    {
      GeneratedAt: ,
      location: ,
      windowSize: ,
      startTime: ,
      endTime: 
    },

  ]
```
