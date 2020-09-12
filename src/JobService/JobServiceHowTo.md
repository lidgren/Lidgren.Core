
# How to use JobService

## Initialization

```
JobService.Initialize();
```

## Enqueue a single job

```
// schedule a single job to run; returns immediately
JobService.Enqueue((x) => { Console.WriteLine("hello from job"); });

// runs action(argument) on a worker thread; returns immediately
JobService.Enqueue("name", action, argument);

// runs action(argument) on a worker thread; calls anotherAction(anotherArgument) when it is complete; returns immediately
JobService.Enqueue("name", action, argument, anotherAction, anotherArgument);
```

## Enqueue jobs to run on as many threads as possible; going wide

```
// schedule a wide job; blocks until all has completed
JobService.EnqueueWideBlock("name", action, argument);

// schedule a wide job; call 'anotherAction' when all has finished; returns immediately
JobService.EnqueueWide("name", action, argument, anotherAction, anotherArgument);

// schedule a wide job; returns immediately
JobService.EnqueueWide("name", action, argument, null, null);
```

## Enqueueing multiple actions to be run with the same argument

```
// run each action in list concurrently with the argument; blocks until all has completed
JobService.ForEachActionBlock("name", actionsList, argument);

// run each action in list concurrently with the argument; when all completed call anotherAction(anotherArgument); returns immediately
JobService.ForEachAction("name", actionsList, argument, anotherAction, anotherArgument);

// schedule work to be done on each argument in list, return immediately
JobService.ForEachAction("name", actionsList, argument, null, null);
```

## Enqueueing an action to be run on each argument in a list

```
// schedule work to be done on each argument in list; blocks until all has completed
JobService.ForEachArgumentBlock("name", action, argumentsList);

// schedule work to be done on each argument in list; when all completed call anotherAction(anotherArgument); returns immediately
JobService.ForEachArgument("name", action, argumentsList, anotherAction, anotherArgument);

// schedule work to be done on each argument in list; returns immediately
JobService.ForEachArgument("name", action, argumentsList, null, null);
```

## Shutdown

```
// shutdown to close all worker threads
JobService.Shutdown();
```
