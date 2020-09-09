
# How to use TimingService

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

	// do time consuming stuff here
}
```

### Shutdown

This is necessary for the myfile.json so be properly formatted and all timing events included.
```
TimingService.TriggerFlush(true); // this will set TimingService.IsEnabled to false
chrome.Dispose();
```

### View results

In chrome, go to `chrome://tracing/` and drag the resulting `myfile.json` into the browser

