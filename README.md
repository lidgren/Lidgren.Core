# Lidgren.Core
Various core functions useful to almost every project; but with an emphasis towards game development.

Goals are:
* No dependencies
* Best possible performance
* No reflection
* Heap allocation free whenever possible
* Use of spans instead of collection interfaces
* Simple and self explanatory
* Everything thoroughly unit tested

Non-goals are:
* Compatibility with older frameworks
* Never changing API

Documentation in the [wiki](https://github.com/lidgren/Lidgren.Core/wiki) or .md files within project

# Highlights
* Random number generation, including seeding
* [Hash utilities](https://github.com/lidgren/Lidgren.Core/wiki/Hashing)
* FastList<T>, alternative to List<T>
* FastDictionary<K,V>, alternative to Dictionary<K,V>
* FixedStringBuilder/ValueStringBuilder
* WildcardMatcher for matching * and ?
* Allocation free span tokenizer
* [Timing service for instrumented profiling](https://github.com/lidgren/Lidgren.Core/wiki/TimingService)
* DataWriter/Reader and span extensions for working with binary data
* [JobService, alternative to the thread pool](https://github.com/lidgren/Lidgren.Core/wiki/JobService)
* DynamicOrdering, for ordering items by local definitions
* DynamicScheduler, for scheduling and executing item in JobService
* Color; including randomizing
* PriorityQueue
* FourCC
* File utils; hashing and comparing
