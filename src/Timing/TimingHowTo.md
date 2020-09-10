
# How to use TimingService
TimingService is a low overhead instrumented profiling tool.

### Initialization

```
TimingService.IsEnabled = true;
var chrome = new ChromeTraceTimingConsumer("myfile.json");
```

### Usage

```
void MyFunction()
{
	using var _ = new Timing("someName"); // constant strings are encouraged to avoid allocations

	// do stuff we want to measure here
}
```

### Shutdown

This is necessary for all timing events to be included and myfile.json to be properly terminated.
```
TimingService.TriggerFlush(true); // this will set TimingService.IsEnabled to false
chrome.Dispose();
```

### View results

In chrome, go to `chrome://tracing/` and drag the resulting `myfile.json` into the browser

